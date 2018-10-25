// LabGoalsScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Screen controller for the Lab Goals screen.
/// </summary>
public class LabGoalsScreen : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// If no special dragon is available, show error message
		if(DragonManager.currentSpecialDragon == null) {
			UIFeedbackText.CreateAndLaunch(
				"TID_LAB_ERROR_NO_SPECIAL_DRAGON_OWNED",
				GameConstants.Vector2.center,
				GetComponentInParent<Canvas>().transform as RectTransform
			).text.color = UIConstants.ERROR_MESSAGE_COLOR;
			return;
		}

		// Tracking
		// [AOC] TODO!! Tournament as reference:
		//HDTrackingManager.Instance.Notify_TournamentClickOnEnter(m_definition.m_name, _flow.currency);

		// Go to play!
		InstanceManager.menuSceneController.OnPlayButton();
	}
}