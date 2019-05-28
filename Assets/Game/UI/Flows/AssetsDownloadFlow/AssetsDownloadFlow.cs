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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ShowHideAnimator m_root = null;
	[Space]
	[SerializeField] private AssetsDownloadFlowProgressBar m_progressBar = null;
	[Space]
	[SerializeField] private GameObject m_downloadingGroup = null;
    [SerializeField] private GameObject m_downloadCompletedGroup = null;
    [SerializeField] private GameObject m_errorGroup = null;
	[SerializeField] private Localizer m_errorText = null;

	// Internal logic
	private bool m_enabled = true;
	private Downloadables.Handle m_handle = null;
	private PopupController m_queuedPopup = null;   // We'll only allow one popup per flow
	private bool m_restartAnim = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Program periodic update
		InvokeRepeating("PeriodicUpdate", 0f, AssetsDownloadFlowSettings.updateInterval);
	}

	/// <summary>
	/// Update at regular intervals.
	/// </summary>
	private void PeriodicUpdate() {
		// Get latest data and refresh!
		RefreshAll(true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Cancel periodic update
		CancelInvoke("PeriodicUpdate");

		// Clear linked popup (if any)
		if(m_queuedPopup != null) {
			PopupManager.RemoveFromQueue(m_queuedPopup, true);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the widget with the given async operation.
	/// </summary>
	/// <param name="_handle">Group download handler. Use <c>null</c> to hide the widget.</param>
	public void InitWithHandle(Downloadables.Handle _handle) {
		// If different than previous handle, restart animation
		if(m_handle != _handle) {
			m_restartAnim = true;
		}

		// Store operation
		m_handle = _handle;

		// Force a first refresh
		RefreshAll(true);
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

	/// <summary>
	/// Checks whether a popup needs to be opened with the current handle.
	/// </summary>
	/// <returns>The opened popup if any was needed.</returns>
	public PopupAssetsDownloadFlow OpenPopupIfNeeded() {
		// Not if not enabled
		if(!m_enabled) return null;

		// Open popup based on handle's state
		return OpenPopupByState(true);
	}

	/// <summary>
	/// Checks whether a popup needs to be opened with the current handle.
	/// If so, puts it in the queue and replaces any popup previously queued by this component.
	/// </summary>
	/// <param name="_onlyMandatoryPopups">Only open the popup if it is mandatory. i.e. "In Progress" popup won't be triggered if this parameter is set to <c>true</c>.</param>
	/// <returns>The opened popup if any was needed.</returns>
	public PopupAssetsDownloadFlow OpenPopupByState(bool _onlyMandatoryPopups) {
		// [AOC] TODO!! Ideally, if the popup we're gonna open is the same we already have opened (and for the same handle), do nothing
		//				For now we'll just replace the old popup by a new clone.

		// Nothing to open if not enabled
		if(!m_enabled) return null;

		// Whatever the result, if we already queued a popup, remove it now from the queue
		if(m_queuedPopup != null) {
			PopupManager.RemoveFromQueue(m_queuedPopup, true);
		}

		// Do we need to open a popup?
		PopupAssetsDownloadFlow downloadPopup = PopupAssetsDownloadFlow.OpenPopupByState(m_handle, _onlyMandatoryPopups);
		if(downloadPopup != null) {
			// Yes! Store its controller
			m_queuedPopup = downloadPopup.GetComponent<PopupController>();
		}

		// Return newly opened popup
		return downloadPopup;
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
			if(m_handle != null && !m_handle.IsAvailable()) {
				m_handle.Update();
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
			//Debug.Log(Color.magenta.Tag("m_enabled false!"));
			show = false;
		}

		// Don't show if we don't have valid data
		else if(m_handle == null) {
			//Debug.Log(Color.magenta.Tag("m_handle is NULL"));
			show = false;
		}

		// Don't show if permission hasn't yet been requested (we will trigger the popup)
		else if(m_handle.NeedsToRequestPermission()) {
			//Debug.Log(Color.magenta.Tag("needs to request permission"));
			show = false;
		}

		// Don't show if download has already finished
		else if(m_handle.IsAvailable()) {
			//Debug.Log(Color.magenta.Tag("download finished"));
			show = false;
		}

		else {
			//Debug.Log(Color.green.Tag("flow needs displaying!"));
		}

		// Apply - Restart animation?
		// Only restart when showing!
		if(show && m_restartAnim) {
			m_root.RestartSet(show);
		} else {
			m_root.Set(show);
		}
		m_restartAnim = false;  // Reset flag

		// Done!
		return show;
	}

	/// <summary>
	/// Update the visuals with latest data.
	/// </summary>
	private void RefreshData() {
		// Progress Bar
		if(m_progressBar != null) {
			m_progressBar.Refresh(m_handle);
            
            if (m_downloadCompletedGroup != null)
            {
                // Hide the download complete icon
                m_downloadCompletedGroup.SetActive(m_handle.Progress == 1);
            }
		}

		// Downloading group
		bool hasError = m_handle.GetError() != Downloadables.Handle.EError.NONE;
		if(m_downloadingGroup != null) {
			// Just show/hide, no custom text for now
			m_downloadingGroup.SetActive(!hasError);
		}

		// Error group
		if(m_errorGroup != null) {
			// Show/hide
			m_errorGroup.SetActive(hasError);
		}

		// Error text
		if(m_errorText != null) {
			// Set text based on error type
			if(hasError) {
				string errorTid = "TID_OTA_ERROR_GENERIC_TITLE";
				switch(m_handle.GetError()) {
					case Downloadables.Handle.EError.NO_WIFI: {
						errorTid = "TID_OTA_ERROR_01_TITLE";
					} break;

					case Downloadables.Handle.EError.NO_CONNECTION: {
						errorTid = "TID_OTA_ERROR_02_TITLE";
					} break;

					case Downloadables.Handle.EError.STORAGE: {
						errorTid = "TID_OTA_ERROR_03_TITLE";
					} break;

					case Downloadables.Handle.EError.STORAGE_PERMISSION: {
						errorTid = "TID_OTA_ERROR_04_TITLE";
					} break;
				}
				m_errorText.Localize("TID_OTA_PROGRESS_BAR_DOWNLOADING_PAUSED", LocalizationManager.SharedInstance.Localize(errorTid));
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The info button has been pressed. Different actions based on group's state.
	/// </summary>
	public void OnInfoButton() {
		// Just open different popups based on current state
		OpenPopupByState(false);
	}
}