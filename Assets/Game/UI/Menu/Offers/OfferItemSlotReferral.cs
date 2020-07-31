// OfferItemSlotReferral.cs
// Hungry Dragon
// 
// Created by  on 11/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for every referral reward preview in the referral popup.
/// Similar to OfferItemSlot but including a conector and some VFX for
/// rewards that are ready to claim.
/// </summary>
[Serializable]
public class OfferItemSlotReferral: OfferItemSlot {


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[Header("Referral Reward specifics")]

	[SerializeField] private GameObject m_connectorDisabled;
	[SerializeField] private GameObject m_connectorHighlighted;
	[SerializeField] private GameObject m_vfxGlow;
	[SerializeField] private GameObject m_tick;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Refresh the widget with the data of a specific offer item.
	/// </summary>
	/// <param name="_item">Item to be used to initialize the slot.</param>
	public void InitFromItem(OfferPackItem _item, OfferPackReferralReward.State _state)
	{
		// If the new order is not specified, use the current value
		InitFromItem(_item);

		// Init the referral reward specifics
		m_connectorDisabled.SetActive( _state == OfferPackReferralReward.State.NOT_AVAILABLE );
		m_connectorHighlighted.SetActive( _state > OfferPackReferralReward.State.NOT_AVAILABLE);
		m_vfxGlow.SetActive(_state == OfferPackReferralReward.State.READY_TO_CLAIM);

		m_tick.SetActive(_state == OfferPackReferralReward.State.CLAIMED);
		m_amountText.gameObject.SetActive(_state < OfferPackReferralReward.State.CLAIMED);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}