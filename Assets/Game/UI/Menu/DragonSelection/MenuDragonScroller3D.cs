// MenuDragonScroller.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

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
/// Show the currently selected dragon in the menu screen.
/// TODO!! Nice animation
/// </summary>
public class MenuDragonScroller3D : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private PathFollower m_cameraPath = null;
	public PathFollower cameraPath { get { return m_cameraPath; }}

	[SerializeField] private PathFollower m_lookAtPath = null;
	public PathFollower lookAtPath { get { return m_lookAtPath; }}

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

	// Solo-properties
	// Delta and snap point are stored in the camera path to avoid keeping control of multiple vars
	public float delta {
		get { 
			if(m_cameraPath == null) return 0f;
			return m_cameraPath.delta; 
		}
		set {
			if(m_cameraPath != null) m_cameraPath.delta = value;
			if(m_lookAtPath != null) m_lookAtPath.delta = value;
		}
	}

	public int snapPoint {
		get { 
			if(m_cameraPath == null) return 0;
			return m_cameraPath.snapPoint; 
		}
		set {
			if(m_cameraPath != null) m_cameraPath.snapPoint = value;
			if(m_lookAtPath != null) m_lookAtPath.snapPoint = value;
		}
	}

	// Internal references
	private MenuScreensController m_menuScreensController = null;

	// Dragon previews
	private Dictionary<string, MenuDragonPreview> m_dragonPreviews = new Dictionary<string, MenuDragonPreview>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_cameraPath != null, "Required field");
		Debug.Assert(m_lookAtPath != null, "Required field");

		// Find and store dragon preview references
		string currentSelectedDragon = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		MenuDragonPreview[] dragonPreviews = GetComponentsInChildren<MenuDragonPreview>();
		for(int i = 0; i < dragonPreviews.Length; i++) {
			Debug.Log("Find preview of dragon " + i + "(" + dragonPreviews[i].sku + ")");
			// Add it into the map
			m_dragonPreviews[dragonPreviews[i].sku] = dragonPreviews[i];
		}
	}

	/// <summary>
	/// First update call
	/// </summary>
	private void Start() {
		// Store reference to menu screens controller for faster access
		m_menuScreensController = InstanceManager.GetSceneController<MenuSceneController>().screensController;

		// Find game object linked to currently selected dragon
		FocusDragon(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon, false);
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// If we're in the dragon selection screen, snap camera to curves
		// [AOC] A bit dirty, but best way I can think of right now (and gacha is waiting)
		if(m_menuScreensController.currentScreenIdx == (int)MenuScreens.DRAGON_SELECTION) {
			// Only if camera is not already moving!
			if(!m_menuScreensController.tweening) {
				// Move camera! ^_^
				if(m_cameraPath != null) {
					m_menuScreensController.camera.transform.position = m_cameraPath.position;
				}

				if(m_lookAtPath != null) {
					m_menuScreensController.camera.transform.LookAt(m_lookAtPath.position);
				}
			}
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Animate to a specific delta in the path.
	/// </summary>
	/// <param name="_delta">Target delta.</param>
	public void GoTo(float _delta) {
		// Just let the paths manage it
		if(m_cameraPath != null) m_cameraPath.GoTo(_delta, m_animDuration, m_animEase);
		if(m_lookAtPath != null) m_lookAtPath.GoTo(_delta, m_animDuration, m_animEase);
	}

	/// <summary>
	/// Animate to a specific point in the path.
	/// </summary>
	/// <param name="_snapPoint">Target control point.</param>
	/// <returns>The delta corresponding to the target snap point</returns>
	public float SnapTo(int _snapPoint) {
		// Just let the paths manage it
		float targetDelta = 0f;
		if(m_cameraPath != null) targetDelta = m_cameraPath.SnapTo(_snapPoint, m_animDuration, m_animEase);
		if(m_lookAtPath != null) m_lookAtPath.SnapTo(_snapPoint, m_animDuration, m_animEase);
		return targetDelta;
	}

	/// <summary>
	/// Focus a specific dragon.
	/// </summary>
	/// <param name="_sku">The dragon identifier.</param>
	/// <param name="_animate">Whether to animate or do an instant camera swap.</param>
	public void FocusDragon(string _sku, bool _animate) {
		// Trust that snap points are placed based on dragons' menuOrder value
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _sku);
		if(def == null) return;
		int menuOrder = def.GetAsInt("order");
		if(_animate) {
			SnapTo(menuOrder);
		} else {
			if(m_cameraPath != null) m_cameraPath.snapPoint = menuOrder;
			if(m_lookAtPath != null) m_lookAtPath.snapPoint = menuOrder;
		}
	}

	/// <summary>
	/// Get the 3D preview of a specific dragon.
	/// </summary>
	/// <returns>The dragon preview object.</returns>
	/// <param name="_sku">The sku of the dragon whose preview we want.</param>
	public MenuDragonPreview GetDragonPreview(string _sku) {
		return m_dragonPreviews[_sku];
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon has been changed.
	/// </summary>
	/// <param name="_sku">The sku of the dragon we want to be the current one.</param>
	private void OnDragonSelected(string _sku) {
		// Move camera to the newly selected dragon
		FocusDragon(_sku, true);
	}
}

