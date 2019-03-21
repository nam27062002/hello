// IAssetsDownloadFlowProgressBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single widget to display the progress of a download.
/// Must be manually refreshed from outside.
/// </summary>
public class AssetsDownloadFlowProgressBar : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private TextMeshProUGUI m_progressText = null;
	[SerializeField] private UIGradient m_progressBarGradient = null;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh to display the progress of the given download handler.
	/// </summary>
	/// <param name="_handle">Handle to be used.</param>
	public void Refresh(Downloadables.Handle _handle) {
		// Just for safety
		if(_handle == null) return;

		// Progress Bar
		if(m_progressBar != null) {
			m_progressBar.normalizedValue = _handle.Progress;
		}

		// Progress Text
		if(m_progressText != null) {
			m_progressText.text = LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION",
				StringUtils.FormatFileSize(_handle.GetDownloadedBytes(), 2),
				StringUtils.FormatFileSize(_handle.GetTotalBytes(), 0)	// No decimals looks better for total size
			);
		}

		// Gradient color
		if(m_progressBarGradient != null) {
			m_progressBarGradient.SetValues(
				AssetsDownloadFlowSettings.GetProgressBarColor(_handle)
			);
		}
	}

	//------------------------------------------------------------------------//
	// DEBUG_METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a color on the bar.
	/// </summary>
	/// <param name="_color">Color to be applied.</param>
	public void DEBUG_SetColor(Gradient4 _color) {
		// Just do it :)
		if(m_progressBarGradient != null) {
			m_progressBarGradient.SetValues(_color);
		}
	}
}