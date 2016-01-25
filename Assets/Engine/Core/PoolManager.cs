using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : SingletonMonoBehaviour<PoolManager> {

	private Dictionary<string, Pool> m_pools = new Dictionary<string, Pool>();

	/// <summary>
	/// Default pool creation. Will be created as child of the Singleton GameObject.
	/// </summary>
	public static void CreatePool(GameObject _prefab, int _initSize = 10, bool _canGrow = true) {
		// Use alternative function
		CreatePool(_prefab, instance.transform, _initSize, _canGrow);
	}

	/// <summary>
	/// Alternative version specifying where to create the pool. All instances will
	/// be created as children of _container.
	/// </summary>
	public static void CreatePool(GameObject _prefab, Transform _container, int _initSize = 10, bool _canGrow = true) {
		// Skip if the pool already exists
		if(!instance.m_pools.ContainsKey(_prefab.name)) {
			Pool pool = new Pool(_prefab, _container, _initSize, _canGrow, _container == instance.transform);	// [AOC] Create new container if given container is the Pool Manager.
			instance.m_pools.Add(_prefab.name, pool);
		}
	}

	/// <summary>
	/// Get the first available instance of the prefab with the given name.
	/// </summary>
	public static GameObject GetInstance(string _id) {
		if(instance.m_pools.ContainsKey(_id)) 
			return instance.m_pools[_id].Get();

		return null;
	}

	/// <summary>
	/// Will destroy all the pools and loose reference to any created instances.
	/// Additionally they can be deleted from the scene.
	/// </summary>
	public static void Clear(bool _delete) {
		if(_delete && instance != null) {
			foreach(KeyValuePair<string, Pool> p in instance.m_pools) {
				p.Value.Clear();
			}
		}
		if ( instance != null )
			instance.m_pools.Clear();
	}

	/// <summary>
	/// Automatically clear ourselves when a new level is loaded.
	/// </summary>
	private void OnLevelWasLoaded() {
		// Clear the pool manager with every new scene
		// Don't delete pool's instances, it will be automatically done due to the scene change that just happened, only clear references
		// Avoid creating new pools in the Awake calls, do it on the Start at least
		Clear(false);
	}
}
