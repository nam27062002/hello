// PersistenceManager.cs
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
	/// <summary>
	/// Auxiliar private serializable class to save/load a game state to persistence.
	/// </summary>
	[Serializable]
	public class SaveData {
		// Add here any required data
		public DateTime timestamp = DateTime.UtcNow;
		public UserProfile.SaveData profile = new UserProfile.SaveData();
		public DragonData.SaveData[] dragons = new DragonData.SaveData[(int)DragonId.COUNT];
	}

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
		PersistenceManager.SaveData data = LoadToObject(_profileName);

		// Load from the object we just read
		LoadFromObject(data);
	}

	/// <summary>
	/// Load the game persistence from a given object into the game managers.
	/// Can be used to load debug profiles (probably require a game restart).
	/// </summary>
	/// <param name="_data">The data object to be loaded.</param>
	public static void LoadFromObject(PersistenceManager.SaveData _data) {
		// Make sure given object is valid
		if(_data == null) return;

		// Restore loaded values
		// Last save timestamp
		m_saveTimestamp = _data.timestamp;
		
		// User profile
		UserProfile.Load(_data.profile);
		
		// Dragons data
		DragonManager.Load(_data.dragons);
	}

	/// <summary>
	/// Load the game persistence for a specific profile into a new persistence object.
	/// </summary>
	/// <returns>The to object where the data is loaded, <c>null</c> if an error ocurred.</returns>
	/// <param name="_profileName">The name of the profile to be loaded.</param>
	public static PersistenceManager.SaveData LoadToObject(string _profileName = "") {
		// From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		PersistenceManager.SaveData data = null;
		string path = GetPersistenceFilePath(_profileName);
		if(File.Exists(path)) {
			// Open the file
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(path, FileMode.Open);
			
			// Load the data object
			try {
				// Read file content and close it
				data = (PersistenceManager.SaveData)bf.Deserialize(file);
				file.Close();
			} catch(Exception e) {
				Debug.Log("An error has occurred when loading persistence, deleting saved data" + e);
				File.Delete(path);
			}
		} else {
			Debug.Log("No saved games were found, starting from 0");
		}

		return data;
	}

	/// <summary>
	/// Save the game state to persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be saved.</param>
	public static void Save(string _profileName = "") {
		// Create a temp data object and fill it
		PersistenceManager.SaveData data = new PersistenceManager.SaveData();

		// User profile
		data.profile = UserProfile.Save();

		// Dragons data
		data.dragons = DragonManager.Save();
		
		// Save the object we just created
		SaveFromObject(_profileName, data);
	}

	/// <summary>
	/// Save a given data object to persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be saved.</param>
	/// <param name="_data">The data object to be saved.</param>
	public static void SaveFromObject(string _profileName, PersistenceManager.SaveData _data) {
		// From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		// Open the file
		string path = GetPersistenceFilePath(_profileName);
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(path);

		// Timestamp
		_data.timestamp = DateTime.UtcNow;

		// Save and close the file
		bf.Serialize(file, _data);
		file.Close();
	}

	/// <summary>
	/// Deletes local persistence file.
	/// The game should be reloaded afterwards.
	/// </summary>
	/// <param name="_profileName">The name of the profile to be cleared.</param>
	public static void Clear(string _profileName = "") {
		// Just delete persistence file
		string path = GetPersistenceFilePath(_profileName);
		File.Delete(path);
		SaveFromObject(_profileName, new SaveData());
	}

	//------------------------------------------------------------------//
	// OTHER AUXILIAR METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given the name of a profile, obtain the full path of its associated persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile whose path we want.</param>
	public static string GetPersistenceFilePath(string _profileName = "") {
		// If not defined, return active profile
		if(_profileName == "") {
			_profileName = activeProfile;
		}

		return saveDir + "/" + _profileName + ".dat";
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
		string[] fileNames = new string[files.Length];
		for(int i = 0; i < files.Length; i++) {
			fileNames[i] = Path.GetFileNameWithoutExtension(files[i].Name);
		}

		return fileNames;
	}

	/// <summary>
	/// Determines if a given profile name has a persistence file attached to it.
	/// </summary>
	/// <returns><c>true</c> if a saved game exists for the specified _profileName; otherwise, <c>false</c>.</returns>
	/// <param name="_profileName">The name of the profile we want to check.</param>
	public static bool HasSavedGame(string _profileName) {
		return File.Exists(GetPersistenceFilePath(_profileName));
	}
}

