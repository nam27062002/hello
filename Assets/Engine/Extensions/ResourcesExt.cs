// ResourcesExt.cs
// 
// Created by Alger Ortín Castellví on 19/05/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the Resources class.
/// Since the Resources class is static (meaning it can't be extended), this is 
/// just another collection of static methods.
/// </summary>
public static class ResourcesExt {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// TRUE STATIC METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Loads all the prefabs with at the given resources folder and all its subfolders.
	/// </summary>
	/// <returns>A list with all the prefabs stored in the given folder.</returns>
	/// <param name="_path">The path of the folder to be loaded. Same syntax as Resources.Load().</param>
	public static List<GameObject> LoadRecursively(string _path) {
		// Create return list
		List<GameObject> prefabsList = new List<GameObject>();

		// Get all prefabs in the target directory, but don't include subdirectories
		DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/Resources/" + _path);
		FileInfo[] files = dirInfo.GetFiles("*.prefab");
		GameObject prefab = null;
		for(int i = 0; i < files.Length; i++) {
			// Load prefab and add it to the list
			prefab = Resources.Load<GameObject>(_path + "/" + Path.GetFileNameWithoutExtension(files[i].Name));
			if(prefab != null) prefabsList.Add(prefab);
		}

		// Iterate subdirectories recursively and attach loaded prefabs to the list
		DirectoryInfo[] subdirs = dirInfo.GetDirectories();
		for(int i = 0; i < subdirs.Length; i++) {
			// Format dir path to something that Unity Resources API understands
			string resourcePath = subdirs[i].FullName.Replace('\\', '/');	// [AOC] Windows uses backward slashes, which Unity doesn't recognize
			resourcePath = resourcePath.Substring(resourcePath.LastIndexOf("/Resources/") + ("/Resources/").Length);	// Get the latest part of the path starting at Resources folder

			// Recursive call
			prefabsList.AddRange(LoadRecursively(resourcePath));
		}

		// Done!
		return prefabsList;
	}
}
