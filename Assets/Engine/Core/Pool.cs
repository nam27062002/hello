using UnityEngine;
using System.Collections.Generic;

public class Pool {
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private GameObject m_containerObj = null;
	private GameObject m_prefab = null;

	private Queue<GameObject> m_freeObjects;
	private HashSet<GameObject> m_notFreeObjects;
	
	private bool m_canGrow;
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

		m_freeObjects = new Queue<GameObject>();
		m_notFreeObjects = new HashSet<GameObject>();

		for( int i = 0; i<_initSize; i++ )
		{
			GameObject go = Instantiate();
			m_freeObjects.Enqueue( go );
		}
		m_canGrow = _canGrow;
	}

	/// <summary>
	/// Manually destroy all created instances. No need to call it for scene changes, 
	/// since scene hierarchy is already cleared.
	/// </summary>
	public void Clear() {
		// Destroy all the created instances
		// Destroying the container is enough, but we don't want to do that if the container wasn't created by us

		if ( m_dontDestroyContainer )
		{
			while( m_freeObjects.Count > 0)
			{
				GameObject go = m_freeObjects.Dequeue();
				GameObject.Destroy( go );
			}
			foreach( GameObject go in m_notFreeObjects)
				GameObject.Destroy( go );
		}
		else
		{
			GameObject.Destroy( m_containerObj );
		}

		m_freeObjects.Clear();
		m_notFreeObjects.Clear();
		m_containerObj = null;
	}
	
	public GameObject Get()
	{			
		if ( m_freeObjects.Count <= 0 && m_canGrow)
		{
			m_freeObjects.Enqueue( Instantiate() );
		}

		if ( m_freeObjects.Count > 0)
		{
			GameObject go = m_freeObjects.Dequeue();
			go.SetActive( true );
			m_notFreeObjects.Add( go );
			return go;
		}
		return null;
	}

	public void Return( GameObject go)
	{
		// In debug check!
		if ( m_notFreeObjects.Contains(go) )
		{
			m_notFreeObjects.Remove( go );
			go.SetActive( false );
			m_freeObjects.Enqueue( go );
		}
		else
		{
			// Debug.LogError("Object " + go.name + "doesn't belong to pool " + m_containerObj.name);
		}

		
	}

	private GameObject Instantiate()
	{
		GameObject inst = GameObject.Instantiate(m_prefab);					
		inst.name = m_prefab.name;
		inst.transform.SetParent(m_containerObj.transform, false);
		inst.SetActive(false);
		return inst;
	}
};