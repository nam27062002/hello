// PopupGoldenFragmentConversion.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/09/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple popup to say bye-bye to golden fragments! :_(
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupGoldenFragmentConversion : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/OtherInfo/PF_PopupGoldenFragmentConversion";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed Reference
	[SerializeField] private TextMeshProUGUI m_gfAmountText = null;
	[SerializeField] private TextMeshProUGUI m_hcAmountText = null;

	[Space]
	[SerializeField] private ProfileCurrencyCounter m_currencyCounter = null;
	[SerializeField] private ShowHideAnimator m_currencyCounterAnim = null;

	[Space]
	[SerializeField] private Transform m_currencyFXStartAnchor = null;
	[SerializeField] private Transform m_currencyFXEndAnchor = null;

	// Internal Logic
	private long m_gfAmount = 0;
	private long m_hcAmount = 0;
	private bool m_rewardCollected = false; // Prevent spamming

	// Internal references
	private ParticlesTrailFX m_currencyFX = null;

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the popup must be triggered considering the current profile.
	/// </summary>
	/// <returns></returns>
	public static bool Check() {
		// Show always if the user has any GF whatsoever
		return UsersManager.currentUser.goldenEggFragments > 0;
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Store amount of golden fragments to be converted
		m_gfAmount = UsersManager.currentUser.goldenEggFragments;

		// Store amount of gems given in return, using the formula in content
		DefinitionNode settingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		float conversionFactor = settingsDef.GetAsFloat("goldenFragmentsToHCCoef", 0.1f);
		m_hcAmount = (long)Mathf.Ceil((float)m_gfAmount * conversionFactor);    // Round up (always in favour of the player)

		// Initialize textfields
		m_gfAmountText.text = StringUtils.FormatNumber(m_gfAmount);
		m_hcAmountText.text = StringUtils.FormatNumber(m_hcAmount);

		// Hide currency counters
		m_currencyCounterAnim.ForceHide(false);
		Messenger.Broadcast<bool>(MessengerEvents.UI_TOGGLE_CURRENCY_COUNTERS, false);
	}

	/// <summary>
	/// The popup has been closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// If we have a currency FX running, move it to the main Canvas so it doesn't disappear
		if(m_currencyFX != null) {
			m_currencyFX.transform.SetParent(InstanceManager.menuSceneController.GetUICanvasGO().transform, false);
			m_currencyFX = null;
		}

		// Restore background currency counters
		Messenger.Broadcast<bool>(MessengerEvents.UI_TOGGLE_CURRENCY_COUNTERS, true);
	}

	/// <summary>
	/// Accept button has been pressed.
	/// </summary>
	public void OnAccept() {
		// Prevent spamming
		if(m_rewardCollected) return;

		// Perform GF transaction
		UsersManager.currentUser.EarnCurrency(
			UserProfile.Currency.GOLDEN_FRAGMENTS,
			(ulong)-m_gfAmount,
			false,
			HDTrackingManager.EEconomyGroup.GOLDEN_FRAGMENTS_REMOVAL
		);

		// Perform HC transaction
		UsersManager.currentUser.EarnCurrency(
			UserProfile.Currency.HARD,
			(ulong)m_hcAmount,
			false,
			HDTrackingManager.EEconomyGroup.GOLDEN_FRAGMENTS_REMOVAL
		);

		// Prevent spamming
		m_rewardCollected = true;

		// [AOC] TODO!! Tracking?

		// Save persistence
		PersistenceFacade.instance.Save_Request();

		// Set the right currency in the counter and show it
		m_currencyCounter.SetCurrency(UserProfile.Currency.HARD);
		m_currencyCounterAnim.ForceShow();

		// Trigger VFX
		// Offset Z a bit so the coins don't collide with the UI elements
		// [AOC] We're assuming that UI canvases (both main and popup) are at Z0
		Vector3 fromWorldPos = m_currencyFXStartAnchor.transform.position;
		Vector3 toWorldPos = m_currencyFXEndAnchor.transform.position;
		fromWorldPos.z = -0.5f;
		toWorldPos.z = -0.5f;
		m_currencyFX = ParticlesTrailFX.LoadAndLaunch(
			ParticlesTrailFX.GetDefaultPrefabPathForCurrency(UserProfile.Currency.HARD),
			this.GetComponentInParent<Canvas>().transform,
			fromWorldPos,
			toWorldPos
		);
		m_currencyFX.speed = new Range(1f, 2f);
		m_currencyFX.rate = 40f;
		m_currencyFX.totalDuration = 1f;

		// Close popup after some delay (to give time to enjoy the VFX)
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				GetComponent<PopupController>().Close(true);
				m_currencyCounterAnim.Hide();
			}, 1.5f
		);
	}

	/// <summary>
	/// Android back button has been pressed.
	/// </summary>
	public void OnBackButton() {
		// Treat it as if the player hit Accept
		OnAccept();
	}
}