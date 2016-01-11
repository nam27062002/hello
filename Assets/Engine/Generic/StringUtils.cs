// StringUtils.cs
// 
// Created by Alger Ortín Castellví on 12/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Static utils to do several string formattings.
/// </summary>
public class StringUtils {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Helper struct
	private struct SSuffix {
		public double threshold;
		public string suffix;

		public SSuffix(double _threshold, string _suffix) {
			threshold = _threshold;
			suffix = _suffix;
		}
	};

	private static readonly List<SSuffix> SUFFIXES = new List<SSuffix> {
		new SSuffix(100000000000000000000000000000000d, "N"),
		new SSuffix(10000000000000000000000000d, "S"),
		new SSuffix(10000000000000000000000d, "s"),
		new SSuffix(1000000000000000000d, "Q"),
		new SSuffix(1000000000000000d, "q"),
		new SSuffix(1000000000000d, "T"),
		new SSuffix(1000000000d, "B"),
		new SSuffix(1000000d, "M"),
		new SSuffix(1000d, "K")
	};

	public enum PathFormat {
		FULL_PATH,							// /Users/myUser/Documents/UnityProject/Assets/Resources/Game/Levels/Collision/SC_Level_0_Collision.unity <---- for C# IO library
		PROJECT_ROOT,						// Assets/Resources/Game/Levels/Collision/SC_Level_0_Collision.unity <---- for AssetsDatabase.Load
		ASSETS_ROOT,						// Resources/Game/Levels/Collision/SC_Level_0_Collision.unity
		RESOURCES_ROOT,						// Game/Levels/Collision/SC_Level_0_Collision.unity
		RESOURCES_ROOT_WITHOUT_EXTENSION,	// Game/Levels/Collision/SC_Level_0_Collision <---- for Resources.Load
		FILENAME_WITH_EXTENSION,			// SC_Level_0_Collision.unity
		FILENAME_WITHOUT_EXTENSION			// SC_Level_0_Collision <---- for Application.LoadLevel, SceneManager.LoadScene, etc.
	}

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Format number with different options and using localized formatting.
	/// </summary>
	/// <returns>The string representing the given number.</returns>
	/// <param name="_num">The number to be represented.</param>
	/// <param name="_iDecimalPlaces">The maximum amount of digits to be displayed in the decimal part. Example: 12.3456, 2 -> "12.34".</param>
	/// <param name="_iZeroPadding">The minimum amount of digits to be displayed in the integer part of the number. Missing digits will be filled with '0'. Example: 12.3456, 4 -> "0012.3456".</param>
	/// <param name="_bUseThousandSeparators">Whether to use thousands separators or not.</param>
	/// <param name="_bForceDecimals">If true, decimal part will be filled with 0s to match _iDecimalPlaces.</param>
	public static string FormatNumber(ulong _num, int _iZeroPadding = 0, bool _bUseThousandSeparators = true) {
		return FormatNumber((double)_num, 0, _iZeroPadding, _bUseThousandSeparators);
	}
	public static string FormatNumber(long _num, int _iZeroPadding = 0, bool _bUseThousandSeparators = true) {
		return FormatNumber((double)_num, 0, _iZeroPadding, _bUseThousandSeparators);
	}
	public static string FormatNumber(double _num, int _iDecimalPlaces, int _iZeroPadding = 0, bool _bUseThousandSeparators = true, bool _bForceDecimals = false) {
		// C# makes it 'easy' for us!
		// see https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx
		// 1. Create a formatting string based on input parameters
		System.IO.StringWriter format = new System.IO.StringWriter();

		// 1.1. Standard format with (or without) thousand separators
		format.Write("#");
		if(_bUseThousandSeparators) format.Write(",");

		// 1.2. Zero padding - at least 1 digit is mandatory
		format.Write("0");
		for(int i = 1; i < _iZeroPadding; i++) {
			format.Write("0");
		}

		// 1.3. Decimal separator
		format.Write(".");

		// 1.4. Precision
		for(int i = 0; i < _iDecimalPlaces; i++) {
			// see https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx
			if(_bForceDecimals) {
				format.Write("0");	// Will add trailing 0s if needed
			} else {
				format.Write("#");	// Will only add the number if it's a non-redundant 0
			}
		}

		// 2. Transform the number using the computed format and the current locale
		// see https://msdn.microsoft.com/en-us/library/d830955a(v=vs.110).aspx
		return _num.ToString(format.ToString(), Localization.culture);
	}

