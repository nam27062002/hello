// PopupLabUnlocked.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupLabUnlocked : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupLabUnlocked";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_giftAmountText = null;

	// Internal
	private long m_gfReward = 0;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the popup must be triggered considering the current profile.
	/// If all checks are passed, opens the popup.
	/// </summary>
	/// <returns>The opened popup if all conditions to display it are met. <c>null</c> otherwise.</returns>
	public static PopupController CheckAndOpen() {
		// All checks passed?
		if(Check()) {
			// Show the popup
			return PopupManager.OpenPopupInstant(PATH);
		}
		return null;
	}

	/// <summary>
	/// Check whether the popup must be triggered considering the current profile.
	/// </summary>
	/// <returns>Must the popup be displayed?</returns>
	public static bool Check() {
		// Don't if already displayed
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.LAB_UNLOCKED)) {
			return false;
		}

		// Don't if a dragon of the required tier is not yet owned
		if(DragonManager.biggestOwnedDragon.tier < DragonDataSpecial.MIN_TIER_TO_UNLOCK) {
			return false;
		}

		// All checks passed! Popup must be displayed
		return true;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Compute GF reward to be given
		DefinitionNode settingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		m_gfReward = settingsDef.GetAsLong("goldenFragmentsGivenTutorial");
	}

	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Initialize reward textfield
		m_giftAmountText.text = UIConstants.GetIconString(
			m_gfReward,
			UserProfile.Currency.GOLDEN_FRAGMENTS,
			UIConstants.IconAlignment.LEFT
		);

		// Perform GF transaction
		UsersManager.currentUser.EarnCurrency(
			UserProfile.Currency.GOLDEN_FRAGMENTS,
			(ulong)m_gfReward,
			false,
			HDTrackingManager.EEconomyGroup.LAB_UNLOCKED_GIFT
		);

		// Mark tutorial as completed
		// [AOC] Do it now to make sure that no one triggers the popup again!
		UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.LAB_UNLOCKED, true);

		// [AOC] TODO!! Tracking?
	}

	/// <summary>
	/// The popup is about to be closed.
	/// </summary>
	public void OnClosePreAnimation() {
		
	}

	/// <summary>
	/// Go to the lab button has been pressed.
	/// </summary>
	public void OnGoToLab() {
		// Close this popup
		GetComponent<PopupController>().Close(true);

		// Go to the lab main screen after a short delay
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				// Lab button does the trick for us
				LabButton.GoToLab();
			}, 0.25f
		);
	} 
}
