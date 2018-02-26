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
	[SerializeField] private ShowHideAnimator m_anim = null;
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
		// Subscribe to external events
		m_anim.OnShowCheck.AddListener(OnShowCheck);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get latest data from the manager
		RefreshData();

		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);
	}

	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	private void UpdatePeriodic() {
		// Skip if we're not active - probably in a screen we don't belong to
		if(!isActiveAndEnabled) return;

		// Refresh the timer!
		RefreshTimer();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		m_anim.OnShowCheck.RemoveListener(OnShowCheck);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh of the offers manager asking for new featured offers to be displayed
	/// </summary>
	public void RefreshData() {
		// Tell the manager to update packs
		OffersManager.instance.Refresh();

		// Get featured offer
		m_targetOffer = OffersManager.featuredOffer;

		// Update the timer
		RefreshTimer();
	}

	/// <summary>
	/// Refresh the timer. To be called periodically.
	/// </summary>
	private void RefreshTimer() {
		// Skip if no target offer
		if(m_targetOffer == null) return;

		// Is featured offer still valid?
		m_targetOffer = OffersManager.instance.RefreshFeaturedOffer();
		if(m_targetOffer != null) {
			// Yes!! Update text
			DateTime serverTime = GameServerManager.SharedInstance.GetEstimatedServerTime();
			m_timerText.text = TimeUtils.FormatTime(
				System.Math.Max(0, (m_targetOffer.endDate - serverTime).TotalSeconds), // Just in case, never go negative
				TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
				4
			);
		} else {
			// No!! Hide ourselves
			m_anim.Hide();
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
		// Show popup!
		PopupManager.OpenPopupInstant(PopupFeaturedOffer.PATH);
	}
}