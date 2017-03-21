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
using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

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
		CreateTemplateWindow.ShowWindow("Editor/CustomEditorTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Extended Property Drawer")]
	public static void CreateExtendedPropertyDrawer() {
		CreateTemplateWindow.ShowWindow("Editor/ExtendedPropertyDrawerTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Editor Window")]
	public static void CreateEditorWindow() {
		CreateTemplateWindow.ShowWindow("Editor/EditorWindowTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/MonoBehaviour")]
	public static void CreateMonoBehaviour() {
		CreateTemplateWindow.ShowWindow("MonoBehaviourTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Sub State Machine")]
	public static void CreateSMB() {
		CreateTemplateWindow.ShowWindow("SMBTemplate");
	}

	//------------------------------------------------------------------//
	// AUX CLASSES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Auxiliar dialog to actually create the template copy.
	/// </summary>
	class CreateTemplateWindow : EditorWindow {
		//------------------------------------------------------------------//
		// MEMBERS AND PROPERTIES											//
		//------------------------------------------------------------------//
		/// Vars
		private string m_templatePath = "";
		private string m_templateName = "";
		private string m_newName = "";

		// Properties (stored in editor settings)
		private string author {
			get { return EditorPrefs.GetString("EditorTemplates.Author", ""); }
			set { EditorPrefs.SetString("EditorTemplates.Author", value); }
		}

		private string projectName {
			get { return EditorPrefs.GetString("EditorTemplates.ProjectName", "Hungry Dragon"); }
			set { EditorPrefs.SetString("EditorTemplates.ProjectName", value); }
		}

		//------------------------------------------------------------------//
		// METHODS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// Opens the window.
		/// </summary>
		/// <param name="_templatePath">The path of the template, starting at TEMPLATES_FOLDER and excluding extension (i.e. Editor/CustomEditorTemplate).</param>
		public static CreateTemplateWindow ShowWindow(string _templatePath) {//string _message, string _initialText) {
			// Get window!
			CreateTemplateWindow window = EditorWindow.GetWindow<CreateTemplateWindow>(true);
			window.titleContent = new GUIContent("Create new " + _templatePath);

			// Init parameters
			window.m_templatePath = TEMPLATES_FOLDER + _templatePath + ".cs";
			window.m_templateName = Path.GetFileNameWithoutExtension(window.m_templatePath);
			window.m_newName = "New" + window.m_templateName;
			window.ValidateName();

			// Set window size
			window.minSize = new Vector2(400f, 100f);
			window.maxSize = window.minSize;	// Not resizeable

			// Show!
			window.Show();

			return window;
		}

		/// <summary>
		/// 
		/// </summary>
		private void OnGUI() {
			// Class name
			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			m_newName = EditorGUILayout.TextField("New Class name", m_newName);
			if(EditorGUI.EndChangeCheck()) {
				// Apply validation
				ValidateName();
			}

			// Author
			EditorGUILayout.Space();
			author = EditorGUILayout.TextField("Author", author);

			// Project Name
			projectName = EditorGUILayout.TextField("Project Name", projectName);

			// Buttons
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal(); {
				// Right aligned
				GUILayout.FlexibleSpace();

				// Cancel
				if(GUILayout.Button("Cancel")) {
					// Just close the dialog
					Close();
					GUIUtility.ExitGUI();	// Interrupt OnGUI properly
				}

				// Ok
				if(GUILayout.Button("Ok!")) {
					// Create the file and close the dialog
					CreateNewFile();
					Close();
					GUIUtility.ExitGUI();	// Interrupt OnGUI properly
				}
			} EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Creates the new file from the template, using current setup.
		/// </summary>
		private void CreateNewFile() {
			// Make sure new name is a valid class name
			ValidateName();

			// Find current path in the project window
			string dirPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			if(dirPath == "") {
				// Path empty, use default
				dirPath = TEMPLATES_FOLDER;
			} else if(Path.GetExtension(dirPath) != "") {
				// An object is selected, pick its container folder
				dirPath = dirPath.Replace(Path.GetFileName(dirPath), "");
			} else {
				dirPath += "/";
			}

			// Duplicate template asset
			string targetPath = dirPath + m_newName + Path.GetExtension(m_templatePath);
			targetPath = AssetDatabase.GenerateUniqueAssetPath(targetPath);	// Make sure name is unique
			if(!AssetDatabase.CopyAsset(m_templatePath, targetPath)) {
				Debug.Log("ERROR!");
				return;
			}

			// Make sure database is ok
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();

			// Open new asset
			TextAsset newAsset = AssetDatabase.LoadMainAssetAtPath(targetPath) as TextAsset;
			if(newAsset != null) {
				// Replace template name by new name inside the file
				string newText = newAsset.text.Replace(m_templateName, m_newName);

				// Put current date
				newText = newText.Replace("@dd", DateTime.Now.Day.ToString("00"));
				newText = newText.Replace("@mm", DateTime.Now.Month.ToString("00"));
				newText = newText.Replace("@yyyy", DateTime.Now.Year.ToString());

				// Put author's name
				newText = newText.Replace("@author", author);

				// Put project's name
				newText = newText.Replace("@projectName", projectName);

				// Save new file
				File.WriteAllText(targetPath, newText);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();

				// Highlight and select new object
				Selection.activeObject = newAsset;
				EditorGUIUtility.PingObject(newAsset);

				// Open in editor
				// We need to reload the asset -_-
				newAsset = AssetDatabase.LoadMainAssetAtPath(targetPath) as TextAsset;
				AssetDatabase.OpenAsset(newAsset);
			}
		}

		/// <summary>
		/// Make sure new name is a valid identifier.
		/// </summary>
		private void ValidateName() {
			// Make sure it's a valid Class name
			m_newName = m_newName.GenerateValidIdentifier();
		}
	}
}