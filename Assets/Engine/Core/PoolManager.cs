using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : UbiBCN.SingletonMonoBehaviour<PoolManager> {
	private class PoolRequest {
		public int size;
		public string path;
	}

	// Entity Pools requests (delayed pool manager)
	private Dictionary<string, PoolRequest> m_poolRequests = new Dictionary<string, PoolRequest>();
	private Dictionary<string, Pool> m_pools = new Dictionary<string, Pool>();
	private List<Pool> m_iterator = new List<Pool>();

	void Update() {
		for (int i = 0; i < m_iterator.Count; i++) {
			m_iterator[i].Update();
		}
	}

	/// <summary>
	/// Request a pool. Stores a pool request, it'll be created later loading the level.
	/// </summary>
	/// <param name="_prefabName">Prefab name. Id to ask for this resource.</param>
	/// <param name="_prefabPath">Prefab path. Resources path without the prefab name.</param>
	/// <param name="_size">Final pool size.</param>
	public static void RequestPool(string _prefabName, string _prefabPath, int _size) {
		PoolRequest pr = null;

		if (instance.m_poolRequests.ContainsKey(_prefabName)) {
			pr = instance.m_poolRequests[_prefabName];
			if (pr.size < _size) 
				pr.size = _size;
		} else {
			pr = new PoolRequest();
			pr.path = _prefabPath;
			pr.size = _size;
		}

		instance.m_poolRequests[_prefabName] = pr;
	}

	public static void Build() {
		List<string> keys = new List<string>(instance.m_poolRequests.Keys);

		for (int i = 0; i < keys.Count; i++) {
			PoolRequest pr = instance.m_poolRequests[keys[i]];
			CreatePool(keys[i], pr.path, pr.size, true, true); // should it grow?
		}

		instance.m_poolRequests.Clear();
	}

	public static void Rebuild() {
		List<string> keys;

		// First eliminate non using prefabs and reduce bigger than need pools
		keys = new List<string>(instance.m_pools.Keys);
		for (int i = 0; i < keys.Count; i++) {
			Pool p = instance.m_pools[keys[i]];

			if (p.isTemporary) {
				if (instance.m_poolRequests.ContainsKey(keys[i])) {
					PoolRequest pr = instance.m_poolRequests[keys[i]];

					if (pr.size < p.Size()) {
						p.Resize(pr.size);
					}
				} else {
					p.Clear();
					instance.m_iterator.Remove(instance.m_pools[keys[i]]);
					instance.m_pools.Remove(keys[i]);
				}
			}
		}

		Resources.UnloadUnusedAssets();

		// Increase and Add new prefabs
		keys = new List<string>(instance.m_poolRequests.Keys);
		for (int i = 0; i < keys.Count; i++) {
			PoolRequest pr = instance.m_poolRequests[keys[i]];

			if (instance.m_pools.ContainsKey(keys[i])) {
				Pool p = instance.m_pools[keys[i]];
			
				if (pr.size > p.Size()) {
					// increase size
					p.Resize(pr.size);
				}
			} else {
				// Create pool
				CreatePool(keys[i], pr.path, pr.size, true, true); // should it grow?
			}
		}

		instance.m_poolRequests.Clear();
	}

    public static bool ContainsPool(string _prefab) {
        return instance.m_pools.ContainsKey(_prefab);
    }

	/// <summary>
	/// Will destroy all the pools and loose reference to any created instances.
	/// Additionally they can be deleted from the scene.
	/// </summary>
	public static void Clear(bool _all) {
		if (instance != null) {
			if (_all) {
				foreach(KeyValuePair<string, Pool> p in instance.m_pools) {
					p.Value.Clear();
				}			
				instance.m_pools.Clear();
			} else {
				// we'll clear only temporary pools (those that don't have to exist between levels)
				List<string> keys = new List<string>(instance.m_pools.Keys);
				for (int i = 0; i < keys.Count; i++) {
					Pool pool = instance.m_pools[keys[i]];
					if (pool.isTemporary) {
						pool.Clear();
						instance.m_iterator.Remove(instance.m_pools[keys[i]]);
						instance.m_pools.Remove(keys[i]);
					}
				}
			}
		}
	}

	/// <summary>
	/// Default pool creation. Will be created as child of the Singleton GameObject.
	/// </summary>
	public static void CreatePool(GameObject _prefab, int _initSize = 10, bool _canGrow = true, bool _temporay = true) {
		// Use alternative function
		CreatePool(_prefab, instance.transform, _initSize, _canGrow, _temporay);
	}

	/// <summary>
	/// Alternative version specifying where to create the pool. All instances will
	/// be created as children of _container.
	/// </summary>
	public static void CreatePool(GameObject _prefab, Transform _container, int _initSize = 10, bool _canGrow = true, bool _temporay = true) {
		// Skip if the pool already exists
		if(!instance.m_pools.ContainsKey(_prefab.name)) {
			Pool pool = new Pool(_prefab, _container, _initSize, _canGrow, _container == instance.transform, _temporay);	// [AOC] Create new container if given container is the Pool Manager.
			instance.m_pools.Add(_prefab.name, pool);
			instance.m_iterator.Add(pool);
		}
	}

	/// <summary>
	/// Creates the pool. Creates a pool for the resource _prefabPath with the id _prefabName. With an initial size _initSize. It can grow if _canGrow
	/// </summary>
	/// <param name="_prefabName">Prefab name. Id to ask for this resource.</param>
	/// <param name="_prefabPath">Prefab path. Resources path without the prefab name.</param>
	/// <param name="_initSize">Init size.</param>
	/// <param name="_canGrow">If set to <c>true</c> can grow.</param>
	public static void CreatePool(string _prefabName, string _prefabPath, int _initSize = 10, bool _canGrow = true, bool _temporay = true) {
		// Use alternative function
		CreatePool(_prefabName, _prefabPath, instance.transform, _initSize, _canGrow, _temporay);
	}

	/// <summary>
	/// Creates the pool.
	/// </summary>
	/// <param name="_prefabName">Prefab name.</param>
	/// <param name="_prefabPath">Prefab path.</param>
	/// <param name="_container">Container.</param>
	/// <param name="_initSize">Init size.</param>
	/// <param name="_canGrow">If set to <c>true</c> can grow.</param>
	private static void CreatePool(string _prefabName, string _prefabPath, Transform _container, int _initSize = 10, bool _canGrow = true, bool _temporay = true) {
		// Skip if the pool already exists
		if (!instance.m_pools.ContainsKey(_prefabName)) {
			GameObject go = Resources.Load<GameObject>(_prefabPath + _prefabName);
			if (go != null) {
				Pool pool = new Pool(go, _container, _initSize, _canGrow, _container == instance.transform, _temporay);	// [AOC] Create new container if given container is the Pool Manager.
				instance.m_pools.Add(_prefabName, pool);
				instance.m_iterator.Add(pool);
			} else {
				Debug.LogError("Can't create a pool for: " + _prefabPath + _prefabName);
			}
		}
	}

    public static void ResizePool(string _prefabName, int _newSize) {        
        if (instance.m_pools.ContainsKey(_prefabName)) {
            Pool pool = instance.m_pools[_prefabName];
            pool.Resize(_newSize);
        }
    }


	/// <summary>
	/// Get the first available instance of the prefab with the given name.
	/// </summary>
	public static GameObject GetInstance(string _id, bool _activate = true) {
		if(instance.m_pools.ContainsKey(_id)) 
			return instance.m_pools[_id].Get(_activate);

		return null;
	}

	/// <summary>
	/// Return the instance to the pool
	/// </summary>
	public static void ReturnInstance( GameObject go )
	{
		if ( instance.m_pools.ContainsKey( go.name ) )
			instance.m_pools[ go.name ].Return(go);
	}

	/// <summary>
	/// Returns the instance by id _name
	/// </summary>
	/// <param name="_name">Name.</param>
	/// <param name="go">Instance</param>
	public static void ReturnInstance( string _name, GameObject go)
	{
		if ( instance.m_pools.ContainsKey( _name ) )
			instance.m_pools[ _name ].Return(go);
	}
}
