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

    //----------------------------------------------------------------------------//
    // ENUM 																	  //
    //----------------------------------------------------------------------------//
    public enum Context
    {

        NOT_SPECIFIED,
        PLAYER_BUYS_TRIGGER_DRAGON,
        PLAYER_BUYS_NOT_DOWNLOADED_DRAGON,
        PLAYER_CLICKS_ON_PET,
        PLAYER_CLICKS_ON_TOURNAMENT,
        LOADING_SCREEN,
        PLAYER_CLICKS_ON_SKINS,
        PLAYER_CLICKS_ANIMOJIS,
        PLAYER_CLICKS_AR,
        PLAYER_BUYS_SPECIAL_DRAGON

    }

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
    [Header("References")]
	[SerializeField] private ShowHideAnimator m_root = null;
	[Space]
	[SerializeField] private AssetsDownloadFlowProgressBar m_progressBar = null;
	[Space]
	[SerializeField] private GameObject m_downloadingGroup = null;
	[Space]
    [SerializeField] private GameObject m_downloadCompletedGroup = null;
	[Space]
    [SerializeField] private GameObject m_errorGroup = null;
	[SerializeField] private Localizer m_errorText = null;

	[Space]
	[Header("Setup")]
	[SerializeField] private bool m_hideOnPopup = false;
	[SerializeField] private float m_popupCurtainAlpha = 0.75f;
	[SerializeField] private bool m_popupShowTeasingInfo = false;

	// Internal logic
	private bool m_enabled = true;
	private Downloadables.Handle m_handle = null;
	public Downloadables.Handle handle {
		get { return m_handle; }
	}
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
		ClearPopup();
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
	public PopupAssetsDownloadFlow OpenPopupIfNeeded(Context _context = Context.NOT_SPECIFIED) {
		// Not if not enabled
		if(!m_enabled) return null;

		// Open popup based on handle's state
		return OpenPopupByState(PopupAssetsDownloadFlow.PopupType.MANDATORY, _context);
	}

    /// <summary>
    /// Checks whether a popup needs to be opened with the current handle.
    /// If so, puts it in the queue and replaces any popup previously queued by this component.
    /// </summary>
    /// <param name="_typeFilterMask">Popup type filter. Multiple types can be filtered using the | operator: <c>TypeMask.MANDATORY | TypeMask.ERROR</c>.</param>
    /// <param name="_context">The situation that triggered the download popup. It will show an adapted message in each case.</param>
    /// <returns>The opened popup if any was needed.</returns>
    public PopupAssetsDownloadFlow OpenPopupByState(PopupAssetsDownloadFlow.PopupType _typeFilterMask, Context _context = Context.NOT_SPECIFIED) {
		// [AOC] TODO!! Ideally, if the popup we're gonna open is the same we already have opened (and for the same handle), do nothing
		//				For now we'll just replace the old popup by a new clone.

		// Nothing to open if not enabled
		if(!m_enabled) return null;

		// Whatever the result, if we already queued a popup, remove it now from the queue
		ClearPopup();

		// Do we need to open a popup?
		PopupAssetsDownloadFlow downloadPopup = PopupAssetsDownloadFlow.OpenPopupByState(m_handle, _typeFilterMask, _context);
		if(downloadPopup != null) {
			// Yes! Store its controller
			m_queuedPopup = downloadPopup.GetComponent<PopupController>();
			m_queuedPopup.OnClose.AddListener(OnPopupClosed);
			m_queuedPopup.OnDestroyed.AddListener(OnPopupClosed);   // In case the popup is destroyed while queued

			// Setup popup
			downloadPopup.curtainAlpha = m_popupCurtainAlpha;
			downloadPopup.showTeasingInfo = m_popupShowTeasingInfo;
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

        // Is the download completed?
        if(m_progressBar.m_state == AssetsDownloadFlowProgressBar.State.COMPLETED)
        {
            // If we didnt show the "download completed" popup to the player yet
            if (! Prefs.GetBoolPlayer(AssetsDownloadFlowSettings.OTA_DOWNLOAD_COMPLETE_POPUP_SHOWN, false) )
            {
                // Open it now it now
                OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY);                

            }
        }
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

		// Depending on the setup, don't show if a popup is open
		else if(m_hideOnPopup && m_queuedPopup != null && m_queuedPopup.isOpen) {
			//Debug.Log(Color.magenta.Tag("Popup open"));
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
                m_downloadCompletedGroup.SetActive(m_handle.Progress >= 1f);
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

	/// <summary>
	/// Close opened popup and nullify it.
	/// </summary>
	private void ClearPopup() {
		if(m_queuedPopup != null) {
			PopupManager.RemoveFromQueue(m_queuedPopup, true);
			m_queuedPopup = null;
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
		OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY);
	}

	/// <summary>
	/// The queued popup has been closed.
	/// </summary>
	/// <param name="_popup">The popup that triggered the event.</param>
	private void OnPopupClosed(PopupController _popup) {
		// Is it the tracked popup?
		if(_popup == m_queuedPopup) {
			// Stop tracking
			m_queuedPopup = null;
		}

		// Refresh visibility
		RefreshVisibility();
	}
}