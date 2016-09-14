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
	/// Creates the pool. Creates a pool for the resource _prefabPath with the id _prefabName. With an initial size _initSize. It can grow if _canGrow
	/// </summary>
	/// <param name="_prefabName">Prefab name. Id to ask for this resource</param>
	/// <param name="_prefabPath">Prefab path. Resources path</param>
	/// <param name="_initSize">Init size.</param>
	/// <param name="_canGrow">If set to <c>true</c> can grow.</param>
	public static void CreatePool(string _prefabName, string _prefabPath, int _initSize = 10, bool _canGrow = true) {
		// Use alternative function
		CreatePool(_prefabName, _prefabPath, instance.transform, _initSize, _canGrow);
	}

	/// <summary>
	/// Creates the pool.
	/// </summary>
	/// <param name="_prefabName">Prefab name.</param>
	/// <param name="_prefabPath">Prefab path.</param>
	/// <param name="_container">Container.</param>
	/// <param name="_initSize">Init size.</param>
	/// <param name="_canGrow">If set to <c>true</c> can grow.</param>
	public static void CreatePool(string _prefabName, string _prefabPath, Transform _container, int _initSize = 10, bool _canGrow = true) {
		// Skip if the pool already exists
		if (!instance.m_pools.ContainsKey(_prefabName)) {
			GameObject go = Resources.Load<GameObject>(_prefabPath);
			if (go != null) {
				Pool pool = new Pool(go, _container, _initSize, _canGrow, _container == instance.transform);	// [AOC] Create new container if given container is the Pool Manager.
				instance.m_pools.Add(_prefabName, pool);
			} else {
				Debug.LogError("Can't create a pool for: " + _prefabPath);
			}
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
