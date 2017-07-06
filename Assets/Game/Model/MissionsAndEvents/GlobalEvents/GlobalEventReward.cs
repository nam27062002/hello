// GlobalEventReward.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single global event object.
/// </summary>
public partial class GlobalEvent {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Reward item and the percentage required to achieve it.
	/// </summary>
	[Serializable]
	public class Reward {
		//------------------------------------------------------------------------//
		// CONSTANTS															  //
		//------------------------------------------------------------------------//
		public enum Type {
			UNKNOWN = -1,
			SC,
			PC,
			GOLDEN_FRAGMENTS,
			EGG,
			PET
		};

		//------------------------------------------------------------------------//
		// MEMBERS																  //
		//------------------------------------------------------------------------//
		public Type type = Type.UNKNOWN;

		// Amount and sku are optional, depending on type we will use one or another
		public float amount = 1f;
		public string sku = "";

		public float targetPercentage = 0f;
		public float targetAmount = 0f;		// Should match target percentage

		//------------------------------------------------------------------------//
		// METHODS																  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Constructor from json data.
		/// </summary>
		/// <param name="_data">Data to be parsed.</param>
		public Reward(SimpleJSON.JSONNode _data) {
			// Reward data
			type = StringToType(_data["type"]);

			// Optional parameters
			if(_data.ContainsKey("sku")) 	sku = _data["sku"];
			if(_data.ContainsKey("amount")) amount = _data["amount"].AsFloat;

			// Init target percentage
			// Target amount should be initialized from outside, knowing the global target
			targetPercentage = _data["targetPercentage"].AsFloat;
		}

		//------------------------------------------------------------------------//
		// STATIC UTILS															  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Given a string, convert to type.
		/// </summary>
		/// <returns>Type corresponding to the given string.</returns>
		/// <param name="_typeString">String to be parsed.</param>
		public static Type StringToType(string _typeString) {
			switch(_typeString) {
				case "sc": return Type.SC;
				case "pc": return Type.PC;
				case "gf": return Type.GOLDEN_FRAGMENTS;
				case "pet": return Type.PET;
				case "egg": return Type.EGG;
			}
			return Type.UNKNOWN;
		}

		/// <summary>
		/// Given a type, convert to string.
		/// </summary>
		/// <returns>Type corresponding to the given string.</returns>
		/// <param name="_type">Type to be parsed.</param>
		public static string TypeToString(Type _type) {
			switch(_type) {
				case Type.SC: return "sc";
				case Type.PC: return "pc";
				case Type.GOLDEN_FRAGMENTS: return "gf";
				case Type.PET: return "pet";
				case Type.EGG: return "egg";
			}
			return string.Empty;
		}

		/// <summary>
		/// Given a type, try to convert it to a in-game currency.
		/// </summary>
		/// <returns>The currency corresponding to the given type. <c>UserProfile.Currency.NONE</c> if type doesn't match to any currency (eggs, pets).</returns>
		/// <param name="_type">Type to be parsed.</param>
		public static UserProfile.Currency TypeToCurrency(Type _type) {
			switch(_type) {
				case Type.SC: 				return UserProfile.Currency.SOFT;
				case Type.PC: 				return UserProfile.Currency.HARD;
				case Type.GOLDEN_FRAGMENTS: return UserProfile.Currency.GOLDEN_FRAGMENTS;
				default:	  				return UserProfile.Currency.NONE;
			}
			return UserProfile.Currency.NONE;
		}
	};
}