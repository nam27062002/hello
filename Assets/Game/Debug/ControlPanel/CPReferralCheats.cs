// CPReferralCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/08/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Debug class to provide cheats to test the referral install feature.
/// </summary>
public class CPReferralCheats : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string DEBUG_REFERRER_ID_KEY = "CPReferralCheats.DebugReferrerId";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private CanvasGroup m_root = null;
	[Space]
	[SerializeField] private CPNumericValueEditor m_referralCountSetter = null;
	[SerializeField] private Button m_setReferralCountButton = null;
	[Space]
	[SerializeField] private TMP_InputField m_referrerIdInput = null;
	[SerializeField] private Button m_setReferrerIdButton = null;

	// Internal
	private string m_debugReferrerId = null;
	private bool m_uiLocked = false;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Initialize debug referrer ID
		// 1. Was it ever defined?
		m_debugReferrerId = PlayerPrefs.GetString(DEBUG_REFERRER_ID_KEY, null);

		// 2. Read from user profile
		if(string.IsNullOrEmpty(m_debugReferrerId)) {
			m_debugReferrerId = UsersManager.currentUser.referralUserId;
		}

		// 3. Read from deep linking
		if(string.IsNullOrEmpty(m_debugReferrerId)) {
			m_debugReferrerId = CaletyDynamicLinks.getReferrerID();
		}

		// Detect changes
		m_referrerIdInput.onValueChanged.AddListener(OnReferrerIdValueChanged);

		// Refresh view
		m_referrerIdInput.text = m_debugReferrerId;
		RefreshReferrerId();

		// Initialize Referral Count Setter
		m_referralCountSetter.SetValue(UsersManager.currentUser.totalReferrals);
		m_referralCountSetter.OnValueChanged.AddListener(OnReferralCountValueChanged);
		RefreshReferralCount();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from events
		m_referrerIdInput.onValueChanged.RemoveListener(OnReferrerIdValueChanged);
		m_referralCountSetter.OnValueChanged.RemoveListener(OnReferralCountValueChanged);
	}

	/// <summary>
	/// Refreshes the Referrer ID input field.
	/// </summary>
	public void RefreshReferrerId() {
		// Refresh visuals
		// Use different color if textfield value doesn't match current value
		bool isNewValue = m_referrerIdInput.text != m_debugReferrerId;
		if(isNewValue) {
			m_referrerIdInput.textComponent.color = Color.yellow;
		} else {
			m_referrerIdInput.textComponent.color = Color.white;
		}

		// Only enable Set button if ID is different
		m_setReferrerIdButton.interactable = isNewValue;
	}

	/// <summary>
	/// Refreshes the Referral Count input field.
	/// </summary>
	public void RefreshReferralCount() {
		// Different color if setter value doesn't match current value
		int value = (int)m_referralCountSetter.GetValue();
		bool isNewValue = value != UsersManager.currentUser.totalReferrals;
		if(isNewValue) {
			m_referralCountSetter.valueInput.textComponent.color = Color.yellow;
		} else {
			m_referralCountSetter.valueInput.textComponent.color = Color.white;
		}

		// Only enable Set button if ID is different
		m_setReferralCountButton.interactable = isNewValue;
	}

	/// <summary>
	/// Lock the UI (i.e. waiting for server response).
	/// </summary>
	/// <param name="_lock">Whether to lock or unlock the UI.</param>
	/// <returns>Whether the action was successfully applied or not.</returns>
	private bool LockUI(bool _lock) {
		// Locking the UI
		if(_lock) {
			// Is it already locked?
			if(m_uiLocked) {
				// Yes! Show feedback and do nothing
				ControlPanel.LaunchTextFeedback("Don't spam! Waiting for server response...", Color.red);
				return false;
			} else {
				// No! Lock it!
				m_uiLocked = true;
				m_root.interactable = false;
				m_root.alpha = 0.5f;
				return true;
			}
		}

		// Unlocking the UI
		else {
			// Is it already unlocked?
			if(!m_uiLocked) {
				// Yes! Nothing to do
				return false;
			} else {
				// No! Unlock it!
				m_uiLocked = false;
				m_root.interactable = true;
				m_root.alpha = 1f;
				return true;
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The referrer id has changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnReferrerIdValueChanged(string _newValue) {
		// Refresh visuals
		RefreshReferrerId();
	}

	/// <summary>
	/// The referal count has been changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnReferralCountValueChanged(float _newValue) {
		// Refresh visuals
		RefreshReferralCount();
	}

	/// <summary>
	/// Read the referrer ID from the input textfield and use it as debug referrer ID.
	/// </summary>
	public void OnSetReferrerID() {
		// Read from the input text and store it to player prefs
		m_debugReferrerId = m_referrerIdInput.text;
		PlayerPrefs.SetString(DEBUG_REFERRER_ID_KEY, m_debugReferrerId);

		// Refresh Visuals
		RefreshReferrerId();
	}

	/// <summary>
	/// Read the referrer ID from the Deep Link tech and use it as debug referrer ID.
	/// </summary>
	public void OnReadReferrerIDFromDeepLink() {
		m_referrerIdInput.text = CaletyDynamicLinks.getReferrerID();
		RefreshReferrerId();
	}

	/// <summary>
	/// Read the referrer ID from the User Profile and use it as debug referrer ID.
	/// </summary>
	public void OnReadReferrerIDFromUserProfile() {
		m_referrerIdInput.text = UsersManager.currentUser.referralUserId;
		RefreshReferrerId();
	}

	/// <summary>
	/// Set a specific amount of referrals to the current user.
	/// </summary>
	public void OnSetReferralCount() {
		// Prevent spam
		if(!LockUI(true)) return;

		// Read target amount of referral from the setter
		int amount = (int)m_referralCountSetter.GetValue();

		// Launch server request
		GameServerManager.SharedInstance.Referral_DEBUG_SetReferralCount(amount, OnSetReferralCountResponse);
	}

	/// <summary>
	/// Reset amount of referrals to the current user to 0.
	/// </summary>
	public void OnResetReferralCount() {
		// Set the UI to 0
		m_referralCountSetter.SetValue(0);
		RefreshReferralCount();
	}

	/// <summary>
	/// Simulate a new referral install with the defined referrer ID.
	/// </summary>
	public void OnSimulateReferralInstall() {
		// Prevent spam
		if(!LockUI(true)) return;

		// Launch server request
		GameServerManager.SharedInstance.Referral_DEBUG_SimulateReferralInstall(m_debugReferrerId, OnSimulateReferralInstallServerResponse);
	}

	/// <summary>
	/// Response from the server was received.
	/// </summary>
	/// <param name="_error">Error data.</param>
	/// <param name="_response">Response data.</param>
	public void OnSimulateReferralInstallServerResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// Unlock UI
		LockUI(false);

		// Show some feedback
		if(_error == null && _response != null && _response.ContainsKey("response") && _response["response"] != null) {
			JSONNode kJSON = JSON.Parse(_response["response"] as string);
			if(kJSON != null && kJSON.ContainsKey("result")) {
				if(kJSON["result"].AsBool == true) {
					ControlPanel.LaunchTextFeedback("Success!", Color.green, 3f);
				} else {
					ControlPanel.LaunchTextFeedback("Unsuccessful! " + kJSON["errorCode"] + ": " + kJSON["errorMsg"], Color.red);
				}
			}
		} else if(_error != null) {
			ControlPanel.LaunchTextFeedback("Something went wrong! Error " + _error.ToString(), Color.red, 3f);
		} else {
			ControlPanel.LaunchTextFeedback("Something went wrong!", Color.red);
		}
	}

	/// <summary>
	/// Response from the server was received.
	/// </summary>
	/// <param name="_error">Error data.</param>
	/// <param name="_response">Response data.</param>
	public void OnSetReferralCountResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// Unlock UI
		LockUI(false);

		// Show some feedback
		if(_error == null && _response != null && _response.ContainsKey("response") && _response["response"] != null) {
			JSONNode kJSON = JSON.Parse(_response["response"] as string);
			if(kJSON != null && kJSON.ContainsKey("result")) {
				if(kJSON["result"].AsBool == true) {
					ControlPanel.LaunchTextFeedback("Success!\nEnter the shop again to refresh the rewards.", Color.green, 3f);
					UsersManager.currentUser.totalReferrals = (int)m_referralCountSetter.GetValue();
				} else {
					ControlPanel.LaunchTextFeedback("Unsuccessful! " + kJSON["errorCode"] + ": " + kJSON["errorMsg"], Color.red);
				}
			}
		} else if(_error != null) {
			ControlPanel.LaunchTextFeedback("Something went wrong! Error " + _error.ToString(), Color.red, 3f);
		} else {
			ControlPanel.LaunchTextFeedback("Something went wrong!", Color.red);
		}

		// Refresh visuals
		RefreshReferralCount();
	}
}