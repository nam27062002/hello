// MenuDragonShowConditionallyEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[CustomEditor(typeof(MenuDragonShowConditionally))]
public class MenuDragonShowConditionallyEditor : Editor {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly float COLUMN_WIDTH = 75f;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	public override void OnInspectorGUI() {
		// Aux vars
		MenuDragonShowConditionally targetCast = target as MenuDragonShowConditionally;

		// Dragons matrix
		EditorGUILayout.BeginVertical(); {
			// Header Row
			EditorGUILayout.BeginHorizontal(); {
				GUILayout.Space(104);	// Approx enough room for the label
				
				EditorGUILayout.BeginHorizontal(EditorStyles.helpBox); {
					string[] labels = new string[] {"Locked", "Available", "Owned"};
					for(int j = 0; j < 3; j++) {
						// Toggle - Apply to all dragons if changed, show mixed value if different values between dragons
						EditorGUI.BeginChangeCheck();
						for(int i = 1; i < targetCast.m_showIf.GetLength(0); i++) {	// Start at second dragon
							// If at least one value is different from the previous one, show mixed value
							if(targetCast.m_showIf[i, j] != targetCast.m_showIf[i-1, j]) {
								EditorGUI.showMixedValue = true;
								break;
							}
						}

						// Show toggle - showMixedValue overrides given value
						// [AOC] For some unknown reason showMixedValue doesn't work automatically. As a workaround, use "ToggleMixed" GUIStyle when required.
						bool toggled = true;
						if(EditorGUI.showMixedValue) {
							toggled = GUILayout.Toggle(targetCast.m_showIf[0, j], labels[j], new GUIStyle("ToggleMixed"), GUILayout.Width(COLUMN_WIDTH));
						} else {
							toggled = GUILayout.Toggle(targetCast.m_showIf[0, j], labels[j], GUILayout.Width(COLUMN_WIDTH));
						}

						// If changed, apply new value to all dragons
						if(EditorGUI.EndChangeCheck()) {
							for(int i = 0; i < targetCast.m_showIf.GetLength(0); i++) {
								targetCast.m_showIf[i, j] = toggled;
							}
						}
						EditorGUI.showMixedValue = false;
					}
				} EditorGUILayoutExt.EndHorizontalSafe();

				// Compact view
				GUILayout.FlexibleSpace();
			} EditorGUILayoutExt.EndHorizontalSafe();
			
			// Dragon Rows
			for(int i = 0; i < targetCast.m_showIf.GetLength(0); i++) {
				EditorGUILayout.BeginHorizontal(); {
					GUILayout.Label(((DragonId)i).ToString(), GUILayout.Width(100));
					for(int j = 0; j < targetCast.m_showIf.GetLength(1); j++) {
						targetCast.m_showIf[i, j] = GUILayout.Toggle(targetCast.m_showIf[i, j], "", GUILayout.Width(COLUMN_WIDTH));
					}

					// Compact view
					GUILayout.FlexibleSpace();
				} EditorGUILayoutExt.EndHorizontalSafe();
			}
		} EditorGUILayoutExt.EndVerticalSafe();
	}
}