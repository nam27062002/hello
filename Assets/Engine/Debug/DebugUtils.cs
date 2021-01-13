// DebugUtils.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
#define ENABLE_ASSERTS

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Testing class.
/// </summary>
public class DebugUtils {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	static private int MAX_ASSERTS = 1; // To prevent assert spamming

	/// <summary>
	/// Delegate signature for custom ToString method.
	/// </summary>
	/// <typeparam name="T">Type of formatted object.</typeparam>
	/// <param name="_obj">Target object.</param>
	/// <returns>String representation of _obj.</returns>
	public delegate string ToStringFunction<T>(T _obj);

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	static private int s_assertCount = 0;	// To prevent assert spamming
	
	//------------------------------------------------------------------//
	// STATIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Evaluate a boolean predicate and, if false, interrupt execution and throw an error.
	/// </summary>
	/// <returns>The result of evaluating the condition.</returns>
	/// <param name="_checkCondition">The condition to be evaluated. If false, the assert will be triggered.</param> 
	/// <param name="_message">The message to be displayed in case of error.</param>
	/// <param name="_contextObj">The object that triggered the asset (optional).</param>
	/// <param name="_softAssert">If true, execution won't be interrupted.</param>
	static public bool Assert(bool _checkCondition, string _message, Object _contextObj = null, bool _softAssert = false) {
		#if ENABLE_ASSERTS
		// Skip if we've reached the asserts limit
		if(s_assertCount >= MAX_ASSERTS) return _checkCondition;

		// Do we pass the check?
		if(!_checkCondition) {
			// No! Trace error into the console and display a dialog
			// Get information on the script that triggered the assert
			StackTrace myTrace = new StackTrace(true);
			StackFrame myFrame = myTrace.GetFrame(1);
			string sAssertInfo = _message + "\n" + "Filename: " + myFrame.GetFileName() + "\nMethod: " + myFrame.GetMethod() + "\nLine: " + myFrame.GetFileLineNumber();

			// If using the editor, show dialog
			if(!_softAssert) {
				#if UNITY_EDITOR
				if(UnityEditor.EditorUtility.DisplayDialog("ASSERT!", sAssertInfo, "Ok")) {
					// Ok pressed
					// Open editor at the file and line that triggered the assert
					UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(myFrame.GetFileName(), myFrame.GetFileLineNumber());
				}
				#else
				// [AOC] TODO!! Show in-game dialog
				// From FGOL:
				//FGOLNativeBinding.ShowMessageBox("Assert!", assertString + "\n" + assertInformation);
				#endif
			}

			// Trace to the console as well
			UnityEngine.Debug.LogError("ASSERT! " + sAssertInfo, _contextObj);

			// Break execution
			if(!_softAssert) {
				UnityEngine.Debug.Break();
			}

			// Update assert count (avoid assert spamming)
			if(Application.isEditor) {
				s_assertCount++;
			}
		}
		#endif

		return _checkCondition;
	}

	/// <summary>
	/// Evaluate a boolean predicate and, if false, throw an error in the console without interrupting the execution.
	/// </summary>
	/// <returns>The result of evaluating the condition.</returns>
	/// <param name="_checkCondition">The condition to be evaluated. If false, the assert will be triggered.</param> 
	/// <param name="_message">The message to be displayed in case of error.</param>
	/// <param name="_contextObj">The object that triggered the asset (optional).</param>
	static public bool SoftAssert(bool _checkCondition, string _message, Object _contextObj = null) {
		return DebugUtils.Assert(_checkCondition, _message, _contextObj, true);
	}

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogException(System.Exception exception)
    {
        UnityEngine.Debug.LogException(exception);
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogException(System.Exception exception, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogException(exception, context);
    }

    public static string ListToString<T>(List<T> list)
    {        
        string returnValue = "[";
        if (list != null)
        {
            int count = list.Count;
            for (int i = 0; i < count; i++)
            {
                returnValue += list[i].ToString();
                if (i < count - 1)
                {
                    returnValue += ",";
                }
            }
        }
        returnValue += "]";

        return returnValue;

    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string _text, UnityEngine.Object _context = null) {
		// Smart string memory usage
		StringBuilder ss = new StringBuilder();

		// Time tag
		System.TimeSpan t = System.TimeSpan.FromSeconds(Time.time);
		//string.Format("{0:D2}:{1:D2}.{2:D3}", t.TotalMinutes, t.Seconds, t.Milliseconds);
		ss.AppendFormat("{0:D2}:{1:D2}.{2:D3}", (int)t.TotalMinutes, t.Seconds, t.Milliseconds);
		//ss.Append(System.TimeSpan.FromSeconds(Time.time).ToString(@"mm\:ss\.fff"));
		string timeTag = ss.ToString();

		// Dim color for prefix
		ss.Length = 0;
		ss.Append("<color=white>");

		// Context name
		if(_context != null) {
			ss.Append(_context.name)
				.Append(": ");
		}

		// End color tag
		ss.Append("</color>");

		// Text
		ss.Append(_text);
			
		// Log!
		Debug.TaggedLog(timeTag, ss.ToString(), _context);
	}

	//------------------------------------------------------------------//
	// FORMATTING														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Pretty format a Rect object for debugging.
	/// </summary>
	/// <param name="_r">The object to be formatted</param>
	/// <returns>A string representing the given object.</returns>
	public static string RectToString(Rect _r) {
		return FloatToString(_r.xMin) + ", " + FloatToString(_r.xMax);
	}

	/// <summary>
	/// Pretty format a float value for debugging.
	/// </summary>
	/// <param name="_value">The value to be formatted</param>
	/// <returns>A string representing the given value.</returns>
	public static string FloatToString(float _value) {
		return string.Format("{0,4:D4}", (int)_value);
	}

	/// <summary>
	/// Pretty format a list for debugging.
	/// </summary>
	/// <typeparam name="T">List type.</typeparam>
	/// <param name="_list">The list to be formatted.</param>
	/// <param name="_multiline">Do a line for each value in the list?</param>
	/// <param name="_customToStringFunction">Optional custom function to format objects as string.</param>
	/// <returns>A string representing the list.</returns>
	public static string ListToString<T>(List<T> _list, bool _multiline = false, ToStringFunction<T> _customToStringFunction = null) {
		// Aux vars
		string str = "";

		// Opening bracket
		str += "[";

		// Push values
		for(int i = 0; i < _list.Count; ++i) {
			// Multiline?
			if(_multiline) {
				str += "\n\t";
			}

			// Push value! Custom formatting?
			if(_customToStringFunction != null) {
				str += _customToStringFunction(_list[i]);
			} else {
				str += _list[i].ToString();
			}

			// Item separator - skip last item
			if(i < _list.Count - 1) {
				str += ",";
				if(!_multiline) str += " ";
			}
		}

		// Closing bracket
		if(_multiline) str += "\n";
		str += "]";

		// Done!
		return str;
	}

	/// <summary>
	/// Pretty format an array for debugging.
	/// </summary>
	/// <typeparam name="T">List type.</typeparam>
	/// <param name="_array">The list to be formatted.</param>
	/// <param name="_multiline">Do a line for each value in the list?</param>
	/// <param name="_customToStringFunction">Optional custom function to format objects as string.</param>
	/// <returns>A string representing the list.</returns>
	public static string ArrayToString<T>(T[] _array, bool _multiline = false, ToStringFunction<T> _customToStringFunction = null) {
		return ListToString(_array.ToList(), _multiline, _customToStringFunction);
	}
}
