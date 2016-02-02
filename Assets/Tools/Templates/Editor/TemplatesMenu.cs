// TemplatesMenu.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;
using System.IO;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auxiliar menu to create new scripts from template.
/// </summary>
public class TemplatesMenu {
	//------------------------------------------------------------------//
	// MENU ENTRIES														//
	//------------------------------------------------------------------//
	public static readonly string TEMPLATES_FOLDER = "Assets/Tools/Templates/";

	//------------------------------------------------------------------//
	// MENU ENTRIES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Different template menu entries
	/// </summary>
	[MenuItem("Assets/Create/C# Script Templates/Custom Editor")]
	public static void CreateCustomEditor() {
		CreateTemplate("Editor/CustomEditorTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Extended Property Drawer")]
	public static void CreateExtendedPropertyDrawer() {
		CreateTemplate("Editor/ExtendedPropertyDrawerTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Editor Window")]
	public static void CreateEditorWindow() {
		CreateTemplate("Editor/EditorWindowTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/MonoBehaviour")]
	public static void CreateMonoBehaviour() {
		CreateTemplate("MonoBehaviourTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Sub State Machine")]
	public static void CreateSMB() {
		CreateTemplate("SMBTemplate");
	}

	//------------------------------------------------------------------//
	// INTERNAL															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Creates the template.
	/// </summary>
	/// <param name="_templatePath">The path of the template, starting at TEMPLATES_FOLDER and excluding extension (i.e. Editor/CustomEditorTemplate).</param>
	private static void CreateTemplate(string _templatePath) {
		// Find current path in the project window
		string dirPath = AssetDatabase.GetAssetPath(Selection.activeObject);
		if(dirPath == "") {
			// Path empty, use default
			dirPath = TEMPLATES_FOLDER;
		} else if(Path.GetExtension(dirPath) != "") {
			// An object is selected, pick it's conatiner folder
			dirPath = dirPath.Replace(Path.GetFileName(dirPath), "");
		} else {
			dirPath += "/";
		}

		// Duplicate template asset
		string templatePath = TEMPLATES_FOLDER + _templatePath + ".cs";
		string targetPath = dirPath + "New" + Path.GetFileName(templatePath);
		targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);	// Make sure name is unique
		if(!AssetDatabase.CopyAsset(templatePath, targetPath)) {
			Debug.Log("ERROR!");
			return;
		}
		// TODO!! Custom naming

		// Make sure database is ok
		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();

		// Highlight and select new object
		Object newAsset = AssetDatabase.LoadMainAssetAtPath(targetPath);
		Selection.activeObject = newAsset;
		EditorGUIUtility.PingObject(newAsset);
	}
}