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
	private string m_userID = null;
	private int m_sessionNb = -1;
	private int m_sessionEventCount = 0;

	private StringBuilder m_stringBuilder = null;

	//------------------------------------------------------------------------//
	// GENEROC METHODS														  //
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
		string path = Application.persistentDataPath + "/miniTracking.csv";
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

		// Do it!
		fileWriter.WriteLine(CreateEventString(_eventID, _params));
		fileWriter.Close();

		// Debug
		LogEvent(_eventID, _params);
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create the string for a new event.
	/// User ID, timestamp and session number are automatically added.
	/// </summary>
	/// <returns>The event string.</returns>
	/// <param name="_eventID">Event I.</param>
	/// <param name="_params">Parameters.</param>
	private static string CreateEventString(string _eventID, TrackingParam[] _params) {
		// If string builder is not created, do it now
		if(instance.m_stringBuilder == null) {
			instance.m_stringBuilder = new StringBuilder();
		}

		// Use the string builder to compose the string
		// Clear it
		instance.m_stringBuilder.Length = 0;

		// Attach common info
		instance.m_stringBuilder
			.Append(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)).Append(";")
			.Append(Application.platform.ToString()).Append(";")
			.Append(SystemInfo.deviceModel).Append(";")
			.Append(GameSettings.internalVersion).Append(";")
			.Append(instance.m_userID).Append(";")
			.Append(instance.m_sessionNb.ToString(CultureInfo.InvariantCulture)).Append(";")
			.Append(instance.m_sessionEventCount.ToString(CultureInfo.InvariantCulture)).Append(";")
			.Append(_eventID).Append(";")
		;

		// Params
		for(int i = 0; i < _params.Length; i++) {
			instance.m_stringBuilder.Append(_params[i].ToString(false)).Append(";");
		}

		// Param labels
		for(int i = 0; i < _params.Length; i++) {
			instance.m_stringBuilder.Append(_params[i].id);
			if(i < _params.Length - 1) {
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
	private static void LogEvent(string _eventID, TrackingParam[] _params) {
		// If string builder is not created, do it now
		if(instance.m_stringBuilder == null) {
			instance.m_stringBuilder = new StringBuilder();
		}

		// Use the string builder to compose the string
		// Clear it
		instance.m_stringBuilder.Length = 0;

		// Attach common info
		instance.m_stringBuilder
			.Append("<b>").Append(_eventID).AppendLine("</b>")
			.Append("Timestamp: ")
			.AppendLine(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture))
			.Append("Platform: ")
			.AppendLine(Application.platform.ToString())
			.Append("Device: ")
			.AppendLine(SystemInfo.deviceModel)
			.Append("Version: ")
			.AppendLine(GameSettings.internalVersion.ToString())
			.Append("UserID: ")
			.AppendLine(instance.m_userID)
			.Append("Session Nb: ")
			.AppendLine(instance.m_sessionNb.ToString(CultureInfo.InvariantCulture))
			.Append("Session Event Idx: ")
			.AppendLine(instance.m_sessionEventCount.ToString(CultureInfo.InvariantCulture))
			.Append("Event ID: ")
			.AppendLine(_eventID)
		;

		// Params
		for(int i = 0; i < _params.Length; i++) {
			instance.m_stringBuilder.AppendLine(_params[i].ToString(true));
		}

		// Done!
		Debug.Log(instance.m_stringBuilder.ToString());
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}