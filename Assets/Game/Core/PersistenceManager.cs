﻿// PersistenceManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class responsible to save/load persistence of the game, either local or online.
/// Static class so it can easily be accessed from anywhere.
/// Saved games names should match persistence profile names, although PersistenceProfile logic
/// is completely independent from the PersistenceManager to allow more flexibility regarding
/// future needs.
/// </summary>
public static class PersistenceManager {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	const string TAG = "PersistenceManager";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Last save timestamp
	private static DateTime m_saveTimestamp = DateTime.UtcNow;
	public static DateTime saveTimestamp {
		get { return m_saveTimestamp; }
	}

	// Path where the data files are stored
	public static string saveDir {
		// Hidden file directory of Unity, see https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		get { return Application.persistentDataPath; }
	}

	// Default persistence profile - it's stored in the player preferences, that way can be set from the editor and read during gameplay
	public static string activeProfile {
		get { return PlayerPrefs.GetString("activeProfile", PersistenceProfile.DEFAULT_PROFILE); }
		set { PlayerPrefs.SetString("activeProfile", value); }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization. Must be called upon starting the application.
	/// </summary>
	public static void Init() {
		// Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
		// From http://answers.unity3d.com/questions/30930/why-did-my-binaryserialzer-stop-working.html
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
	}

	//------------------------------------------------------------------//
	// MAIN PUBLIC METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load the game persistence for a specific profile into the game managers.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be loaded.</param>
	public static void Load(string _profileName = "") {
		// Load the persistence object for the given profile
		SimpleJSON.JSONClass data = LoadToObject(_profileName);

		// Load from the object we just read
		LoadFromObject(data);
	}

	/// <summary>
	/// Load the game persistence from a given JSON into the game managers.
	/// Can be used to load debug profiles (probably require a game restart).
	/// </summary>
	/// <param name="_data">The JSON object to be loaded.</param>
	public static void LoadFromObject(SimpleJSON.JSONClass _data) {
		// Make sure given object is valid
		if(_data == null) return;

		// Restore loaded values
		// Order is relevant!
		// Last save timestamp
		/*
		if ( _data.ContainsKey("timestamp") )
			m_saveTimestamp = DateTime.Parse( _data["timestamp"] );
		else 
			m_saveTimestamp = DateTime.Now;
		*/
		// User profile
		UsersManager.currentUser.Load( _data );

		DragonManager.SetupUser( UsersManager.currentUser );
		EggManager.SetupUser( UsersManager.currentUser );
		MissionManager.SetupUser( UsersManager.currentUser );

	}

	/// <summary>
	/// Load the game persistence for a specific profile into a new JSON object.
	/// </summary>
	/// <returns>The to JSON where the data is loaded, <c>null</c> if an error ocurred.</returns>
	/// <param name="_profileName">The name of the profile to be loaded.</param>
	public static SimpleJSON.JSONClass LoadToObject(string _profileName = "") {
		// From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		SimpleJSON.JSONClass data = null;
		string path = GetPersistenceFilePath(_profileName);
		if(File.Exists(path)) 
		{
			StreamReader sr = new StreamReader( path );
			string profileJSONStr = sr.ReadToEnd();
			sr.Close();
			data = SimpleJSON.JSON.Parse( profileJSONStr ) as SimpleJSON.JSONClass;
		} else {
			// No saved games found for the given profile, try to load the profile data
			CheckProfileName(ref _profileName);
			Debug.Log("No saved games were found, loading profile " + _profileName);

			// Load profile and use it as initial data
			data = GetDefaultDataFromProfile(_profileName);
			if(data == null) {
				Debug.Log("Profile " + _profileName + " couldn't be found, starting from 0");
			}
		}

		return data;
	}

	/// <summary>
	/// Save the game state to persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be saved.</param>
	public static void Save(string _profileName = "") {

		// User profile
		SimpleJSON.JSONClass data = UsersManager.currentUser.Save();

		// Save the object we just created
		SaveFromObject(_profileName, data);
	}

	/// <summary>
	/// Save a given data object to persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be saved.</param>
	/// <param name="_data">The data object to be saved.</param>
	public static void SaveFromObject(string _profileName, SimpleJSON.JSONClass _data) 
	{
		// From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		// Open the file
		string path = GetPersistenceFilePath(_profileName);

		_data.Add("timestamp", DateTime.UtcNow.ToString());
		System.IO.File.WriteAllText( path , _data.ToString() );
	}

	/// <summary>
	/// Deletes local persistence file.
	/// The game should be reloaded afterwards.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be cleared.</param>
	public static void Clear(string _profileName = "") 
	{
		// Delete persistence file
		string path = GetPersistenceFilePath(_profileName);
		File.Delete(path);

		// Create a new save file with the default data from the profile
		SimpleJSON.JSONClass data = GetDefaultDataFromProfile(_profileName);
		if (data != null) {
			SaveFromObject(_profileName, data);
		}
	}

	//------------------------------------------------------------------//
	// OTHER AUXILIAR METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given the name of a profile, obtain the full path of its associated persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile whose path we want.</param>
	private static string GetPersistenceFilePath(string _profileName = "") {
		// If not defined, return active profile
		CheckProfileName(ref _profileName);
		return saveDir + "/" + _profileName + ".json";
	}

	/// <summary>
	/// Obtain the list of current saved games.
	/// </summary>
	/// <returns>The list of saved games in the persistence dir.</returns>
	public static string[] GetSavedGamesList() {
		// C# makes it easy for us
		DirectoryInfo dirInfo = new DirectoryInfo(saveDir);
		FileInfo[] files = dirInfo.GetFiles();

		// Strip filename from full file path
		List<string> fileNames = new List<string>();
		for(int i = 0; i < files.Length; i++) 
		{
			if ( files[i].Name.EndsWith("json") )
			{
				fileNames.Add(Path.GetFileNameWithoutExtension(files[i].Name));
			}
		}

		return fileNames.ToArray();
	}

	/// <summary>
	/// Determines if a given profile name has a persistence file attached to it.
	/// </summary>
	/// <returns><c>true</c> if a saved game exists for the specified _profileName; otherwise, <c>false</c>.</returns>
	/// <param name="_profileName">The name of the profile we want to check.</param>
	public static bool HasSavedGame(string _profileName) {
		return File.Exists(GetPersistenceFilePath(_profileName));
	}


	/// <summary>
	/// Get the default data stored in the given profile prefab.
	/// </summary>
	/// <returns>The data from profile.</returns>
	/// <param name="_profileName">The name of the profile to be loaded.</param>
	public static SimpleJSON.JSONClass GetDefaultDataFromProfile(string _profileName = "") 
	{
		// Load data from prefab
		CheckProfileName(ref _profileName);
		TextAsset defaultProfile = Resources.Load<TextAsset>(PersistenceProfile.RESOURCES_FOLDER + _profileName);
		if(defaultProfile != null) 
		{
			return SimpleJSON.JSON.Parse( defaultProfile.text ) as SimpleJSON.JSONClass;
		}

		return null;
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// If the given profile name is empty, replace it by the current active profile name.
	/// </summary>
	/// <param name="_profileName">The profile name to be checked.</param>
	private static void CheckProfileName(ref string _profileName) {
		if(_profileName == "") {
			_profileName = activeProfile;
		}
	}
}

