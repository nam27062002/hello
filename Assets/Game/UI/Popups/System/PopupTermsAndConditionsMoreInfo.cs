// PopupTermsAndConditionsMoreInfo.cs
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
/// COPPA/GDPR More info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTermsAndConditionsMoreInfo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Message/PF_PopupTermsAndConditionsMoreInfo";

	[System.Serializable]
	public class CheckGroup {
		public Toggle checkbox = null;
		public GameObject tooltip = null;
		[NonSerialized] public bool consented = false;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private CheckGroup m_trackingConsentGroup = null;
	[SerializeField] private CheckGroup m_adsConsentGroup = null;

	// Public properties
	private PopupController m_popupController = null;
	public PopupController popupController {
		get {
			if(m_popupController == null) {
				m_popupController = GetComponent<PopupController>();
			}
			return m_popupController;
		}
	}

	public bool trackingConsented {
		get { return m_trackingConsentGroup.consented; }
	}

	public bool adsConsented {
		get { return m_adsConsentGroup.consented; }
	}

	// Internal logic
	private bool m_minAgeReached = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given setup.
	/// </summary>
	/// <param name="_minAgeReached">Is the player old enough to give his/her consent?</param>
	/// <param name="_trackingConsented">Does the player consent using his/her data for tracking purposes?</param>
	/// <param name="_adsConsented">Does the player consent using his/her data for targeted ads?</param>
	public void Init(bool _minAgeReached, bool _trackingConsented, bool _adsConsented) {
		// Store values
		m_minAgeReached = _minAgeReached;
		m_trackingConsentGroup.consented = _trackingConsented;
		m_adsConsentGroup.consented = _adsConsented;

		// Only allow changes if the player is old enough
		m_trackingConsentGroup.checkbox.interactable = _minAgeReached;
		m_adsConsentGroup.checkbox.interactable = _minAgeReached;

		// Initialize visuals
		RefreshVisuals();
	}

	/// <summary>
	/// Update popup visuals with latest data.
	/// </summary>
	private void RefreshVisuals() {
		// Checkboxes
		// Never toggled when minimum age not reached
		m_trackingConsentGroup.checkbox.isOn = m_minAgeReached && trackingConsented;
		m_adsConsentGroup.checkbox.isOn = m_minAgeReached && adsConsented;

		// Toggle tooltips
		// Never toggled when minimum age not reached
		m_trackingConsentGroup.tooltip.SetActive(m_minAgeReached && !trackingConsented);
		m_adsConsentGroup.tooltip.SetActive(m_minAgeReached && !adsConsented);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The tracking consent toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value of the toggle.</param>
	public void OnTrackingConsentChanged(bool _newValue) {
		// Store new value
		m_trackingConsentGroup.consented = _newValue;

		// Refresh
		RefreshVisuals();
	}

	/// <summary>
	/// The ads consent toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value of the toggle.</param>
	public void OnAdsConsentChanged(bool _newValue) {
		// Store new value
		m_adsConsentGroup.consented = _newValue;

		// Refresh
		RefreshVisuals();
	}
}
