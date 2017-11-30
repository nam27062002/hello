// RenderQueueSettter.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class RenderQueueSetter : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	public class Target {
		public Renderer renderer = null;
		public int newRenderQueue = 0;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private bool m_applyOnStart = true;
	[SerializeField] private Target[] m_targets = new Target[0];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		if(m_applyOnStart) Apply();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Do the actual render queue change.
	/// </summary>
	public void Apply() {
		for(int i = 0; i < m_targets.Length; ++i) {
			for(int j = 0; j < m_targets[i].renderer.materials.Length; ++j) {
				m_targets[i].renderer.materials[j].renderQueue = m_targets[i].newRenderQueue;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}