// TweenSequenceElementEditor.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 06/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the TweenSequenceElement class.
/// </summary>
[CustomPropertyDrawer(typeof(TweenSequenceElement))]
public class TweenSequenceElementPropertyDrawer : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TweenSequenceElementPropertyDrawer() {
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
		// [AOC] HACK! When adding elements to an array, Unity doesn't respect default
		//		 values as defined in the class, which is a great inconvenience in 
		//		 this particular case. To work around this, reset property if the Ease
		//		 field hasn't been initialized.
		SerializedProperty easeProp = _property.FindPropertyRelative("ease");
		if(easeProp.enumValueIndex == (int)DG.Tweening.Ease.Unset) {
			// Only those properties whose default value is different from the type's default value
			_property.FindPropertyRelative("startTime").floatValue = 0f;
			_property.FindPropertyRelative("endTime").floatValue = 0.25f;
			_property.FindPropertyRelative("from").boolValue = true;
			_property.FindPropertyRelative("ease").enumValueIndex = (int)DG.Tweening.Ease.Linear;
			return;
		}

		// Useful properties
		SerializedProperty typeProp = _property.FindPropertyRelative("type");
		SerializedProperty targetProp = _property.FindPropertyRelative("target");

		// Aux vars
		m_pos.height = EditorGUIUtility.singleLineHeight;
		Rect pos = m_pos;
		float spaceWidth = 5f;
		bool isIdle = typeProp.enumValueIndex == (int)TweenSequenceElement.Type.IDLE;

		// Reset indentation and label size (no indentation within the widget, m_pos already contains global indentation)
		EditorGUIUtility.labelWidth = 80f;
		int indentLevelBackup = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		// Line 1: Foldout + Target + Type + Timing
		// Foldout
		pos.width = 5f;
		_property.isExpanded = EditorGUI.Foldout(pos, _property.isExpanded, GUIContent.none);

		// Target and type can't be changed while playing
		GUI.enabled = !Application.isPlaying;

		// Target
		pos.x += pos.width;
		pos.width = 90f;
		if(isIdle) {
			EditorGUI.LabelField(pos, "Idle");
		} else {
			// Show warning if target is missing
			if(targetProp.objectReferenceValue == null) {
				EditorGUI.LabelField(pos, "<color=red>!</color>", CustomEditorStyles.richLabel);
				pos.x += 10f;
				pos.width -= 10f;
			}
			EditorGUI.PropertyField(pos, targetProp, GUIContent.none, true);
		}

		// Type
		pos.x += pos.width;
		pos.width = 60f;
		EditorGUI.PropertyField(pos, typeProp, GUIContent.none);

		GUI.enabled = true;

		// Space
		pos.x += pos.width;
		pos.width = spaceWidth;

		// Timing
		SerializedProperty startTimeProp = _property.FindPropertyRelative("startTime");
		SerializedProperty endTimeProp = _property.FindPropertyRelative("endTime");
		float startTime = startTimeProp.floatValue;
		float endTime = endTimeProp.floatValue;

		float timeTextWidth = 30f;
		float timeSliderWidth = m_pos.x + m_pos.width - pos.x - 2 * timeTextWidth - 3 * spaceWidth;	// Flexible; Remaining space minus the 2 time text fields and the spaces in between

		// Start time text
		pos.x += pos.width;
		pos.width = timeTextWidth;
		startTime = EditorGUI.DelayedFloatField(pos, startTime);

		// Space
		pos.x += pos.width;
		pos.width = spaceWidth;

		// Slider
		pos.x += pos.width;
		pos.width = timeSliderWidth;
		EditorGUI.MinMaxSlider(pos, ref startTime, ref endTime, 0f, 1f);

		// Space
		pos.x += pos.width;
		pos.width = spaceWidth;

		// End time text
		pos.x += pos.width;
		pos.width = timeTextWidth;
		endTime = EditorGUI.DelayedFloatField(pos, endTime);

		// Space
		pos.x += pos.width;
		pos.width = spaceWidth;

		// Store new start and end time
		startTimeProp.floatValue = Mathf.Clamp(startTime, 0f, endTime);;
		endTimeProp.floatValue = Mathf.Clamp(endTime, startTime, 1f);

		// Next Line
		AdvancePos(m_pos.height, 2f);

		// Foldout group
		if(_property.isExpanded) {
			// Indent in
			m_pos.x += 30f;
			m_pos.width -= 30f;

			// Line 2: From flag + Target value
			if(!isIdle) {
				// From flag
				pos = m_pos;
				pos.width = 60f;
				SerializedProperty fromProp = _property.FindPropertyRelative("from");
				if(GUI.Button(pos, fromProp.boolValue ? "FROM" : "TO")) {
					fromProp.boolValue = !fromProp.boolValue;
				}

				// Target value, depends on type
				SerializedProperty valueProp = GetValueProperty(_property);
				if(valueProp != null) {
					pos.x += pos.width + 5f;
					pos.width = m_pos.width - pos.width - 5f;
					EditorGUI.PropertyField(pos, valueProp, GUIContent.none, true);
				}

				// Next line
				AdvancePos(m_pos.height, 2f);
			}

			// Line 3: Ease function
			if(!isIdle) DrawAndAdvance(_property, "ease", null);

			// Events: Group in a foldout
			m_pos.height = EditorGUIUtility.singleLineHeight;
			SerializedProperty onStartProp = _property.FindPropertyRelative("OnStart");
			onStartProp.isExpanded = EditorGUI.Foldout(m_pos, onStartProp.isExpanded, "Events");
			AdvancePos();
			if(onStartProp.isExpanded) {
				DrawAndAdvance(_property, "OnStart", null);
				AdvancePos(2f);
				DrawAndAdvance(_property, "OnEnd", null);
				AdvancePos(2f);
			}

			// Final space (only when unfolded)
			AdvancePos(2f);
		}

		// Restore indentation and label size
		EditorGUIUtility.labelWidth = 0f;
		EditorGUI.indentLevel = indentLevelBackup;
	}

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	override protected GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		return _defaultLabel;
	}

	/// <summary>
	/// Get the value property based on element's type.
	/// </summary>
	/// <returns>The property corresponding to the tween's value, based on element's type.</returns>
	/// <param name="_elementProp">Root tween sequence element property.</param>
	private SerializedProperty GetValueProperty(SerializedProperty _elementProp) {
		// Figure out type
		TweenSequenceElement.Type type = (TweenSequenceElement.Type)_elementProp.FindPropertyRelative("type").enumValueIndex;

		// Depending on type, find the right value property
		switch(type) {
			case TweenSequenceElement.Type.IDLE: return null;	// No value to tween
			case TweenSequenceElement.Type.FADE: return _elementProp.FindPropertyRelative("floatValue");	// float value
			default: 							 return _elementProp.FindPropertyRelative("vectorValue");	// Vector3 value
		}
		return null;
	}
}