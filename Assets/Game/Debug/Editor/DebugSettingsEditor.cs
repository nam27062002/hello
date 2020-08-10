// DebugSettingsEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/09/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the DebugSettings class.
/// </summary>
[CustomEditor(typeof(DebugSettings), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class DebugSettingsEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private DebugSettings m_targetDebugSettings = null;

	// Cached properties
	private SerializedProperty m_useDebugSafeAreaProp = null;
	private SerializedProperty m_useUnitySafeAreaProp = null;
	private SerializedProperty m_debugSafeAreaProp = null;
	private SerializedProperty m_simulatedSpecialDeviceProp = null;

	private SerializedProperty m_customizerTestEnabledProp = null;
	private SerializedProperty m_customizerTestConfigProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetDebugSettings = target as DebugSettings;

		// Cache some properties
		m_useDebugSafeAreaProp = serializedObject.FindProperty("m_useDebugSafeArea");
		m_useUnitySafeAreaProp = serializedObject.FindProperty("m_useUnitySafeArea");
		m_debugSafeAreaProp = serializedObject.FindProperty("m_debugSafeArea");
		m_simulatedSpecialDeviceProp = serializedObject.FindProperty("m_simulatedSpecialDevice");

		m_customizerTestEnabledProp = serializedObject.FindProperty("m_customizerPopupTestEnabled");
		m_customizerTestConfigProp = serializedObject.FindProperty("m_customizerPopupTestConfig");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetDebugSettings = null;

		// Clear cached properties
		m_useDebugSafeAreaProp = null;
		m_useUnitySafeAreaProp = null;
		m_debugSafeAreaProp = null;
		m_simulatedSpecialDeviceProp = null;

		m_customizerTestEnabledProp = null;
		m_customizerTestConfigProp = null;
	}

	/// <summary>
	/// Draw the inspector.
	/// </summary>
	public override void OnInspectorGUI() {
		// Instead of modifying script variables directly, it's advantageous to use the SerializedObject and 
		// SerializedProperty system to edit them, since this automatically handles private fields, multi-object 
		// editing, undo, and prefab overrides.

		// Update the serialized object - always do this in the beginning of OnInspectorGUI.
		serializedObject.Update();

		// Loop through all serialized properties and work with special ones
		SerializedProperty p = serializedObject.GetIterator();
		p.Next(true);	// To get first element
		do {
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				bool wasEnabled = GUI.enabled;
				GUI.enabled = false;
				EditorGUILayout.PropertyField(p, true);
				GUI.enabled = wasEnabled;
			}

			// Safe Area settings
			else if(p.name == m_useUnitySafeAreaProp.name) {
				// Draw "Use Unity Safe Area" toggle
				EditorGUILayout.PropertyField(m_useUnitySafeAreaProp, true);

				// Draw "Use Debug Safe Area" toggle
				EditorGUILayout.PropertyField(m_useDebugSafeAreaProp, true);

				// Debugging Enabled?
				EditorGUI.BeginDisabledGroup(!m_useDebugSafeAreaProp.boolValue); {
					// Indent in
					EditorGUI.indentLevel++;

					// If Unity Safe Area toggled, enable Unity-like debug safeArea
					EditorGUI.BeginDisabledGroup(!m_useUnitySafeAreaProp.boolValue); {
						EditorGUILayout.PropertyField(m_debugSafeAreaProp, true);
					} EditorGUI.EndDisabledGroup();

					// Otherwise enable simulated special device chooser
					EditorGUI.BeginDisabledGroup(m_useUnitySafeAreaProp.boolValue); {
						EditorGUILayout.PropertyField(m_simulatedSpecialDeviceProp, true);
					} EditorGUI.EndDisabledGroup();

					// Indent out
					EditorGUI.indentLevel--;
				} EditorGUI.EndDisabledGroup();
			}

			// Customizer popup test
			else if(p.name == m_customizerTestEnabledProp.name) {
				// Section separator
				EditorGUILayoutExt.Separator("Customizer Popup Test");

				// Draw "Customizer Popup Test Enabled" toggle
				EditorGUILayout.PropertyField(m_customizerTestEnabledProp, true);

				// Test Enabled?
				EditorGUI.BeginDisabledGroup(!m_customizerTestEnabledProp.boolValue); {
					// Indent in
					EditorGUI.indentLevel++;

					// Info Box
					EditorGUILayout.HelpBox(
						"Press 'C' in the menu to open the customizer popup\n" +
						"See https://confluence.ubisoft.com/display/ubm/%5BHD%5D+11.+Customizer+Popup+Actions \n" +
						"This is an offline simulation, not all parameters may work as real!",
						MessageType.Info,
						true
					);

					// Customizer test config property
					EditorGUILayout.PropertyField(m_customizerTestConfigProp, true);

					// Open customizer popup button
					GUILayout.BeginHorizontal();
					GUILayout.Space(EditorGUI.indentLevel * 10f);   // Cheap trick to simulate indentation on a GUILayout.Button
					GUI.color = Colors.paleGreen;
					if(GUILayout.Button("Open Customizer Popup", GUILayout.Height(40f))) {
						// Only while playing
						if(!Application.isPlaying) {
							EditorWindow.focusedWindow.ShowNotification(new GUIContent("Only while playing!"));
						}

						// Only in the menu...
						else if(InstanceManager.menuSceneController == null) {
							EditorWindow.focusedWindow.ShowNotification(new GUIContent("Only in the menu!"));
						}

						// ...and only in some screens
						else if(InstanceManager.menuSceneController.currentScreen != MenuScreen.PLAY
							 && InstanceManager.menuSceneController.currentScreen != MenuScreen.DRAGON_SELECTION) {
							EditorWindow.focusedWindow.ShowNotification(new GUIContent("Only in the PLAY or DRAGON_SELECTION screens!"));
						}

						// All checks passed!
						else {
							// Copied from MenuInterstitialPopupsController
							Calety.Customiser.CustomiserPopupConfig config = DebugSettings.CUSTOMIZER_POPUP_TEST_CONFIG;
							string popupPath = PopupCustomizer.PATH + "PF_PopupLayout_" + config.m_iLayout;
							PopupController pController = PopupManager.EnqueuePopup(popupPath);
							PopupCustomizer pCustomizer = pController.GetComponent<PopupCustomizer>();
							pCustomizer.InitFromConfig(config);
						}
					}
					GUI.color = Colors.white;
					GUILayout.EndHorizontal();

					// Indent out
					EditorGUI.indentLevel--;
				} EditorGUI.EndDisabledGroup();
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags" ||
					p.name == m_useDebugSafeAreaProp.name ||
			        p.name == m_useUnitySafeAreaProp.name ||
			        p.name == m_simulatedSpecialDeviceProp.name ||
			        p.name == m_debugSafeAreaProp.name ||
					p.name == m_customizerTestConfigProp.name) {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));		// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

		// Apply changes to the serialized object - always do this in the end of OnInspectorGUI.
		serializedObject.ApplyModifiedProperties();
	}

	/// <summary>
	/// The scene is being refreshed.
	/// </summary>
	public void OnSceneGUI() {
		// Scene-related stuff
	}
}