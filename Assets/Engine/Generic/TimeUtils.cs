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
		"TID_GEN_TIME_YEARS_ABBR",	// EPrecision.YEARS
		"TID_GEN_TIME_DAYS_ABBR",	// EPrecision.DAYS
		"TID_GEN_TIME_HOURS_ABBR",	// EPrecision.HOURS
		"TID_GEN_TIME_MINUTES_ABBR",	// EPrecision.MINUTES
		"TID_GEN_TIME_SECONDS_ABBR"	// EPrecision.SECONDS
	};

	private static readonly string[] TIDS_SINGULAR = {
		"TID_GEN_TIME_YEAR",	// EPrecision.YEARS
		"TID_GEN_TIME_DAY",		// EPrecision.DAYS
		"TID_GEN_TIME_HOUR",	// EPrecision.HOURS
		"TID_GEN_TIME_MINUTE",	// EPrecision.MINUTES
		"TID_GEN_TIME_SECOND"	// EPrecision.SECONDS
	};

	private static readonly string[] TIDS_PLURAL = {
		"TID_GEN_TIME_YEARS",	// EPrecision.YEARS
		"TID_GEN_TIME_DAYS",	// EPrecision.DAYS
		"TID_GEN_TIME_HOURS",	// EPrecision.HOURS
		"TID_GEN_TIME_MINUTES",	// EPrecision.MINUTES
		"TID_GEN_TIME_SECONDS"	// EPrecision.SECONDS
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
	/// <param name="_forcePrecision">Force the given precision, showing left-side zeroes (e.g. "03:25" -> "00:03:25)"</param></param>
	public static string FormatTime(double _seconds, EFormat _format, int _numFields, EPrecision _precision = EPrecision.YEARS, bool _forcePrecision = false) {
		// 1. If seconds amount is negative, convert to positive to do all the maths and restore sign afterwards
		bool isNegative = false;
		if(_seconds < 0) {
			isNegative = true;
			_seconds *= -1;
		}
		
		// 2. Round up seconds
		ulong seconds = (ulong)(_seconds + 0.5f);
		
		// 3. Compute the amount of each precision field, starting with the defined precision
		ulong[] precisionValues = new ulong[(int)EPrecision.COUNT];
		for(int i = (int)_precision; i < (int)EPrecision.COUNT; i++) {
			precisionValues[i] = seconds/SECONDS_IN_PRECISION[i];
			seconds -= precisionValues[i] * SECONDS_IN_PRECISION[i];
		}
		
		// 4. Find first and last values to be used, combining precision, number of fields, and ignoring 0 values (unless precision is forced)
		// 4.1. First index: is precision forced?
		int firstIdx = (int)EPrecision.SECONDS;	// Default precision
		if(_forcePrecision) {
			// Yes!! Start at given precision, even if it's 0
			firstIdx = (int)_precision;
		} else {
			// No! Find first non-0 precision index starting at the given precision
			for(int i = (int)_precision; i < precisionValues.Length; i++) {
				if(precisionValues[i] != 0) {
					firstIdx = i;	// Store precision (first non-zero value, starting at given precision)
					break;	// Break loop
				}
			}
		}
		
		// 4.2. Last index: if enough fields, initial precision + _numFields, otherwise go up to max precision
		int lastIdx = Math.Min(firstIdx + _numFields, precisionValues.Length);
		
		// 5. Do the formatting depending on selected format
		ulong val;
		StringWriter writer = new StringWriter();

		// If the amount was originally negative, insert negative sign
		if(isNegative) writer.Write("-");

		// Do the rest
		int addedFieldsCount = 0;
		for(int i = firstIdx; i < lastIdx; i++) {
			// Get value
			val = precisionValues[i];
			
			// Do the formatting
			switch(_format) {
				case EFormat.WORDS:
				case EFormat.ABBREVIATIONS:
				case EFormat.ABBREVIATIONS_WITHOUT_0_VALUES: {
					// Special case if not including 0 values
					if(_format == EFormat.ABBREVIATIONS_WITHOUT_0_VALUES && val == 0) {
						// Skip if value is 0, unless it's the only field standing or forced
						if(!(i == lastIdx - 1 && addedFieldsCount == 0) && !_forcePrecision) {
							continue;
						}
					}
					
					// Insert space if not the first field
					if(i != firstIdx) {
						writer.Write(" ");
					}
					
					// Insert field value, properly formatted
					writer.Write(StringUtils.FormatNumber(val));

					// Increase counter
					addedFieldsCount++;
					
					// Insert field name, abbreviation, singular or plural
					if(_format == EFormat.ABBREVIATIONS || _format == EFormat.ABBREVIATIONS_WITHOUT_0_VALUES) {
						writer.Write(LocalizationManager.SharedInstance.Localize(TIDS_ABBREVIATED[i]));
					} else if(val == 1) {
						writer.Write(" ");
                        writer.Write(LocalizationManager.SharedInstance.Localize(TIDS_SINGULAR[i]));
					} else {
						writer.Write(" ");
                        writer.Write(LocalizationManager.SharedInstance.Localize(TIDS_PLURAL[i]));
					}
				} break;
					
				case EFormat.DIGITS: {
					// Specification says "YYYY:DD HH:MM:SS"
					// Put value properly formatted
					if(i == (int)EPrecision.YEARS) {
						writer.Write(StringUtils.FormatNumber(val, 4, false));
					} else {
						writer.Write(StringUtils.FormatNumber(val, 2, false));
					}

					// Increase counter
					addedFieldsCount++;
					
					// Put separator where needed
					// No separator for the last field
					if(i < lastIdx - 1) {
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
