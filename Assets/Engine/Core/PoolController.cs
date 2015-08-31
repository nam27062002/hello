using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolController : MonoBehaviour {

	class Pool {
		//-----------------------------------------------
		// Attributes
		//-----------------------------------------------
		private GameObject gameObject = null;
		private GameObject m_prefab = null;

		private List<GameObject> m_instances;

		private int m_growth;

		//-----------------------------------------------
		// Methods
		//-----------------------------------------------
		public Pool(GameObject _prefab, Transform _parent, int _initSize, bool _canGrow) {
			m_prefab = _prefab;

			gameObject = new GameObject();
			gameObject.name = "Pool of " + m_prefab.name;
			gameObject.transform.parent = _parent;

			m_instances = new List<GameObject>();

			Instantiate(_initSize);

			if (_canGrow) {
				m_growth = _initSize;
			} else {
				m_growth = 0;
			}
		}

		public GameObject Get() {			
			int i = 0; 
			for (i = 0; i < m_instances.Count; i++) {
				if (!m_instances[i].activeInHierarchy) {
					m_instances[i].SetActive(true);
					return m_instances[i];
				}
			}

			if (m_growth > 0) {
				Instantiate(m_growth);
				m_growth = Mathf.Max(1, m_growth / 2);
				
				m_instances[i].SetActive(true);
				return m_instances[i];
			}

			return null;
		}

		private void Instantiate(int _count) {

			for (int i = 0; i < _count; i++) {
				GameObject inst = (GameObject)Object.Instantiate(m_prefab);					
				inst.name = m_prefab.name;
				inst.transform.parent = gameObject.transform;
				inst.SetActive(false);
				
				m_instances.Add(inst);
			}
		}
	};

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
