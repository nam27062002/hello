// OfferPackItem.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single item within an offer pack.
/// </summary>
[Serializable]
public class OfferPackItem {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private bool m_featured = false;
	public bool featured {
		get { return m_featured; }
	}

	private string m_type = "";		// Matching Metagame.Reward.TYPE_CODEs
	public string type {
		get { return m_type; }
	}

	private Metagame.Reward m_reward = null;
	public Metagame.Reward reward {
		get { return m_reward; }
	}

	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public OfferPackItem() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~OfferPackItem() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize from definition.
	/// </summary>
	/// <param name="_def">Def.</param>
	public void InitFromDefinition(DefinitionNode _def) {
		// Store definition
		m_def = _def;
		if(m_def == null) {
			m_reward = null;
			m_type = "";
			return;
		}

		// Store type
		m_type = _def.GetAsString("type", "sc");	// Default to SC to prevent crashes in case of bad config

		// Initialize reward
		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = m_type;
		rewardData.sku = _def.GetAsString("itemSku", "");
		rewardData.amount = _def.GetAsLong("amount", 1);
		m_reward = Metagame.Reward.CreateFromData(
			rewardData,
			HDTrackingManager.EEconomyGroup.SHOP_OFFER_PACK,
			""
		);

		// Featured?
		m_featured = _def.GetAsBool("featured", false);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}