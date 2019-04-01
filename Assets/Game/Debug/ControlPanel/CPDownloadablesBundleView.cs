// CPDownloadablesBundleView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class CPDownloadablesBundleView : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private TMP_Text m_nameText = null;
	[SerializeField] private Slider m_downloadProgressBar = null;
	[SerializeField] private TMP_Text m_downloadProgressText = null;

	[Space]
	[SerializeField] private TMP_Text m_errorText = null;
	[SerializeField] private Color m_errorTextColor = Colors.red;
	private Color m_errorTextColorNoError = Colors.black;

	[Space]
	[SerializeField] private Image m_barFill = null;
	[SerializeField] private Color m_barColorError = Colors.pink;
	[SerializeField] private Color m_barColorProgress = Colors.paleYellow;
	[SerializeField] private Color m_barColorFinished = Colors.paleGreen;

	// Internal
	private Downloadables.Error.EType m_errorType = Downloadables.Error.EType.None;
	private Downloadables.CatalogEntryStatus m_entry = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Save some default values
		m_errorTextColorNoError = m_errorText.color;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Refresh data
		Refresh(false);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this view with a given bundle data.
	/// </summary>
	/// <param name="_entry">Data to be used.</param>
	public void InitWithData(Downloadables.CatalogEntryStatus _entry) {
		// Store data
		m_entry = _entry;

		// If entry is null, just hide ourselves
		if(m_entry == null) {
			gameObject.SetActive(false);
		} else {
			gameObject.SetActive(true);

			// Initialize name
			m_nameText.text = m_entry.Id;

			// Initialize progress bar
			m_downloadProgressBar.minValue = 0f;
			m_downloadProgressBar.maxValue = m_entry.GetTotalBytes();

			// Perform a first refresh
			Refresh(true);
		}
	}

	/// <summary>
	/// Update everything with latest data from the bundle.
	/// </summary>
	/// <param name="_forced">If set to <c>false</c>, visuals will only be changed if values are different from the displayed ones.</param>
	private void Refresh(bool _forced) {
		// Nothing to do if we don't have valid data
		if(m_entry == null) return;

		// Error
		Downloadables.Error.EType errorType = Downloadables.Error.EType.None;
		if(m_entry == null) {
			errorType = Downloadables.Error.EType.Internal_NotAvailable;
		} else {
            errorType = m_entry.GetErrorBlockingDownload();
            if (errorType == Downloadables.Error.EType.None && m_entry.LatestError != null) {
                errorType = m_entry.LatestError.Type;
            }
		}

		if(!_forced) {
			_forced = errorType != m_errorType;
		}

		if(_forced) {
			m_errorType = errorType;
			m_errorText.text = "Error: " + errorType.ToString();
			m_errorText.color = GetErrorColor(m_errorType);
		}

		// Progress bar
		if(m_downloadProgressBar != null) {
			// Value
			m_downloadProgressBar.value = m_entry.GetBytesDownloadedSoFar();

			// Color based on state
			if(errorType != Downloadables.Error.EType.None) {
				m_barFill.color = m_barColorError;
			} else if(m_entry.IsAvailable(false)) {
				m_barFill.color = m_barColorFinished;
			} else {
				m_barFill.color = m_barColorProgress;
			}
		}

		// Progress text
		if(m_downloadProgressText != null) {
			// 300 KB/800 MB
			m_downloadProgressText.text =
				StringUtils.FormatFileSize(m_entry.GetBytesDownloadedSoFar(), 2) +
				"/" +
				StringUtils.FormatFileSize(m_entry.GetTotalBytes(), 0); // No decimals looks better for total size
		}
	}

	/// <summary>
	/// Auxiliar method to get a text color for the given error.
	/// </summary>
	/// <returns>The text color to be used.</returns>
	/// <param name="_errorType">Error type to be checked.</param>
	private Color GetErrorColor(Downloadables.Error.EType _errorType) {
		return _errorType == Downloadables.Error.EType.None ? m_errorTextColorNoError : m_errorTextColor;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Delete button has been pressed.
	/// </summary>
	public void OnDelete() {
		if(m_entry != null) {
			m_entry.DeleteDownload();
		}
	}
}