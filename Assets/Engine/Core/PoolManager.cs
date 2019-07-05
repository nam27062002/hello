using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO;


public class PoolManager : UbiBCN.SingletonMonoBehaviour<PoolManager> {
	
	private class PoolContainer {
		public Pool 		pool;
		public PoolHandler 	handler;
		public int          size;
	}

    public enum PoolLimits {
        Limited = 0,
        Unlimited
    }

    public static bool sm_printPools = false;

	// Entity Pools requests (delayed pool manager)
	private SortedDictionary<string, PoolContainer> m_pools = new SortedDictionary<string, PoolContainer>();
	private List<Pool> m_iterator = new List<Pool>();

	private SortedDictionary<string, int> m_poolSizes = new SortedDictionary<string, int>();


	private float m_printTimer = 10f;

    private PoolLimits m_poolLimits = PoolLimits.Limited;
    public PoolLimits poolLimits {
        get { return m_poolLimits; }
        set { m_poolLimits = value; }
    }

    //---------------------------------------------------------------//
    //-- Static Methods ---------------------------------------------//
    //---------------------------------------------------------------//

    /// <summary>
    /// Request a pool. Stores a pool request, it'll be created later loading the level.
    /// </summary>
    /// <param name="_prefabName">Prefab name. Id to ask for this resource.</param>
    /// <param name="_size">Final pool size.</param>
    public static PoolHandler RequestPool(string _prefabName, int _size) {
		return instance.__RequestPool(_prefabName, _size);
	}

	public static PoolHandler GetHandler(string _prefabName) {
		if (instance.m_pools.ContainsKey(_prefabName)) {
			return instance.m_pools[_prefabName].handler;
		}
		return null;
	}

	public static bool ContainsPool(string _prefabName) {
		return instance.m_pools.ContainsKey(_prefabName);
	}

	public static void PreBuild() {
        Clear(true);
		instance.GetPoolSizesForCurrentArea();
		Broadcaster.Broadcast(BroadcastEventType.POOL_MANAGER_READY);
	}

	public static void Build() { 
		instance.__Build();
	}

	public static void Rebuild() {
        Build();
        //instance.__Rebuild();
	}

	/// <summary>
	/// Creates the pool. Creates a pool for the resource _prefabPath with the id _prefabName. With an initial size _initSize. It can grow if _canGrow
	/// </summary>
	/// <param name="_prefabName">Prefab name. Id to ask for this resource.</param>
	/// <param name="_initSize">Init size.</param>
	/// <param name="_canGrow">If set to <c>true</c> can grow.</param>
	public static PoolHandler CreatePool(string _prefabName, int _initSize = 10, bool _canGrow = true, bool _temporay = true) {
		PoolHandler handler 	= instance.__RequestPool(_prefabName, _initSize);
		PoolContainer container 	= instance.m_pools[_prefabName];

		instance.__CreatePool(container, _prefabName, _canGrow, _temporay);

		return handler;
	}

	public static void ResizePool(string _prefabName, int _newSize) {        
		if (instance.m_pools.ContainsKey(_prefabName)) {
			Pool pool = instance.m_pools[_prefabName].pool;
			pool.Resize(_newSize);
		}
	}

	/// <summary>
	/// Will destroy all the pools and loose reference to any created instances.
	/// Additionally they can be deleted from the scene.
	/// </summary>
	public static void Clear(bool _all) { 
		instance.__Clear(_all);
	}



	//---------------------------------------------------------------//
	//-- Instance Methods -------------------------------------------//
	//---------------------------------------------------------------//

	void Update() {
		if (sm_printPools) {			
			m_printTimer -= Time.deltaTime;
			if (m_printTimer <= 0f) {
				if (LevelManager.currentLevelData != null) {
					string fileName = "NPC_Pools_" + LevelManager.currentLevelData.def.sku + "_" + LevelManager.currentArea + ".xml";
					using (StreamWriter sw = new StreamWriter(fileName, false)) {
						sw.WriteLine("<Definitions>");
						foreach (KeyValuePair<string, PoolContainer> pair in m_pools) {
							if (pair.Value.pool != null) {
								sw.WriteLine("<Definition sku=\"" + pair.Key + "\" poolSize=\"" + pair.Value.pool.Size() + "\"/>");
							}
						}
						sw.WriteLine("</Definitions>");
						sw.Close();
					}
				}
				m_printTimer = 10f;
			}
		}

		for (int i = 0; i < m_iterator.Count; i++) {
			m_iterator[i].Update();
		}
	}

