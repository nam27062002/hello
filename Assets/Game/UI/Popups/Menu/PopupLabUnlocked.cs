// PopupLabUnlocked.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
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
	[SerializeField] private Localizer m_messageText = null;
	[SerializeField] private TextMeshProUGUI m_giftAmountText = null;
	[Space]
	[SerializeField] private GameObject m_goToLabButton = null;
	[SerializeField] private GameObject m_dismissButton = null;

	// Internal
	private long m_gfReward = 0;
	private MenuScreen m_sourceScreen = MenuScreen.DRAGON_SELECTION;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the popup must be triggered considering the current profile.
	/// </summary>
	/// <returns>Must the popup be displayed?</returns>
	public static bool Check() {
		// Don't if already displayed
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.LAB_UNLOCKED)) {
			return false;
		}

		// Always if a special dragon is already owned! (probably purchased via offer pack)
		List<IDragonData> specialDragons = DragonManager.GetDragonsByOrder(IDragonData.Type.SPECIAL);
		for(int i = 0; i < specialDragons.Count; ++i) {
			if(specialDragons[i].isOwned) {
				return true;
			}
		}

		// Don't if a dragon of the required tier is not yet owned


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
	/// Initialize this popup with different variations depending on where it's triggered.
	/// </summary>
	/// <param name="_sourceScreen">The screen that triggered this popup.</param>
	public void Init(MenuScreen _sourceScreen) {
		// Store params
		m_sourceScreen = _sourceScreen;

		// Toggle some visual elements depending on source screen
		m_dismissButton.SetActive(_sourceScreen == MenuScreen.LAB_DRAGON_SELECTION);
		m_goToLabButton.SetActive(_sourceScreen != MenuScreen.LAB_DRAGON_SELECTION);

		// Different text depending on screen
		if(_sourceScreen == MenuScreen.LAB_DRAGON_SELECTION) {
			m_messageText.Localize("TID_LAB_UNLOCKED_POPUP_MESSAGE_1_ALT");
		} else {
			m_messageText.Localize("TID_LAB_UNLOCKED_POPUP_MESSAGE_1");
		}
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
	/// Dismiss button has been pressed.
	/// </summary>
	public void OnDismiss() {
		// Close the popup
		GetComponent<PopupController>().Close(true);

		// Go to the lab if not already there
		if(m_sourceScreen != MenuScreen.LAB_DRAGON_SELECTION) {
			// Clear any other queued poppup
			// [AOC] This is a bit risky, since we could be preventing an 
			//       important popup from appearing, but on the other hand 
			//       we don't want queued popups to pop when entering the lab screen!
			PopupManager.ClearQueue();

			// Go to the lab main screen after a short delay
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					// Lab button does the trick for us
					LabButton.GoToLab();
				}, 0.25f
			);
		}
	} 
}
