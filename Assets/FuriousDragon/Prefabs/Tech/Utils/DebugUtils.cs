// DebugUtils.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------

#define ENABLE_ASSERTS

using UnityEngine;
using System.Collections;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Testing class.
/// </summary>
public class DebugUtils {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	static private int MAX_ASSERTS = 1;	// To prevent assert spamming
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	static private int mAssertCount = 0;	// To prevent assert spamming
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	static public void Assert(bool _bCheckCondition, string _sMessage) {
		#if ENABLE_ASSERTS
		// Skip if we've reached the asserts limit
		if(mAssertCount >= MAX_ASSERTS) return;

		// Do we pass the check?
		if(!_bCheckCondition) {
			// No! Trace error into the console and display a dialog
			// Get information on the script that triggered the assert
			StackTrace myTrace = new StackTrace(true);
			StackFrame myFrame = myTrace.GetFrame(1);
			string sAssertInfo = "Filename: " + myFrame.GetFileName() + "\nMethod: " + myFrame.GetMethod() + "\nLine: " + myFrame.GetFileLineNumber();

			// If using the editor, show dialog
			#if UNITY_EDITOR
			if(UnityEditor.EditorUtility.DisplayDialog("ASSERT!", _sMessage + "\n" + sAssertInfo, "Ok")) {
				// Ok pressed
				// Open editor at the file and line that triggered the assert
				UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(myFrame.GetFileName(), myFrame.GetFileLineNumber());
				UnityEngine.Debug.Log(sAssertInfo);
			}
			#else
			// [AOC] TODO!! Show in-game dialog
			// From FGOL:
			//FGOLNativeBinding.ShowMessageBox("Assert!", assertString + "\n" + assertInformation);
			#endif

			// Break execution
			UnityEngine.Debug.Break();

			// Update assert count (avoid assert spamming)
			if(Application.isEditor) {
				mAssertCount++;
			}
		}
		#endif
	}
	#endregion
}
#endregion
