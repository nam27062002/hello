// PopupTermsAndConditions.cs
// Hungry Dragon
//
// Copyright (c) 2018 Ubisoft. All rights reserved.

#if UNITY_EDITOR
	#define TEST_COPPA
	#define TEST_GDPR
#endif

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
/// This COPPA/GDPR Popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTermsAndConditions : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Message/PF_PopupTermsAndConditions";

	public const string VERSION_PREFS_KEY = "LegalVersionAgreed";
	public const int LEGAL_VERSION = 1;

	public const string TRACKING_CONSENT_KEY = "PopupTermsAndConditions.TrackingConsent";
	public const string ADS_CONSENT_KEY = "PopupTermsAndConditions.AdsConsent";

	public enum Mode {
		LOADING_FUNNEL,
		MANUAL,
		TERMS_AND_CONDITIONS_ONLY
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Localizer m_titleText = null;
	[SerializeField] private Slider m_ageSlider = null;
	[SerializeField] private TextMeshProUGUI m_ageText = null;
	[Space]
	[SerializeField] private Button m_acceptButton = null;
	[SerializeField] private GameObject m_cancelButton = null;
	[Space]
	[SerializeField] private GameObject m_ageGroup = null;
	[SerializeField] private GameObject m_termsGroup = null;
	[SerializeField] private GameObject m_consentGroup = null;

	// Public setup
	private Mode m_mode = Mode.LOADING_FUNNEL;

	// Internal refs
    private PopupController m_popupController;
	private PopupTermsAndConditionsMoreInfo m_moreInfoPopup = null;
   
	// Internal logic
	private float m_timeAtOpen = 0f;

	private bool m_ageEnabled = true;
	private int m_initialAgeValue = -1;
	private int m_ageValue = -1;

	private bool m_consentEnabled = true;

	private bool m_initialTrackingConsent = true;
	private bool m_trackingConsent = true;

	private bool m_initialAdsConsent = true;
	private bool m_adsConsent = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Init(Mode _mode) {
		// Init internal vars
        m_timeAtOpen = Time.unscaledTime;
		m_mode = _mode;

		// Subscribe to popup's close event
        m_popupController = GetComponent<PopupController>();

		// Show Age Group?
		m_ageEnabled = m_mode != Mode.TERMS_AND_CONDITIONS_ONLY && GDPRManager.SharedInstance.IsAgeRestrictionRequired();
#if TEST_COPPA
		m_ageEnabled = true;
#endif
		if(m_ageEnabled) {
			// Special title
			m_titleText.Localize("TID_AGE_GATE_TITLE");

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
		m_consentEnabled = m_mode != Mode.TERMS_AND_CONDITIONS_ONLY && GDPRManager.SharedInstance.IsConsentRequired();
#if TEST_GDPR
		m_consentEnabled = true;
#endif
		if(m_consentEnabled) {
			m_trackingConsent = Prefs.GetBoolPlayer(TRACKING_CONSENT_KEY, true);
			m_initialTrackingConsent = m_trackingConsent;

			m_adsConsent = Prefs.GetBoolPlayer(ADS_CONSENT_KEY, true);
			m_initialAdsConsent = m_adsConsent;

			m_consentGroup.SetActive(true);
		} else {
			m_consentGroup.SetActive(false);
		}

        // Show Cancel button?
        m_cancelButton.SetActive(m_mode == Mode.MANUAL);

        //Tracking
        HDTrackingManager.Instance.Notify_ConsentPopupDisplay(m_mode == Mode.MANUAL);
    }

	/// <summary>
	/// Default destructor.
	/// </summary>
    void OnDestroy() {
        // [AOC] Should never happen, but just in case
		if(m_moreInfoPopup != null) {
			m_moreInfoPopup.popupController.Close(true);
			m_moreInfoPopup = null;
		}
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Opens the URL after a short delay.
	/// </summary>
	/// <param name="_url">URL to be opened.</param>
	private void OpenUrlDelayed(string _url) {
		// Add some delay to give enough time for SFX to be played before losing focus
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				Application.OpenURL(_url);
			}, 0.15f
		);
	}

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
		} else {
			m_ageText.color = Colors.WithAlpha(m_ageText.color, 1f);
		}

		// Enable accept button?
		m_acceptButton.interactable = m_ageValue > 0;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The privacy policy button has been pressed.
	/// </summary>
	public void OnPrivacyPolicyButton() {
		string privacyPolicyUrl = "https://legal.ubi.com/privacypolicy/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(privacyPolicyUrl);
	}

	/// <summary>
	/// The EULA button has been pressed.
	/// </summary>
	public void OnEulaButton() {
		string eulaUrl = "https://legal.ubi.com/eula/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(eulaUrl);
	}

	/// <summary>
	/// The terms of use button has been pressed.
	/// </summary>
	public void OnTermsOfUseButton() {
		string touUrl = "https://legal.ubi.com/termsofuse/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(touUrl);
	}

	/// <summary>
	/// More info button has been pressed.
	/// </summary>
	public void OnMoreInfoButton() {
		// Load more info popup
		PopupController popup = PopupManager.LoadPopup(PopupTermsAndConditionsMoreInfo.PATH);
		m_moreInfoPopup = popup.GetComponent<PopupTermsAndConditionsMoreInfo>();

		// Initialize it with current settings
		m_moreInfoPopup.Init(
			m_ageValue >= GDPRManager.SharedInstance.GetAgeToCheck(),
			m_trackingConsent, m_adsConsent
		);

		// When the popup closes, we'll store the new settings
		popup.OnClosePreAnimation.AddListener(OnMoreInfoPopupClosed);

		// Open the popup!
		popup.Open();
	}

	/// <summary>
	/// The accept button has been pressed.
	/// </summary>
	public void OnAccept() {
		// Aux vars
		bool hasChanged = false;

		// Store version
        PlayerPrefs.SetInt(VERSION_PREFS_KEY, LEGAL_VERSION);

		// Process changes
		// Age
		if(m_ageEnabled) {
			// Tell GDPR manager
			GDPRManager.SharedInstance.SetUserAge(m_ageValue);

			// Has age changed?
			if(m_ageValue != m_initialAgeValue) {
				hasChanged |= true;
			}
		}

		// Consent
		if(m_consentEnabled) {
			// Tell GDPR manager
			// [AOC] Give consent only when both consents (tracking and ads) are given
			GDPRManager.SharedInstance.SetUserConsentGiven(m_trackingConsent && m_adsConsent);

			// Store new value to user prefs
			Prefs.SetBoolPlayer(TRACKING_CONSENT_KEY, m_trackingConsent);
			Prefs.SetBoolPlayer(ADS_CONSENT_KEY, m_adsConsent);

			// Has consent changed?
			if(m_trackingConsent != m_initialTrackingConsent
			|| m_adsConsent != m_initialAdsConsent) {
				hasChanged |= true;
			}
		}

		// Tracking
		int duration = Convert.ToInt32(Time.unscaledTime - m_timeAtOpen);
        HDTrackingManager.Instance.Notify_ConsentPopupAccept(m_ageValue, m_trackingConsent, m_adsConsent, "1_1_1", duration);

		// Loading Funnel
		if(m_mode == Mode.LOADING_FUNNEL) {
			HDTrackingManager.Instance.Notify_LegalPopupClosed(duration, true);
		}

		// Close popup
		m_popupController.Close(true);

		// Restart flow? Only in manual mode
		if(m_mode == Mode.MANUAL && hasChanged) {
			ApplicationManager.instance.NeedsToRestartFlow = true;
		}
	}

	/// <summary>
	/// Cancel button has been presed.
	/// </summary>
	public void OnCancel() {
		// Only allow cancelling when the popup is triggered manually
		if(m_mode != Mode.MANUAL) return;

		// Close popup
		m_popupController.Close(true);
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
	}

	/// <summary>
	/// The more info popup has been closed.
	/// </summary>
	private void OnMoreInfoPopupClosed() {
		// Just in case
		if(m_moreInfoPopup == null) return;

		// Gather new values and store them
		m_trackingConsent = m_moreInfoPopup.trackingConsented;
		m_adsConsent = m_moreInfoPopup.adsConsented;

		// Remove listener and clear popup reference
		m_moreInfoPopup.popupController.OnClosePreAnimation.RemoveListener(OnMoreInfoPopupClosed);
		m_moreInfoPopup = null;
	}
}
