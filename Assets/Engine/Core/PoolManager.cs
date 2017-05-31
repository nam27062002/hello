using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : UbiBCN.SingletonMonoBehaviour<PoolManager> {
	private class PoolData {
		public int 			size;
		public string 		path;
	}

	private class PoolContaier {
		public Pool 		pool;
		public PoolHandler 	handler;
		public PoolData		buildData;
	}

	// Entity Pools requests (delayed pool manager)
	private Dictionary<string, PoolContaier> m_pools = new Dictionary<string, PoolContaier>();
	private List<Pool> m_iterator = new List<Pool>();



	//---------------------------------------------------------------//
	//-- Static Methods ---------------------------------------------//
	//---------------------------------------------------------------//

	/// <summary>
	/// Request a pool. Stores a pool request, it'll be created later loading the level.
	/// </summary>
	/// <param name="_prefabName">Prefab name. Id to ask for this resource.</param>
	/// <param name="_prefabPath">Prefab path. Resources path without the prefab name.</param>
	/// <param name="_size">Final pool size.</param>
	public static PoolHandler RequestPool(string _prefabName, string _prefabPath, int _size) {
		return instance.__RequestPool(_prefabName, _prefabPath, _size);
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

	public static void Build() { 
		instance.__Build();
	}

	public static void Rebuild() {
		instance.__Rebuild();
	}

	/// <summary>
	/// Creates the pool. Creates a pool for the resource _prefabPath with the id _prefabName. With an initial size _initSize. It can grow if _canGrow
	/// </summary>
	/// <param name="_prefabName">Prefab name. Id to ask for this resource.</param>
	/// <param name="_prefabPath">Prefab path. Resources path without the prefab name.</param>
	/// <param name="_initSize">Init size.</param>
	/// <param name="_canGrow">If set to <c>true</c> can grow.</param>
	public static PoolHandler CreatePool(string _prefabName, string _prefabPath, int _initSize = 10, bool _canGrow = true, bool _temporay = true) {
		PoolHandler handler 	= instance.__RequestPool(_prefabName, _prefabPath, _initSize);
		PoolContaier container 	= instance.m_pools[_prefabName];

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
		for (int i = 0; i < m_iterator.Count; i++) {
			m_iterator[i].Update();
		}
	}

	private PoolHandler __RequestPool(string _prefabName, string _prefabPath, int _size) {
		PoolContaier container;

		if (m_pools.ContainsKey(_prefabName)) {
			container = m_pools[_prefabName];
			PoolData data = container.buildData;
			if (data.size < _size)
				data.size = _size;			
		} else {
			PoolData data = new PoolData();
			data.path = _prefabPath;
			data.size = _size;

			container = new PoolContaier();
			container.buildData = data;
			container.handler = new PoolHandler();

			m_pools[_prefabName] = container;
		}

		return container.handler;
	}

	private void __Build() {
		List<string> keys = new List<string>(m_pools.Keys);

		for (int i = 0; i < keys.Count; i++) {
			__CreatePool(m_pools[keys[i]], keys[i], true, true);
		}
	}

	private void __Rebuild() {
		List<string> keys;

		// First eliminate non using prefabs and reduce bigger than need pools
		keys = new List<string>(m_pools.Keys);
		for (int i = 0; i < keys.Count; i++) {
			PoolContaier container = m_pools[keys[i]];
			Pool p = container.pool;

			if (p != null && p.isTemporary) {
				PoolData data = container.buildData;
				if (data.size > 0) {
					if (data.size < p.Size()) {
						p.Resize(data.size);
					}
				} else {
					p.Clear();
					m_iterator.Remove(p);

					container.pool = null;
					container.handler.Invalidate();
				}
			}
		}

		Resources.UnloadUnusedAssets();

		// Increase and Add new prefabs
		keys = new List<string>(m_pools.Keys);
		for (int i = 0; i < keys.Count; i++) {
			PoolContaier container = m_pools[keys[i]];
			Pool p = container.pool;

			if (p != null) {
				PoolData data = container.buildData;
				if (data.size > p.Size()) {
					// increase size
					p.Resize(data.size);
				}
			} else {
				// Create pool
				__CreatePool(container, keys[i], true, true);
			}
		}
	}

	private void __CreatePool(PoolContaier _container, string _prefabName, bool _canGrow, bool _temporay) {
		if (_container.pool == null) {
			PoolData data = _container.buildData;
			GameObject go = Resources.Load<GameObject>(data.path + _prefabName);
			if (go != null) {
				Pool pool = new Pool(go, transform, data.size, _canGrow, true, _temporay);
				_container.pool = pool;
				m_iterator.Add(pool);
			} else {
				Debug.LogError("Can't create a pool for: " + data.path + _prefabName);
			}
			data.size = 0;
		}

		_container.handler.AssignPool(_container.pool);
	}

	private void __Clear(bool _all) {
		if (instance != null) {
			if (_all) {
				foreach(KeyValuePair<string, PoolContaier> pc in m_pools) {
					if (pc.Value.pool != null) {
						pc.Value.pool.Clear();
						pc.Value.pool = null;
					}
				}
				m_iterator.Clear();
			} else {
				// we'll clear only temporary pools (those that don't have to exist between levels)
				List<string> keys = new List<string>(m_pools.Keys);
				for (int i = 0; i < keys.Count; i++) {
					PoolContaier container = m_pools[keys[i]];
					Pool p = container.pool;
					if (p.isTemporary) {
						p.Clear();

						m_iterator.Remove(p);
						container.pool = null;
					}
				}
			}
		}
	}    
}
