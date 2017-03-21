// StringExt.cs
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the String class.
/// </summary>
public static class StringExt {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Remove all accents from the given string.
	/// </summary>
	/// <returns>The input string with all characters with accent replaced by their base character.</returns>
	/// <param name="_string">Input string.</param>
	public static string RemoveDiacritics(this string _string) {
		// From http://code.commongroove.com/2011/04/29/c-string-extension-to-replace-accented-characters/
		string normalizedString = _string.Normalize(NormalizationForm.FormD);
		StringBuilder stringBuilder = new StringBuilder();
		foreach(char c in normalizedString) {
			UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if(unicodeCategory != UnicodeCategory.NonSpacingMark) {
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
	}

	/// <summary>
	/// Creates a valid C# identifier (for classes, variables, etc).
	/// That usually means remove all spaces, accents and special characters.
	/// No capitalization process is done.
	/// </summary>
	/// <returns>The input string after validation.</returns>
	/// <param name="_string">String to be validated.</param>
	public static string GenerateValidIdentifier(this string _string) {
		// Remove all accents
		string validString = _string.RemoveDiacritics();

		// Remove all spaces and special characters
		validString = Regex.Replace(validString, @"[^a-zA-Z0-9_]", "");

		return validString;
	}
}
