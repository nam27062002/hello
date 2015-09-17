using UnityEngine;
using System.Collections.Generic;

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