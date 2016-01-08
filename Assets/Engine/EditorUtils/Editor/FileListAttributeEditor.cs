// FileListAttributeEditor.cs
// 
// Created by Alger Ortín Castellví on 14/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Drawer for the FileList custom attribute.
/// </summary>
[CustomPropertyDrawer(typeof(FileListAttribute))]
public class FileListAttributeEditor : ExtendedPropertyDrawer {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Data that mus be kept between opening the sku list popup and capturing its result
	private List<FileInfo> m_files = null;
	private SerializedProperty m_targetProperty = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// A change has occurred in the inspector, draw the property and store new values.
	/// Use the m_pos member as position reference and update it using AdvancePos() method.
	/// </summary>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override protected void OnGUIImpl(SerializedProperty _property, GUIContent _label) {
		// Check field type
		if(_property.propertyType != SerializedPropertyType.String) {
			m_pos.height = EditorStyles.largeLabel.lineHeight;
			EditorGUI.LabelField(m_pos, _label.text, "ERROR! FileList attribute can only be applied to string properties!");
			AdvancePos();
			return;
		}

		// Obtain the attribute
		FileListAttribute attr = attribute as FileListAttribute;

		// Create the skus list
		m_files = new List<FileInfo>();

		// Insert "NONE" option at the beginning
		if(attr.m_allowEmptyValue) {
			m_files.Add(null);	// "NONE" value is ""
		}

		// Get the file list
		string dirPath = Application.dataPath + "/" + attr.m_folderPath;
		DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
		m_files.AddRange(dirInfo.GetFiles(attr.m_filter));	// Use file filter

		// Clean up displayed names
		// Strip filename from full file path
		string[] options = new string[m_files.Count];
		for(int i = 0; i < options.Length; i++) {
			if(i == 0 && attr.m_allowEmptyValue) {
				options[i] = "---- NONE";
			} else {
				options[i] = Path.GetFileNameWithoutExtension(m_files[i].Name);
			}
		}

		// Find out current selected value
		// If current value was not found, force it to first value (which will be "NONE" if allowed)
		int selectedIdx = 0;
		if(!String.IsNullOrEmpty(_property.stringValue)) {
			for(int i = 0; i < m_files.Count; i++) {
				if(m_files[i] != null && m_files[i].FullName.Contains(_property.stringValue)) {
					selectedIdx = i;
					break;
				}
			}
		}
		
		// Display the property
		// Show button using the popup style with the current value
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		Rect contentPos = EditorGUI.PrefixLabel(m_pos, _label);
		if(GUI.Button(contentPos, options[selectedIdx], EditorStyles.popup)) {
			m_targetProperty = _property;
			SelectionPopupWindow.Show(options, OnFileSelected);
		}

		// Leave room for next property drawer
		AdvancePos();
	}

	/// <summary>
	/// A file has been selected in the selection popup.
	/// </summary>
	/// <param name="_selectedIdx">The index of the newly selected option.</param>
	private void OnFileSelected(int _selectedIdx) {
		// Store new value - "NONE" will be stored as "" when allowed
		FileListAttribute attr = attribute as FileListAttribute;
		if(attr.m_allowEmptyValue && _selectedIdx == 0) {
			// "NONE" option selected
			m_targetProperty.stringValue = String.Empty;
		} else {
			// File selected: Store in the selected format
			m_targetProperty.stringValue = StringUtils.FormatPath(m_files[_selectedIdx].FullName, attr.m_format);	// Options array match the file list, so no need to do anything else :)
		}
		m_targetProperty.serializedObject.ApplyModifiedProperties();
	}
}

