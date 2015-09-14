// DebugUtils.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
#define ENABLE_ASSERTS

using UnityEngine;
using System.Collections;
using System.Diagnostics;

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
	/// <param name="_softAssert">If true, execution won't be interrupted.</param>
	static public bool Assert(bool _checkCondition, string _message, bool _softAssert = false) {
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
			UnityEngine.Debug.LogError("ASSERT! " + sAssertInfo);

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
	static public bool SoftAssert(bool _checkCondition, string _message) {
		return DebugUtils.Assert(_checkCondition, _message);
	}
}
