// Localization.cs
// 
// Created by Alger Ortín Castellví on 15/07/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Globalization;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Utils to localize texts.
/// </summary>
public static class Localization {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Current localization code as defined by the C# standards
	// @see https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx
	private static string _code = CultureInfo.CurrentCulture.Name;
	public static string code {
		get { return _code; }
	}

	private static CultureInfo _culture = CultureInfo.CurrentCulture;
	public static CultureInfo culture {
		get { return _culture; }
	}

	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Define the language code to be used for localization and formatting.
	/// </summary>
	/// <param name="_sLanguageCode">The code representing the language to be used. One of these: https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx</param>
	public static void SetLanguage(string _sLanguageCode) {
		// Store language code
		_code = _sLanguageCode;

		// Create a new culture with the given code
		_culture = CultureInfo.CreateSpecificCulture(_sLanguageCode);

		// Just in case there is any wild formatting out there, change default current culture as well
		System.Threading.Thread.CurrentThread.CurrentCulture = _culture;

		// Notify the rest of the game
		Messenger.Broadcast(EngineEvents.EVENT_LANGUAGE_CHANGED);
	}

	/// <summary>
	/// Translate the given TID into the current localization language.
	/// </summary>
	/// <param name="_sTID">TID to be translated.</param>
	/// /// <param name="replacements">Optional list of replacements for the localized string. Will replace all "{0}", "{1}" instances found in the tranlated strings, respectively.</param>
	public static string Localize(string _sTID, params string[] replacements) {
		// [AOC] TODO!! Do the translation

		// Apply replacements
		for(int i = 0; i < replacements.Length; i++) {
			_sTID = _sTID.Replace("{" + i.ToString() + "}", replacements[i]);
		}

		// Done!
		return _sTID;
	}
}
