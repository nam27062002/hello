// PopupDailyRewardsEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the PopupDailyRewards class.
/// </summary>
[CustomEditor(typeof(PopupDailyRewards), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class PopupDailyRewardsEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private int DEBUG_rewardIdx {
		get { return EditorPrefs.GetInt("PopupDailyRewardsEditor.DEBUG_rewardIdx", 0); }
		set { EditorPrefs.SetInt("PopupDailyRewardsEditor.DEBUG_rewardIdx", value); }
	}

	private bool DEBUG_canCollect {
		get { return Prefs.GetBoolEditor("PopupDailyRewardsEditor.DEBUG_canCollect", true); }
		set { Prefs.SetBoolEditor("PopupDailyRewardsEditor.DEBUG_canCollect", value); }
	}

	private bool DEBUG_canDouble {
		get { return Prefs.GetBoolEditor("PopupDailyRewardsEditor.DEBUG_canDouble", true); }
		set { Prefs.SetBoolEditor("PopupDailyRewardsEditor.DEBUG_canDouble", value); }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Use default inspector
		DrawDefaultInspector();

		// Add tools to simulate different states
		EditorGUILayoutExt.Separator("Debug Tools (Edit mode only)");

		// Not in play mode!
		EditorGUI.BeginDisabledGroup(Application.isPlaying); {
			// Choose debug day
			DEBUG_rewardIdx = EditorGUILayout.IntSlider("Reward Idx", DEBUG_rewardIdx + 1, 1, DailyRewardsSequence.SEQUENCE_SIZE) - 1;  // [0..6], show [1..7]

			// Can collect or cooldown?
			DEBUG_canCollect = EditorGUILayout.Toggle("Can Collect?", DEBUG_canCollect);

			// Can double? (never if we can't collect)
			EditorGUI.BeginDisabledGroup(!DEBUG_canCollect); {
				DEBUG_canDouble = EditorGUILayout.Toggle("Can Double?", DEBUG_canDouble);
			} EditorGUI.EndDisabledGroup();

			// Apply button
			if(GUILayout.Button("Apply!", GUILayout.Height(50f))) {
				// Do it!
				(target as PopupDailyRewards).DEBUG_Init(DEBUG_rewardIdx, DEBUG_canCollect, DEBUG_canDouble);
			}
		} EditorGUI.EndDisabledGroup();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}