// PopupTermsAndConditions.cs
// Hungry Dragon
//
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// The Legal Consent Popup triggered during the loading funnel.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupConsentLoading : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Message/Consent/PF_PopupConsentLoading";

	public const string VERSION_PREFS_KEY = "LegalVersionAgreed";
	public const int LEGAL_VERSION = 1;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal refs
	protected PopupController m_popupController;

	// Internal logic
	protected float m_timeAtOpen = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public virtual void Init() {
		// Init internal vars
        m_timeAtOpen = Time.unscaledTime;

		// Get popup reference
        m_popupController = GetComponent<PopupController>();

        //Tracking
        HDTrackingManager.Instance.Notify_ConsentPopupDisplay(false);
    }

	/// <summary>
	/// Perform the required tracking for the Accept button.
	/// </summary>
	protected virtual void OnAcceptTracking() {
		// Tracking
		int duration = Convert.ToInt32(Time.unscaledTime - m_timeAtOpen);
		HDTrackingManager.Instance.Notify_ConsentPopupAccept(
			GDPRManager.SharedInstance.GetCachedUserAge(), 
			true, 
			true, 
			"1_1_1", 
			duration
		);

		// Loading Funnel
		HDTrackingManager.Instance.Notify_LegalPopupClosed(duration, true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The privacy policy button has been pressed.
	/// </summary>
	public void OnPrivacyPolicyButton() {
		IPopupConsentTermsAndConditions.OpenPrivacyPolicy();
	}

	/// <summary>
	/// The EULA button has been pressed.
	/// </summary>
	public void OnEulaButton() {
		IPopupConsentTermsAndConditions.OpenEULA();
	}

	/// <summary>
	/// The terms of use button has been pressed.
	/// </summary>
	public void OnTermsOfUseButton() {
		IPopupConsentTermsAndConditions.OpenTOU();
	}

	/// <summary>
	/// The accept button has been pressed.
	/// </summary>
	public virtual void OnAccept() {
		// Store version
        PlayerPrefs.SetInt(VERSION_PREFS_KEY, LEGAL_VERSION);

		// Tracking
		OnAcceptTracking();

		// Close popup
		m_popupController.Close(true);
	}
}
