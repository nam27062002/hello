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
	[SerializeField] private Animator m_iconAnim = null;

	// Internal
	private bool m_toggle = true;
	private AnimationTriggers m_defaultTriggers = null;
	
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
		bool toggle = (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_LEAGUES_AT_RUN);

		// If a dragon of the required tier is acquired before completing the minimum amount of runs (via HC), toggle anyways
		if(DragonManager.biggestOwnedDragon.tier >= DragonDataSpecial.MIN_TIER_TO_UNLOCK) {
			toggle = true;
		}

		// Requires a Selectable component
		if(m_target != null) {
			//m_target.interactable = toggle;

			// Backup original triggers
			if(m_defaultTriggers == null) {
				m_defaultTriggers = new AnimationTriggers();
				m_defaultTriggers.normalTrigger = m_target.animationTriggers.normalTrigger;
				m_defaultTriggers.disabledTrigger = m_target.animationTriggers.disabledTrigger;
				m_defaultTriggers.highlightedTrigger = m_target.animationTriggers.highlightedTrigger;
				m_defaultTriggers.pressedTrigger = m_target.animationTriggers.pressedTrigger;
			}

			// Don't actually disable the button, allow tapping to it to give feedabck on when the button will be unlocked
			// To do the trick, we'll switch the button's state animation triggers
			if(toggle) {
				m_target.animationTriggers.normalTrigger = m_defaultTriggers.normalTrigger;
				m_target.animationTriggers.highlightedTrigger = m_defaultTriggers.highlightedTrigger;
				m_target.animationTriggers.pressedTrigger = m_defaultTriggers.pressedTrigger;
			} else {
				m_target.animationTriggers.normalTrigger = m_defaultTriggers.disabledTrigger;
				m_target.animationTriggers.highlightedTrigger = m_defaultTriggers.disabledTrigger;
				m_target.animationTriggers.pressedTrigger = m_defaultTriggers.disabledTrigger;
			}

			// Apply to icon animation
			if(m_iconAnim != null) {
				m_iconAnim.enabled = toggle;
			}
		} else {
			Debug.LogError("<color=red>SELECTABLE NOT FOUND</color>");
		}

		// If state has changed, force a visual refresh of the button
		if(toggle != m_toggle) {
			// [AOC] How to force a refresh? Unity style -____-
			m_target.interactable = !m_target.interactable;
			m_target.interactable = !m_target.interactable;
		}

		// Store new state
		m_toggle = toggle;
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
            HDLiveDataManager.instance.SwitchToLeague();
    
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
		// If not allowed, show feedback and return
		if(!m_toggle) {
			// Show error message
			int remainingRuns = GameSettings.ENABLE_LEAGUES_AT_RUN - UsersManager.currentUser.gamesPlayed;
			string tid = remainingRuns == 1 ? "TID_MORE_RUNS_REQUIRED" : "TID_MORE_RUNS_REQUIRED_PLURAL";
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize(tid, remainingRuns.ToString()),
				new Vector2(0.5f, 0.5f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
			return;
		}

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