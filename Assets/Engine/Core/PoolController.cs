using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolController : MonoBehaviour {

	public List<GameObject> m_prefabs;
	Dictionary<string, Pool> m_pools;

	void Awake () {
		m_pools = new Dictionary<string, Pool>();

		foreach (GameObject go in m_prefabs) {
			CreatePool(go);
		}

		InstanceManager.pools = this;
	}

	public void CreatePool(GameObject _gameObject, int _initSize = 10, bool _canGrow = true) {
		if (!m_pools.ContainsKey(_gameObject.name)) {
			Pool pool = new Pool(_gameObject, transform, _initSize, _canGrow);
			m_pools.Add(_gameObject.name, pool);
		}
	}

	public GameObject GetInstance(string _id) {
		if (m_pools.ContainsKey(_id)) 
			return m_pools[_id].Get();

		return null;
	}
}
