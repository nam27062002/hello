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
	
	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Format number with different options and using localized formatting, adding 
	/// the right ordinal suffix (1st, 2nd, 3rd...).
	/// </summary>
	/// <returns>The string representing the given number.</returns>
	/// <param name="_num">The number to be represented.</param>
	/// <param name="_zeroPadding">The minimum amount of digits to be displayed in the integer part of the number. Missing digits will be filled with '0'. Example: 12.3456, 4 -> "0012.3456".</param>
	/// <param name="_useThousandSeparators">Whether to use thousands separators or not.</param>
	public static string FormatOrdinalNumber(long _num, int _zeroPadding = 0, bool _useThousandSeparators = true) {
		// Treat negative numbers as positive
		long num = Math.Abs(_num);

		// Choose the right ordinal suffix for the target number
		// If the number is outside our scope, find equivalent within scope
		long suffixIdx = num % ORDINAL_SCOPE;   // i.e. 3 -> 3, 150 -> 50, 201 -> 1, etc.

		// Format TID string
		string tid = String.Format(TID_ORDINAL_TEMPLATE, suffixIdx, CultureInfo.InvariantCulture);  // i.e. TID_GEN_ORDINAL_{0} -> TID_GEN_ORDINAL_25

		// Perform the localization and number formatting all at once and return
		return LocalizationManager.SharedInstance.Localize(
			tid, 
			StringUtils.FormatNumber(_num, _zeroPadding, _useThousandSeparators)
		);
	}
}