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
				case Rarity.COMMON:		return "common";		break;
				case Rarity.RARE:		return "rare";			break;
				case Rarity.EPIC:		return "epic";			break;
				case Rarity.SPECIAL:	return "special";		break;
				default:				return string.Empty;	break;
			}
			return string.Empty;
		}

		/// <summary>
		/// Constructor from json data.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public static Reward CreateFromJson(SimpleJSON.JSONNode _data) {			
			string type = _data["type"];
			type = type.ToLower();
			
			string data = "";
			if(_data.ContainsKey("sku")) 	data = _data["sku"];
			if(_data.ContainsKey("amount")) data = _data["amount"];

			return CreateFromTypeCode(type, data);
		}

		/// <summary>
		/// Creates from type code. String codes used in server comunication are: sc, pc, fg, egg, gegg, pet.
		/// </summary>
		/// <returns>A reward.</returns>
		/// <param name="_typeCode">Type code.</param>
		/// <param name="_data">Data.</param>
		public static Reward CreateFromTypeCode(string _typeCode, string _data) {			
			switch(_typeCode) {
				case RewardSoftCurrency.TYPE_CODE:	 return CreateTypeSoftCurrency(long.Parse(_data));
				case RewardHardCurrency.TYPE_CODE:	 return CreateTypeHardCurrency(long.Parse(_data));
				case RewardGoldenFragments.TYPE_CODE: return CreateTypeGoldenFragments(int.Parse(_data), Rarity.COMMON);
				case RewardEgg.TYPE_CODE:			 return CreateTypeEgg(_data);
				case RewardPet.TYPE_CODE:			 return CreateTypePet(_data);
			}
			return null;
		}

		public static RewardSoftCurrency CreateTypeSoftCurrency(long _amount)			{ return new RewardSoftCurrency(_amount, Rarity.COMMON); }
		public static RewardHardCurrency CreateTypeHardCurrency(long _amount)			{ return new RewardHardCurrency(_amount, Rarity.COMMON); }
		public static RewardGoldenFragments CreateTypeGoldenFragments(int _amount, Rarity _rarity) { return new RewardGoldenFragments(_amount, _rarity); }
		public static RewardEgg CreateTypeEgg(string _sku) 								{ return new RewardEgg(_sku); }
		public static RewardEgg CreateTypeEgg(string _sku, string _rewardSku) 			{ return new RewardEgg(_sku, _rewardSku); }
		public static RewardPet CreateTypePet(string _sku)								{ return new RewardPet(_sku); }
		public static RewardPet CreateTypePet(DefinitionNode _def)						{ return new RewardPet(_def); }
		#endregion

		//------------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES												  //
		//------------------------------------------------------------------------//
		// Mandatory:
		protected Rarity m_rarity = Rarity.COMMON;
		public Rarity rarity { get { return m_rarity; } }

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
	}
}