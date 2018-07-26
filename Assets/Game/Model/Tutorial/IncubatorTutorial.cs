// DragonSelectionTutorial.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Incubator tutorial script.
/// </summary>
public class IncubatorTutorial : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_delay = 0.5f;

	// Internal
	private bool m_showPending = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake()  {
		
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Show egg info popup if:
		m_showPending = false;
		if(//!EggManager.isInventoryEmpty		// We have a valid egg in the inventory
			!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INFO)    // We have not done it yet
			&& UsersManager.currentUser.gamesPlayed >= 2	// We've played at least a couple of games
		    && UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.PRE_REG_REWARDS)) {	// Previous required step
			m_showPending = true;
		}

		// Disable the component and don't check again until next menu run if no 
		// unlock conditions can be triggered during this menu run
		else if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.PRE_REG_REWARDS)) {
			this.enabled = false;
		}
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// If we must show the popup, do it with some delay
		// Make sure we're on the right screen
		if(m_showPending && InstanceManager.menuSceneController.currentScreen == MenuScreen.DRAGON_SELECTION) {
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					// Tracking
					string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoEgg.PATH);
					HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");

					// Open popup
					PopupManager.OpenPopupInstant(PopupInfoEgg.PATH);

					// Mark tutorial as completed
					UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.EGG_INFO);

					// Disable this component (we don't need it anymore)
					this.enabled = false;
				},
				m_delay
			);
			m_showPending = false;
		}
	}

	//------------------------------------------------------------------------//
	// CUSTOM METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}