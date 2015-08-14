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

		//-----------------------------------------------
		// Methods
		//-----------------------------------------------
		public Pool(GameObject _prefab, Transform _parent) {
			m_prefab = _prefab;

			gameObject = new GameObject();
			gameObject.name = "Pool of " + m_prefab.name;
			gameObject.transform.parent = _parent;

			m_instances = new List<GameObject>();
		}

		public GameObject Get() {			
			for (int i = 0; i < m_instances.Count; i++) {
				if (!m_instances[i].activeInHierarchy) {
					m_instances[i].SetActive(true);
					return m_instances[i];
				}
			}

			GameObject inst = (GameObject)Object.Instantiate(m_prefab);					
			inst.name = m_prefab.name;
			inst.transform.parent = gameObject.transform;
			inst.SetActive(true);

			m_instances.Add(inst);

			return m_instances[m_instances.Count - 1];
		}
	};

	public List<GameObject> m_prefabs;
	Dictionary<string, Pool> m_pools;

	void Awake () {
		m_pools = new Dictionary<string, Pool>();

		foreach (GameObject go in m_prefabs) {
			CreatePool(go);
		}
	}

	public void CreatePool(GameObject _gameObject) {
		if (!m_pools.ContainsKey(_gameObject.name)) {
			Pool pool = new Pool(_gameObject, transform);
			m_pools.Add(_gameObject.name, pool);
		}
	}

	public GameObject GetInstance(string _id) {
		if (m_pools.ContainsKey(_id)) 
			return m_pools[_id].Get();

		return null;
	}
}