	/// <summary>
	/// Formats a given number in the way suitable for the game UI.
	/// </summary>
	/// <returns>A string with the given number properly formatted.</returns>
	/// <param name="_num">The number to be formatted.</param>
	public static string FormatBigNumber(double _num) {
		// Recursive call for negative numbers
		if(_num < 0) return "-" + FormatBigNumber(-_num);

		// Find out suffix and adapt number
		string suffix = "";
		for(int i = 0; i < SUFFIXES.Count; i++) {
			if(_num > SUFFIXES[i].threshold * 1000d) {	// We don't want 1K, 10K, 100K, we would be losing precision and it looks weird, so let's start with 1000K
				// That's it!
				_num /= SUFFIXES[i].threshold;
				suffix = SUFFIXES[i].suffix;
				break;
			}
		}

		// Format using the current localization settings and return
		// [AOC] If number is smaller than 1, use 2 decimals if possible
		if(_num < 1d && _num > 0.009d) {
			return String.Format("{0}{1}", FormatNumber(_num, 2), suffix);
		} else {
			return String.Format("{0}{1}", FormatNumber(_num, 0), suffix);
		}
	}

	/// <summary>
	/// Converts a multiplier value to percentage increase representation.
	/// <para>Example: multiplier 1.5 -> "50%" / "+50%"</para>
	/// <para>Example: multiplier 0.75 -> "25%" / "-25%"</para>
	/// </summary>
	/// <returns>The string representation of the percentage increase represented by the given multiplier, including "%" symbol.</returns>
	/// <param name="_multiplier">The multiplier factor.</param>
	/// <param name="_bIncludeSign">Whether to force the "-"/"+" signs in the returned string. If <c>false</c>, no sign at all will be included.</param>
	public static string MultiplierToPercentageIncrease(double _multiplier, bool _bIncludeSign) {
		double percentageIncrease = (_multiplier * 100) - 100;
		string sResult = String.Format("{0}%", FormatNumber(Math.Abs(percentageIncrease), Math.Abs(percentageIncrease) >= 1 ? 0 : 2));	// Only use decimals if percentage is lower than 1
		if(_bIncludeSign) {
			if(percentageIncrease < 0) {
				sResult = "-" + sResult;
			} else {
				sResult = "+" + sResult;
			}
		}
		return sResult;
	}

	/// <summary>
	/// Format a given full path into one of the known formats.
	/// </summary>
	/// <returns>The formatted value.</returns>
	/// <param name="_fullPath">The path to be formatted. Full path from system's root, including file extension, as it would be in C# FileInfo.FullName (e.g. /Users/myUser/Documents/UnityProject/Assets/Resources/Game/Levels/Collision/SC_Level_0_Collision.unity).</param>
	/// <param name="_format">The format to be used.</param>
	public static string FormatPath(string _fullPath, PathFormat _format) {
		// [AOC] Windows uses backward slashes, which Unity doesn't recognize;
		_fullPath = _fullPath.Replace('\\', '/');

		// Select target format and do the required transformations
		switch(_format) {
			case PathFormat.FULL_PATH: {
				return _fullPath;
			}

			case PathFormat.PROJECT_ROOT: {
				return _fullPath.Replace(Application.dataPath, "Assets");
			}

			case PathFormat.ASSETS_ROOT: {
				return _fullPath.Replace(Path.Combine(Application.dataPath, "/"), "");
			}

			case PathFormat.RESOURCES_ROOT: {
				return _fullPath.Replace(Path.Combine(Application.dataPath, "Resources/"), "");
			}

			case PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION: {
				_fullPath = _fullPath.Replace(Path.Combine(Application.dataPath, "Resources/"), "");
				return Path.Combine(Path.GetDirectoryName(_fullPath), Path.GetFileNameWithoutExtension(_fullPath));
			}

			case PathFormat.FILENAME_WITH_EXTENSION: {
				return Path.GetFileName(_fullPath);
			}

			case PathFormat.FILENAME_WITHOUT_EXTENSION: {
				return Path.GetFileNameWithoutExtension(_fullPath);
			}
		}
		return _fullPath;
	}
}
