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
using System.Collections.Generic;

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

	[Serializable]
	public class TransformTarget {
		public Transform target = null;
		public int newRenderQueue = 0;
		public bool graphics = true;
		public bool renderers = true;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private bool m_applyOnStart = true;
	[SerializeField] private Target[] m_targets = new Target[0];
	[SerializeField] private UITarget[] m_uiTargets = new UITarget[0];
	[SerializeField] private TransformTarget[] m_transformTargets = new TransformTarget[0];

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
			Apply(m_targets[i].renderer, m_targets[i].newRenderQueue);
		}

		// UI Graphics
		for(int i = 0; i < m_uiTargets.Length; ++i) {
			Apply(m_uiTargets[i].target, m_uiTargets[i].newRenderQueue);
		}

		// Transforms
		for(int i = 0; i < m_transformTargets.Length; ++i) {
			if (m_transformTargets[i].renderers) {
				List<Renderer> renderers = m_transformTargets[i].target.FindComponentsRecursive<Renderer>();
				for (int r = 0; r < renderers.Count; r++) {
					Apply(renderers[r], m_transformTargets[i].newRenderQueue);
				}
			}

			if (m_transformTargets[i].graphics) {
				List<Graphic> graphics = m_transformTargets[i].target.FindComponentsRecursive<Graphic>();
				for (int g = 0; g < graphics.Count; g++) {
					Apply(graphics[g], m_transformTargets[i].newRenderQueue);
				}
			}
		}

	}

	private void Apply(Renderer _target, int _queue) {
		for(int j = 0; j < _target.materials.Length; ++j) {
			_target.materials[j].renderQueue = _queue;
		}
	}

	private void Apply(Graphic _target, int _queue) {
		// If using the default material, create a new instance (we don't want to change the shared material!!)
		if(_target.material == _target.defaultMaterial) {
			_target.material = new Material(_target.defaultMaterial);
		}
		_target.material.renderQueue = _queue;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}