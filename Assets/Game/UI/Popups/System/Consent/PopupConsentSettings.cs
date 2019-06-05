// PopupConsentSettings.cs
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
/// GDPR Popup triggered from the settings menu.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupConsentSettings : IPopupConsentGDPR {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Message/Consent/PF_PopupConsentSettings";


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_consentGroup = null;
	[SerializeField] private GameObject m_acceptButton = null;

	// Internal logic
    private float m_timeAtOpen = 0f;

	private bool m_initialTrackingConsent = true;
	private bool m_initialAdsConsent = true;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given setup.
	/// </summary>
	/// <param name="_minAgeReached">Is the player old enough to give his/her consent?</param>
	/// <param name="_trackingConsented">Does the player consent using his/her data for tracking purposes?</param>
	/// <param name="_adsConsented">Does the player consent using his/her data for targeted ads?</param>
	override public void Init(bool _minAgeReached, bool _trackingConsented, bool _adsConsented) {
        m_timeAtOpen = Time.unscaledTime;

        // Store initial values
		m_initialTrackingConsent = _trackingConsented;
		m_initialAdsConsent = _adsConsented;

		// Tune up some stuff if consent is not required
		if(!GDPRManager.SharedInstance.IsConsentRequired()) {
			// Hide consent group
			m_consentGroup.SetActive(false);

			// Hide accept button
			m_acceptButton.SetActive(false);
		}

		// Let parent do the rest
		base.Init(_minAgeReached, _trackingConsented, _adsConsented);


        //Tracking
        HDTrackingManager.Instance.Notify_ConsentPopupDisplay(true);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	override public void OnOpenPreAnimation() {
		// Initialize it!
		Init(
			GDPRManager.SharedInstance.GetCachedUserAge() >= GDPRManager.SharedInstance.GetAgeToCheck(),
			Prefs.GetBoolPlayer(TRACKING_CONSENT_KEY, true),
			Prefs.GetBoolPlayer(ADS_CONSENT_KEY, true)
		);

		// Call parent
		base.OnOpenPreAnimation();
	}

	/// <summary>
	/// The accept button has been pressed.
	/// </summary>
	public void OnAccept() {
		// Check consent changes
		if(GDPRManager.SharedInstance.IsConsentRequired()) {
			// Restart flow? Only in if some value has actually changed
			if(m_initialTrackingConsent != trackingConsented
			|| m_initialAdsConsent != adsConsented) {
				// Tell GDPR manager				
				GDPRManager.SharedInstance.SetUserConsentGiven(trackingConsented, adsConsented);

				// Store new value to user prefs
				Prefs.SetBoolPlayer(IPopupConsentGDPR.TRACKING_CONSENT_KEY, trackingConsented);
				Prefs.SetBoolPlayer(IPopupConsentGDPR.ADS_CONSENT_KEY, adsConsented);

                // According to design marketing id has to be sent after the user changes her consent
                HDTrackingManager.Instance.Notify_MarketingID(HDTrackingManager.EMarketingIdFrom.Settings);

                // We need to force logout so the game will login again when restarting and it will send whether or not the user is a child to the server, which is needed to ban children from tournaments
                GameServerManager.SharedInstance.LogOut();
				ApplicationManager.instance.NeedsToRestartFlow = true;
			}
		}

        int duration = Convert.ToInt32(Time.unscaledTime - m_timeAtOpen);
        HDTrackingManager.Instance.Notify_ConsentPopupAccept(GDPRManager.SharedInstance.GetCachedUserAge(), trackingConsented, adsConsented, "1_1_1", duration);        

		// Close the popup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// Cancel button has been pressed.
	/// </summary>
	public void OnCancel() {
		// Close popup
		GetComponent<PopupController>().Close(true);
	}

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
}
