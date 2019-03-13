// AssetsDownloadFlowProgressUI.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/03/2019.
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
/// Simple UI widget to show the progress of an addressable download.
/// </summary>
public class AssetsDownloadFlow : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const float UPDATE_INTERVAL = 1f;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private TextMeshProUGUI m_progressText = null;
	[SerializeField] private Localizer m_statusText = null;
	[SerializeField] private Localizer m_errorText = null;

	// Internal logic
	private bool m_enabled;
	private TMP_AssetsGroupData m_group = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Program periodic update
		InvokeRepeating("PeriodicUpdate", 0f, UPDATE_INTERVAL);
	}

	/// <summary>
	/// Update at regular intervals.
	/// </summary>
	private void PeriodicUpdate() {
		// Get latest data and refresh!
		RefreshAll(true);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the widget with the given async operation.
	/// </summary>
	/// <param name="_group">Async operation. Use <c>null</c> to hide the widget.</param>
	public void InitWithGroupData(TMP_AssetsGroupData _group) {
		// Store operation
		m_group = _group;
	}

	/// <summary>
	/// Allow to manually enable/disable this component.
	/// Useful to sync with UI animations, transitions, etc.
	/// </summary>
	/// <param name="_enabled">Enable or disable the component?</param>
	public void Toggle(bool _enabled) {
		// Store state
		m_enabled = _enabled;

		// If enabling, force a refresh
		if(_enabled) {
			RefreshAll(true);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Invokes all the Refresh operations in the logic order.
	/// </summary>
	/// <param name="_updateState">Update logic state of the download group?</param>
	private void RefreshAll(bool _updateState) {
		// Update logic state?
		if(_updateState) {
			// Unless group is all ok
			if(m_group != null && !m_group.isDone) {
				m_group.UpdateState();
			}
		}

		// Check visibility
		bool show = RefreshVisibility();

		// Refresh info (if visible)
		if(show) RefreshData();
	}

	/// <summary>
	/// Check whether the widget must be displayed or not based on several conditions.
	/// Applies it to the current object.
	/// </summary>
	/// <returns>Whether the widget must be displayed or not.</returns>
	private bool RefreshVisibility() {
		// Check several conditions
		// Order is relevant!
		bool show = true;

		// If manually disabled, there's nothing else to discuss
		if(!m_enabled) {
			show = false;
		}

		// Don't show if we don't have valid data
		else if(m_group == null) {
			show = false;
		}

		// Don't show if permission hasn't yet been requested (probably downloading in background via wifi)
		else if(!m_group.dataPermissionRequested) {
			show = false;
		}

		// Don't show if download has already finished
		else if(m_group.isDone) {
			show = false;
		}

		// Apply and return
		gameObject.SetActive(show); // [AOC] TODO!! ShowHide Animator?
		return show;
	}

	/// <summary>
	/// Update the visuals with latest data.
	/// </summary>
	private void RefreshData() {
		// Progress Bar
		if(m_progressBar != null) {
			m_progressBar.normalizedValue = m_group.progress;
		}

		// Progress Text
		if(m_progressText != null) {
			m_progressText.text = LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION",
				StringUtils.FormatFileSize(m_group.totalBytes)
			);
		}

		// Status text
		bool hasError = m_group.error != TMP_AssetsGroupData.Error.NONE;
		if(m_statusText != null) {
			// Just show/hide, no custom text for now
			m_statusText.gameObject.SetActive(!hasError);
		}

		// Error text
		if(m_errorText != null) {
			// Show/hide
			m_errorText.gameObject.SetActive(hasError);

			// Set text based on error type
			if(hasError) {
				string errorTid = "TID_OTA_ERROR_GENERIC_TITLE";
				switch(m_group.error) {
					case TMP_AssetsGroupData.Error.NO_WIFI: {
						errorTid = "TID_OTA_ERROR_01_TITLE";
					} break;

					case TMP_AssetsGroupData.Error.NO_CONNECTION: {
						errorTid = "TID_OTA_ERROR_02_TITLE";
					} break;

					case TMP_AssetsGroupData.Error.STORAGE: {
						errorTid = "TID_OTA_ERROR_03_TITLE";
					} break;

					case TMP_AssetsGroupData.Error.STORAGE_PERMISSION: {
						errorTid = "TID_OTA_ERROR_04_TITLE";
					} break;
				}
				m_errorText.Localize("TID_OTA_PROGRESS_BAR_DOWNLOADING_PAUSED", LocalizationManager.SharedInstance.Localize(errorTid));
			}
		}
	}

	/// <summary>
	/// Opens the right popup according to current group's state.
	/// </summary>
	private void OpenPopupByState() {
		// Ignore if group is not valid (shouldn't happen)
		if(m_group == null) return;

		// Same if the group is ready (shouldn't happen either)
		if(m_group.isDone) return;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed. Different actions based on group's state.
	/// </summary>
	public void OnInfoButton() {
		// Just open different popups based on current state
		OpenPopupByState();
	}
}