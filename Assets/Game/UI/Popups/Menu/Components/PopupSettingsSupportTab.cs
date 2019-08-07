// PopupSettingsSupportTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class PopupSettingsSupportTab : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Localizer m_versionText = null;
	[SerializeField] private Localizer m_userIdText = null;
	[Space]
	[SerializeField] private GameObject m_privacySettingsGroup = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Set version number
		m_versionText.Localize(m_versionText.tid, GameSettings.internalVersion.ToString() + " (" + ServerManager.SharedInstance.GetRevisionVersion() + ")");

		// Only show Privacy Settings for countries requiring it (EU)
		m_privacySettingsGroup.SetActive(GDPRManager.SharedInstance.IsConsentRequired());
	}

	/// <summary>
	/// The component has been enabled
	/// </summary>
	private void OnEnable() {
		// Refresh user IDs (might have changed from the last time we opened the popup, so refresh it every time
		string text = string.Empty;

		// User ID
		string uid = GameSessionManager.SharedInstance.GetUID();
		if(string.IsNullOrEmpty(uid)) {
			text += "-";
		} else {
			text += uid;
		}

		// Tracking ID
		string trackingId = HDTrackingManager.Instance.GetDNAProfileID();
		text += " / ";
		if(string.IsNullOrEmpty(trackingId)) {
			text += "-";
		} else {
			text += trackingId;
		}

		// Dont show if any of the ids are initialized (we never ever did a successful auth)
		if(string.IsNullOrEmpty(uid) && string.IsNullOrEmpty(trackingId)) {
			m_userIdText.gameObject.SetActive(false); 
		} else {
			m_userIdText.gameObject.SetActive(true); 
			m_userIdText.Localize(m_userIdText.tid, text);	
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The credits button has been pressed.
	/// </summary>
	public void OnCreditsButton() {
		// Open the credits popup
		PopupManager.OpenPopupInstant(PopupCredits.PATH);
	}

	/// <summary>
	/// The privacy settings button has been pressed.
	/// </summary>
	public void OnPrivacySettingsButton() {
		PopupController popupController = PopupManager.OpenPopupInstant(PopupConsentSettings.PATH);
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

	/// <summary>
	/// The comments button has been pressed.
	/// </summary>
	public void OnCommentsButton() {
		MiscUtils.SendFeedbackEmail();
	}

	/// <summary>
	/// The CS button has been pressed.
	/// </summary>
	public void OpenCustomerSupport() {
		//CSTSManager.SharedInstance.OpenView(TranslationsManager.Instance.ISO.ToString(), PersistenceManager.Instance.IsPayer);
		if(Application.internetReachability != NetworkReachability.NotReachable) {
            PopupSettings.CS_OpenPopup();
        } else {
			string str = LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION");
			UIFeedbackText.CreateAndLaunch(str, new Vector2(0.5f, 0.5f), GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}    
}