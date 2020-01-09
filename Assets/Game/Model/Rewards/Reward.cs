// Reward.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on 18/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

using System.Collections.Generic;

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
			UNKNOWN = -1,

			COMMON, 
			RARE, 
			EPIC,

			COUNT
		}

		public class Data {
			public string typeCode = "";
			public string sku = "";
			public long amount = 1;
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
			}
			return Rarity.UNKNOWN;
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
			Data rewardData = new Data();

			rewardData.typeCode = _data["type"];
			rewardData.typeCode = rewardData.typeCode.ToLower();
			if (rewardData.typeCode == "hc")
				rewardData.typeCode = "pc";

			if (_data.ContainsKey("sku")) {
				rewardData.sku = _data["sku"];
			}

			if (_data.ContainsKey("amount")) {
				rewardData.amount = _data["amount"].AsLong;
			}

			Reward newReward = CreateFromData(rewardData, _economyGroup, _source);

			// In case the new reward has extra data to be parsed
			newReward.LoadCustomJsonData(_data);

			return newReward;
		}

		/// <summary>
		/// Creates from data. String codes used in server comunication are: sc, pc, gf, egg, pet, skin. TODO: dragon
		/// </summary>
		/// <returns>A reward.</returns>
		/// <param name="_data">Data for the reward to be created.</param>
		public static Reward CreateFromData(Data _data, HDTrackingManager.EEconomyGroup _economyGroup, string _source) {			
			switch(_data.typeCode) {
				// Currency rewards: pretty straight forward
				case RewardSoftCurrency.TYPE_CODE: {
					return CreateTypeSoftCurrency(_data.amount, _economyGroup, _source);
				}

				case RewardHardCurrency.TYPE_CODE:
				case "hc": {    // [AOC] Just in case
					return CreateTypeHardCurrency(_data.amount, _economyGroup, _source);
				}
					
				case RewardGoldenFragments.TYPE_CODE: {
					return CreateTypeGoldenFragments((int)_data.amount, Rarity.COMMON, _economyGroup, _source);
				}	

				// Egg reward: if amount is > 1, create a multi reward instead
				case RewardEgg.TYPE_CODE: {
					if(_data.amount > 1) {
						return CreateTypeMultiEgg(_data.amount, _data.sku, _source);
					} else {
						return CreateTypeEgg(_data.sku, _source);
					}
				}

                // Pet reward - ignoring amount (pets can only be rewarded once)
				case RewardPet.TYPE_CODE: {
					return CreateTypePet(_data.sku, _source);
				}

				// Skin reward - ignoring amount (skins can only be rewarded once)
				case RewardSkin.TYPE_CODE: {
					return CreateTypeSkin(_data.sku, _source);
				}

				// Dragon reward - ignoring amount (dragons can only be rewarded once)
				case RewardDragon.TYPE_CODE: {
					return CreateTypeDragon(_data.sku, _source);
				}

				// Multi-reward: Cannot be created using this method
				case RewardMulti.TYPE_CODE: { 
					return CreateTypeMulti(new List<Data>(), _source, _economyGroup);	// No rewards will be created, must be added afterwards via LoadCustomjsonData() or manually
				}

                case RewardMultiEgg.TYPE_CODE: {
                    return CreateTypeMultiEgg(_data.amount, _data.sku, _source);
                }

                case RewardRemoveAds.TYPE_CODE: {
                    return CreateTypeRemoveAds();
                }
            }
			return null;
		}


        /// <summary>
        /// Find the reward(s) asociated to the IAP, it will look for in the
        /// shopPacks and offerPack definitions.
        /// </summary>
        /// <param name="_iapSku"></param>
        /// <returns></returns>
        public static List<Reward> GetRewardsFromIAP(string _iapSku)
        {
            List<Reward> rewards = new List<Reward>();

            OfferPack offerPack = OffersManager.GetOfferPackByIAP(_iapSku);

            if (offerPack == null)
            {
                // Then, is an standard shop pack
                DefinitionNode m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, _iapSku);
                
                Metagame.Reward.Data shopPack = new Metagame.Reward.Data
                {
                    sku = m_def.sku,
                    typeCode = m_def.Get("type"),
                    amount = m_def.GetAsLong("amount")
                };

                // Just in case
                if (shopPack.typeCode == "hc")
                    shopPack.typeCode = "pc";

                rewards.Add( CreateFromData(shopPack, HDTrackingManager.EEconomyGroup.UNKNOWN, null) );

            }
            else
            {
                // It is an offer pack, extract all its items
                foreach (OfferPackItem item in offerPack.items)
                {
                    rewards.Add(item.reward);
                }

            }

            return rewards;

        }



		// Currencies
		public static RewardSoftCurrency CreateTypeSoftCurrency(long _amount, HDTrackingManager.EEconomyGroup _economyGroup, string _source, Rarity _rarity = Rarity.COMMON) {
			return new RewardSoftCurrency(_amount, _rarity, _economyGroup, _source);
		}

		public static RewardHardCurrency CreateTypeHardCurrency(long _amount, HDTrackingManager.EEconomyGroup _economyGroup, string _source, Rarity _rarity = Rarity.COMMON) {
			return new RewardHardCurrency(_amount, _rarity, _economyGroup, _source);
		}

		public static RewardGoldenFragments CreateTypeGoldenFragments(int _amount, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup, string _source) {
			return new RewardGoldenFragments(_amount, _rarity, _economyGroup, _source);
		}

		public static RewardCurrency CreateTypeCurrency(long _amount, UserProfile.Currency _currency, Rarity _rarity, HDTrackingManager.EEconomyGroup _economyGroup, string _source) {
			switch(_currency) {
				case UserProfile.Currency.SOFT: return CreateTypeSoftCurrency(_amount, _economyGroup, _source, _rarity);
				case UserProfile.Currency.HARD: return CreateTypeHardCurrency(_amount, _economyGroup, _source, _rarity);
				case UserProfile.Currency.GOLDEN_FRAGMENTS: return CreateTypeGoldenFragments((int)_amount, _rarity, _economyGroup, _source);
			}
			return null;
		}

		// Eggs
		public static RewardEgg CreateTypeEgg(string _sku, string _source) 				{ return new RewardEgg(_sku, _source); }
		public static RewardMultiEgg CreateTypeMultiEgg(long _amount, string _sku, string _source) { return new RewardMultiEgg(_amount, _sku, _source); }

		// Pets
		public static RewardPet CreateTypePet(string _sku, string _source)				{ return new RewardPet(_sku, _source); }
		public static RewardPet CreateTypePet(DefinitionNode _def, string _source)		{ return new RewardPet(_def, _source); }

		// Skins
		public static RewardSkin CreateTypeSkin(string _sku, string _source)			{ return new RewardSkin(_sku, _source); }
		public static RewardSkin CreateTypeSkin(DefinitionNode _def, string _source)	{ return new RewardSkin(_def, _source); }

		// Dragons
		public static RewardDragon CreateTypeDragon(string _sku, string _source) 			{ return new RewardDragon(_sku, _source); }
		public static RewardDragon CreateTypeDragon(DefinitionNode _def, string _source) 	{ return new RewardDragon(_def, _source); }
        
        // Remove ads
        public static RewardRemoveAds CreateTypeRemoveAds ()                            { return new RewardRemoveAds();  }

		// Others
		public static RewardMulti CreateTypeMulti(List<Data> _datas, string _source, HDTrackingManager.EEconomyGroup _economyGroup = HDTrackingManager.EEconomyGroup.UNKNOWN)	{ return new RewardMulti(_datas, _source, _economyGroup); }
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
		public string type { get { return m_type; }}

		// Optional:
		// To be used by each reward type if needed

		// Currency corresponding to this reward
		protected UserProfile.Currency m_currency = UserProfile.Currency.NONE;
		public UserProfile.Currency currency { get { return m_currency; } }

		// Rewarded amount
		private long m_amount = 1;
        public long amount { get { return m_amount + UnityEngine.Mathf.FloorToInt((m_amount * m_bonusPercentage) / 100.0f); } }

        protected float m_bonusPercentage;
        public float bonusPercentage { set { m_bonusPercentage = value; } }

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
            m_bonusPercentage = 0f;
			m_sku = string.Empty;
			m_def = null;
			m_replacement = null;
		}

        protected void Init(string _type, long _amount) {
            Init(_type);
            m_amount = _amount;
        }

		//------------------------------------------------------------------------//
		// PUBLIC METHODS														  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Collect the reward! If the reward is going to be replaced, collect the replacement instead.
		/// </summary>
		public virtual void Collect() {
			// If we're at the top of the stack, remove ourselves!
			// Before invoking the DoCollect(), which may add new rewards to the stack!
			if(UsersManager.currentUser.rewardStack.Count > 0
			&& UsersManager.currentUser.rewardStack.Peek() == this) {
				UsersManager.currentUser.PopReward();
			}

			// If the reward is going to be replaced, collect the replacement instead.
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
		/// Obtain the generic TID to describe this reward type.
		/// </summary>
		/// <returns>TID describing this reward type.</returns>
		/// <param name="_plural">Singular or plural TID?</param>
		public abstract string GetTID(bool _plural);

		/// <summary>
		/// Checks whether this reward needs to be replaced and creates a replacement
		/// reward if needed.
		/// </summary>
		public virtual void CheckReplacement() {
			// To be implemented by heirs if needed
		}

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
        /// This method checks if the reward is already owned by the player. This is applicable in
        /// non-consumable items like dragons, skins and the remove ads offer. Currency packs will be always
        /// marked as not owned.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsAlreadyOwned()
        {
            return false;
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
		public virtual SimpleJSON.JSONNode ToJson() {
			// Create new object, initialize and return it
			SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

			// Reward type
			data.Add("type", type);

			// Sku
			if(!string.IsNullOrEmpty(sku)) {
				data.Add("sku", sku);
			}

			// Amount
			data.Add("amount", amount.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));

			// Source
			data.Add("source", source);

			return data;
		}

		/// <summary>
		/// For those types requiring it, parse extra data from a json node.
		/// </summary>
		/// <param name="_data">Json to be parsed.</param>
		public virtual void LoadCustomJsonData(SimpleJSON.JSONNode _data) {
			// To be overriden if needed
		}
	}
}