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
using System.Globalization;

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
	public static readonly string[] ITEM_TYPE_ORDER = {
        Metagame.RewardRemoveAds.TYPE_CODE,
		Metagame.RewardSoftCurrency.TYPE_CODE,
		Metagame.RewardHardCurrency.TYPE_CODE,
		Metagame.RewardGoldenFragments.TYPE_CODE,
		Metagame.RewardEgg.TYPE_CODE,
		Metagame.RewardPet.TYPE_CODE,
		Metagame.RewardDragon.TYPE_CODE,
		Metagame.RewardSkin.TYPE_CODE
	};
	
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

	private string m_sku = null;
	public string sku {
		get { return m_sku; }
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

	/// <summary>
	/// Clear the item.
	/// </summary>
	public void Clear() {
		m_reward = null;
		m_type = "";
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize from definition.
	/// </summary>
	/// <param name="_def">Def.</param>
	/// <param name="_itemIdx">Index of the item within the pack (1..N)</param>
	/// <param name="_ecoGroup">Group ID used for tracking when the reward is collected.</param>
	public void InitFromDefinition(DefinitionNode _def, int _itemIdx, HDTrackingManager.EEconomyGroup _ecoGroup) {
		// Aux vars
		string prefix = GetPrefix(_itemIdx);

		// Check definition
		if(_def == null) {
			Clear();
			return;
		}

		// If unknown type, clear data and return
		m_type = _def.GetAsString(prefix + "Type", OffersManager.settings.emptyValue);
		if(m_type == OffersManager.settings.emptyValue || string.IsNullOrEmpty(m_type)) {
			Clear();
			return;
		}

		// Item Sku
		m_sku = _def.GetAsString(prefix + "Sku");

		// Initialize reward
		Metagame.Reward.Data rewardData = new Metagame.Reward.Data();
		rewardData.typeCode = m_type;
		rewardData.sku = m_sku;
		rewardData.amount = _def.GetAsLong(prefix + "Amount", 1);
		m_reward = Metagame.Reward.CreateFromData(
			rewardData,
			_ecoGroup,
			""
		);

		// Featured?
		m_featured = _def.GetAsBool(prefix + "Featured", false);
	}

	/// <summary>
	/// Very custom method to make sure a definition corresponding to an offer pack 
	/// item contains all required default values.
	/// If a parameter is missing in the definition, it will be added with the right
	/// default value for that parameter.
	/// </summary>
	/// <param name="_def">Definition to be filled.</param>
	/// <param name="_itemIdx">Index of the item within the pack (0..N-1)</param>
	public void ValidateDefinition(DefinitionNode _def, int _itemIdx) {
		string prefix = GetPrefix(_itemIdx);
		OfferPack.SetValueIfMissing(ref _def, prefix + "Featured", 	bool.FalseString.ToLowerInvariant());
		OfferPack.SetValueIfMissing(ref _def, prefix + "Type", 		string.Empty);
		OfferPack.SetValueIfMissing(ref _def, prefix + "Amount", 	1.ToString(CultureInfo.InvariantCulture));
		OfferPack.SetValueIfMissing(ref _def, prefix + "Sku", 		string.Empty);
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generate the prefix used in definitions columns for the item with the given index.
	/// </summary>
	/// <returns>The prefix.</returns>
	/// <param name="_itemIdx">Index of the item within the pack (0..N-1)</param>
	public static string GetPrefix(int _itemIdx) {
		return "item" + _itemIdx;
	}

	/// <summary>
	/// Function used to sort offer packs items.
	/// </summary>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	/// <param name="_item1">First item to compare.</param>
	/// <param name="_item2">Second item to compare.</param>
	public static int Compare(OfferPackItem _item1, OfferPackItem _item2) {
		// Depends on type
		return ITEM_TYPE_ORDER.IndexOf(_item1.type).CompareTo(ITEM_TYPE_ORDER.IndexOf(_item2.type));
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}