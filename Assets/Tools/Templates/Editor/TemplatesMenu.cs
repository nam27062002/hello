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
	[MenuItem("Assets/Create/C# Script Templates/Class", false, 1)]
	public static void CreateClass() {
		CreateFromTemplate("ClassTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/MonoBehaviour", false, 2)]
	public static void CreateMonoBehaviour() {
		CreateFromTemplate("MonoBehaviourTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Custom Editor", false, 51)]
	public static void CreateCustomEditor() {
		CreateFromTemplate("Editor/CustomEditorTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Editor Window", false, 52)]
	public static void CreateEditorWindow() {
		CreateFromTemplate("Editor/EditorWindowTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Extended Property Drawer", false, 53)]
	public static void CreateExtendedPropertyDrawer() {
		CreateFromTemplate("Editor/ExtendedPropertyDrawerTemplate");
	}

	[MenuItem("Assets/Create/C# Script Templates/Sub State Machine", false, 101)]
	public static void CreateSMB() {
		CreateFromTemplate("SMBTemplate");
	}

	//------------------------------------------------------------------//

	[MenuItem("Assets/Smart Duplicate", false, 1)]
	public static void SmartDuplicate() {
		string selectedFilePath = AssetDatabase.GetAssetPath(Selection.activeObject);
		CreateTemplateWindow.ShowWindow(selectedFilePath);
	}

	/// <summary>
	/// Validate the SmartDuplicate option.
	/// Only for text files.
	/// </summary>
	/// <returns>Whether the SmartDuplicate action can be used or not.</returns>
	[MenuItem("Assets/Smart Duplicate", true, 1)]
	static bool SmartDuplicateValidation() {
		// Only if selected file is a text asset
		return Selection.activeObject is TextAsset;
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Create a duplicate from one of the known templates.
	/// </summary>
	/// <param name="_templateName">The path of the template, starting at TEMPLATES_FOLDER and excluding extension (i.e. Editor/CustomEditorTemplate).</param>
	private static void CreateFromTemplate(string _templateName) {
		CreateTemplateWindow.ShowWindow(TEMPLATES_FOLDER + _templateName + ".cs");
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

		private bool openInEditor {
			get { return EditorPrefs.GetBool("EditorTemplates.OpenInEditor", true); }
			set { EditorPrefs.SetBool("EditorTemplates.OpenInEditor", value); }
		}

		// Internal
		private bool m_initialFocusPending = false;

		//------------------------------------------------------------------//
		// METHODS															//
		//------------------------------------------------------------------//
		/// <summary>
		/// Opens the window.
		/// </summary>
		/// <param name="_templatePath">The path of the template, starting with "Assets/" and including extension (i.e. Assets/Tools/Templates/Editor/CustomEditorTemplate.cs).</param>
		public static CreateTemplateWindow ShowWindow(string _templatePath) {
			// Get window!
			CreateTemplateWindow window = EditorWindow.GetWindow<CreateTemplateWindow>(true);

			// Init parameters
			window.m_templatePath = _templatePath;
			window.m_templateName = Path.GetFileNameWithoutExtension(window.m_templatePath);
			window.m_newName = window.m_templateName + "New";
			window.ValidateName();

			// Windows title
			window.titleContent = new GUIContent("Create new " + window.m_templateName);

			// Set window size
			window.minSize = new Vector2(400f, EditorGUIUtility.singleLineHeight * 8);//100f);
			window.maxSize = window.minSize;	// Not resizeable

			// Show!
			window.Show();

			return window;
		}

		/// <summary>
		/// Window has been opened.
		/// </summary>
		private void OnEnable() {
			m_initialFocusPending = true;
		}

		/// <summary>
		/// 
		/// </summary>
		private void OnGUI() {
			// Class name
			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName("NameTextfield");
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

			// Open in editor
			openInEditor = EditorGUILayout.Toggle("Open in Editor", openInEditor);

			// Buttons
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal(); {
				// Right aligned
				GUILayout.FlexibleSpace();

				// Cancel
				if(GUILayout.Button("Cancel")) {
					OnCancel();
				}

				// Ok
				if(GUILayout.Button("Ok!")) {
					OnSumbit();
				}
			} EditorGUILayout.EndHorizontal();

			// If initial focus is pending, do it now
			if(m_initialFocusPending) {
				EditorGUI.FocusTextInControl("NameTextfield");
				m_initialFocusPending = false;
			}

			// Capture Enter and Escape keys for fast submit/cancel
			switch(Event.current.type) {
				case EventType.keyDown: {
					switch(Event.current.keyCode) {
						case KeyCode.Return:
						case KeyCode.KeypadEnter: {
							OnSumbit();	
						} break;

						case KeyCode.Escape: {
							OnCancel();
						} break;
					}
				} break;
			}
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

			// Show progress bar
			string progressBarTitle = "Creating new file " + m_newName + "...";
			string progressBarMessage = "Cloning from " + m_templatePath + "\nto " + targetPath;
			EditorUtility.DisplayProgressBar(progressBarTitle, progressBarMessage, 0f);

			// Make sure database is ok
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();

			// Update progress bar
			EditorUtility.DisplayProgressBar(progressBarTitle, progressBarMessage, 0.33f);

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

				// Update progress bar
				EditorUtility.DisplayProgressBar(progressBarTitle, progressBarMessage, 0.66f);

				// Highlight and select new object
				Selection.activeObject = newAsset;
				EditorGUIUtility.PingObject(newAsset);

				// Update progress bar
				EditorUtility.DisplayProgressBar(progressBarTitle, progressBarMessage, 1f);

				// Open in editor (if requested)
				if(openInEditor) {
					// We need to reload the asset -_-
					newAsset = AssetDatabase.LoadMainAssetAtPath(targetPath) as TextAsset;
					AssetDatabase.OpenAsset(newAsset);
				}

				// Hide progress bar
				EditorUtility.ClearProgressBar();
			}
		}

		/// <summary>
		/// Make sure new name is a valid identifier.
		/// </summary>
		private void ValidateName() {
			// Make sure it's a valid Class name
			m_newName = m_newName.GenerateValidIdentifier();
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The cancel button has been pressed.
		/// </summary>
		private void OnCancel() {
			// Just close the dialog
			Close();
			GUIUtility.ExitGUI();	// Interrupt OnGUI properly
		}

		/// <summary>
		/// The submit button has been pressed.
		/// </summary>
		private void OnSumbit() {
			// Create the file and close the dialog
			CreateNewFile();
			Close();
			GUIUtility.ExitGUI();	// Interrupt OnGUI properly
		}
	}
}