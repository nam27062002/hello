using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class UIPoolManager : UbiBCN.SingletonMonoBehaviour<UIPoolManager> {
	// Entity Pools requests (delayed pool manager)
	private Dictionary<string, Pool> m_pools = new Dictionary<string, Pool>();
	private Dictionary<Pool, PoolHandler> m_handlers = new Dictionary<Pool, PoolHandler>();
	private List<Pool> m_iterator = new List<Pool>();


	//---------------------------------------------------------------//
	//-- Static Methods ---------------------------------------------//
	//---------------------------------------------------------------//
	/// <summary>
	/// Alternative version specifying where to create the pool. All instances will
	/// be created as children of _container.
	/// </summary>
	public static PoolHandler CreatePool(GameObject _prefab, Transform _container, int _initSize = 10, bool _canGrow = true, bool _temporary = true) {
		// Skip if the pool already exists
		if (instance.m_pools.ContainsKey(_prefab.name)) {
			return instance.m_handlers[instance.m_pools[_prefab.name]];
		} else {
			Pool pool = new Pool(_prefab, _prefab.name, null, _container, _initSize, _canGrow, _container == instance.transform, _temporary);	// [AOC] Create new container if given container is the Pool Manager.
			PoolHandler handler = new PoolHandler(pool);

			instance.m_pools.Add(_prefab.name, pool);
			instance.m_handlers.Add(pool, handler);
			instance.m_iterator.Add(pool);

			return handler;
		}
	}

	public static PoolHandler CreatePool(GameObject _prefab, int _initSize = 10, bool _canGrow = true, bool _temporary = true) {
		return CreatePool(_prefab, instance.transform, _initSize, _canGrow, _temporary);
	}

	public static PoolHandler GetHandler(string _prefabName) {
		if (instance.m_pools.ContainsKey(_prefabName)) {
			return instance.m_handlers[instance.m_pools[_prefabName]];
		}
		return null;
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

	private void __Clear(bool _all) {
		if (instance != null) {
			if (_all) {
				foreach(KeyValuePair<string, Pool> p in m_pools) {
					p.Value.Clear();
				}			
				m_pools.Clear();
			} else {
				// we'll clear only temporary pools (those that don't have to exist between levels)
				List<string> keys = new List<string>(m_pools.Keys);
				for (int i = 0; i < keys.Count; i++) {
					Pool pool = m_pools[keys[i]];
					if (pool.isTemporary) {
						pool.Clear();

						m_iterator.Remove(pool);
						m_handlers[pool].Invalidate();
						m_handlers.Remove(pool);
						m_pools.Remove(keys[i]);
					}
				}
			}
		}
	}
    
}
