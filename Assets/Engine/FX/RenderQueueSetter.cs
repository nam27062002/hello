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
	public const int DEFAULT_UI_QUEUE = 3000;

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
		Apply(-1);
	}

	/// <summary>
	/// Do the actual render queue change.
	/// </summary>
	/// <param name="_overrideRenderQueue">Force a specific renderQueue? Use -1 to respect values defined in the inspector.</param>
	public void Apply(int _overrideRenderQueue) {
		// Renderers
		for(int i = 0; i < m_targets.Length; ++i) {
			Apply(m_targets[i].renderer, _overrideRenderQueue < 0 ? m_targets[i].newRenderQueue : _overrideRenderQueue);
		}

		// UI Graphics
		for(int i = 0; i < m_uiTargets.Length; ++i) {
			Apply(m_uiTargets[i].target, _overrideRenderQueue < 0 ? m_uiTargets[i].newRenderQueue : _overrideRenderQueue);
		}

		// Transforms
		for(int i = 0; i < m_transformTargets.Length; ++i) {
			Apply(
				m_transformTargets[i].target, 
				_overrideRenderQueue < 0 ? m_transformTargets[i].newRenderQueue : _overrideRenderQueue,
				m_transformTargets[i].renderers,
				m_transformTargets[i].graphics
			);
		}
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	public static void Apply(Renderer _target, int _queue) {
		for(int j = 0; j < _target.materials.Length; ++j) {
			_target.materials[j].renderQueue = _queue;
		}
	}

	public static void Apply(Graphic _target, int _queue) {
		// Special case for TMPro Textfields
		if(_target is TMPro.TMP_Text) {
			(_target as TMPro.TMP_Text).SetRenderQueue(_queue);	// [AOC] TODO!! Doesn't seem to work properly :(
		} 

		else {
			// If using the default material, create a new instance (we don't want to change the shared material!!)
			if(_target.material == _target.defaultMaterial) {
				_target.material = new Material(_target.defaultMaterial);
			}
			_target.material.renderQueue = _queue;
		}
	}

	public static void Apply(Transform _t, int _queue, bool _applyToRenderers = true, bool _applyToGraphics = true) {
		if (_applyToRenderers) {
			List<Renderer> renderers = _t.FindComponentsRecursive<Renderer>();
			for (int r = 0; r < renderers.Count; r++) {
				Apply(renderers[r], _queue);
			}
		}

		if (_applyToGraphics) {
			List<Graphic> graphics = _t.FindComponentsRecursive<Graphic>();
			for (int g = 0; g < graphics.Count; g++) {
				Apply(graphics[g], _queue);
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}