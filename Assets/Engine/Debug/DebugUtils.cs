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
	static private int MAX_ASSERTS = 1;	// To prevent assert spamming

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
}
