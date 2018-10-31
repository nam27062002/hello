// LabButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Button to access the lab.
/// </summary>
public class LabButton : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Selectable m_target = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);

		// Apply initial visibility
		RefreshVisibility();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Subscribe to external events
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether required conditions are met or not to show the button.
	/// </summary>
	public void RefreshVisibility() {
		// Skip if current user profile is not ready
		if(UsersManager.currentUser == null) {
			return;
		}

		// Check required number of runs
		bool toggle = (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_LAB_AT_RUN);

		// If a dragon of the required tier is acquired before completing the minimum amount of runs (via HC), toggle anyways
		if(DragonManager.biggestOwnedDragon.tier >= DragonDataSpecial.MIN_TIER_TO_UNLOCK) {
			toggle = true;
		}

		// Requires a Selectable component
		if(m_target != null) {
			m_target.interactable = toggle;
		} else {
			Debug.LogError("<color=red>SELECTABLE NOT FOUND</color>");
		}
	}

	/// <summary>
	/// Go to the lab main screen.
	/// </summary>
	public static void GoToLab() {
        if ( InstanceManager.menuSceneController.transitionManager.transitionAllowed )
        {
    		// Tracking
    		HDTrackingManager.Instance.Notify_LabEnter();
    
    		// Change mode
    		SceneController.SetMode(SceneController.Mode.SPECIAL_DRAGONS);
    
    		// Go to lab main screen!
    		InstanceManager.menuSceneController.GoToScreen(MenuScreen.LAB_DRAGON_SELECTION);
        }
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The button has been pressed.
	/// </summary>
	public void OnLabButton() {
		// Go to the lab!
		GoToLab();
	}

	/// <summary>
	/// A dragon has been acquired.
	/// </summary>
	/// <param name="_dragon">The dragon.</param>
	private void OnDragonAcquired(IDragonData _dragon) {
		// Just apply visibility
		RefreshVisibility();
	}
}