	private void GetPoolSizesForCurrentArea(){
		m_poolSizes.Clear();

		string category = "POOL_MANAGER_SETTINGS_" + LevelManager.currentLevelData.def.sku + "_" + LevelManager.currentArea;
		List<DefinitionNode> poolSizes = DefinitionsManager.SharedInstance.GetDefinitionsList(category.ToUpper());
		for (int i = 0; i < poolSizes.Count; ++i) {
			m_poolSizes.Add(poolSizes[i].Get("sku"), poolSizes[i].GetAsInt("poolSize"));
		}
	}


	private PoolHandler __RequestPool(string _prefabName, int _size) {
		PoolContainer container;

		if (m_poolSizes.ContainsKey(_prefabName)) {
			_size = m_poolSizes[_prefabName];
		}

		if (m_pools.ContainsKey(_prefabName)) {
			container = m_pools[_prefabName];
			if (container.size < _size)
                container.size = _size;			
		} else {
			container = new PoolContainer();
			container.size = _size;
			container.handler = new PoolHandler();

			m_pools[_prefabName] = container;
		}

		return container.handler;
	}

	private void __Build() {
		List<string> keys = new List<string>(m_pools.Keys);

		for (int i = 0; i < keys.Count; i++) {
            __CreatePool(m_pools[keys[i]], keys[i], m_poolLimits == PoolLimits.Unlimited, true);
		}
	}

	private void __Rebuild() {
        /*
		List<string> keys;
		List<string> toDelete = new List<string>Resources.UnloadUnusedAssets();();

		// First eliminate non using prefabs and reduce bigger than need pools
		keys = new List<string>(m_pools.Keys);
		for (int i = 0; i < keys.Count; i++) {
			PoolContaier container = m_pools[keys[i]];
			Pool p = container.pool;

			if (p != null && p.isTemporary) {
				if (container.size > 0) {
					if (container.size < p.Size()) {
						p.Resize(container.size);
					}
				} else {
					p.Clear();
					m_iterator.Remove(p);

					container.pool = null;
					container.handler.Invalidate();
					toDelete.Add(keys[i]);
				}
			}
		}

		for (int i = 0; i < toDelete.Count; ++i) {
			m_pools.Remove(toDelete[i]);
        }
		toDelete.Clear();

		Resources.UnloadUnusedAssets();

		// Increase and Add new prefabs
		keys = new List<string>(m_pools.Keys);
		for (int i = 0; i < keys.Count; i++) {
			PoolContaier container = m_pools[keys[i]];
			Pool p = container.pool;

			if (p != null) {				
				if (container.size > p.Size()) {
					// increase size
					p.Resize(container.size);
				}
			} else {
				// Create pool
                __CreatePool(container, keys[i], m_poolLimits == PoolLimits.Unlimited, true);
			}
		}*/
    }

    private void __CreatePool(PoolContainer _container, string _prefabName, bool _canGrow, bool _temporay) {
        if (_container.pool == null) {
            GameObject go = HDAddressablesManager.Instance.LoadAsset<GameObject>(_prefabName);

            if (go != null) {
				int size = _container.size;

				if (sm_printPools) size = 1;
               
                 Pool pool = new Pool(go, _prefabName, null, transform, size, _canGrow, true, _temporay);
				_container.pool = pool;
				m_iterator.Add(pool);
			} else {
				Debug.LogError("Can't create a pool for: " + _prefabName);
			}
            _container.size = 0;
		}

		_container.handler.AssignPool(_container.pool);
	}

	private void __Clear(bool _all) {
		if (instance != null) {
            List<string> keys = new List<string>(m_pools.Keys);
            if (_all) {
                for (int i = 0; i < keys.Count; i++) {
                    PoolContainer container = m_pools[keys[i]];
                    if (container.pool != null) {
                        container.pool.Clear();
                        container.pool = null;
                        container.handler.Invalidate();
					}
				}
                m_pools.Clear();
                m_iterator.Clear();
			} else {
				// we'll clear only temporary pools (those that don't have to exist between levels)				
				for (int i = 0; i < keys.Count; i++) {
					PoolContainer container = m_pools[keys[i]];
					Pool p = container.pool;
					if (p.isTemporary) {
						p.Clear();

						m_iterator.Remove(p);
						container.pool = null;
						container.handler.Invalidate();
                        m_pools.Remove(keys[i]);
                    }
				}
			}
            Resources.UnloadUnusedAssets();
        }
	}    
}
