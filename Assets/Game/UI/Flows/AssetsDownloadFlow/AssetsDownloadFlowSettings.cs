// DailyRewardViewSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/03/2019.
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
/// Scriptable object to store different shared UI values for the Assets Download Flow.
/// </summary>
//[CreateAssetMenu]
public class AssetsDownloadFlowSettings : SingletonScriptableObject<AssetsDownloadFlowSettings> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Separator("General Settings")]
	[SerializeField] private float m_updateInterval = 0.5f;
	public static float updateInterval {
		get { return instance.m_updateInterval; }
	}

	[Separator("Download Progress Bar Colors")]
	[SerializeField] private Gradient4 m_progressBarErrorColor = new Gradient4();
	public static Gradient4 progressBarErrorColor { 
		get { return instance.m_progressBarErrorColor; } 
	}

	[SerializeField] private Gradient4 m_progressBarDownloadingColor = new Gradient4();
	public static Gradient4 progressBarDownloadingColor {
		get { return instance.m_progressBarDownloadingColor; }
	}

	[SerializeField] private Gradient4 m_progressBarFinishedColor = new Gradient4();
	public static Gradient4 progressBarFinishedColor {
		get { return instance.m_progressBarFinishedColor; }
	}

	[Separator("Other Colors")]
	[SerializeField] private Color m_filesizeTextHighlightColor = Colors.orange;
	public static Color filesizeTextHighlightColor {
		get { return instance.m_filesizeTextHighlightColor; }
	}

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Select the right progress bar color based on group's download state.
	/// </summary>
	/// <returns>The progress bar color.</returns>
	/// <param name="_group">Group to be checked.</param>
	public static Gradient4 GetProgressBarColor(TMP_AssetsGroupData _group) {
		// Just in case
		if(_group == null) return progressBarErrorColor;

		// a) Download completed
		if(_group.isDone) return progressBarFinishedColor;

		// b) No error
		if(_group.error == TMP_AssetsGroupData.Error.NONE) return progressBarDownloadingColor;

		// c) Error
		return progressBarErrorColor;
	}
}