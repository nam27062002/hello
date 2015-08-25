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
/// Singleton class, work with it via its static methods only.
/// <see cref="https://youtu.be/64uOVmQ5R1k?t=20m16s"/>
/// </summary>
public class PersistenceManager : Singleton<PersistenceManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string SAVE_PATH = "/SavePersistenceManager.dat";

	/// <summary>
	/// Auxiliar private serializable class to save/load a game state to persistence.
	/// </summary>
	[Serializable]
	class SaveData {
		// Add here any required data
		public UserProfile.SaveData profile;
		public DateTime timestamp;
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//


	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Last save timestamp
	private DateTime m_saveTimestamp = DateTime.UtcNow;
	public DateTime saveTimestamp {
		get { return m_saveTimestamp; }
	}

	// Path of the data file
	private string saveFile {
		// Hidden file directory of Unity, see https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		get { return Application.persistentDataPath + SAVE_PATH; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
		// From http://answers.unity3d.com/questions/30930/why-did-my-binaryserialzer-stop-working.html
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load the game persistence.
	/// </summary>
	public static void Load() {
		// From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		if(File.Exists(saveFile)) {
			// Open the file
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(saveFile, FileMode.Open);
			
			// Load the data object
			try {
				// Read file content and close it
				PersistenceManager.SaveData data = (PersistenceManager.SaveData)bf.Deserialize(file);
				file.Close();
				
				// Restore loaded values - order is relevant!
				// Last save timestamp
				instance.m_saveTimestamp = data.timestamp;

				// User profile
				UserProfile.Load(data.profile);
			} catch(Exception e) {
				Debug.Log("An error has occurred when loading persistence, deleting saved data" + e);
				File.Delete(saveFile);
			}
		} else {
			Debug.Log("No saved games were found, starting from 0");
		}
	}

	/// <summary>
	/// Save the game state to persistence.
	/// </summary>
	public static void Save() {
		// From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
		// Open the file
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(saveFile);
		
		// Create a temp data object and fill it
		PersistenceManager.SaveData data = new PersistenceManager.SaveData();

		// Timestamp
		data.timestamp = DateTime.UtcNow;

		// User profile
		data.profile = UserProfile.Save();
		
		// Save and close the file
		bf.Serialize(file, data);
		file.Close();
	}
}

