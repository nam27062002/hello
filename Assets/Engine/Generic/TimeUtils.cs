// TimeUtils.cs
// ArmyTapper
// 
// Created by Alger Ortín Castellví on 15/07/2015.
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
/// Several time-related utilities. Imported from RW.
/// </summary>
public class TimeUtils {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// [AOC] Doesn't make any sense to use months, since every month has a different amount of days
	public enum EPrecision {
		YEARS,
		DAYS,
		HOURS,
		MINUTES,
		SECONDS,
		COUNT
	};
	
	public enum EFormat {
		WORDS,			// 1 year 2 days 3 hours 4 minutes 5 seconds
		ABBREVIATIONS,	// 1y 2d 3h 4m 5s
		ABBREVIATIONS_WITHOUT_0_VALUES,	// Same as abbreviations, but ignoring fields with value 0 (i.e. "5m 0s" -> "5m")
		DIGITS			// 0001:03 02:03:04
	};

	private static readonly ulong[] SECONDS_IN_PRECISION = {
		60*60*24*365,	// EPrecision.YEARS
		60*60*24,		// EPrecision.DAYS
		60*60,			// EPrecision.HOURS
		60,				// EPrecision.MINUTES
		1				// EPrecision.SECONDS
	};

	private static readonly string[] TIDS_ABBREVIATED = {
		"y",	//"TID_GEN_TIME_YEARS_ABR",	// EPrecision.YEARS
		"d",	//"TID_GEN_TIME_DAYS_ABR",	// EPrecision.DAYS
		"h",	//"TID_GEN_TIME_HOURS_ABR",	// EPrecision.HOURS
		"m",	//"TID_GEN_TIME_MINUTES_ABR",	// EPrecision.MINUTES
		"s",	//"TID_GEN_TIME_SECONDS_ABR"	// EPrecision.SECONDS
	};

	private static readonly string[] TIDS_SINGULAR = {
		"year",		//"TID_GEN_TIME_YEAR",	// EPrecision.YEARS
		"day",		//"TID_GEN_TIME_DAY",		// EPrecision.DAYS
		"hour",		//"TID_GEN_TIME_HOUR",	// EPrecision.HOURS
		"minute",	//"TID_GEN_TIME_MINUTE",	// EPrecision.MINUTES
		"second",	//"TID_GEN_TIME_SECOND"	// EPrecision.SECONDS
	};

	private static readonly string[] TIDS_PLURAL = {
		"years",	//"TID_GEN_TIME_YEARS",	// EPrecision.YEARS
		"days",		//"TID_GEN_TIME_DAYS",	// EPrecision.DAYS
		"hours",	//"TID_GEN_TIME_HOURS",	// EPrecision.HOURS
		"minutes",	//"TID_GEN_TIME_MINUTES",	// EPrecision.MINUTES
		"seconds",	//"TID_GEN_TIME_SECONDS"	// EPrecision.SECONDS
	};

