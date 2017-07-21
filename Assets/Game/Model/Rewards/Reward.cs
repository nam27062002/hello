namespace Metagame {
	public abstract class Reward {
		#region Factory
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
			switch (_typeCode) {
				case RewardSoftCurrency.Code:	 return CreateTypeSoftCurrency(long.Parse(_data));
				case RewardHardCurrency.Code:	 return CreateTypeHardCurrency(long.Parse(_data));
				case RewardGoldenFragments.Code: return CreateTypeGoldenFragments(int.Parse(_data));
				case RewardEgg.Code:			 return CreateTypeEgg(_data);
				case RewardGoldenEgg.Code:		 return CreateTypeGoldenEgg(_data);
				case RewardPet.Code:			 return CreateTypePet(_data);
			}
			return null;
		}

		public static Reward CreateTypeSoftCurrency(long _amount)	{ return new RewardSoftCurrency(_amount); }
		public static Reward CreateTypeHardCurrency(long _amount)	{ return new RewardHardCurrency(_amount); }
		public static Reward CreateTypeGoldenFragments(int _amount) { return new RewardGoldenFragments(_amount); }
		public static Reward CreateTypeEgg(string _sku) 			{ return new RewardEgg(_sku); }
		public static Reward CreateTypeGoldenEgg(string _sku) 		{ return new RewardGoldenEgg(_sku); }
		public static Reward CreateTypePet(string _sku) 			{ return new RewardPet(_sku); }
		#endregion


		//------------------------------------------//
		//	Attributes and Properties
		//------------------------------------------//
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