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
		// Only check if the tutorial should be displayed once per menu (don't check between screen changes!)
		// We can easily do that by doing it on the Awake() call
		m_showPending = false;

		// Show egg info popup if:
		if(//!EggManager.isInventoryEmpty		// We have a valid egg in the inventory
			!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.EGG_INFO)	// We have not done it yet
			&& UsersManager.currentUser.gamesPlayed >= 2) {	// We've played at least a couple of games
			m_showPending = true;
		}

		// If we don't have to show the tutorial, disable the component
		if(!m_showPending) this.enabled = false;
	}

	private void Update() {
		// If we must show the popup, do it with some delay
		// Make sure we're on the right screen
		if(m_showPending && InstanceManager.menuSceneController.screensController.currentMenuScreen == MenuScreens.DRAGON_SELECTION) {
			CoroutineManager.DelayedCall(
				() => {
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