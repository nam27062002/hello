// ActivateOnEnable.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/11/2018.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Very simple script to activate/deactivate specific instances linked to this object's state.
/// </summary>
public class ActivateOnEnable : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private bool m_reverseOnDisable = true;
	[SerializeField] private GameObject[] m_toActivateOnEnable = new GameObject[] {};
	[SerializeField] private GameObject[] m_toDeactivateOnEnable = new GameObject[] {};

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Just do it! :D
		for(int i = 0; i < m_toActivateOnEnable.Length; i++) {
			m_toActivateOnEnable[i].SetActive(true);
		}

		for(int i = 0; i < m_toDeactivateOnEnable.Length; i++) {
			m_toDeactivateOnEnable[i].SetActive(false);
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Reverse it if required
		if(m_reverseOnDisable) {
			for(int i = 0; i < m_toActivateOnEnable.Length; i++) {
				m_toActivateOnEnable[i].SetActive(false);
			}

			for(int i = 0; i < m_toDeactivateOnEnable.Length; i++) {
				m_toDeactivateOnEnable[i].SetActive(true);
			}
		}
	}
}