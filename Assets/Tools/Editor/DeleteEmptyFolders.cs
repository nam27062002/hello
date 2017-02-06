﻿using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;

// Utility script which deletes all empty folders under Assets folder
public class DeleteEmptyFolders : EditorWindow {
	private static List<string> m_foldersToDelete;
	private static Vector2 m_scrollPos;

	public static void DeleteFolders() {
		m_foldersToDelete = new List<string>();
		m_scrollPos = Vector2.zero;

		GetEmptyFolders(Application.dataPath);

		var window = EditorWindow.GetWindow(typeof(DeleteEmptyFolders));
		window.titleContent.text = "Delete empty";
		window.Show();
	}

	// Recursively scans for empty folders and add them to a list
	private static void GetEmptyFolders(string path) {
		// Add exceptions
		string[] directories = Directory.GetDirectories(path)
			.Where((string _name) => {
				return (!_name.Contains("Plugins")
					 && !_name.Contains("Calety"));
			}).ToArray();

		// Don't consider meta files as files
		string[] files = Directory.GetFiles(path).Where(name => !name.Contains(".meta")).ToArray();

		// Scan inner directories first
		if(directories != null && directories.Length > 0) {
			for(int i = 0; i < directories.Length; i++) {
				GetEmptyFolders(directories[i]);
			}
		}

		// Conditions
		// 1. There are no inner folders or files - this is an empty folder for sure
		// 2. There are only empty folders within this folder and no other files - also considered an empty folder
		if((directories.Length == 0) && (files.Length == 0) || ((files.Length == 0) && directories.Length > 0 && AreAllEmpty(directories))) {
			m_foldersToDelete.Add(path);
		}
	}

	// Checks if all given folders are within empty folders list
	// Please note that this method will function properly only if inner folders are scanned before the parents - see above recursive method order of operation
	private static bool AreAllEmpty(string[] directories) {
		for(int i = 0; i < directories.Length; i++) {
			if(!m_foldersToDelete.Contains(directories[i])) {
				return false;
			}
		}

		return true;
	}

	// Immidiate mode GUI
	private void OnGUI() {
		if(m_foldersToDelete != null && m_foldersToDelete.Count > 0) {
			// Display all empty folders paths
			m_scrollPos = GUILayout.BeginScrollView(m_scrollPos);
			GUILayout.BeginVertical();
			for(int i = 0; i < m_foldersToDelete.Count; i++) {
				GUILayout.Label(m_foldersToDelete[i]);
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();

			// Delete button
			if(GUILayout.Button("Delete ALL", GUILayout.Width(100))) {
				// Iterate over all empty folders and delete them along with their meta files
				for(int i = 0; i < m_foldersToDelete.Count; i++) {
					// Delete folder
					//Directory.Delete(m_foldersToDelete[i]);

					// Delete folder's meta file which is one layer above the folder in hierarchy
					// TODO: test path combining on OSX
					//string[] directoryPathSplitted = m_foldersToDelete[i].Split('/');
					//string metaFilePath = string.Format("{0}\\{1}.meta", string.Join("\\", directoryPathSplitted, 0, directoryPathSplitted.Length - 1), directoryPathSplitted[directoryPathSplitted.Length - 1]);
					//File.Delete(metaFilePath);

					// Using asset database method which also deletes meta files
					AssetDatabase.DeleteAsset("Assets" + m_foldersToDelete[i].Substring(Application.dataPath.Length).ToString());
				}

				// Refresh assets to save changes
				AssetDatabase.Refresh();
				m_foldersToDelete = new List<string>();

				// Get further empty folders if exists - shouldn't be any if the recursive scan works correctly
				GetEmptyFolders(Application.dataPath);
			}
		} else {
			GUILayout.Label("No empty folders found");
		}
	}
}
