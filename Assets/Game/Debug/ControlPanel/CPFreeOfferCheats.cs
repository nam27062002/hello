// CPFreeOfferCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/10/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Allow several operations related to the mission system from the Control Panel.
/// </summary>
public class CPFreeOfferCheats : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private TextMeshProUGUI m_timerText = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Update timer text
		System.TimeSpan remainingTime = OffersManager.freeOfferRemainingCooldown;
		m_timerText.text = "Skip Free Offer CD\n" + TimeUtils.FormatTime(remainingTime.TotalSeconds, TimeUtils.EFormat.DIGITS, 3);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Skip cooldown timer.
	/// </summary>
	public void OnSkipCooldownTimer() {
		// Tell the offer manager
		OffersManager.DEBUG_SkipFreeOfferCooldown();

		// Save persistence
		PersistenceFacade.instance.Save_Request();
	}
}