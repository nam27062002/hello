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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Localizer m_titleText = null;
	[SerializeField] private Slider m_ageSlider = null;
	[SerializeField] private TextMeshProUGUI m_ageText = null;
	[SerializeField] private Button m_acceptButton = null;
	[Space]
	[SerializeField] private GameObject m_ageGroup = null;
	[SerializeField] private GameObject m_termsGroup = null;
	[SerializeField] private GameObject m_consentGroup = null;

	// Internal refs
    private PopupController m_popupController;
	private PopupTermsAndConditionsMoreInfo m_moreInfoPopup = null;
   
	// Internal logic
	private bool m_hasBeenAccepted = false;
	private float m_timeAtOpen = 0f;

	private int m_ageValue = -1;

	// Internal properties
	private bool trackingConsent {
		get { return Prefs.GetBoolPlayer(TRACKING_CONSENT_KEY, true); }
		set { Prefs.SetBoolPlayer(TRACKING_CONSENT_KEY, value); }
	}

	private bool adsConsent {
		get { return Prefs.GetBoolPlayer(ADS_CONSENT_KEY, true); }
		set { Prefs.SetBoolPlayer(ADS_CONSENT_KEY, value); }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
    void Awake() {
		// Init internal vars
        m_hasBeenAccepted = false;
        m_timeAtOpen = Time.unscaledTime;

		// Subscribe to popup's close event
        m_popupController = GetComponent<PopupController>();
        m_popupController.OnClosePreAnimation.AddListener(OnClose);

		// Show Age Group?
		if(GDPRManager.SharedInstance.IsAgeRestrictionRequired()) {
			// Special title
			m_titleText.Localize("TID_AGE_GATE_TITLE");

			// Init age text and subscribe to slider's OnChange event
			m_ageValue = GDPRManager.SharedInstance.GetCachedUserAge();
			m_ageSlider.value = m_ageValue;
			m_ageSlider.onValueChanged.AddListener(OnAgeChanged);	// After setting the initial value! Otherwise our age value will be changed

			// Initialize age text
			SetAgeText(m_ageValue);
		} else {
			m_ageGroup.SetActive(false);
		}

		// Show Consent Group?
		m_consentGroup.SetActive(GDPRManager.SharedInstance.IsConsentRequired());
    }

	/// <summary>
	/// Default destructor.
	/// </summary>
    void OnDestroy() {
        if (m_popupController != null) { 
            m_popupController.OnClosePreAnimation.RemoveListener(OnClose);
        }

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
			trackingConsent, adsConsent
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
		// Update internal flags
        m_hasBeenAccepted = true;

		// Store version
        PlayerPrefs.SetInt(VERSION_PREFS_KEY, LEGAL_VERSION);

		// Tell GDPR manager
		// Age
		if(GDPRManager.SharedInstance.IsAgeRestrictionRequired()) {
			GDPRManager.SharedInstance.SetUserAge(m_ageValue);
		}

		// Consent
		if(GDPRManager.SharedInstance.IsConsentRequired()) {
			// [AOC] Give consent only when both consents (tracking and ads) are given
			GDPRManager.SharedInstance.SetUserConsentGiven(trackingConsent && adsConsent);
		}

		// Close popup
		m_popupController.Close(true);
	}

	/// <summary>
	/// The popup has been closed.
	/// </summary>
    private void OnClose() {
        int duration = Convert.ToInt32(Time.unscaledTime - m_timeAtOpen);
        HDTrackingManager.Instance.Notify_LegalPopupClosed(duration, m_hasBeenAccepted);
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
		trackingConsent = m_moreInfoPopup.trackingConsented;
		adsConsent = m_moreInfoPopup.adsConsented;

		// Remove listener and clear popup reference
		m_moreInfoPopup.popupController.OnClosePreAnimation.RemoveListener(OnMoreInfoPopupClosed);
		m_moreInfoPopup = null;
	}
}
