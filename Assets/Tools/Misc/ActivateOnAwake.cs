// ActivateOnAwake.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Very simple script to activat/deactivate specific instances upon scene Awakening.
/// </summary>
public class ActivateOnAwake : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private GameObject[] m_toActivateOnAwake = new GameObject[] {};
	[SerializeField] private GameObject[] m_toDeactivateOnAwake = new GameObject[] {};

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Just do it! :D
		for(int i = 0; i < m_toActivateOnAwake.Length; i++) {
            if (m_toActivateOnAwake[i] != null)
            {
                m_toActivateOnAwake[i].SetActive(true);
            }
		}

		for(int i = 0; i < m_toDeactivateOnAwake.Length; i++) {
            if (m_toDeactivateOnAwake[i] != null)
            {
                m_toDeactivateOnAwake[i].SetActive(false);
            }
		}
	}
}