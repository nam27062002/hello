// OfferFeaturedIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the featured offer icon in the menu.
/// </summary>
public class OfferFeaturedIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private MenuShowConditionally m_showConditioner = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;

	// Internal
	private OfferPack m_targetOffer = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get latest data from the manager
		RefreshData(true);
		RefreshVisibility(false, true);		// [AOC] Attempting to fix https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-1862

		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);

		// Subscribe to external events
		m_showConditioner.targetAnimator.OnShowCheck.AddListener(OnShowCheck);
		Messenger.AddListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
		Messenger.AddListener(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
	}

	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	private void UpdatePeriodic() {
		// Skip if we're not active - probably in a screen we don't belong to
		if(!isActiveAndEnabled) return;

		// Refresh the timer!
		RefreshTimer(true);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		m_showConditioner.targetAnimator.OnShowCheck.RemoveListener(OnShowCheck);
		Messenger.RemoveListener(MessengerEvents.OFFERS_RELOADED, OnOffersReloaded);
		Messenger.RemoveListener(MessengerEvents.OFFERS_CHANGED, OnOffersChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh of the offers manager asking for new featured offers to be displayed
	/// </summary>
	/// <param name="_refreshManager">Force a refresh on the manager?</param>
	public void RefreshData(bool _refreshManager) {
		// Tell the manager to update packs
		if(_refreshManager) {
			OffersManager.instance.Refresh();
		}

		// Get featured offer
		m_targetOffer = OffersManager.featuredOffer;

		// Update the timer
		RefreshTimer(_refreshManager);

		// Update visibility
		RefreshVisibility();
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// </summary>
	/// <param name="_refreshManager">Force a refresh on the manager?</param>
	private void RefreshTimer(bool _refreshManager) {
		// Skip if no target offer
		if(m_targetOffer == null) return;

		// Is featured offer still valid?
		if(_refreshManager) {
			m_targetOffer = OffersManager.instance.RefreshFeaturedOffer();
		}
			
		// Yes!! Update text
		if(m_targetOffer != null) {
			if(m_targetOffer.isTimed) {
				m_timerText.text = TimeUtils.FormatTime(
					System.Math.Max(0, m_targetOffer.remainingTime.TotalSeconds), // Just in case, never go negative
					TimeUtils.EFormat.ABBREVIATIONS,
					4
				);
			} else {
				m_timerText.text = string.Empty;
			}
		} else {
			// No!! Hide ourselves
			m_showConditioner.targetAnimator.Hide();
		}
	}

	/// <summary>
	/// Check whether the icon can be displayed or not.
	/// </summary>
	private void RefreshVisibility(bool _animate = true, bool _force = false) {
		// Consider conditional shower
		bool show = m_targetOffer != null && m_showConditioner.Check();
		if(_force) {
			m_showConditioner.targetAnimator.ForceSet(show, _animate);
		} else {
			m_showConditioner.targetAnimator.Set(show, _animate);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The animator is requesting check validation
	/// </summary>
	/// <param name="_anim">Animation.</param>
	private void OnShowCheck(ShowHideAnimator _anim) {
		// Only show if we have a valid featured offer!
		if(m_targetOffer == null) _anim.SetCheckFailed();
	}

	/// <summary>
	/// Button has been pressed!
	/// </summary>
	public void OnTap() {
		// Just in case, ignore if target offer is not valid!!
		if(m_targetOffer == null) return;

		// Show popup!
		PopupController popup = PopupManager.LoadPopup(PopupFeaturedOffer.PATH);
		popup.GetComponent<PopupFeaturedOffer>().InitFromOfferPack(m_targetOffer);
		popup.Open();

		// Tracking
		HDTrackingManager.Instance.Notify_OfferShown(true, m_targetOffer.def.GetAsString("iapSku"));
	}

	/// <summary>
	/// The offers manager has been reloaded.
	/// </summary>
	private void OnOffersReloaded() {
		RefreshData(false);
	}

	/// <summary>
	/// Offers list has changed.
	/// </summary>
	private void OnOffersChanged() {
		RefreshData(false);
	}
}