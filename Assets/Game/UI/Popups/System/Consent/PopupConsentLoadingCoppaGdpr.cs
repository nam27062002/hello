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
/// The COPPA/GDPR Popup triggered during the loading funnel.
/// </summary>
public class PopupConsentLoadingCoppaGdpr : PopupConsentLoading {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH_COPPA_GDPR = "UI/Popups/Message/Consent/PF_PopupConsentLoadingCoppaGdpr";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] protected Button m_acceptButton = null;
	[Space]
	[SerializeField] private Slider m_ageSlider = null;
	[SerializeField] private TextMeshProUGUI m_ageText = null;
	[SerializeField] private GameObject m_ageHighlight = null;
	[Space]
	[SerializeField] private GameObject m_ageGroup = null;
	[SerializeField] private GameObject m_termsGroup = null;
	[SerializeField] private GameObject m_consentGroupFolded = null;
	[SerializeField] private GameObject m_consentGroupExpanded = null;

	// Internal logic
	private bool m_ageEnabled = true;
	private int m_initialAgeValue = -1;
	private int m_ageValue = -1;

	private bool m_consentEnabled = true;

	private bool m_initialTrackingConsent = true;
	private bool m_initialAdsConsent = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override public void Init() {
		// Call parent
		base.Init();

		// Show Age Group?
		m_ageEnabled = GDPRManager.SharedInstance.IsAgePopupNeededToBeShown();  // Country requires age restriction and age has never been set before
		if(m_ageEnabled) {
			// Init age text and subscribe to slider's OnChange event
			m_ageValue = GDPRManager.SharedInstance.GetCachedUserAge();
			m_initialAgeValue = m_ageValue;
			m_ageSlider.value = m_ageValue;
			m_ageSlider.onValueChanged.AddListener(OnAgeChanged);	// After setting the initial value! Otherwise our age value will be changed

			// Initialize age text
			SetAgeText(m_ageValue);
		} else {
			m_ageGroup.SetActive(false);
		}

		// Show Consent Group?
		m_consentEnabled = GDPRManager.SharedInstance.IsConsentRequired();
		if(m_consentEnabled) {
			m_initialTrackingConsent = Prefs.GetBoolPlayer(IPopupConsentGDPR.TRACKING_CONSENT_KEY, true);
			m_initialAdsConsent = Prefs.GetBoolPlayer(IPopupConsentGDPR.ADS_CONSENT_KEY, true);
			m_consentGroupFolded.SetActive(true);

			// Initialize consent data
			Init(m_initialAgeValue, m_initialTrackingConsent, m_initialAdsConsent);
		} else {
			m_consentGroupFolded.SetActive(false);
		}

		// Expanded Consent Group always starts folded
		m_consentGroupExpanded.SetActive(false);
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Sets the age text with a given age value.
	/// </summary>
	/// <param name="_age">Age value.</param>
	private void SetAgeText(int _age) {
		// A couple of special cases
		_age = Mathf.Clamp(_age, (int)m_ageSlider.minValue, (int)m_ageSlider.maxValue);
		if(_age >= m_ageSlider.maxValue) {	// [AOC] Support our elders!
			m_ageText.text = StringUtils.FormatNumber(_age) + "+";
		} else {
			m_ageText.text = StringUtils.FormatNumber(_age);
		}

		// Adjust color for invalid values
		if(_age <= 0) {	// Age never initialized
			m_ageText.color = Colors.WithAlpha(m_ageText.color, 0.5f);
			m_ageHighlight.gameObject.SetActive(true);
		} else {
			m_ageText.color = Colors.WithAlpha(m_ageText.color, 1f);
			m_ageHighlight.gameObject.SetActive(false);
		}

		// Enable accept button?
		m_acceptButton.interactable = m_ageValue > 0;
	}

	/// <summary>
	/// Perform the required tracking for the Accept button.
	/// </summary>
	override protected void OnAcceptTracking() {
		// Tracking
		int duration = Convert.ToInt32(Time.unscaledTime - m_timeAtOpen);
		HDTrackingManager.Instance.Notify_ConsentPopupAccept(
			m_ageValue, 
			trackingConsented, 
			adsConsented, 
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
	/// More info button has been pressed.
	/// </summary>
	public void OnMoreInfoButton() {
		// Expand consent group
		m_consentGroupExpanded.SetActive(true);

		// Hide compact consent group
		m_consentGroupFolded.SetActive(false);

		// Make sure consent group data is updated
		Init(m_ageValue, trackingConsented, adsConsented);

		// Scroll to the beginning of the text
		// For some unknown reason, we need to delay a little bit the initial scrolling of the scroll view, otherwise it gets resetted to 0
		UbiBCN.CoroutineManager.DelayedCall(
			() => { m_scroll.verticalNormalizedPosition = 1f; },
			0.1f
		);
	}

	/// <summary>
	/// The accept button has been pressed.
	/// </summary>
	override public void OnAccept() {
		// Process changes
		// Age
		if(m_ageEnabled) {
			// Tell GDPR manager
			GDPRManager.SharedInstance.SetUserAge(m_ageValue);
		}

		// Consent
		if(m_consentEnabled) {
			// Tell GDPR manager			
			GDPRManager.SharedInstance.SetUserConsentGiven(trackingConsented, adsConsented);

			// Store new value to user prefs
			Prefs.SetBoolPlayer(IPopupConsentGDPR.TRACKING_CONSENT_KEY, trackingConsented);
			Prefs.SetBoolPlayer(IPopupConsentGDPR.ADS_CONSENT_KEY, adsConsented);
		}

		// Let parent handle the rest
		base.OnAccept();
	}

	/// <summary>
	/// The age slider has changed its current value.
	/// </summary>
	/// <param name="_newValue">New value of the slider.</param>
	private void OnAgeChanged(float _newValue) {
		// Store new value
		m_ageValue = (int)_newValue;

		// Update text
		SetAgeText(m_ageValue);

		// Update opt-ins
		Init(m_ageValue, trackingConsented, adsConsented);
	}

	/// <summary>
	/// Popup has finished opening.
	/// </summary>
	public void OnOpenPostAnimation() {
		// In order to properly initialize the links, we need to disable and re-enable the terms group. TMPro things :(
		m_termsGroup.SetActive(false);
		m_termsGroup.SetActive(true);
	}
}
