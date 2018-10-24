// PopupInfoMissions.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoMissions : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoMissions";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Localizer m_messageText = null;

	//------------------------------------------------------------------------//
	// GENERAL METHODS														  //
	//------------------------------------------------------------------------//
	public void Awake() {
		// Set the proper message depending on current game mode
		switch(SceneController.mode) {
			case SceneController.Mode.SPECIAL_DRAGONS: {
				m_messageText.Localize("TID_MISSIONS_SPECIALS_SUBTITLE");
			} break;

			default: {
				m_messageText.Localize("TID_MISSIONS_SUBTITLE");
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	public void OnClosePreAnimation() {
		if (!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.SECOND_RUN)) {
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._09_close_missions_popup);
		}
	}
}
