// MenuDragonScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Show the currently selected level in the menu screen.
/// </summary>
public class MenuLevelScroller3D : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum AnimDir {
		BACKWARDS = -1,
		NONE = 0,	// Don't animate
		FORWARD = 1
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private BezierCurve m_path = null;
	public BezierCurve path { get { return m_path; }}

	// Setup
	[Space(10)]
	[SerializeField] private float m_animDuration = 0.5f;
	public float animDuration {
		get { return m_animDuration; }
		set { m_animDuration = value; }
	}

	[SerializeField] private Ease m_animEase = Ease.InOutCubic;
	public Ease animEase {
		get { return m_animEase; }
		set { m_animEase = value; }
	}

	// Internal references
	private List<MenuLevelPreview> m_levels = new List<MenuLevelPreview>();	// Sorted by order as defined in the content

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get all level previews
		// Sort them by order as defined in their definition
		m_levels.AddRange(GetComponentsInChildren<MenuLevelPreview>(true));
		m_levels.Sort((_x, _y) => _x.def.GetAsInt("order").CompareTo(_y.def.GetAsInt("order")));
	}

	/// <summary>
	/// First update call
	/// </summary>
	private void Start() {
		// Distribute levels uniformly through the path
		float deltaOffset = 1f/m_levels.Count;
		for(int i = 0; i < m_levels.Count; i++) {
			m_levels[i].follower.GoTo(i * deltaOffset);	// No animation
		}

		// Focus current selected level
		FocusLevel(InstanceManager.GetSceneController<MenuSceneController>().selectedLevel, AnimDir.NONE);
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Focus a specific level.
	/// </summary>
	/// <param name="_sku">The level identifier.</param>
	/// <param name="_dir">In which direction to animate.</param>
	public void FocusLevel(string _sku, AnimDir _dir) {
		// Find out target index and get its path follower
		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.LEVELS, _sku);
		int targetIdx = def.GetAsInt("order");

		// Compute delta offset for the target level in the requested direction
		// Target level should be at delta 0 (or 1, which is equivalent in a closed path)
		float targetDelta = 1f;
		if(_dir == AnimDir.FORWARD) targetDelta = 0f;
		float deltaOffset = targetDelta - m_levels[targetIdx].follower.delta;

		// Apply the same offset to all levels
		for(int i = 0; i < m_levels.Count; i++) {
			targetDelta = m_levels[i].follower.delta + deltaOffset;
			if(_dir == AnimDir.NONE) {
				m_levels[i].follower.GoTo(targetDelta);
			} else {
				m_levels[i].follower.GoTo(targetDelta, m_animDuration, m_animEase);
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}

