// TMP_AssetsGroupData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Temporary class used as skeleton for UI development while the Asset Groups are 
/// being implemented.
/// </summary>
public class TMP_AssetsGroupData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Error {
		NO_WIFI,    // and no data permission granted
		NO_CONNECTION,  // neither wifi nor data
		STORAGE,
		STORAGE_PERMISSION,
		UNKNOWN,

		NONE
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// General data
	public string groupId = "";

	// Size and progress
	public float totalBytes = 100f * 1024f * 1024f;
	public float downloadedBytes = 0f;

	public float progress {
		get { return Mathf.Clamp01((float)downloadedBytes / (float)totalBytes); }
	}

	public bool isDone {
		get { return progress >= 1f; }
	}

	// State and error handling
	public Error error = Error.NONE;

	// Permissions
	public bool dataPermissionRequested = false;
	public bool dataPermissionGranted = false;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// To be called manually to refresh group state, progress, errors, etc.
	/// </summary>
	public void UpdateState() {
		// [AOC] TODO!!
	}
}