// GDPRSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global setup of COPPA / GDPR settings.
/// </summary>
//[CreateAssetMenu]
public class GDPRSettings : SingletonScriptableObject<GDPRSettings> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Flags]
	public enum CountryGroup {
		// [AOC] Max 32 values (try inheriting from long if more are needed)
		NONE = 0,

		GDPR = 1 << 1,
		COPPA = 1 << 2,
		NO_UNDERAGE_PURCHASE_INCENTIVATION = 1 << 3,

		ALL = ~(0)      // http://stackoverflow.com/questions/7467722/how-to-set-all-bits-of-enum-flag
	}

	[Serializable]    
	public class CountrySetup {
        // Default value needs to meet the latest changes in GDPR requirements (https://mdc-web-tomcat17.ubisoft.org/confluence/pages/viewpage.action?pageId=610147488):        
        // -)Extension of the GDPR consent flow WW
        // -)Extension of the Age Gate WW, for apps defined as "Family apps" by Legal.
        // -)Minors age defined as =< 15 y.o at WW level
        public bool hasAgeRestriction = true;
		public int ageRestriction = 16;
		public bool requiresConsent = true;
		public CountryGroup group = CountryGroup.GDPR;
	}

	[Serializable]
	public class CountrySetupDictionary : SerializableDictionary<string, CountrySetup> { }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private CountrySetupDictionary m_countryList = new CountrySetupDictionary();

    // Although all countries share the same privacy policy we still need to use this list because some countries might need to use specific groups, for example NO_UNDERAGE_PURCHASE_INCENTIVATION
    // for Germany
	public static Dictionary<string, CountrySetup> countryList {
		get { return instance.m_countryList.dict; }
	}

	// Internal
	private CountrySetup m_defaultSetup = new CountrySetup();

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the GDPR setup for a specific country.
	/// </summary>
	/// <returns>The setup for that country, if defined. Default setup otherwise, never null.</returns>
	/// <param name="_countryCode">2 letter ISO 3166-1 country code. See https://ca.wikipedia.org/wiki/ISO_3166-1.</param>
	public static CountrySetup GetSetup(string _countryCode) {
        // Do we have a setup for this country?
        return (countryList.ContainsKey(_countryCode)) ? countryList[_countryCode] : instance.m_defaultSetup;
	}

	/// <summary>
	/// Get the age restriction for a specific country.
	/// </summary>
	/// <returns>The age restriction for that country, if defined. -1 otherwise.</returns>
	/// <param name="_countryCode">2 letter ISO 3166-1 country code. See https://ca.wikipedia.org/wiki/ISO_3166-1.</param>
	public static int GetAgeRestriction(string _countryCode) {
		return GetSetup(_countryCode).ageRestriction;
	}

	/// <summary>
	/// Get the consent requirement for a specific country.
	/// </summary>
	/// <returns>The consent requirement for that country, if defined. false otherwise.</returns>
	/// <param name="_countryCode">2 letter ISO 3166-1 country code. See https://ca.wikipedia.org/wiki/ISO_3166-1.</param>
	public static bool GetRequiresConsent(string _countryCode) {
		return GetSetup(_countryCode).requiresConsent;
	}

	/// <summary>
	/// Check whether a specific country belongs to a given country group or not.
	/// </summary>
	/// <returns><c>true</c>, if the target country belongs to the given country group, <c>false</c> otherwise.</returns>
	/// <param name="_countryCode">2 letter ISO 3166-1 country code. See https://ca.wikipedia.org/wiki/ISO_3166-1.</param>
	/// <param name="_toCheck">Country group to check. Can be a combination of multiple country groups, result will be positive if the country belongs to at least one of the groups.</param>
	public static bool CheckCountryGroup(string _countryCode, CountryGroup _toCheck) {
		// Invalid params
		if(_toCheck == CountryGroup.NONE) return false;
		if(_toCheck == CountryGroup.ALL) return true;

		// Check target country's groups against the given groups
		return (GetSetup(_countryCode).group & _toCheck) != 0;	// group is included in _toCheck if the result of this operation is not 0
	}
}
