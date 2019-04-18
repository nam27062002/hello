// ShareScreenDragonEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ShareScreenDragon class.
/// </summary>
[CustomEditor(typeof(ShareScreenDragon), true)]	// True to be used by heir classes as well
[CanEditMultipleObjects]
public class ShareScreenDragonEditor : Editor {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static readonly Vector2 BUTTON_SIZE = new Vector2(20f, 20f);

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Casted target object
	private ShareScreenDragon m_targetShareScreenDragon = null;

	// Important properties
	private SerializedProperty m_dragonPosesProp = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The editor has been enabled - target object selected.
	/// </summary>
	private void OnEnable() {
		// Get target object
		m_targetShareScreenDragon = target as ShareScreenDragon;

		// Gather important properties
		m_dragonPosesProp = serializedObject.FindProperty("m_dragonPoses");
	}

	/// <summary>
	/// The editor has been disabled - target object unselected.
	/// </summary>
	private void OnDisable() {
		// Clear target object
		m_targetShareScreenDragon = null;

		// Clear properties
		m_dragonPosesProp = null;
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
		p.Next(true);   // To get first element
		do {
			// Properties requiring special treatment
			// Unity's "script" property
			if(p.name == "m_Script") {
				// Draw the property, disabled
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.PropertyField(p, true);
				EditorGUI.EndDisabledGroup();
			}

			// Dragon poses list
			else if(p.name == m_dragonPosesProp.name) {
				// Aux vars
				int toRemove = -1;
				int toMoveUp = -1;
				int toMoveDown = -1;

				// Group in a foldout
				EditorGUILayoutExt.Separator();
				m_dragonPosesProp.isExpanded = EditorGUILayout.Foldout(m_dragonPosesProp.isExpanded, m_dragonPosesProp.displayName);
				if(m_dragonPosesProp.isExpanded) {
					// Indent in
					EditorGUI.indentLevel++;

					// Show them using the sku as label
					for(int i = 0; i < m_dragonPosesProp.arraySize; i++) {
						// Aux vars
						SerializedProperty poseProp = m_dragonPosesProp.GetArrayElementAtIndex(i);
						SerializedProperty skuProp = poseProp.FindPropertyRelative("sku");

						// Horizontal layout with a preview button
						EditorGUILayout.BeginHorizontal();
						{
							GUILayout.Space(EditorGUI.indentLevel * 20f);

							// Buttons
							EditorGUILayout.BeginHorizontal(GUILayout.Width(BUTTON_SIZE.x * 4), GUILayout.ExpandWidth(false));
							{
								// Reload preview button
								GUI.color = Colors.skyBlue;
								if(GUILayout.Button("*", GUILayout.Width(BUTTON_SIZE.x), GUILayout.Height(BUTTON_SIZE.y))) {
									// Reload dragon preview using this dragon
									m_targetShareScreenDragon.dragonLoader.LoadDragon(skuProp.stringValue, IDragonData.GetDefaultDisguise(skuProp.stringValue).sku, true);
								}
								GUI.color = Colors.white;

								// Move buttons
								EditorGUI.BeginDisabledGroup(i == 0);
								if(GUILayout.Button("▲", GUILayout.Width(BUTTON_SIZE.x), GUILayout.Height(BUTTON_SIZE.y))) {
									toMoveUp = i;
								}
								EditorGUI.EndDisabledGroup();

								GUILayout.Space(-4f);	// We want buttons closer -_-

								EditorGUI.BeginDisabledGroup(i == m_dragonPosesProp.arraySize - 1);
								if(GUILayout.Button("▼", GUILayout.Width(BUTTON_SIZE.x), GUILayout.Height(BUTTON_SIZE.y))) {
									toMoveDown = i;
								}
								EditorGUI.EndDisabledGroup();

								GUILayout.Space(-4f);   // We want buttons closer -_-

								// Remove button
								GUI.color = Colors.red;
								if(GUILayout.Button("X", GUILayout.Width(BUTTON_SIZE.x), GUILayout.Height(BUTTON_SIZE.y))) {
									toRemove = i;
								}
								GUI.color = Colors.white;
							}
							EditorGUILayout.EndHorizontal();

							// Detect changes
							EditorGUI.BeginChangeCheck();

							// Use default drawer, using dragon's sku as label
							EditorGUILayout.PropertyField(poseProp, new GUIContent(skuProp.stringValue), true);

							// If some value has chaned, apply!
							if(EditorGUI.EndChangeCheck()) {
								// Don't do shit if dragon loader is not initialized
								if(m_targetShareScreenDragon.dragonLoader == null) {
									Debug.LogError(Colors.red.Tag("ERROR: dragon loader not initialized!"));
									continue;
								}

								// Position and scale
								Transform t = m_targetShareScreenDragon.dragonLoader.transform;
								t.localPosition = poseProp.FindPropertyRelative("offset").vector3Value;
								t.SetLocalScale(poseProp.FindPropertyRelative("scale").floatValue);

								// Animation frame: only if we have a valid dragon loaded!
								if(m_targetShareScreenDragon.dragonLoader.dragonInstance != null) {
									// Start the animation at a random frame (usually first frame looks shitty :s)
									Animator anim = m_targetShareScreenDragon.dragonLoader.dragonInstance.animator;
									AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);  //could replace 0 by any other animation layer index

									// Specific frame
									AnimatorClipInfo clipInfo = anim.GetCurrentAnimatorClipInfo(0)[0];
									int numFrames = (int)(clipInfo.clip.length * clipInfo.clip.frameRate);
									int targetFrame = poseProp.FindPropertyRelative("animationFrame").intValue;
									anim.Play(state.fullPathHash, -1, Mathf.InverseLerp(0, numFrames, targetFrame));  // Go to frame X
									anim.speed = 0;
								}
							}
						}
						EditorGUILayout.EndHorizontal();
					}

					// Perform operations now, outside the loop
					if(toRemove >= 0) {
						m_dragonPosesProp.DeleteArrayElementAtIndex(toRemove);
					} else if(toMoveUp >= 0) {
						m_dragonPosesProp.MoveArrayElement(toMoveUp, toMoveUp - 1);
					} else if(toMoveDown >= 0) {
						m_dragonPosesProp.MoveArrayElement(toMoveDown, toMoveDown + 1);
					}

					// Add entry button
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(EditorGUI.indentLevel * 20f);
					GUI.color = Colors.paleGreen;
					if(GUILayout.Button("+", GUILayout.Width(50f), GUILayout.Height(25f))) {
						m_dragonPosesProp.arraySize = m_dragonPosesProp.arraySize + 1;
					}
					GUI.color = Colors.white;
					EditorGUILayout.EndHorizontal();

					// Indent out
					EditorGUI.indentLevel--;
				}

				EditorGUILayout.Space();

				// Unload preview button
				EditorGUI.BeginDisabledGroup(m_targetShareScreenDragon.dragonLoader != null && m_targetShareScreenDragon.dragonLoader.dragonInstance == null);
				GUI.color = Colors.coral;
				if(GUILayout.Button("DELETE PREVIEW", GUILayout.Height(25f))) {
					m_targetShareScreenDragon.dragonLoader.UnloadDragon();
				}
				GUI.color = Colors.white;
				EditorGUI.EndDisabledGroup();

				// Dump Button
				if(GUILayout.Button("LOG", GUILayout.Height(25f))) {
					m_targetShareScreenDragon.LogPoses();
				}
			}

			// Properties we don't want to show
			else if(p.name == "m_ObjectHideFlags") {
				// Do nothing
			}

			// Default property display
			else {
				EditorGUILayout.PropertyField(p, true);
			}
		} while(p.NextVisible(false));      // Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)

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