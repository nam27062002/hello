// ResourcesExt.cs
// 
// Created by Alger Ortín Castellví on 19/05/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
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

	/// <summary>
	/// Loads a single sprite from the Resources folder, either a single sprite or a sprite within a spritesheet.
	/// If a single sprite with the given path doesn't exist, the last section of the path will be considered as a
	/// sprite id within a spritesheet and the rest of the path as the spritesheet path in Resources.
	/// Not very efficient, specially with large atlases. Don't abuse it.
	/// If several sprites from the same spritesheet are to be loaded, better check out LoadSpritesheet() method.
	/// </summary>
	/// <returns>The requested sprite, <c>null</c> if it wasn't found.</returns>
	/// <param name="_path">The path of either the single sprite or the spritesheet + the sprite name. Relative to the Resources folder. No extension.</param>
	public static Sprite LoadSprite(string _path) {
		// Attempt to load it as a sprite
		Sprite target = Resources.Load<Sprite>(_path);

		// If success, we're done!
		if(target != null) return target;

		// If not successful, strip last part of the path and try to load it as a spritesheet
		string spriteName = Path.GetFileNameWithoutExtension(_path);	// That should do it
		string spritesheetPath = _path.Replace("/" + spriteName, "");
		Debug.Log(spriteName + "\n" + spritesheetPath);

		// Use the other aux methods in this class
		return LoadFromSpritesheet(spritesheetPath, spriteName);
	}

	/// <summary>
	/// Loads a single sprite from a spritesheet, by name.
	/// Not very efficient, specially with large atlases. Don't abuse it.
	/// If several sprites from the same spritesheet are to be loaded, better check out LoadSpritesheet() method.
	/// </summary>
	/// <returns>The requested sprite, <c>null</c> if it wasn't found.</returns>
	/// <param name="_spritesheetPath">The path of the spritesheet relative to the Resources folder. No extension.</param>
	/// <param name="_spriteName">The name of the sprite within the spritesheet.</param>
	public static Sprite LoadFromSpritesheet(string _spritesheetPath, string _spriteName) {
		// Load the spritesheet
		Sprite[] sprites = Resources.LoadAll<Sprite>(_spritesheetPath);

		// Find our target sprite
		for(int i = 0; i < sprites.Length; i++) {
			if(sprites[i].name == _spriteName) return sprites[i];
		}

		// Sprite not found :(
		return null;
	}

	/// <summary>
	/// Loads a spritesheet into a dictionary, by name.
	/// </summary>
	/// <returns>A new dictionary with containing all the sprites in a spritesheets by name.</returns>
	/// <param name="_spritesheetPath">The path of the spritesheet relative to the Resources folder. No extension.</param>
	public static Dictionary<string, Sprite> LoadSpritesheet(string _spritesheetPath) {
		// Load the spritesheet
		Sprite[] sprites = Resources.LoadAll<Sprite>(_spritesheetPath);

		// Create and initialize the dictionary
		Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>(sprites.Length);
		for(int i = 0; i < sprites.Length; i++) {
			dict.Add(sprites[i].name, sprites[i]);
		}

		// Done!
		return dict;
	}
}
