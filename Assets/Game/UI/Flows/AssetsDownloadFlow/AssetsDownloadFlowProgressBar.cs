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
	/// Refresh to display the progress of the given group.
	/// </summary>
	/// <param name="_group">Group.</param>
	public void Refresh(TMP_AssetsGroupData _group) {
		// Just for safety
		if(_group == null) return;

		// Progress Bar
		if(m_progressBar != null) {
			m_progressBar.normalizedValue = _group.progress;
		}

		// Progress Text
		if(m_progressText != null) {
			m_progressText.text = LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION",
				StringUtils.FormatNumber(_group.downloadedBytes, 2),
				StringUtils.FormatFileSize(_group.totalBytes, 0)	// No decimals looks better for total size
			);
		}

		// Gradient color
		if(m_progressBarGradient != null) {
			m_progressBarGradient.SetValues(
				AssetsDownloadFlowSettings.GetProgressBarColor(_group)
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