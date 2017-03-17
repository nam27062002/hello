// ChestFlagUI.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Initializes and controls the Chests info panel in the Goals Screen.
/// </summary>
public class GoalsScreenChestsInfoPanel : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// External refs
	[SerializeField] private Localizer m_collectedText = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private Slider m_timerBar = null;

	// Internal
	private Transform m_3dAnchor = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get anchor ref (if any)
		MenuSceneController menuController = InstanceManager.menuSceneController;
		if(menuController != null) {
			MenuScreenScene scene = menuController.screensController.GetScene((int)MenuScreens.GOALS);
			if(scene != null) {
				GoalsSceneController goalScene = scene.GetComponent<GoalsSceneController>();
				m_3dAnchor = goalScene.infoPanelUIAnchor;
			}
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.CHESTS_RESET, Refresh);
		Messenger.AddListener(GameEvents.CHESTS_PROCESSED, Refresh);

		// Refresh
		Refresh();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.CHESTS_RESET, Refresh);
		Messenger.RemoveListener(GameEvents.CHESTS_PROCESSED, Refresh);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if(!isActiveAndEnabled) return;

		// Keep anchored
		if(m_3dAnchor != null) {
			// Get camera and apply the inverse transformation
			if(InstanceManager.sceneController.mainCamera != null) {
				// From http://answers.unity3d.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
				// We can do it that easily because we've adjusted the containers to match the camera viewport coords
				Vector2 posScreen = InstanceManager.sceneController.mainCamera.WorldToViewportPoint(m_3dAnchor.position);
				RectTransform rt = this.transform as RectTransform;
				rt.anchoredPosition = Vector2.zero;
				rt.anchorMin = posScreen;
				rt.anchorMax = posScreen;
			}
		}

		// Refresh time
		RefreshTime();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh all info.
	/// </summary>
	private void Refresh() {
		// Collected count
		if(m_collectedText != null) {
			m_collectedText.Localize("TID_CHEST_DAILY_DESC", ChestManager.collectedChests.ToString(), ChestManager.NUM_DAILY_CHESTS.ToString());
		}

		// Time info
		RefreshTime();
	}

	/// <summary>
	/// Refresh timer info.
	/// </summary>
	private void RefreshTime() {
		// Aux vars
		TimeSpan timeToReset = ChestManager.timeToReset;

		// Text
		if(m_timerText != null) {
			m_timerText.text = TimeUtils.FormatTime(timeToReset.TotalSeconds, TimeUtils.EFormat.DIGITS, 3);
		}

		// Bar
		if(m_timerBar != null) {
			m_timerBar.normalizedValue = (float)((ChestManager.RESET_PERIOD - timeToReset.TotalHours)/ChestManager.RESET_PERIOD);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}