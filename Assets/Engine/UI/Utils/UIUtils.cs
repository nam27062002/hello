// UIUtils.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Globalization;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public partial class UIUtils {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const string TID_ORDINAL_TEMPLATE = "TID_GEN_ORDINAL_{0}";
	private const long ORDINAL_SCOPE = 100;

	public enum OrdinalSuffixFormat {
		DEFAULT,
		SUPERSCRIPT,
		SUBSCRIPT
	}
	
	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Format number with different options and using localized formatting, adding 
	/// the right ordinal suffix (1st, 2nd, 3rd...).
	/// </summary>
	/// <returns>The string representing the given number.</returns>
	/// <param name="_num">The number to be represented.</param>
	/// <param name="_format">Format for the suffix.</param>
	/// <param name="_zeroPadding">The minimum amount of digits to be displayed in the integer part of the number. Missing digits will be filled with '0'. Example: 12.3456, 4 -> "0012.3456".</param>
	/// <param name="_useThousandSeparators">Whether to use thousands separators or not.</param>
	public static string FormatOrdinalNumber(long _num, OrdinalSuffixFormat _format, int _zeroPadding = 0, bool _useThousandSeparators = true) {
		// Format the number
		string number = StringUtils.FormatNumber(_num, _zeroPadding, _useThousandSeparators);

		// Get the localized ordinal string
		string ordinal = LocalizationManager.SharedInstance.Localize(
			GetOrdinalSuffixTid(_num),
			number
		);
        
		// If required, perform formatting transformations
		switch(_format) {
			case OrdinalSuffixFormat.SUPERSCRIPT: {
				// We'll do the bold assumption that the ordinal suffix is always after the number
				int suffixStartPos = ordinal.IndexOf(number, StringComparison.InvariantCulture) + number.Length;
				ordinal = ordinal.Insert(suffixStartPos, "<sup>");	// Insert tag opening
				ordinal += "</sup>";	// Append tag closing
			} break;

			case OrdinalSuffixFormat.SUBSCRIPT: {
				// We'll do the bold assumption that the ordinal suffix is always after the number
					int suffixStartPos = ordinal.IndexOf(number, StringComparison.InvariantCulture) + number.Length;
					ordinal = ordinal.Insert(suffixStartPos, "<sub>");  // Insert tag opening
					ordinal += "</sub>";    // Append tag closing
			} break;

            case OrdinalSuffixFormat.DEFAULT: {
            } break;

        }

		// Done!
		return ordinal;
	}

	/// <summary>
	/// Gets the TID corresponding to the ordinal suffix for a given number (st, nd, th...).
	/// </summary>
	/// <returns>The TID for the ordinal suffix. Text will have the format "%U0st".</returns>
	/// <param name="_num">Number.</param>
	public static string GetOrdinalSuffixTid(long _num) {
		// Treat negative numbers as positive
		long num = Math.Abs(_num);

		// Choose the right ordinal suffix for the target number
		// If the number is outside our scope, find equivalent within scope
		long suffixIdx = num % ORDINAL_SCOPE;   // i.e. 3 -> 3, 150 -> 50, 201 -> 1, etc.

		// Format TID string and return
		return String.Format(TID_ORDINAL_TEMPLATE, suffixIdx, CultureInfo.InvariantCulture);  // i.e. TID_GEN_ORDINAL_{0} -> TID_GEN_ORDINAL_25
	}
}