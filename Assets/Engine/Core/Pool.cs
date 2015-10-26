using UnityEngine;
using System.Collections.Generic;

class Pool {
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private GameObject m_containerObj = null;
	private GameObject m_prefab = null;
	
	private List<GameObject> m_instances;
	
	private int m_growth;
	private bool m_dontDestroyContainer;
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	public Pool(GameObject _prefab, Transform _parent, int _initSize, bool _canGrow, bool _createContainer) {
		m_prefab = _prefab;

		// Create a new container or use parent transform as a container?
		m_dontDestroyContainer = !_createContainer;
		if(_createContainer) {
			m_containerObj = new GameObject();
			m_containerObj.name = "Pool of " + m_prefab.name;
			m_containerObj.transform.parent = _parent;
		} else {
			m_containerObj = _parent.gameObject;
		}
		
		m_instances = new List<GameObject>();
		
		Instantiate(_initSize);
		
		if (_canGrow) {
			m_growth = _initSize;
		} else {
			m_growth = 0;
		}
	}

	/// <summary>
	/// Manually destroy all created instances. No need to call it for scene changes, 
	/// since scene hierarchy is already cleared.
	/// </summary>
	public void Clear() {
		// Destroy all the created instances
		// Destroying the container is enough, but we don't want to do that if the container wasn't created by us
		if(m_dontDestroyContainer) {
			for(int i = 0; i < m_instances.Count; i++) {
				GameObject.Destroy(m_instances[i]);
			}
		} else {
			GameObject.Destroy(m_containerObj);
		}
		m_instances.Clear();
		m_containerObj = null;
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
			inst.transform.parent = m_containerObj.transform;
			inst.SetActive(false);
			
			m_instances.Add(inst);
		}
	}
};