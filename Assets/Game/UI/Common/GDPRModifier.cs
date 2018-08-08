// AgeRestrictionModifier.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/08/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to apply some UI actions based on COPPA/GDPR restrictions.
/// </summary>
public class GDPRModifier : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Condition {
		// [AOC] DON'T CHANGE THE ORDER!!
		UNDERAGE,
		CONSENT_NOT_GIVEN
	}

	public enum Action {
		// [AOC] DON'T CHANGE THE ORDER!!
		DISABLE,
		CHANGE_TEXT
	}

	public enum CountryGroup {
		ALL_COUNTRIES = 0,
		GDPR,
		COPPA,
		GERMANY,
		UNKNOWN
	}

	private static readonly string[][] COUNTRY_GROUPS = new string[][] {
		new string[] { },
		new string[] { "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "RO", "SK", "SI", "ES", "SE", "GB" },
		new string[] { "US" },
		new string[] { "DE" },
		new string[] { "Unknown", "--" }
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Condition m_condition = Condition.UNDERAGE;

	[SerializeField] private Action m_action = Action.DISABLE;

	[SerializeField] private CountryGroup m_countryGroup = CountryGroup.ALL_COUNTRIES;

	[Separator("Optional")]
	[SerializeField] private string m_replacementTID = "";
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Just do it!
		Apply();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply GDPR Modification.
	/// </summary>
	public void Apply() {
		// Aux vars
		bool underage = GDPRManager.SharedInstance.IsAgeRestrictionEnabled();
		bool consented = !GDPRManager.SharedInstance.IsConsentRestrictionEnabled();

		// Check condition - return if condition is not met
		switch(m_condition) {
			case Condition.UNDERAGE: {
				if(!underage) return;
			} break;

			case Condition.CONSENT_NOT_GIVEN: {
				if(consented) return;
			} break;
		}

		// Check country - return if current country is not in the list
		if(m_countryGroup != CountryGroup.ALL_COUNTRIES) {
			string country = GDPRManager.SharedInstance.GetCachedUserCountryByIP();
			if(COUNTRY_GROUPS[(int)m_countryGroup].IndexOf(country) == -1) return;
		}

		// Perform action
		switch(m_action) {
			case Action.DISABLE: {
				this.gameObject.SetActive(false);
			} break;

			case Action.CHANGE_TEXT: {
				Localizer loc = GetComponent<Localizer>();
				if(loc != null) loc.Localize(m_replacementTID, loc.replacements);   // Reuse replacements
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}