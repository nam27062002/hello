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
	[Tooltip("Seconds to travel one dragon's distance (min) and all dragon's distance (max).\nFinal animation duration will be interpolated from that.")]
	[SerializeField] private Range m_animSpeed = new Range(0.3f, 1f);
	public Range animSpeed {
		get { return m_animSpeed; }
		set { m_animSpeed = value; }
	}

	[SerializeField] private Ease m_animEase = Ease.InOutQuad;
	public Ease animEase {
		get { return m_animEase; }
		set { m_animEase = value; }
	}

	// Solo-properties
	// Delta and snap point are stored in the camera path to avoid keeping control of multiple vars
	public float delta {
		get { 
			return 0f;
			return m_cameraPath.delta; 
		}
		set {
			m_cameraPath.delta = value;
			m_lookAtPath.delta = value;
		}
	}

	public int snapPoint {
		get { 
			return 0;
			return m_cameraPath.snapPoint; 
		}
		set {
			m_cameraPath.snapPoint = value;
			m_lookAtPath.snapPoint = value;
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
		MenuDragonPreview[] dragonPreviews = GetComponentsInChildren<MenuDragonPreview>();
		for(int i = 0; i < dragonPreviews.Length; i++) {
			// Add it into the map
			m_dragonPreviews[dragonPreviews[i].sku] = dragonPreviews[i];
		}

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
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
		
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// If we're in the dragon selection screen, snap camera to curves
		// [AOC] A bit dirty, but best way I can think of right now (and gacha is waiting)
		if(m_menuScreensController.currentScene == m_menuScreensController.GetScene((int)MenuScreens.DRAGON_SELECTION)) {
			// Only if camera is not already moving!
			if(!m_menuScreensController.tweening) {
				// Move camera! ^_^
				m_menuScreensController.camera.transform.position = m_cameraPath.position;
				m_menuScreensController.camera.transform.LookAt(m_lookAtPath.position);
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
		// Adapt duration to the distance to traverse
		int targetSnapPoint = m_cameraPath.path.GetPointAt(_delta);
		float dist = Mathf.Abs((float)(targetSnapPoint - m_cameraPath.snapPoint));
		float delta = Mathf.InverseLerp(0, m_cameraPath.path.pointCount, dist);
		float duration = m_animSpeed.Lerp(delta);

		// Just let the paths manage it
		m_cameraPath.GoTo(_delta, duration, m_animEase);
		m_lookAtPath.GoTo(_delta, duration, m_animEase);
	}

	/// <summary>
	/// Animate to a specific point in the path.
	/// </summary>
	/// <param name="_snapPoint">Target control point.</param>
	/// <returns>The delta corresponding to the target snap point</returns>
	public float SnapTo(int _snapPoint) {
		// Adapt duration to the distance to traverse
		float dist = Mathf.Abs((float)(_snapPoint - m_cameraPath.snapPoint));
		float delta = Mathf.InverseLerp(0, m_cameraPath.path.pointCount, dist);
		float duration = m_animSpeed.Lerp(delta);

		// Just let the paths manage it
		float targetDelta = 0f;
		targetDelta = m_cameraPath.SnapTo(_snapPoint, duration, m_animEase);
		m_lookAtPath.SnapTo(_snapPoint, duration, m_animEase);
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
			m_cameraPath.snapPoint = menuOrder;
			m_lookAtPath.snapPoint = menuOrder;
		}
	}

	/// <summary>
	/// Get the 3D preview of a specific dragon.
	/// </summary>
	/// <returns>The dragon preview object.</returns>
	/// <param name="_sku">The sku of the dragon whose preview we want.</param>
	public MenuDragonPreview GetDragonPreview(string _sku) {
		// Try to get it from the dictionary
		MenuDragonPreview ret = null;
		m_dragonPreviews.TryGetValue(_sku, out ret);

		// If not found on the dictionary, try to find it in the hierarchy
		if(ret == null) {
			// We have need to check all the dragons anyway, so update them all
			MenuDragonPreview[] dragonPreviews = GetComponentsInChildren<MenuDragonPreview>();
			for(int i = 0; i < dragonPreviews.Length; i++) {
				// Add it into the map
				m_dragonPreviews[dragonPreviews[i].sku] = dragonPreviews[i];

				// Is it the one we're looking for?
				if(dragonPreviews[i].sku == _sku) {
					ret = dragonPreviews[i];
				}
			}
		}
		return ret;
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
		// If the current menu screen is not using the dragon selection 3D scene, skip animation
		MenuScreenScene currentScene = m_menuScreensController.currentScene;
		MenuScreenScene dragonSelectionScene = m_menuScreensController.GetScene((int)MenuScreens.DRAGON_SELECTION);
		if(currentScene == dragonSelectionScene) {
			FocusDragon(_sku, true);
		} else {
			FocusDragon(_sku, false);
		}
	}
}

