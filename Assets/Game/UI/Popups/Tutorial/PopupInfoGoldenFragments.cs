// PopupInfoGoldenFragments.cs
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
public class PopupInfoGoldenFragments : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoGoldenFragments";

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the popup should be displayed given a reward.
	/// </summary>
	/// <returns>Should the popup be displayed?</returns>
	/// <param name="_reward">Reward used to perform the check.</param>
	public static bool Check(Metagame.Reward _reward) {
		// Will the reward be replaced?
		if(_reward.WillBeReplaced()) {
			// Will it be replaced with Golden Fragments?
			if(_reward.replacement.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
				// Is the tutorial completed?
				if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO)) {
					// All checks passed!
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Show the popup.
	/// </summary>
	/// <param name="_trackingAction">Tracking action.</param>
	public static void Show(PopupLauncher.TrackingAction _trackingAction) {
		// Tracking
		string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoGoldenFragments.PATH);
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, PopupLauncher.TrackingActionToString(_trackingAction));

		// Open
		PopupManager.OpenPopupInstant(PopupInfoGoldenFragments.PATH);

		// Mark tutorial as completed
		UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.GOLDEN_FRAGMENTS_INFO, true);
	}

	/// <summary>
	/// Check whether the popup must be displayed with the given reward and open it if so.
	/// </summary>
	/// <param name="_reward">Reward used to perform the checks.</param>
	/// <param name="_delay">Delay before opening the popup. Used to sync with other UI animations.</param>
	/// <param name="_trackingAction">Tracking action.</param>
	public static void CheckAndShow(Metagame.Reward _reward, float _delay, PopupLauncher.TrackingAction _trackingAction) {
		if(Check(_reward)) {
			// Show popup after some extra delay
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					Show(_trackingAction);
				},
				_delay,
				false
			);
		}
	}
}
