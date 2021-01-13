// ProbabilitySetEditor.cs
// 
// Created by Alger Ortín Castellví on 12/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom editor for the ProbabilitySet class.
/// </summary>
[CustomPropertyDrawer(typeof(ProbabilitySet))]
public class ProbabilitySetEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References to keep between methods
	// Will be null outside the OnGUIImpl call
	ProbabilitySet m_target = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ProbabilitySetEditor() {
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
		// Register undo
		Undo.RecordObject(_property.serializedObject.targetObject, "Probability Set");

		// Draw property label + foldout widget
		m_pos.height = EditorStyles.largeLabel.lineHeight;	// Advance pointer just the size of the label
		_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, _label);
		AdvancePos();

		// If unfolded, draw children
		if(_property.isExpanded) {
			// Indent in
			EditorGUI.indentLevel++;

			// Get the reference to both the set and the elements serialized property
			m_target = fieldInfo.GetValue(_property.serializedObject.targetObject) as ProbabilitySet;	// [AOC] Small trick from http://answers.unity3d.com/questions/425012/get-the-instance-the-serializedproperty-belongs-to.html

			// Elements
			DoElements();

			// Add/Remove buttons
			AdvancePos(5f);		// Spacing
			DoAddRemoveButtons();

			// Reset Buttons
			AdvancePos(5f);		// Spacing
			DoResetButtons();

			// Indent back out and make sure gui is enabled
			EditorGUI.indentLevel--;
			GUI.enabled = true;
		}

		// Make sure all changes are applied
		_property.serializedObject.ApplyModifiedProperties();
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

	//------------------------------------------------------------------//
	// INTERNAL DRAWERS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Draw the elements one by one.
	/// </summary>
	private void DoElements() {
		// Draw line by line
		bool locked;
		string label;
		float probability;
		for(int i = 0; i < m_target.numElements; i++) {
			// Aux
			m_pos.height = EditorStyles.numberField.lineHeight + EditorStyles.numberField.margin.vertical;
			Rect pos = new Rect(m_pos);

			// Lock?
			GUI.enabled = true;
			pos.width = 40;
			EditorGUI.BeginChangeCheck();
			locked = !EditorGUI.Toggle(pos, !m_target.IsLocked(i));	// [AOC] A little weird: what actually makes sense is that the toggle is marked when the element is not locked
			if(EditorGUI.EndChangeCheck()) m_target.SetLocked(i, locked);
			pos.x += pos.width;
			GUI.enabled = !locked;

			// [AOC] Temporarily remove indentation so following controls are next to each other
			int indentBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Label - Editable
			// [AOC] Draw a textfield using the label style
			pos.width = 120;
			EditorGUI.BeginChangeCheck();
			label = EditorGUI.TextField(pos, m_target.GetLabel(i), EditorStyles.label);
			if(EditorGUI.EndChangeCheck()) m_target.SetLabel(i, label);
			pos.x += pos.width;

			// Use a slider to draw it and store new value
			pos.width = m_pos.width - (pos.x - m_pos.x);	// [AOC] Fill the rest of the space (Original reserved width minus the advance of the cursor from the original cursor)
			EditorGUI.BeginChangeCheck();
			probability = EditorGUI.Slider(pos, m_target.GetProbability(i), 0f, 1f);
			if(EditorGUI.EndChangeCheck()) m_target.SetProbability(i, probability);

			// Move to next line for the next element
			AdvancePos();

			// Restore indentation for next element
			EditorGUI.indentLevel = indentBackup;
		}
	}

	/// <summary>
	/// Draw the add and remove buttons.
	/// </summary>
	private void DoAddRemoveButtons() {
		// Aux vars
		GUI.enabled = true;
		Rect pos = EditorGUI.IndentedRect(m_pos);	// [AOC] Since buttons don't automatically respect EditorGUI.indentLevel
		pos.width = pos.width/2f;

		// Reset button - don't respect locks
		if(GUI.Button(pos, "Add Element")) {
			m_target.AddElement("Element " + (m_target.numElements + 1).ToString());
		}

		// Move cursor
		pos.x += pos.width;

		// Remove element button
		// Disable if there are no elements to remove
		GUI.enabled = m_target.numElements > 0;
		if(GUI.Button(pos, "Remove Element")) {
			m_target.RemoveElement();
		}
		GUI.enabled = true;

		// Move to next line
		AdvancePos();
	}

	/// <summary>
	/// Draw the reset buttons.
	/// </summary>
	private void DoResetButtons() {
		// Aux vars
		GUI.enabled = true;
		Rect pos = EditorGUI.IndentedRect(m_pos);	// [AOC] Since buttons don't automatically respect EditorGUI.indentLevel
		pos.width = pos.width/2f;

		// Reset button - don't respect locks
		if(GUI.Button(pos, "Reset")) {
			m_target.Reset(false);
		}

		// Move cursor
		pos.x += pos.width;

		// Reset button - respect locks
		if(GUI.Button(pos, "Reset (respect locks)")) {
			m_target.Reset(true);
		}

		// Move to next line
		AdvancePos();
	}
}