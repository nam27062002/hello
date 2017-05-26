// PoolReturnOnDisable.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/07/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to automatically return an object to its pool upon disable.
/// </summary>
public class PoolReturnOnDisable : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum PoolType {
		PoolManager = 0,
		UIPoolManager,
		ParticleManager
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private PoolType m_returnTo = PoolType.PoolManager;

	private PoolHandler m_poolHandler = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		if (m_poolHandler == null) {
			GetHandler();
		}

		if (m_poolHandler != null) {
			m_poolHandler.ReturnInstance(this.gameObject);
		}
	}

	private void GetHandler() {
		switch (m_returnTo) {
			case PoolType.PoolManager:		m_poolHandler = PoolManager.GetHandler(gameObject.name); break;
			case PoolType.UIPoolManager:	m_poolHandler = UIPoolManager.GetHandler(gameObject.name); break;
			case PoolType.ParticleManager:	break;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}