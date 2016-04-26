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
		// Focus current selected level
		FocusLevel(InstanceManager.GetSceneController<MenuSceneController>().selectedLevel, false);
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Focus a specific level.
	/// </summary>
	/// <param name="_sku">The level identifier.</param>
	/// <param name="_animate">Whether to animate or not.</param>
	public void FocusLevel(string _sku, bool _animate) {
		// Find out target index
		DefinitionNode def = DefinitionsManager.GetDefinition(DefinitionsCategory.LEVELS, _sku);
		int selectedIndex = def.GetAsInt("order");

		// Compute the target delta in the path for each level
		// [AOC] This is a bit tricky:
		// Basically we want the selected level at the center of the curve (delta 0.5)
		// and the rest at its sides at a "constant" distance.
		// To compute that distance, we will distribute the curve in 2N-1 divisions,
		// N being the total amount of levels.
		// Then, we will compute the target deltas for all the levels considering selected
		// one is 0.5 and at what order distance they are from it.

		// Pre-math
		int slots = m_levels.Count * 2 - 1;
		float deltaDist = 1f/slots;

		// Go level by level and move them to their target deltas
		for(int i = 0; i < m_levels.Count; i++) {
			// Compute target delta
			int dif = i - selectedIndex;
			float targetDelta = 0.5f + dif * deltaDist;
			targetDelta = Mathf.Clamp01(targetDelta);	// Should never happen (because of maths), but just in case

			// Launch animation! Use a path follower for that
			PathFollower follower = m_levels[i].GetComponent<PathFollower>();
			if(follower != null) {
				if(_animate) {
					follower.GoTo(targetDelta, m_animDuration, m_animEase);
				} else {
					follower.GoTo(targetDelta);
				}
			} else {
				// No follower was found, set position directly (shouldn't happen)
				Vector3 targetPos = m_path.GetValue(targetDelta);
				m_levels[i].transform.DOKill();
				if(_animate) {
					m_levels[i].transform.DOMove(targetPos, m_animDuration).SetEase(m_animEase).SetRecyclable(true);
				} else {
					m_levels[i].transform.position = targetPos;
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon has been changed.
	/// </summary>
	/// <param name="_sku">The sku of the dragon we want to be the current one.</param>
	private void OnLevelSelected(string _sku) {
		// Move camera to the newly selected dragon
		FocusLevel(_sku, true);
	}
}

