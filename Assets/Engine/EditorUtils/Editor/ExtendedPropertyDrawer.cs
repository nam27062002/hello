// ExtendedPropertyDrawer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Extension of Unity's property drawer, adding helpers to keep position tracking and other utils.
/// Inherit from this class rather than PropertyDrawer directly and use the OnGUIImpl() method
/// instead of the OnGUI().
/// </summary>
public abstract class ExtendedPropertyDrawer : PropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Aux vars
	protected Rect m_pos = new Rect();
	protected int m_indentLevelBackup = 0;
	protected SerializedProperty m_rootProperty = null;
	protected string m_currentKey = "";

	// There is only one instance of the editor for all properties of the same type 
	// in the same component, so we must store the individual heights of each one
	// of them since they might be different.
	// We will use the properties path as key, since it should be unique and invariable.
	protected Dictionary<string, float> m_heights = new Dictionary<string, float>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor
	/// </summary>
	public ExtendedPropertyDrawer() {
		// Nothing to do
	}

	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// </summary>
	/// <param name="_position">The area in the inspector assigned to draw this property.</param>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override public sealed void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {	// [AOC] Add the "sealed" qualifier to avoid heirs overriding it
		// Aux vars
		m_pos = _position;
		m_indentLevelBackup = EditorGUI.indentLevel;
		m_rootProperty = _property.Copy();
		m_currentKey = _property.propertyPath;

		// Reset property height
		SetHeight(m_currentKey, 0f);

		// If using a custom label, get it now
		GUIContent customLabel = GetLabel(_property, _label);

		// Let's do it
		EditorGUI.BeginProperty(_position, customLabel, _property);

		// Let heir do the hard work :D
		OnGUIImpl(_property, customLabel);
		
		// If total height doesn't match the height we were given to draw the property, we need to force a repaint to adjust to the new height
		if(GetHeight(m_currentKey) != _position.height) {
			EditorUtility.SetDirty(_property.serializedObject.targetObject);
		}

		// Done!
		EditorGUI.EndProperty();
		EditorGUI.indentLevel = m_indentLevelBackup;
	}
	
	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override public sealed float GetPropertyHeight(SerializedProperty _property, GUIContent _label) {
		// Look for a pre-calculated height for this property, will return default value if not found
		return GetHeight(_property.propertyPath);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Advance position cursor a given amount of pixels, optionally adding some extra margin. 
	/// If given amount is 0, position will be advanced using its current height value.
	/// The total height of the drawer will also be updated.
	/// Should be called after drawing each property so the next one is placed right below it.
	/// </summary>
	/// <param name="_amount">The amount of pixels to advance.</param>
	/// <param name="_margin">The extra margin to be added.</param>
	protected void AdvancePos(float _amount = 0f, float _margin = 0f) {
		// If default parameter, use current pos height instead
		//Debug.Log(_amount);
		if(_amount == 0f) _amount = m_pos.height;

		// Increase both position cursor and total height addition
		m_pos.y += _amount + _margin;
		SetHeight(m_currentKey, GetHeight(m_currentKey) + _amount + _margin);
	}

	/// <summary>
	/// Draw a property using its default inspector drawer and automatically 
	/// advance position.
	/// </summary>
	/// <param name="_property">The property to be drawn.</param>
	protected void DrawAndAdvance(SerializedProperty _property) {
		m_pos.height = EditorGUI.GetPropertyHeight(_property);
		EditorGUI.PropertyField(m_pos, _property, true);
		AdvancePos();
	}

	/// <summary>
	/// Draw an indented separator and advance position.
	/// </summary>
	protected void DrawSeparatorAndAdvance() {
		Rect indentedPos = EditorGUI.IndentedRect(m_pos);
		m_pos.height = EditorGUILayoutExt.Separator(indentedPos, new SeparatorAttribute());
		AdvancePos();
	}

	/// <summary>
	/// Get the stored height of a property in the dictionary.
	/// If not found, a new entry is added with the default value.
	/// </summary>
	/// <returns>The height of the given property.</returns>
	/// <param name="_key">The id of property whose height value we want. Should be the SerializedProperty.propertyPath value.</param>
	private float GetHeight(string _key) {
		if(!m_heights.ContainsKey(_key)) {
			m_heights.Add(_key, 0f);
		}
		return m_heights[_key];
	}

	/// <summary>
	/// Store the height of a property in the dictionary.
	/// If not found, a new entry is added with the given value.
	/// </summary>
	/// <param name="_key">The id of property whose height value we want. Should be the SerializedProperty.propertyPath value.</param>
	/// <param name="_height">The new height value.</param>
	private void SetHeight(string _key, float _height) {
		if(m_heights.ContainsKey(_key)) {
			m_heights[_key] = _height;
		} else {
			m_heights.Add(_key, 0f);
		}
	}

	//------------------------------------------------------------------//
	// VIRTUAL AND ABSTRACT METHODS										//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	protected abstract void OnGUIImpl(SerializedProperty _property, GUIContent _label);

	/// <summary>
	/// Optionally override to give a custom label for this property field.
	/// </summary>
	/// <returns>The new label for this property.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_defaultLabel">The default label for this property.</param>
	protected virtual GUIContent GetLabel(SerializedProperty _property, GUIContent _defaultLabel) {
		return _defaultLabel;
	}
}