	//------------------------------------------------------------------//
	// FORMATTING METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Format the given amount of seconds into readable string.
	/// Fields with 0 value won't be displayed unless required.
	/// Many combinations are possible, choose your parameters accordingly.
	/// <b>WARNING<b>This method is not suitable to get a date style formatting, only suitable for timers/countdowns/clocks/etc.
	/// </summary>
	/// <returns>A string containing the given amount of seconds formatted as defined by the parameters.</returns>
	/// <param name="_fSeconds">The amount of seconds to be formatted. Minimum precision is 1 second, so everything will be rounded up to the higher int value.</param>
	/// <param name="_eFormat">The output format.</param>
	/// <param name="_iNumFields">The amount of fields to be displayed (e.g. 4220s: 2 fields -> "1h 10m"; 3 fields -> "1h 10m 20s")</param>
	/// <param name="_ePrecision">Maximum time unit to be displayed (e.g. 4220s: precision >= HOURS -> "1h 10m 20s"; precision MINUTES -> "70m 20s")</param>
	public static string FormatTime(double _fSeconds, EFormat _eFormat, int _iNumFields, EPrecision _ePrecision = EPrecision.YEARS) {
		// 1. If seconds amount is negative, convert to positive to do all the maths and restore sign afterwards
		bool bIsNegative = false;
		if(_fSeconds < 0) {
			bIsNegative = true;
			_fSeconds *= -1;
		}
		
		// 2. Round up seconds
		ulong iSeconds = (ulong)(_fSeconds + 0.5f);
		
		// 3. Compute the amount of each precision field, starting with the defined precision
		ulong[] precisionValues = new ulong[(int)EPrecision.COUNT];
		for(int i = (int)_ePrecision; i < (int)EPrecision.COUNT; i++) {
			precisionValues[i] = iSeconds/SECONDS_IN_PRECISION[i];
			iSeconds -= precisionValues[i] * SECONDS_IN_PRECISION[i];
		}
		
		// 4. Find first and last values to be used, combining precision, number of fields, and ignoring 0 values
		// 4.1. First index: find first non-0 index starting at the given precision
		int iFirstIdx = (int)EPrecision.SECONDS;	// Default precision if all values are 0
		for(int i = (int)_ePrecision; i < precisionValues.Length; i++) {
			if(precisionValues[i] != 0) {
				iFirstIdx = i;	// Store precision
				break;	// Break loop
			}
		}
		
		// 4.2. Last index: if enough fields, initial precision + _iNumFields, otherwise go up to max precision
		int iLastIdx = Math.Min(iFirstIdx + _iNumFields, precisionValues.Length);
		
		// 5. Do the formatting depending on selected format
		ulong iVal;
		StringWriter writer = new StringWriter();

		// If the amount was originally negative, insert negative sign
		if(bIsNegative) writer.Write("-");

		// Do the rest
		for(int i = iFirstIdx; i < iLastIdx; i++) {
			// Get value
			iVal = precisionValues[i];
			
			// Do the formatting
			switch(_eFormat) {
				case EFormat.WORDS:
				case EFormat.ABBREVIATIONS:
				case EFormat.ABBREVIATIONS_WITHOUT_0_VALUES: {
					// Special case if not including 0 values
					if(_eFormat == EFormat.ABBREVIATIONS_WITHOUT_0_VALUES && iVal == 0) continue;	// Skip if value is 0
					
					// Insert space if not the first field
					if(i != iFirstIdx) {
						writer.Write(" ");
					}
					
					// Insert field value, properly formatted
					writer.Write(StringUtils.FormatNumber(iVal));
					
					// Insert field name, abbreviation, singular or plural
					writer.Write(" ");
					if(_eFormat == EFormat.ABBREVIATIONS || _eFormat == EFormat.ABBREVIATIONS_WITHOUT_0_VALUES) {
						writer.Write(Localization.Localize(TIDS_ABBREVIATED[i]));
					} else if(iVal == 1) {
						writer.Write(Localization.Localize(TIDS_SINGULAR[i]));
					} else {
						writer.Write(Localization.Localize(TIDS_PLURAL[i]));
					}
				} break;
					
				case EFormat.DIGITS: {
					// Specification says "YYYY:DD HH:MM:SS"
					// Put value properly formatted
					if(i == (int)EPrecision.YEARS) {
						writer.Write(StringUtils.FormatNumber(iVal, 4, false));
					} else {
						writer.Write(StringUtils.FormatNumber(iVal, 2, false));
					}
					
					// Put separator where needed
					// No separator for the last field
					if(i < iLastIdx - 1) {
						switch((EPrecision)i) {
							case EPrecision.YEARS:
							case EPrecision.HOURS:
							case EPrecision.MINUTES: {
								writer.Write(":");
							} break;
								
							case EPrecision.DAYS: {
								writer.Write(" ");
							} break;
						}
					}
				} break;
			}
		}
		
		// Done! ^_^
		return writer.ToString();
	}
}
