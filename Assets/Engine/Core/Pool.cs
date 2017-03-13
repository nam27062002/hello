﻿using UnityEngine;
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
			m_containerObj.transform.SetParent(_parent, false);
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

	public int Size() {
		return (m_freeObjects.Count + m_notFreeObjects.Count);
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
	
	public GameObject Get(bool _activate)
	{			
		if ( m_freeObjects.Count <= 0 && m_canGrow)
		{
			m_freeObjects.Enqueue( Instantiate() );
		}

		if ( m_freeObjects.Count > 0)
		{
			GameObject go = m_freeObjects.Dequeue();
            if (_activate) go.SetActive(true);
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
			go.transform.SetParent(m_containerObj.transform);		// Return particle system to pool's hierarchy
			go.SetActive( false );
			m_freeObjects.Enqueue( go );
		}
		else
		{
			// Debug.LogError("Object " + go.name + "doesn't belong to pool " + m_containerObj.name);
		}
	}

	/// <summary>
	/// Resize the pool to the target size.
	/// If smaller than current size, some objects might be destroyed!
	/// </summary>
	/// <param name="_newSize">New size of the pool.</param>
	public void Resize(int _newSize) {
		// At least 0!
		_newSize = Mathf.Max(0, _newSize);

		// Compare to current size
		int sizeDiff = _newSize - (m_freeObjects.Count + m_notFreeObjects.Count);

		// Smaller than current size?
		if(sizeDiff < 0) {
			// Destroy objects
			// If there are not enough objects in the free queue, pull back some objects from the not free queue first
			int toDestroy = Mathf.Abs(sizeDiff);
			int toReturn = toDestroy - m_freeObjects.Count;
			if(toReturn > 0) {
				// Store them first in a list (only way to iterate a hash set is via iterator)
				List<GameObject> toReturnList = new List<GameObject>();
				foreach(GameObject obj in m_notFreeObjects) {
					// If we reach the target amount, stop looping
					toReturnList.Add(obj);
					if(toReturnList.Count == toReturn) break;
				}

				// Return objects to the pool
				for(int i = 0; i < toReturnList.Count; i++) {
					Return(toReturnList[i]);
				}
			}

			// Destroy as many objects from the free list as needed
			while(toDestroy > 0 && m_freeObjects.Count > 0) {
				GameObject obj = m_freeObjects.Dequeue();
				GameObject.Destroy(obj);
				toDestroy--;
			}
		}

		// Bigger than current size?
		else if(sizeDiff > 0) {
			// Create as many objects as needed
			for(int i = 0; i < sizeDiff; i++) {
				m_freeObjects.Enqueue(Instantiate());
			}
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