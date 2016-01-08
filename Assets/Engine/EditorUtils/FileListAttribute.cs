// FileListAttribute.cs
// 
// Created by Alger Ortín Castellví on 21/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom attribute to select from all the files in a directory, storing its path into the linked property.
/// </summary>
public class FileListAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public string m_folderPath = "";		// Path of the folder to be read, starting at Assets (Application.datapath) (but not including it, e.g. "Game/MyFolder")
	public StringUtils.PathFormat m_format = StringUtils.PathFormat.PROJECT_ROOT;	// The format in which to store the selected file path
	public string m_filter = "*";			// Name filter for the file list
	public bool m_allowEmptyValue = true;	// Whether to allow leaving the field empty or not

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_folderPath">Path of the folder whose files we wanna list, starting at Assets (Application.datapath) (but not including it, e.g. "Game/MyFolder").</param>
	/// <param name="_fileExtensions">Optionally add a search filter to the list. Match the format as in https://msdn.microsoft.com/en-us/library/8he88b63(v=vs.110).aspx.</param>
	/// <param name="_allowEmptyValue">Whether to allow leaving the field empty or not.</param>
	public FileListAttribute(string _folderPath, StringUtils.PathFormat _format, string _filter = "*", bool _allowEmptyValue = true) {
		m_folderPath = _folderPath;
		m_format = _format;
		m_filter = _filter;
		m_allowEmptyValue = _allowEmptyValue;
	}
}

