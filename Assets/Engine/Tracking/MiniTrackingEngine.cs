// MiniTrackingEngine.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class.
/// </summary>
public class TrackingParam {
	public string id = "";
	public System.Object value = null;

	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_id">Param Identifier.</param>
	/// <param name="_value">Param Value.</param>
	public TrackingParam(string _id, System.Object _value) {
		id = _id;
		value = _value;
	}

	/// <summary>
	/// Create a string representation of the parameter, using the following format: "id:value".
	/// </summary>
	/// <returns>The string representation of the parameter.</returns>
	public string ToString(bool _withID) {
		return (_withID ? id + ":" : "") 
			+ (value == null ? "" : value.ToString());
	}
}

/// <summary>
/// Simple class to do a some local tracking for early play-tests, etc.
/// Not meant to be used as a main tracking tool.
/// </summary>
public class MiniTrackingEngine : Singleton<MiniTrackingEngine> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Tracking session vars
	private string m_userID = null;
	private int m_sessionNb = -1;
	private int m_sessionEventCount = 0;

	// Aux vars
	private StringBuilder m_stringBuilder = null;

	// Others
	public static string filePath {
		get { return Application.persistentDataPath + "/miniTracking.csv"; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// SINGLETON STATIC METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Starts a new tracking session.
	/// </summary>
	/// <param name="_userID">ID to be used to track this user.</param>
	public static void InitSession(string _userID) {
		// Store new user ID
		instance.m_userID = _userID;

		// Find out session number for this user and automatically increase it
		instance.m_sessionNb = PlayerPrefs.GetInt("MINI_TRACKING_ENGINE_SESSION_NB_FOR_USER_" + _userID, 0);
		instance.m_sessionNb++;
		PlayerPrefs.SetInt("MINI_TRACKING_ENGINE_SESSION_NB_FOR_USER_" + _userID, instance.m_sessionNb);

		Debug.Log("<color=green>Tracking session started with user id " + _userID + "\nSession nb: " + instance.m_sessionNb + "</color>");

		// Reset event counter
		instance.m_sessionEventCount = 0;
	}

	/// <summary>
	/// Adds an event entry to the list.
	/// User ID, timestamp and session number are automatically added.
	/// </summary>
	/// <param name="_eventID">Event ID.</param>
	/// <param name="_params">Parameters.</param>
	public static void TrackEvent(string _eventID, params TrackingParam[] _params) {
		// Is session initialized?
		if(instance.m_userID == null) {
			Debug.LogError("Attempting to track an event but session has not been started.");
			return;
		}

		// If file is not yet created, do it now!
		StreamWriter fileWriter = null;
		string path = filePath;
		if(!File.Exists(path)) {
			fileWriter = File.CreateText(path);
		} else {
			fileWriter = File.AppendText(path);
		}

		// Check for errors
		if(fileWriter == null) {
			Debug.LogError("Unable to open file " + path);
			return;
		}

		// Increase event counter
		instance.m_sessionEventCount++;

		// Attach default parameters
		List<TrackingParam> paramsList = new List<TrackingParam>();
		paramsList.Add(new TrackingParam("timestamp", DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)));
		paramsList.Add(new TrackingParam("platform", Application.platform.ToString()));
		paramsList.Add(new TrackingParam("device", SystemInfo.deviceModel));
		paramsList.Add(new TrackingParam("version", GameSettings.internalVersion.ToString()));
		paramsList.Add(new TrackingParam("user_id", instance.m_userID));
		paramsList.Add(new TrackingParam("session_nb", instance.m_sessionNb.ToString(CultureInfo.InvariantCulture)));
		paramsList.Add(new TrackingParam("session_event_idx", instance.m_sessionEventCount.ToString(CultureInfo.InvariantCulture)));
		paramsList.Add(new TrackingParam("event_id", _eventID));

		// Add extra parameters
		paramsList.AddRange(_params);

		// Do it!
		fileWriter.WriteLine(CreateEventString(paramsList));
		fileWriter.Close();

		// Debug
		LogEvent(_eventID, paramsList);
	}

	/// <summary>
	/// Delete tracking file. Use with caution! All tracking data will be lost!
	/// </summary>
	public static void DeleteTrackingFile() {
		string path = filePath;
		if(File.Exists(path)) {
			File.Delete(path);
		}
	}

	/// <summary>
	/// Get all the content from the tracking file.
	/// Careful, might be quite large!
	/// </summary>
	/// <returns>The tracking file content.</returns>
	public static string ReadTrackingFile() {
		// Nothing to do if file doesn't exist
		string path = filePath;
		if(!File.Exists(path)) return "";

		// Open file for reading and return its content
		StreamReader reader = File.OpenText(path);
		string res = reader.ReadToEnd();
		reader.Close();
		return res;
	}

    /// <summary>
    /// Send the tracking file to our server.
    /// </summary>
    /// <param name="onDone">Callback called once the operation is done. The parameter of this callback states whether or not
    /// the operation has been performed successfully.</param>
    public static void SendTrackingFile(bool silent, System.Action<FGOL.Server.Error, Dictionary<string, object>> onDone) {
        string trackingData = ReadTrackingFile();
        GameServerManager.SharedInstance.SendPlayTest(silent, instance.m_userID, trackingData, onDone);            
    }    

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create the string for a new event.
	/// User ID, timestamp and session number are automatically added.
	/// </summary>
	/// <returns>The event string.</returns>
	/// <param name="_params">Parameters.</param>
	private static string CreateEventString(List<TrackingParam> _params) {
		// If string builder is not created, do it now
		if(instance.m_stringBuilder == null) {
			instance.m_stringBuilder = new StringBuilder();
		}

		// Use the string builder to compose the string
		// Clear it
		instance.m_stringBuilder.Length = 0;

		// Attach rest of the params
		for(int i = 0; i < _params.Count; i++) {
			instance.m_stringBuilder.Append(_params[i].ToString(false)).Append(";");
		}

		// Last special parameter containing all the labels for each parameter
		for(int i = 0; i < _params.Count; i++) {
			instance.m_stringBuilder.Append(_params[i].id);
			if(i < _params.Count - 1) {
				instance.m_stringBuilder.Append(",");	// Comma separated
			}
		}

		// Done!
		return instance.m_stringBuilder.ToString();
	}

	/// <summary>
	/// Show event to the console. Debug purposes only.
	/// </summary>
	/// <returns>The event string.</returns>
	/// <param name="_eventID">Event I.</param>
	/// <param name="_params">Parameters.</param>
	private static void LogEvent(string _eventID, List<TrackingParam> _params) {
		// If string builder is not created, do it now
		if(instance.m_stringBuilder == null) {
			instance.m_stringBuilder = new StringBuilder();
		}

		// Use the string builder to compose the string
		// Clear it
		instance.m_stringBuilder.Length = 0;

		// Attach event ID
		instance.m_stringBuilder.Append("<b>").Append(_eventID).AppendLine("</b>");

		// Params
		for(int i = 0; i < _params.Count; i++) {
			instance.m_stringBuilder.AppendLine(_params[i].ToString(true));
		}

		// Done!
		Debug.Log(instance.m_stringBuilder.ToString());
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}