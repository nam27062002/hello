using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : Singleton<PoolManager> {

	private Dictionary<string, Pool> m_pools = new Dictionary<string, Pool>();

	void Awake() {
		base.Awake();
	}

	public static void CreatePool(GameObject _gameObject, int _initSize = 10, bool _canGrow = true) {
		if(!instance.m_pools.ContainsKey(_gameObject.name)) {
			Pool pool = new Pool(_gameObject, instance.transform, _initSize, _canGrow);
			instance.m_pools.Add(_gameObject.name, pool);
		}
	}

	public static GameObject GetInstance(string _id) {
		if(instance.m_pools.ContainsKey(_id)) 
			return instance.m_pools[_id].Get();

		return null;
	}

	public static void Clear() {
		instance.m_pools.Clear();
	}
}
