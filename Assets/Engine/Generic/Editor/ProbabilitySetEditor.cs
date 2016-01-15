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
	SerializedProperty m_elementsProp = null;	// To keep the reference between methods

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
		// Draw property label + foldout widget
		m_pos.height = EditorStyles.largeLabel.lineHeight;	// Advance pointer just the size of the label
		_property.isExpanded = EditorGUI.Foldout(m_pos, _property.isExpanded, _label);
		AdvancePos();

		// If unfolded, draw children
		if(_property.isExpanded) {
			// Indent in
			EditorGUI.indentLevel++;

			// Get the values and labels arrays
			m_elementsProp = _property.FindPropertyRelative("m_elements");

			// Elements
			DoElements();

			// Spacing
			AdvancePos(5f);

			// Add/Remove buttons
			DoAddRemoveButtons();

			// Spacing
			AdvancePos(5f);

			// Reset Buttons
			DoResetButtons();

			// Indent back out and make sure gui is enabled
			EditorGUI.indentLevel--;
			GUI.enabled = true;
		}
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
		for(int i = 0; i < m_elementsProp.arraySize; i++) {
			// Aux
			SerializedProperty currentElementProp = m_elementsProp.GetArrayElementAtIndex(i);
			m_pos.height = EditorStyles.numberField.lineHeight + EditorStyles.numberField.margin.vertical;
			Rect pos = new Rect(m_pos);

			// Lock?
			// [AOC] We will use the isExpanded property to store the status (so dirty xD)
			SerializedProperty lockedProp = currentElementProp.FindPropertyRelative("locked");
			GUI.enabled = true;
			pos.width = 40;
			lockedProp.boolValue = !EditorGUI.Toggle(pos, !lockedProp.boolValue);	// [AOC] A little weird: what actually makes sense is that the toggle is marked when the element is not locked
			pos.x += pos.width;
			GUI.enabled = !lockedProp.boolValue;

			// [AOC] Temporarily remove indentation so following controls are next to each other
			int indentBackup = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			// Label - Editable
			// [AOC] Draw a textfield using the label style
			pos.width = 120;
			SerializedProperty labelProp = currentElementProp.FindPropertyRelative("label");
			labelProp.stringValue = EditorGUI.TextField(pos, labelProp.stringValue, EditorStyles.label);
			pos.x += pos.width;

			// Use a slider to draw it and store new value
			EditorGUI.BeginChangeCheck();
			pos.width = m_pos.width - (pos.x - m_pos.x);	// [AOC] Fill the rest of the space (Original reserved width minus the advance of the cursor from the original cursor)
			currentElementProp.FindPropertyRelative("value").floatValue = EditorGUI.Slider(pos, currentElementProp.FindPropertyRelative("value").floatValue, 0f, 1f);
			if(EditorGUI.EndChangeCheck()) {
				// If the value has changed, redistribute probabilities
				Redistribute(i);
			}

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
			// This will add a new element at the end cloning the last element on the list
			m_elementsProp.arraySize++;

			// Set initial values
			// Give it 0 value to avoid affecting the global balance, unless there's only one element, in which case we'll give it value 1
			SerializedProperty newElementProp = m_elementsProp.GetArrayElementAtIndex(m_elementsProp.arraySize - 1);
			if(m_elementsProp.arraySize == 1) {
				newElementProp.FindPropertyRelative("value").floatValue = 1f;
			} else {
				newElementProp.FindPropertyRelative("value").floatValue = 0f;
			}
			newElementProp.FindPropertyRelative("label").stringValue = "NewElement";
			newElementProp.FindPropertyRelative("locked").boolValue = false;
		}

		// Move cursor
		pos.x += pos.width;

		// Remove element button
		// Disable if there are no elements to remove
		GUI.enabled = m_elementsProp.arraySize > 0;
		if(GUI.Button(pos, "Remove Element")) {
			// Before removing, redistribute values simulating that the element we will remove goes to 0
			SerializedProperty lastElement = m_elementsProp.GetArrayElementAtIndex(m_elementsProp.arraySize - 1);
			lastElement.FindPropertyRelative("value").floatValue = 0f;
			Redistribute(m_elementsProp.arraySize - 1, true);

			// Just remove last element (if enough elements)
			if(m_elementsProp.arraySize > 0) {
				m_elementsProp.arraySize--;
			}
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
			for(int i = 0; i < m_elementsProp.arraySize; i++) {
				SerializedProperty currentElementProp = m_elementsProp.GetArrayElementAtIndex(i);
				currentElementProp.FindPropertyRelative("value").floatValue = 1f/(float)m_elementsProp.arraySize;
				currentElementProp.FindPropertyRelative("locked").boolValue = false;
			}
		}

		// Move cursor
		pos.x += pos.width;

		// Reset button - respect locks
		if(GUI.Button(pos, "Reset (respect locks)")) {
			// Count non-locked elements
			float nonLockedCount = 0f;
			float toDistribute = 1f;
			for(int i = 0; i < m_elementsProp.arraySize; i++) {
				SerializedProperty currentElementProp = m_elementsProp.GetArrayElementAtIndex(i);
				if(!currentElementProp.FindPropertyRelative("locked").boolValue) {
					nonLockedCount++;
				} else {
					toDistribute -= currentElementProp.FindPropertyRelative("value").floatValue;
				}
			}

			// Distribute uniformly among non-locked elements
			for(int i = 0; i < m_elementsProp.arraySize; i++) {
				SerializedProperty currentElementProp = m_elementsProp.GetArrayElementAtIndex(i);
				if(!currentElementProp.FindPropertyRelative("locked").boolValue) {
					currentElementProp.FindPropertyRelative("value").floatValue = toDistribute/nonLockedCount;
				}
			}
		}

		// Move to next line
		AdvancePos();
	}

	//------------------------------------------------------------------//
	// INTERNAL AUX														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Re-distribute probability within all elements.
	/// </summary>
	/// <param name="_changedIdx">The index of the element that has been changed.</param>
	/// <param name="_overrideLocks">If set to <c>true</c>, locked sliders will be modified too.</param> 
	private void Redistribute(int _changedIdx, bool _overrideLocks = false) {
		// [AOC] From HumbleBundle's web page code:
		// Calculate the splits for all the siblings.
		// Conceptually, we remove the active slider from the mix. Then we normalize the siblings to 1 to
		// determine their weights relative to each other. Then we divide the split that is left over from the moved
		// slider with these relative weights.

		// Get changed element
		SerializedProperty changedElementProp = m_elementsProp.GetArrayElementAtIndex(_changedIdx);

		// Amount to distribute between the siblings
		float totalToDistribute = (1f - changedElementProp.FindPropertyRelative("value").floatValue);

		// Compute total amount of unlocked siblings to distribute proportionally
		// Adjust amount to distribute by ignoring locked sliders
		float unlockedSiblingsTotal = 0f;
		float unlockedSiblingsCount = 0f;	// [AOC] Use float directly to avoid casting later on
		for(int j = 0; j < m_elementsProp.arraySize; j++) {
			// Skip current
			if(j == _changedIdx) continue;

			// Is it locked?
			SerializedProperty siblingProp = m_elementsProp.GetArrayElementAtIndex(j);
			if(_overrideLocks || !siblingProp.FindPropertyRelative("locked").boolValue) {
				unlockedSiblingsTotal += siblingProp.FindPropertyRelative("value").floatValue;
				unlockedSiblingsCount++;
			} else {
				totalToDistribute -= siblingProp.FindPropertyRelative("value").floatValue;
			}
		}

		// [AOC] Extra
		// Locked sliders may limit new value, check it here
		// If the amount to distribute is negative, add that amount to changed slider value
		if(!_overrideLocks && totalToDistribute < 0f) {
			changedElementProp.FindPropertyRelative("value").floatValue += totalToDistribute;
			totalToDistribute = 0;
		}

		// Compute and assign new value to each sibling based on its weight
		float remainingToDistribute = totalToDistribute;
		for(int i = 0; i < m_elementsProp.arraySize; i++) {
			// Skip ourselves
			if(i == _changedIdx) continue;

			// Skip if locked
			SerializedProperty siblingProp = m_elementsProp.GetArrayElementAtIndex(i);
			if(!_overrideLocks && siblingProp.FindPropertyRelative("locked").boolValue) continue;

			// Store previous value
			float oldSiblingValue = siblingProp.FindPropertyRelative("value").floatValue;

			// Compute relative weight of this sibling in relation to all active siblings (or something like that :s)
			float siblingWeight = 0;
			if(unlockedSiblingsTotal == 0f) {
				// If all sliders except the one being moved are at 0, we split the movement evently amongst them
				siblingWeight = 1f/unlockedSiblingsCount;
			} else {
				siblingWeight = oldSiblingValue/unlockedSiblingsTotal;
			}

			// Compute new value for the sibling and store it to the property
			siblingProp.FindPropertyRelative("value").floatValue = totalToDistribute * siblingWeight;
			remainingToDistribute -= totalToDistribute * siblingWeight;
		}

		// [AOC] Extra
		// If not everything could be distributed, limit the movement of the target slider.
		// This could happen when all siblings are either locked or already 0
		if(remainingToDistribute > 0f) {
			changedElementProp.FindPropertyRelative("value").floatValue += remainingToDistribute;
		}

		// Make sure changes are stored
		m_elementsProp.serializedObject.ApplyModifiedProperties();
	}
}