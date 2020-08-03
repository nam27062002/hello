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
	// STATIC															//
	//------------------------------------------------------------------//
	static StringWriter m_writer = new StringWriter();
    static ulong[] precisionValues = new ulong[(int)EPrecision.COUNT];
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
		WORDS,							// 1 year 2 days 3 hours 0 minutes 5 seconds
		WORDS_WITHOUT_0_VALUES,			// 1 year 2 days 3 hours 5 seconds
		ABBREVIATIONS,					// 1y 2d 3h 0m 5s
		ABBREVIATIONS_WITHOUT_0_VALUES,	// 1y 2d 3h 5s
		DIGITS,							// 1:03 2:03:04
		DIGITS_0_PADDING				// 0001:03 02:03:04
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

    private static readonly int[] SECONDS_IN_PRECISION_INVERTED = {
        1,				// secs
        60,				// mins
		60*60,			// hours
		60*60*24,		// days
		60 *60*24*365	// years
	};

    //------------------------------------------------------------------//
    // CONVERSION METHODS												//
    //------------------------------------------------------------------//
    /// <summary>
    /// Retrieve the unix timestamp (milliseconds/seconds elapsed since 00:00:00 1 January 1970) given a DateTime object.
    /// Use DateTime.UtcNow to get current unix timestamp.
    /// <see cref="https://en.wikipedia.org/wiki/Unix_time"/>
    /// </summary>
    /// <returns>The unix timestamp corresponding to the given date.</returns>
    /// <param name="_date">The date to be converted.</param>
    /// <param name="_millis">Timestamp represented in millis or seconds?</param>
    public static long DateToTimestamp(DateTime _date, bool _millis = true) {
		TimeSpan ts = _date.Subtract(new DateTime(1970, 1, 1));
		if(_millis) {
			return (long)ts.TotalMilliseconds;
		} else {
			return (long)ts.TotalSeconds;
		}
	}

	/// <summary>
	/// Parse the given unix timestamp (milliseconds/seconds elapsed since 00:00:00 1 January 1970) into a DateTime object.
	/// <see cref="https://en.wikipedia.org/wiki/Unix_time"/>
	/// </summary>
	/// <returns>The date corresponding to the given unix timestamp.</returns>
	/// <param name="_timestamp">The unix timestamp to be parsed.</param>
	/// <param name="_millis">Timestamp represented in millis or seconds?</param>
	public static DateTime TimestampToDate(long _timestamp, bool _millis = true) {
		double d = _millis ? _timestamp / 1000.0 : (double)_timestamp;
		return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(d);
	}

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

		// Clean
		m_writer.GetStringBuilder().Length = 0;

		// If the amount was originally negative, insert negative sign
		if(isNegative) m_writer.Write("-");

		// Do the rest
		int addedFieldsCount = 0;
		for(int i = firstIdx; i < lastIdx; i++) {
			// Get value
			val = precisionValues[i];
			
			// Do the formatting
			switch(_format) {
				case EFormat.WORDS:
				case EFormat.WORDS_WITHOUT_0_VALUES:
				case EFormat.ABBREVIATIONS:
				case EFormat.ABBREVIATIONS_WITHOUT_0_VALUES: {
					// Special case if not including 0 values
					if(_format == EFormat.WORDS_WITHOUT_0_VALUES || _format == EFormat.ABBREVIATIONS_WITHOUT_0_VALUES) {
						// Is value 0?
						if(val == 0) {
							// Skip, unless it's the only field standing or forced
							if(!(i == lastIdx - 1 && addedFieldsCount == 0) && !_forcePrecision) {
								continue;
							}
						}
					}
					
					// Insert space if not the first field
					if(i != firstIdx) {
						m_writer.Write(" ");
					}
					
					// Insert field value, properly formatted
					m_writer.Write(StringUtils.FormatNumber(val));

					// Increase counter
					addedFieldsCount++;
					
					// Insert field name, abbreviation, singular or plural
					if(_format == EFormat.ABBREVIATIONS || _format == EFormat.ABBREVIATIONS_WITHOUT_0_VALUES) {
						m_writer.Write(LocalizationManager.SharedInstance.Localize(TIDS_ABBREVIATED[i]));
					} else if(val == 1) {
						m_writer.Write(" ");
                        m_writer.Write(LocalizationManager.SharedInstance.Localize(TIDS_SINGULAR[i]));
					} else {
						m_writer.Write(" ");
                        m_writer.Write(LocalizationManager.SharedInstance.Localize(TIDS_PLURAL[i]));
					}
				} break;
					
				case EFormat.DIGITS:
				case EFormat.DIGITS_0_PADDING: {
					// Specification says "YYYY:DD HH:MM:SS"
					bool zeroPadding = true;
					if(_format == EFormat.DIGITS) {
						// No zero-padding on the first field
						zeroPadding = i > firstIdx;
					}

					// Put value properly formatted
					if(i == (int)EPrecision.YEARS) {
						m_writer.Write(StringUtils.FormatNumber(val, zeroPadding ? 4 : 0, false));
					} else {
						m_writer.Write(StringUtils.FormatNumber(val, zeroPadding ? 2 : 0, false));
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
								m_writer.Write(":");
							} break;
								
							case EPrecision.DAYS: {
								m_writer.Write(" ");
							} break;
						}
					}
				} break;
			}
		}
		
		// Done! ^_^
		return m_writer.ToString();
	}


    /// <summary>
	/// This method will take and amount of seconds, separate it in units (secs, mins, hours...), take the last
    /// specified units discarding the rest, and will convert it back to a total amount of seconds.
    /// Will use this to round up the amount of seconds in the missions, so when the numer is formatted afterwards
    /// it wont show a lot of units.
    /// I.e: 3661 secs (1h 1min 1sec) can be rounded to 3600 secs (1h) if we just want to show 1 unit
    /// or can be rounded to 3660 (1h 1min) if we want to show 2 unit
	/// </summary>
	/// <returns>An amount of seconds after applying the round up.</returns>
	/// <param name="_seconds">The amount of seconds to be formatted.</param>
	/// <param name="_amountUnits">The amount of units we will use to round up the timestamp.</param>
    public static long RoundSeconds (long _seconds, int _amountUnits)
    {
        if (_amountUnits <= 0 || _seconds <= 0)
            return 0;

        // 1. Break up in smaller units
        TimeSpan timeSpan = TimeSpan.FromSeconds(_seconds);
        int[] units = { timeSpan.Seconds, timeSpan.Minutes, timeSpan.Hours, timeSpan.Days};

        // 2. Find the last (non zero) element
        int lastElement = -1;
        int pointer = units.Length - 1;

        while (pointer >=0 && lastElement == -1)
        {
            if (units [pointer] > 0)
            {
                lastElement = pointer;
            }

            pointer--;
        }

        if (lastElement == -1)
            return 0;


        // 3. Get the last N elements and discard the rest
        int resultSecs = 0;
        int firstElement = lastElement - (_amountUnits - 1);
        for (int i = lastElement; i >= firstElement ; i--)
        {
            if (i < 0) {
                break;
            }

            resultSecs += units[i] * SECONDS_IN_PRECISION_INVERTED[i];

        }
        
        // Return the total amount of seconds
        return resultSecs;        

    }
}
