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
using System.Linq;
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
	private List<FileInfo> m_files = new List<FileInfo>();
	private List<string> m_options = new List<string>();
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

		// Store property
		m_targetProperty = _property;

		// Obtain the attribute
		FileListAttribute attr = attribute as FileListAttribute;

		// Clear both the files and the options lists
		m_files.Clear();
		m_options.Clear();

		// Insert "NONE" option at the beginning
		if(attr.m_allowEmptyValue) {
			m_files.Add(null);	// "NONE" value is ""
			m_options.Add("-");
		}

		// Get the file list (recursively)
		string dirPath = Application.dataPath + "/" + attr.m_folderPath;
		DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
		ScanFiles(dirInfo, attr.m_filter);

		// Find out current selected value
		// If current value was not found, force it to first value (which will be "NONE" if allowed)
		int selectedIdx = 0;
		if(!String.IsNullOrEmpty(_property.stringValue)) {
			for(int i = 0; i < m_files.Count; i++) {
				if(m_files[i] == null) continue;

				// [AOC] Windows uses backward slashes, which Unity doesn't recognize
				if(StringUtils.SafePath(m_files[i].FullName).Contains(StringUtils.SafePath(_property.stringValue))) {
					selectedIdx = i;
					break;
				}
			}
		}
		
		// Display the property
		// Unity's Popup control manages the sub-menu logic! ^_^
		m_pos.height = EditorStyles.popup.lineHeight + 5;	// [AOC] Default popup field height + some margin
		int newSelectedIdx = EditorGUI.Popup(m_pos, _label.text, selectedIdx, m_options.ToArray());
		if(newSelectedIdx != selectedIdx) {
			OnFileSelected(newSelectedIdx);
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

	/// <summary>
	/// Scan all the files (recursively) within a folder and adds them both to the m_files and m_options lists.
	/// Doesn't clear the lists.
	/// Unity's *.meta files will be excluded.
	/// </summary>
	/// <param name="_dirInfo">Directory to scan.</param>
	/// <param name="_filter">The search string to match against the names of files in path. This parameter can contain a combination of valid literal path and wildcard (* and ?) characters (see Remarks), but doesn't support regular expressions.</param>
	/// <param name="_optionsPrefix">The prefix to be used when adding the file name to the options list. Used for nesting the foldable menu.</param>
	private void ScanFiles(DirectoryInfo _dirInfo, string _filter, string _prefix = "") {
		// Scan target dir's files
		FileInfo[] files = _dirInfo.GetFiles(_filter).Where(_file => !_file.Extension.EndsWith(".meta")).ToArray();	// Use file filter, exclude .meta files

		// Add files to files list
		m_files.AddRange(files);

		// Clean up displayed names and add to the options list as well
		for(int i = 0; i < files.Length; i++) {
			m_options.Add(_prefix + Path.GetFileNameWithoutExtension(files[i].Name));	// Strip filename from full file path and attach prefix
		}

		// Recursively scan subdirs as well
		DirectoryInfo[] dirs = _dirInfo.GetDirectories();
		for(int i = 0; i < dirs.Length; i++) {
			// Update prefix (so all files in this subdir are in nested menus
			string prefix = _prefix + dirs[i].Name + "/";
			ScanFiles(dirs[i], _filter, prefix);
		}
	}
}

