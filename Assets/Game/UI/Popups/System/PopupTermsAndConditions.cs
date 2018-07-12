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

	public const string AGE_PREFS_KEY = "PopupTermsAndConditions.Age";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Slider m_ageSlider = null;
	[SerializeField] private TextMeshProUGUI m_ageText = null;
	[SerializeField] private Button m_acceptButton = null;
	[Space]
	[SerializeField] private GameObject m_ageGroup = null;
	[SerializeField] private GameObject m_termsGroup = null;

	// Internal refs
    private PopupController m_popupController;
   
	// Internal logic
    private bool HasBeenAccepted { get; set; }
    private float TimeAtOpen { get; set; }

	private int m_ageValue = -1;
	private int m_initialAgeValue = -1;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
    void Awake() {
		// Init internal vars
        // Show loading until we know country or age

        HasBeenAccepted = false;
        TimeAtOpen = Time.unscaledTime;

		// Subscribe to popup's close event
        m_popupController = GetComponent<PopupController>();
        m_popupController.OnClosePreAnimation.AddListener(OnClose);

		// Init age text and subscribe to slider's OnChange event
		m_ageValue = PlayerPrefs.GetInt(AGE_PREFS_KEY, -1);
		m_initialAgeValue = m_ageValue;
		m_ageSlider.onValueChanged.AddListener(OnAgeChanged);
		m_ageSlider.value = m_ageValue;

		// Start with button disabled if age has never been initialized
		m_acceptButton.interactable = m_ageValue >= 0;
    }

	/// <summary>
	/// Default destructor.
	/// </summary>
    void OnDestroy() {
        if (m_popupController != null) { 
            m_popupController.OnClosePreAnimation.RemoveListener(OnClose);
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
		if(_age < 0) {	// Age never initialized
			m_ageText.text = string.Empty;
		} else if(_age >= m_ageSlider.maxValue) {	// [AOC] Support our elders!
			m_ageText.text = StringUtils.FormatNumber(_age) + "+";
		} else {
			m_ageText.text = StringUtils.FormatNumber(_age);
		}
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
	/// The accept button has been pressed.
	/// </summary>
	public void OnAccept() {
        HasBeenAccepted = true;
        PlayerPrefs.SetInt(VERSION_PREFS_KEY, LEGAL_VERSION);
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// The popup has been closed.
	/// </summary>
    private void OnClose() {
        int duration = Convert.ToInt32(Time.unscaledTime - TimeAtOpen);
        HDTrackingManager.Instance.Notify_LegalPopupClosed(duration, HasBeenAccepted);
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

		// Enable accept button!
		m_acceptButton.interactable = m_ageValue >= 0;
	}
}
