// RenderQueueSettter.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
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

	[Serializable]
	public class UITarget {
		public Graphic target = null;
		public int newRenderQueue = 0;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private bool m_applyOnStart = true;
	[SerializeField] private Target[] m_targets = new Target[0];
	[SerializeField] private UITarget[] m_uiTargets = new UITarget[0];
	
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
		// Renderers
		for(int i = 0; i < m_targets.Length; ++i) {
			for(int j = 0; j < m_targets[i].renderer.materials.Length; ++j) {
				m_targets[i].renderer.materials[j].renderQueue = m_targets[i].newRenderQueue;
			}
		}

		// UI Graphics
		for(int i = 0; i < m_uiTargets.Length; ++i) {
			// If using the default material, create a new instance (we don't want to change the shared material!!)
			if(m_uiTargets[i].target.material == m_uiTargets[i].target.defaultMaterial) {
				m_uiTargets[i].target.material = new Material(m_uiTargets[i].target.defaultMaterial);
			}
			m_uiTargets[i].target.material.renderQueue = m_uiTargets[i].newRenderQueue;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}