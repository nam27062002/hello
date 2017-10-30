// Reward.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 18/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace Metagame {
	/// <summary>
	/// Base abstract class for all rewards.
	/// </summary>
	public abstract class Reward {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public enum Rarity {
			COMMON = 0, 
			RARE, 
			EPIC, 
			SPECIAL 
		}

		//------------------------------------------------------------------------//
		// FACTORY METHODS														  //
		//------------------------------------------------------------------------//
		#region Factory
		/// <summary>
		/// Convert a sku to a rarity enum.
		/// </summary>
		public static Rarity SkuToRarity(string _raritySku) {			
			switch (_raritySku) {
				case "common":	return Rarity.COMMON; 
				case "rare":	return Rarity.RARE;	
				case "epic":	return Rarity.EPIC;	
				case "special":	return Rarity.SPECIAL;
			}
			return Rarity.COMMON;
		}

		/// <summary>
		/// Given a rarity, return its sku.
		/// </summary>
		/// <returns>The sku of the given rarity. Empty string if unknown.</returns>
		/// <param name="_rarity">Rarity whose sku we want.</param>
		public static string RarityToSku(Rarity _rarity) {
			// We could double-check with content, but it's much faster if we hardcode it (and these skus are not supposed to change)
			switch(_rarity) {
				case Rarity.COMMON:		return "common";
				case Rarity.RARE:		return "rare";
				case Rarity.EPIC:		return "epic";
				case Rarity.SPECIAL:	return "special";
			}
			return string.Empty;
		}

		/// <summary>
		/// Constructor from json data.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public static Reward CreateFromJson(SimpleJSON.JSONNode _data) {	
			// Parse economy group (if any)
			string key = "economyGroup";
			HDTrackingManager.EEconomyGroup parsedEconomyGroup = HDTrackingManager.EEconomyGroup.UNKNOWN;
			if(_data.ContainsKey(key)) {
				 parsedEconomyGroup = HDTrackingManager.StringToEconomyGroup(_data[key]);
			}

			// Parse source
			key = "source";
			string parsedSource = "";
			if(_data.ContainsKey(key)) {
				parsedSource = _data[key];
			}

			// Use the parametrized method
			return CreateFromJson(_data, parsedEconomyGroup, parsedSource);
		}

		/// <summary>
		/// Constructor from json data, with economy group and source manually set.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public static Reward CreateFromJson(SimpleJSON.JSONNode _data, HDTrackingManager.EEconomyGroup _economyGroup, string _source) {			
			string type = _data["type"];
			type = type.ToLower();
			
			string data = "";
			if(_data.ContainsKey("sku")) {
				data = _data["sku"];
			} else if(_data.ContainsKey("amount")) {
				data = _data["amount"];
			}

			return CreateFromTypeCode(type, data, _economyGroup, _source);
		}

		/// <summary>
		/// Creates from type code. String codes used in server comunication are: sc, pc, fg, egg, gegg, pet.
		/// </summary>
		/// <returns>A reward.</returns>
		/// <param name="_typeCode">Type code.</param>
		/// <param name="_data">Data.</param>
		public static Reward CreateFromTypeCode(string _typeCode, string _data, HDTrackingManager.EEconomyGroup _economyGroup, string _source) {			
			switch(_typeCode) {
				case RewardSoftCurrency.TYPE_CODE:	  return CreateTypeSoftCurrency(long.Parse(_data), _economyGroup, _source);
				case RewardHardCurrency.TYPE_CODE:	  return CreateTypeHardCurrency(long.Parse(_data), _economyGroup, _source);
				case RewardGoldenFragments.TYPE_CODE: return CreateTypeGoldenFragments(int.Parse(_data), Rarity.COMMON, _economyGroup, _source);
				case RewardEgg.TYPE_CODE:			  return CreateTypeEgg(_data, _source);
				case RewardPet.TYPE_CODE:			  return CreateTypePet(_data, _source);
			}
			return null;
		}

		public static RewardSoftCurrency CreateTypeSoftCurrency(long _amount, HDTrackingManager.EEconomyGroup _economyGroup, string _source)						{ return new RewardSoftCurrency(_amount, Rarity.COMMON, _economyGroup, _source); }
		public static RewardHardCurrency CreateTypeHardCurrency(long _amount, HDTrackingManager.EEconomyGroup _economyGroup, string _source) 						{ return new RewardHardCurrency(_amount, Rarity.COMMON, _economyGroup, _source); }
		public static RewardGoldenFragments CreateTypeGoldenFragments(int _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup, string _source) 	{ return new RewardGoldenFragments(_amount, _rarity, _economyGroup, _source); }
		public static RewardEgg CreateTypeEgg(string _sku, string _source) 				{ return new RewardEgg(_sku, _source); }
		public static RewardPet CreateTypePet(string _sku, string _source)				{ return new RewardPet(_sku, _source); }
		public static RewardPet CreateTypePet(DefinitionNode _def, string _source)		{ return new RewardPet(_def, _source); }
		#endregion

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		// Mandatory:
		protected Rarity m_rarity = Rarity.COMMON;
		public Rarity rarity { 
			get { return m_rarity; } 
			set { m_rarity = value; }
		}

		protected string m_type = "";
		public string type { get { return m_type; } }

		// Optional:
		// To be used by each reward type if needed

		// Currency corresponding to this reward
		protected UserProfile.Currency m_currency = UserProfile.Currency.NONE;
		public UserProfile.Currency currency { get { return m_currency; } }

		// Rewarded amount
		protected long m_amount = 1;
		public long amount { get { return m_amount; } }

		// Sku of the rewarded item, if any
		protected string m_sku = "";
		public string sku { get { return m_sku; }}

		protected string m_source = "";
		public string source { get { return m_source; } }

		// Definition of the rewarded item, if any
		protected DefinitionNode m_def = null;
		public DefinitionNode def { get { return m_def; }}

		// Replacement reward, if any (if the rewarded item is already owned, we're giving this reward instead)
		protected Reward m_replacement;
		public Reward replacement { get { return m_replacement; }}

		//------------------------------------------------------------------------//
		// INTERNAL METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Initialize with default values.
		/// To be called by heirs before overriding any field.
		/// </summary>
		/// <param name="_type">Type of reward.</param>
		protected void Init(string _type) {
			// Store type
			m_type = _type;

			// Set initial default values on the rest of fields
			m_rarity = Rarity.COMMON;
			m_currency = UserProfile.Currency.NONE;
			m_amount = 1;
			m_sku = string.Empty;
			m_def = null;
			m_replacement = null;
		}

		//------------------------------------------------------------------------//
		// PUBLIC METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Collect the reward! If the reward is going to be replaced, collect the replacement instead.
		/// </summary>
		public virtual void Collect() {
			if(m_replacement != null) {
				m_replacement.Collect();
			} else {
				DoCollect();
			}

			// Save persistence to prevent opening this reward twice in case of interruption
			PersistenceFacade.instance.Save_Request(true);
		}

		/// <summary>
		/// To be implemented by heirs.
		/// Do the actual collection based on reward type.
		/// </summary>
		protected abstract void DoCollect();

		/// <summary>
		/// Simple method to know if the reward is a duplicate and will be replaced by another reward.
		/// You can check the replacement reward with the <c>replacement</c> property.
		/// Keep in mind that not all reward types can be replaced!
		/// </summary>
		/// <returns><c>true</c> if the reward is going to be replaced, <c>false</c> otherwise.</returns>
		public bool WillBeReplaced() {
			return m_replacement != null;
		}

		/// <summary>
		/// Return a visual representation of the reward.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="Metagame.Reward"/>.</returns>
		public override string ToString() {			
			return "[" + GetType() + ": " + m_amount + "]";
		}

		/// <summary>
		/// Create and return a persistence save data json initialized with this reward's data. 
		/// </summary>
		/// <returns>A new data json to be stored to persistence.</returns>
		public SimpleJSON.JSONNode ToJson() {
			// Create new object, initialize and return it
			SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

			// Reward type
			data.Add("type", type);

			// Sku and amount - sku prevails
			if(!string.IsNullOrEmpty(sku)) {
				data.Add("sku", sku);
			} else {
				data.Add("amount", amount.ToString(PersistenceManager.JSON_FORMATTING_CULTURE));
			}

			// Source
			data.Add("source", source);

			// Economy group (only for currency rewards)
			if(this is Metagame.RewardCurrency) {
				data.Add("economyGroup", HDTrackingManager.EconomyGroupToString((this as Metagame.RewardCurrency).EconomyGroup));
			}

			return data;
		}
	}
}