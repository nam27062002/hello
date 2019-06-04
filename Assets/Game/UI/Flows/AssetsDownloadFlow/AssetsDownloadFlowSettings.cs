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


    [Separator("Icons colors")]
    [SerializeField] private Color m_iconErrorColor = Color.red;
    public static Color iconErrorColor {
        get { return instance.m_iconErrorColor; }
    }

    [SerializeField] private Color m_iconDownloadingColor = Color.yellow;
    public static Color iconDownloadingColor {
        get { return instance.m_iconDownloadingColor; }
    }

    [SerializeField] private Color m_iconFinishedColor = Color.green;
    public static Color iconFinishedColor {
        get { return instance.m_iconFinishedColor; }
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
    /// Select the right progress bar color based on handle's download state.
    /// </summary>
    /// <returns>The progress bar color.</returns>
    /// <param name="_handle">Handle to be checked.</param>
    public static Gradient4 GetProgressBarColor(Downloadables.Handle _handle) {
		// Just in case
		if(_handle == null) return progressBarErrorColor;

		// a) Download completed
		if(_handle.IsAvailable()) return progressBarFinishedColor;

		// b) No error
		if(_handle.GetError() == Downloadables.Handle.EError.NONE) return progressBarDownloadingColor;

		// c) Error
		return progressBarErrorColor;
	}
}