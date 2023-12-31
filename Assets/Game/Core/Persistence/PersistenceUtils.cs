﻿using FGOL.Save;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Globalization;
using System.ComponentModel;

public class PersistenceUtils
{
    // Path where the data files are stored
    private static string saveDir
    {
        // Hidden file directory of Unity, see https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
        get { return Application.persistentDataPath; }
    }

    /// <summary>
    /// Get the default data stored in the given profile prefab.
    /// </summary>
    /// <returns>The data from profile.</returns>
    /// <param name="_profileName">The name of the profile to be loaded.</param>
    public static SimpleJSON.JSONClass GetDefaultDataFromProfile(string _profileName = "", string _initialDragonSku=null, string _extraDragonSku=null, string _socialState=null, int _timePlayed=0)
    {
        SimpleJSON.JSONClass _returnValue = null;

        bool _useDefaultProfile = string.IsNullOrEmpty(_profileName) || _profileName == PersistenceProfile.DEFAULT_PROFILE;
        if (_useDefaultProfile)
        {
            // The default profile is created from rules
			// ignore xml to avoid hackers modfying it
			_returnValue = new SimpleJSON.JSONClass();
			DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
			DefinitionNode initialSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "initialSettings");

            if (initialSettingsDef != null)
            {
                //string _sc = initialSettingsDef.Get("softCurrency");
                //string _pc = initialSettingsDef.Get("hardCurrency");
				string _sc = "0";
                string _pc = "0";
                string _gf = initialSettingsDef.Get("goldenFragments");
                if (_initialDragonSku == null)
                {
                    _initialDragonSku = initialSettingsDef.Get("initialDragonSKU");
                }

                // User Profile: sc, pc, currentDragon
                SimpleJSON.JSONClass _userProfile = new SimpleJSON.JSONClass();
                _userProfile.Add("sc", _sc);
                _userProfile.Add("pc", _pc);
                _userProfile.Add("gf", _gf);
                _userProfile.Add("keys", 0);	// [AOC] HARDCODED!!
                _userProfile.Add("currentDragon", _initialDragonSku);
                _userProfile.Add("currentLevel", "level_0");	// Only one level now

                if (_socialState != null)
                    _userProfile.Add("socialState", _socialState);

                _returnValue.Add("userProfile", _userProfile);

                if (_timePlayed > 0)
                {
                    SimpleJSON.JSONClass _user = new SimpleJSON.JSONClass();
                    _user.Add("TimePlayed", _timePlayed);
                    _returnValue.Add("User", _user);
                }

                // Dragons array
                SimpleJSON.JSONArray _dragons = new SimpleJSON.JSONArray();

                // Initial dragon
                SimpleJSON.JSONClass _dragon = new SimpleJSON.JSONClass();
                _dragon.Add("sku", _initialDragonSku);
                _dragon.Add("owned", "true");
                _dragons.Add(_dragon);

                if (!string.IsNullOrEmpty(_extraDragonSku))
                {
                    _dragon = new SimpleJSON.JSONClass();
                    _dragon.Add("sku", _extraDragonSku);
                    _dragon.Add("owned", "true");
                    _dragons.Add(_dragon);
                }
                _returnValue.Add("dragons", _dragons);
            }

            // Shall we start map countdown automatically?
            if (!ABTest.Evaluate(ABTest.Test.INITIAL_MAP_COUNTDOWN_TRIGGER_BY_PLAYER))
            {
                // [AOC] Start with the map unlocked
                // Use default 24hrs timer if the settings rules are not ready
                System.DateTime mapResetTimestamp = GameServerManager.GetEstimatedServerTime().AddHours(24);
                if (gameSettingsDef != null)
                {
                    mapResetTimestamp = GameServerManager.GetEstimatedServerTime().AddMinutes(gameSettingsDef.GetAsDouble("miniMapTimer")); // Minutes
                }
                _returnValue.Add("mapResetTimestamp", mapResetTimestamp.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE));
            }
        }
        else
        {
            // Other profiles are loaded from resources
            TextAsset defaultProfile = Resources.Load<TextAsset>(PersistenceProfile.RESOURCES_FOLDER + _profileName);
            if (defaultProfile != null)
            {
                _returnValue = SimpleJSON.JSON.Parse(defaultProfile.text) as SimpleJSON.JSONClass;
            }
        }

        return _returnValue;
    }    

    /// <summary>
	/// Given the name of a profile, obtain the full path of its associated persistence file.
	/// </summary>
	/// <param name="_profileName">The name of the profile whose path we want.</param>
	private static string GetPersistenceFilePath(string _profileName = "")
    {
        return saveDir + "/" + _profileName + ".sav";
    }

    /// <summary>
	/// Obtain the list of current saved games.
	/// </summary>
	/// <returns>The list of saved games in the persistence dir.</returns>
	public static string[] GetSavedGamesList()
    {
        // C# makes it easy for us
        DirectoryInfo dirInfo = new DirectoryInfo(saveDir);
        FileInfo[] files = dirInfo.GetFiles();

        // Strip filename from full file path
        List<string> fileNames = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Name.EndsWith("sav"))
            {
                fileNames.Add(Path.GetFileNameWithoutExtension(files[i].Name));
            }
        }

        return fileNames.ToArray();
    }

    /// <summary>
    /// Load the game persistence for a specific profile into a new JSON object.
    /// </summary>
    /// <returns>The to JSON where the data is loaded, <c>null</c> if an error ocurred.</returns>
    /// <param name="_profileName">The name of the profile to be loaded.</param>	
    public static SimpleJSON.JSONClass LoadToObject(string _profileName = "")
    {
        // From https://unity3d.com/learn/tutorials/modules/beginner/live-training-archive/persistence-data-saving-loading
        SimpleJSON.JSONClass data = null;
        string path = GetPersistenceFilePath(_profileName);
        if (File.Exists(path))
        {                  
            PersistenceData _persistenceData = new PersistenceData(_profileName);
            _persistenceData.Load(SaveUtilities.GetSavePath(_profileName));
            string profileJSONStr = _persistenceData.ToString();
            data = SimpleJSON.JSON.Parse(profileJSONStr) as SimpleJSON.JSONClass;
        }
        else
        {
            // No saved games found for the given profile, try to load the profile data            
            Debug.Log("No saved games were found, loading profile " + _profileName);

            // Load profile and use it as initial data
            data = GetDefaultDataFromProfile(_profileName);
            if (data == null)
            {
                Debug.Log("Profile " + _profileName + " couldn't be found, starting from 0");
            }
        }

        return data;
    }

    /// <summary>
    /// Deletes local persistence file.
    /// The game should be reloaded afterwards.
    /// </summary>
    /// <param name="_profileName">The name of the profile to be cleared.</param>
    public static void Clear(string _profileName = null)
    {
        // Delete persistence file
        string path = GetPersistenceFilePath(_profileName);
        File.Delete(path);

        // Create a new save file with the default data from the profile
        SimpleJSON.JSONClass data = GetDefaultDataFromProfile(_profileName);
        if (data != null)
        {
            SaveFromObject(_profileName, data);
        }
    }

    /// <summary>
	/// Determines if a given profile name has a persistence file attached to it.
	/// </summary>
	/// <returns><c>true</c> if a saved game exists for the specified _profileName; otherwise, <c>false</c>.</returns>
	/// <param name="_profileName">The name of the profile we want to check.</param>
	public static bool HasSavedGame(string _profileName)
    {
        return File.Exists(GetPersistenceFilePath(_profileName));
    }

    /// <summary>
    /// Save a given data object to persistence file. This method should be called only by the editor.
    /// </summary>
    /// <param name="_profileName">The name of the profile to be saved.</param>
    /// <param name="_data">The data object to be saved.</param>
    public static void SaveFromObject(string _profileName, SimpleJSON.JSONClass _data)
    {
        // The file needs to be encrypted and compressed in order to make it work as it will work on production
        PersistenceData _persistenceData = new PersistenceData(_profileName);
        _persistenceData.Merge(_data.ToString());
        _persistenceData.Save(SaveUtilities.GetSavePath(_profileName));
    }

	//------------------------------------------------------------------------//
	// FORMATTING UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Format the given object into a string representation to inject into Persistence 
	/// and Server Communication JSONs.
	/// </summary>
	/// <param name="_value">The object to be formatted.</param>
	/// <returns>A string representation of the input object, safe for Persistence/Server parsing.</returns>
	public static string SafeToString(IConvertible _value) {
		// IConvertible interface makes it easy for us! ^^
		return _value.ToString(PersistenceFacade.JSON_FORMATTING_CULTURE);
	}

	/// <summary>
	/// Parse the given string using Persistence and Server Comms formatting.
	/// If it fails, try using device's local formatting.
	/// </summary>
	/// <typeparam name="T">The type we want to parse.</typeparam>
	/// <param name="_toParse">The string to be parsed.</param>
	/// <returns>Parsed value. Will be the default value of <typeparamref name="T"/> if parsing not successful.</returns>
	public static T SafeParse<T>(string _toParse) where T : IConvertible {
        // Aux vars
        T val;

        // Try parsing the string
        if(SafeTryParse(_toParse, out val)) {
            return val;         // Success! Return value
        } else {
            return default(T);  // Couldn't parse input string, return default value for this type
        }
    }

    /// <summary>
    /// Parse the given string using Persistnece and Server Comms formatting.
    /// If it fails, try using device's local formatting.
    /// </summary>
    /// <typeparam name="T">The type we want to parse.</typeparam>
    /// <param name="_toParse">The string to be parsed.</param>
    /// <param name="_val">Out variable where the parse result will be stored.</param>
    /// <returns>Whether the parsing was successful or not.</returns>
    public static bool SafeTryParse<T>(string _toParse, out T _val) where T : IConvertible {
        // Try first with invariant culture
        if(TryParse<T>(_toParse, PersistenceFacade.JSON_FORMATTING_CULTURE, out _val)) {
            return true;
        }

        // Try with local culture
        else if(TryParse<T>(_toParse, ApplicationManager.originalCulture, out _val)) {
            return true;
        }

        // Couldn't parse input string, return error
        return false;
    }

	/// <summary>
	/// Generic version of the TryParse method for IConvertibles.
	/// </summary>
	/// <typeparam name="T">The type we want to parse.</typeparam>
	/// <param name="_toParse">The string to be parsed.</param>
	/// <param name="_format">Culture to be used for the parsing.</param>
	/// <param name="_val">Parsed value. Will have the default value of <typeparamref name="T"/> if parsing not successful.</param>
	/// <returns>Whether the parsing was successful or not.</returns>
	public static bool TryParse<T>(string _toParse, IFormatProvider _format, out T _val) where T : IConvertible {
		// [AOC] Because TryParse methods are not generic, have different signatures and don't implement any interface,
		//		 there is no simpler way than going type by type and invoke the corresponding TryParse method.
		//		 Luckily for us, the IConvertible interface makes this a bit easier for us.
		// Based on https://stackoverflow.com/questions/10574504/how-to-use-t-tryparse-in-a-generic-method-while-t-is-either-double-or-int/10575011
		bool success = false;
		T parsedValue = default(T);
		TypeCode typeCode = parsedValue.GetTypeCode();
		switch(typeCode) {
			// aka bool
			case TypeCode.Boolean: {
				success = Boolean.TryParse(_toParse, out Boolean v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka char
			case TypeCode.Char: {
				success = Char.TryParse(_toParse, out Char v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka sbyte
			case TypeCode.SByte: {
				success = SByte.TryParse(_toParse, NumberStyles.Any, _format, out SByte v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka byte
			case TypeCode.Byte: {
				success = Byte.TryParse(_toParse, NumberStyles.Any, _format, out Byte v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka short
			case TypeCode.Int16: {
				success = Int16.TryParse(_toParse, NumberStyles.Any, _format, out Int16 v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka ushort
			case TypeCode.UInt16: {
				success = UInt16.TryParse(_toParse, NumberStyles.Any, _format, out UInt16 v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka int
			case TypeCode.Int32: {
				success = Int32.TryParse(_toParse, NumberStyles.Any, _format, out Int32 v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka uint
			case TypeCode.UInt32: {
				success = UInt32.TryParse(_toParse, NumberStyles.Any, _format, out UInt32 v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka long
			case TypeCode.Int64: {
				success = Int64.TryParse(_toParse, NumberStyles.Any, _format, out Int64 v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka ulong
			case TypeCode.UInt64: {
				success = UInt64.TryParse(_toParse, NumberStyles.Any, _format, out UInt64 v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka float
			case TypeCode.Single: {
				success = Single.TryParse(_toParse, NumberStyles.Any, _format, out Single v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka double
			case TypeCode.Double: {
				success = Double.TryParse(_toParse, NumberStyles.Any, _format, out Double v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			// aka decimal
			case TypeCode.Decimal: {
				success = Decimal.TryParse(_toParse, NumberStyles.Any, _format, out Decimal v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;

			case TypeCode.DateTime: {
				success = DateTime.TryParse(_toParse, _format, DateTimeStyles.None, out DateTime v);
				parsedValue = (T)Convert.ChangeType(v, typeCode);
			} break;
		}

		// Save parsed value to out parameter
		_val = parsedValue;

		// Done! Return success
		return success;
	}
}
