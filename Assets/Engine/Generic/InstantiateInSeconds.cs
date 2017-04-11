// InstantiateinSeconds.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to instantiate a prefab in a given amount of time.
/// </summary>
public class InstantiateInSeconds : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private GameObject m_prefab = null;
	public GameObject prefab {
		get { return m_prefab; }
		set { m_prefab = value; }
	}

	[SerializeField] private Transform m_targetParent = null;
	public Transform targetParent {
		get { return m_targetParent; }
		set { m_targetParent = value; }
	}

	[SerializeField] private float m_delay = 1f;
	public float delay {
		get { return m_delay; }
		set { m_delay = value; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// If it has to be instantiated immediately, do it now
		if(m_delay <= 0f) {
			DoInstantiate();
		}
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Timer!
		if(m_delay >= 0f) {
			m_delay -= Time.deltaTime;
			if(m_delay <= 0f) {
				DoInstantiate();
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Perform the instantiation.
	/// </summary>
	private void DoInstantiate() {
		// Use ourselves as parent if not defined
		Transform parent = m_targetParent == null ? this.transform : m_targetParent;
		GameObject.Instantiate(m_prefab, m_targetParent, false);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}