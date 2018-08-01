// IPopupConsentGDPR.cs
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
/// Interface to handle the GDPR checkboxes logic.
/// </summary>
public class IPopupConsentGDPR : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	public class CheckGroup {
		public Toggle checkbox = null;
		public Transform tooltipAnchor = null;
		[NonSerialized] public bool consented = false;
	}

	public const string TRACKING_CONSENT_KEY = "GDPR.TrackingConsent";
	public const string ADS_CONSENT_KEY = "GDPR.AdsConsent";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] protected CheckGroup m_trackingConsentGroup = null;
	[SerializeField] protected CheckGroup m_adsConsentGroup = null;
	[Space]
	[SerializeField] protected GameObject m_tooltip = null;
	[SerializeField] protected Localizer m_tooltipText = null;
	[Space]
	[SerializeField] protected ScrollRect m_scroll = null;

	// Public properties
	public bool trackingConsented {
		get { return m_trackingConsentGroup.consented; }
	}

	public bool adsConsented {
		get { return m_adsConsentGroup.consented; }
	}

	// Internal logic
	protected bool m_minAgeReached = true;

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
	public virtual void Init(bool _minAgeReached, bool _trackingConsented, bool _adsConsented) {
		// Store values
		m_minAgeReached = _minAgeReached;
		m_trackingConsentGroup.consented = _trackingConsented;
		m_adsConsentGroup.consented = _adsConsented;

		// Only allow changes if the player is old enough
		m_trackingConsentGroup.checkbox.interactable = _minAgeReached;
		m_adsConsentGroup.checkbox.interactable = _minAgeReached;

		// Initialize visuals
		RefreshVisuals();

		// Tooltip hidden
		m_tooltip.SetActive(false);
	}

	/// <summary>
	/// Update popup visuals with latest data.
	/// </summary>
	protected virtual void RefreshVisuals() {
		// Checkboxes
		// Never toggled when minimum age not reached
		m_trackingConsentGroup.checkbox.isOn = m_minAgeReached && trackingConsented;
		m_adsConsentGroup.checkbox.isOn = m_minAgeReached && adsConsented;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The tracking consent toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value of the toggle.</param>
	public void OnTrackingConsentChanged(bool _newValue) {
		// Store new value (only if age allows it)
		if(m_minAgeReached) m_trackingConsentGroup.consented = _newValue;

		// Refresh
		RefreshVisuals();

		// Toggle tooltip
		if(m_minAgeReached) {
			// Place in position
			m_tooltip.transform.position = m_trackingConsentGroup.tooltipAnchor.position;

			// Set text according to this toggle
			m_tooltipText.Localize("TID_LEGAL_MANAGE_OFF_1");

			// Activate only if toggle had been turned off
			m_tooltip.SetActive(!_newValue);
		}
	}

	/// <summary>
	/// The ads consent toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value of the toggle.</param>
	public void OnAdsConsentChanged(bool _newValue) {
		// Store new value (only if age allows it)
		if(m_minAgeReached) m_adsConsentGroup.consented = _newValue;

		// Refresh
		RefreshVisuals();

		// Toggle tooltip
		if(m_minAgeReached) {
			// Place in position
			m_tooltip.transform.position = m_adsConsentGroup.tooltipAnchor.position;

			// Set text according to this toggle
			m_tooltipText.Localize("TID_LEGAL_MANAGE_OFF_2");

			// Activate only if toggle had been turned off
			m_tooltip.SetActive(!_newValue);
		}
	}

	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public virtual void OnOpenPreAnimation() {
		// For some unknown reason, we need to delay a little bit the initial scrolling of the scroll view, otherwise it gets resetted to 0
		UbiBCN.CoroutineManager.DelayedCall(
			() => { m_scroll.verticalNormalizedPosition = 1f; },
			0.2f
		);
	}
}
