// TransitionEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the Camera Test Transition class.
/// </summary>
[CustomPropertyDrawer(typeof(Transition))]
public class TransitionEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TransitionEditor() {
		// Nothing to do
	}

	//------------------------------------------------------------------//
	// PARENT IMPLEMENTATION											//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Foldout widget + destination screen
		m_pos.height = EditorStyles.largeLabel.lineHeight;
		Rect foldLabelPos = new Rect(m_pos);
		foldLabelPos.width = 65f;
		_property.isExpanded = EditorGUI.Foldout(foldLabelPos, _property.isExpanded, "To");

		int indentBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		Rect popupPos = new Rect(m_pos);
		popupPos.x = foldLabelPos.xMax;
		popupPos.width = 150f;

		SerializedProperty destinationProp = m_rootProperty.FindPropertyRelative("destination");
		//destinationProp.enumValueIndex = EditorGUI.EnumPopup(popupPos, (MenuScreen)destinationProp.enumValueIndex);
		EditorGUIUtility.labelWidth = 1f;
		EditorGUI.PropertyField(popupPos, destinationProp, GUIContent.none, true);
		EditorGUIUtility.labelWidth = 0f;

		EditorGUI.indentLevel = indentBackup;

		AdvancePos();

		// If unfolded, draw children
		if(_property.isExpanded) {
			// Aux vars
			int baseDepth = _property.depth;
			int processedProps = 0;
			SerializedProperty showOverlayProp = m_rootProperty.FindPropertyRelative("showOverlay");
			bool showOverlay = showOverlayProp.boolValue;

			// Set indentation
			EditorGUI.indentLevel++;

			// Iterate through all the children of the property
			bool loop = _property.Next(true);	// Enter to the first level of depth
			while(loop) {
				// Properties requiring special treatment

				// Duration: Enabled?
				if(_property.name == "overrideDuration") {
					// Don't show if using overlay
					if(!showOverlay) {
						// Prefix label
						Rect labelPos = new Rect(m_pos);
						labelPos.width = EditorGUIUtility.labelWidth;
						EditorGUI.PrefixLabel(labelPos, new GUIContent(_property.displayName));

						// Reset indent for the content
						indentBackup = EditorGUI.indentLevel;
						EditorGUI.indentLevel = 0;

						// Toggle
						float toggleWidth = EditorStyles.toggle.CalcSize(GUIContent.none).x;
						Rect togglePos = new Rect(m_pos);
						togglePos.x = labelPos.x + labelPos.width;
						togglePos.width = toggleWidth;
						_property.boolValue = EditorGUI.Toggle(togglePos, _property.boolValue);

						// Disable group and value selector
						EditorGUI.BeginDisabledGroup(!_property.boolValue); {
							Rect valuePos = new Rect(m_pos);
							valuePos.x = togglePos.x + togglePos.width;
							valuePos.width = m_pos.width - togglePos.width - labelPos.width;
							SerializedProperty valueProp = m_rootProperty.FindPropertyRelative("duration");
							EditorGUI.PropertyField(valuePos, valueProp, GUIContent.none, true);
						} EditorGUI.EndDisabledGroup();

						// Restore indentation and advance line
						EditorGUI.indentLevel = indentBackup;
						AdvancePos();
					}
				}

				// Duration: Enabled?
				else if(_property.name == "overrideEase") {
					// Don't show if using overlay
					if(!showOverlay) {
						// Prefix label
						Rect labelPos = new Rect(m_pos);
						labelPos.width = EditorGUIUtility.labelWidth;
						EditorGUI.PrefixLabel(labelPos, new GUIContent(_property.displayName));

						// Reset indent for the content
						indentBackup = EditorGUI.indentLevel;
						EditorGUI.indentLevel = 0;

						// Toggle
						float toggleWidth = EditorStyles.toggle.CalcSize(GUIContent.none).x;
						Rect togglePos = new Rect(m_pos);
						togglePos.x = labelPos.x + labelPos.width;
						togglePos.width = toggleWidth;
						_property.boolValue = EditorGUI.Toggle(togglePos, _property.boolValue);

						// Disable group and value selector
						EditorGUI.BeginDisabledGroup(!_property.boolValue); {
							Rect valuePos = new Rect(m_pos);
							valuePos.x = togglePos.x + togglePos.width;
							valuePos.width = m_pos.width - togglePos.width - labelPos.width;
							SerializedProperty valueProp = m_rootProperty.FindPropertyRelative("ease");

							// [AOC] Use enum field rather than property field because in this case we don't want to see the curve preview provided by the Ease custom property drawer
							//EditorGUI.PropertyField(valuePos, valueProp, GUIContent.none, true);
							valueProp.enumValueIndex = (int)(Ease)EditorGUI.EnumPopup(valuePos, GUIContent.none, (Ease)System.Enum.GetValues(typeof(Ease)).GetValue(valueProp.enumValueIndex));
						} EditorGUI.EndDisabledGroup();

						// Restore indentation and advance line
						EditorGUI.indentLevel = indentBackup;
						AdvancePos();
					}
				}

				// Path intial and final points: 
				// - Don't show if path not defined
				// - Show a list of points to choose
				else if((_property.name == "initialPathPoint"
				|| _property.name == "finalPathPoint")) {
					// Don't show if using overlay
					if(!showOverlay) {
						// Indented
						EditorGUI.indentLevel++;

						// Get path property
						SerializedProperty pathProp = m_rootProperty.FindPropertyRelative("path");

						// Don't display initial/final points if path is not assigned
						BezierCurve path = (BezierCurve)pathProp.objectReferenceValue;
						if(path != null) {
							// Let the player choose between all the named points in the curve
							int selectedIdx = -1;
							List<string> optionsList = new List<string>();
							for(int i = 0; i < path.points.Count; ++i) {
								// Skip points with no custom name
								if(!string.IsNullOrEmpty(path.points[i].name)) {
									// Add option
									optionsList.Add(path.points[i].name);

									// Is it the current selected value?
									if(string.Equals(path.points[i].name, _property.stringValue)) {
										selectedIdx = optionsList.Count - 1;
									}
								}
							}

							// If the curve has no named points, show an error message instead
							if(optionsList.Count == 0) {
								// Error message
								Rect pos = EditorGUI.PrefixLabel(m_pos, processedProps, new GUIContent(_property.displayName));
								EditorGUI.HelpBox(pos, "Selected curve has no NAMED control points!", MessageType.Error);
							} else {
								// Display the list and store new value
								m_pos.height = EditorStyles.popup.lineHeight + 5;   // [AOC] Default popup field height + some margin
								int newSelectedIdx = EditorGUI.Popup(m_pos, _property.displayName, selectedIdx, optionsList.ToArray());
								if(selectedIdx != newSelectedIdx) {
									_property.stringValue = optionsList[newSelectedIdx];
								}
							}
							AdvancePos();

							// Indented
							EditorGUI.indentLevel--;
						}
					}
				}

				// Path: remark that it's optional
				else if(_property.name == "path") {
					// Don't show if using overlay
					if(!showOverlay) {
						// Add comment
						// [AOC] Compute required height to draw the text using our custom box style with the current inspector window width
						GUIContent content = new GUIContent("If path is not defined, camera position will be linearly interpolated between snap points\n" +
														"Override duration to <= 0 to move instantly to target snap point.");
						m_pos.height = CustomEditorStyles.commentLabelLeft.CalcHeight(content, Screen.width - 35f);   // Screen.width gives us the size of the current inspector window. Unfortunately it doesn't compute the internal margins of the window, so try to compensate with a hardcoded value :P
						EditorGUI.LabelField(m_pos, content, CustomEditorStyles.commentLabelLeft);
						AdvancePos();

						// Draw path property
						DrawAndAdvance(_property, _property.displayName + " (optional)");
					}
				}

				// Properties to skip (already processed under other properties
				else if(_property.name == "destination"
					 || _property.name == "duration"
					 || _property.name == "ease") {
					// Do nothing
				}

				// Default
				else {
					DrawAndAdvance(_property);
				}

				// Move to next property
				// Only direct children, not grand-children (will be drawn by default if using the default EditorGUI.PropertyField)
				loop = _property.Next(false);
				processedProps++;

				// If within an array, Next() will give the next element of the array, which will 
				// already be drawn by itself afterwards, so we don't want it - check depth to prevent it
				if(loop) {
					if(_property.depth <= baseDepth) loop = false;
				}
			}

			// Restore indentation
			EditorGUI.indentLevel--;
		}
	}

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		SerializedProperty destProp = _property.FindPropertyRelative("destination");
		return new GUIContent("To " + destProp.enumNames[destProp.enumValueIndex]);
	}
}