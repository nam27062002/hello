namespace Metagame {
	public abstract class Reward {
		public enum Rarity { COMMON = 0, RARE, EPIC, SPECIAL }

		#region Factory
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
		/// Constructor from json data.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public static Reward CreateFromJson(SimpleJSON.JSONNode _data) {	
			string type = _data["type"];
			string data = "";

			if(_data.ContainsKey("sku")) 	data = _data["sku"];
			if(_data.ContainsKey("amount")) data = _data["amount"];

			return CreateFromTypeCode(_data["type"], data);
		}

		/// <summary>
		/// Creates from type code. String codes used in server comunication are: sc, pc, fg, egg, gegg, pet.
		/// </summary>
		/// <returns>A reward.</returns>
		/// <param name="_typeCode">Type code.</param>
		/// <param name="_data">Data.</param>
		public static Reward CreateFromTypeCode(string _typeCode, string _data) {			
			switch (_typeCode) {
				case RewardSoftCurrency.Code:	 return CreateTypeSoftCurrency(long.Parse(_data));
				case RewardHardCurrency.Code:	 return CreateTypeHardCurrency(long.Parse(_data));
				case RewardGoldenFragments.Code: return CreateTypeGoldenFragments(int.Parse(_data), Rarity.COMMON);
				case RewardEgg.Code:			 return CreateTypeEgg(_data);
				case RewardPet.Code:			 return CreateTypePet(_data);
			}
			return null;
		}

		public static Reward CreateTypeSoftCurrency(long _amount)					{ return new RewardSoftCurrency(_amount, Rarity.COMMON); }
		public static Reward CreateTypeHardCurrency(long _amount)					{ return new RewardHardCurrency(_amount, Rarity.COMMON); }
		public static Reward CreateTypeGoldenFragments(int _amount, Rarity _rarity) { return new RewardGoldenFragments(_amount, _rarity); }
		public static Reward CreateTypeEgg(string _sku) 							{ return new RewardEgg(_sku); }
		public static Reward CreateTypeEgg(string _sku, string _rewardSku) 			{ return new RewardEgg(_sku, _rewardSku); }
		public static Reward CreateTypePet(string _sku)								{ return new RewardPet(_sku); }
		public static Reward CreateTypePet(DefinitionNode _def)						{ return new RewardPet(_def); }
		#endregion


		//------------------------------------------//
		//	Attributes and Properties
		//------------------------------------------//
		protected Rarity m_rarity;
		public Rarity rarity { get { return m_rarity; } }

		protected UserProfile.Currency m_currency;
		public UserProfile.Currency currency { get { return m_currency; } }

		protected string m_value;
		public string value { get { return m_value; } }
		//------------------------------------------//


		//------------------------------------------//
		//	Methods
		//------------------------------------------//
		public abstract void Collect();

		public override string ToString() {			
			return "[" + GetType() + ": " + m_value + "]";
		}
		//------------------------------------------//
	}
}