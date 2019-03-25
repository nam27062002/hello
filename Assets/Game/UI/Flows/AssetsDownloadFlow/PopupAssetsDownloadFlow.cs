// IAssetsDownloadFlowPopup.cs
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
/// Auxiliar popup to the Assets Download Flow.
/// </summary>
public class PopupAssetsDownloadFlow : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH_PERMISSION = 				"UI/Popups/AssetsDownloadFlow/PF_PopupAssetsDownloadPermission";
	public const string PATH_PROGRESS = 				"UI/Popups/AssetsDownloadFlow/PF_PopupAssetsDownloadProgress";
	public const string PATH_ERROR_NO_WIFI = 			"UI/Popups/AssetsDownloadFlow/PF_PopupAssetsDownloadErrorNoWifi";
	public const string PATH_ERROR_NO_CONNECTION = 		"UI/Popups/AssetsDownloadFlow/PF_PopupAssetsDownloadErrorNoConnection";
	public const string PATH_ERROR_STORAGE = 			"UI/Popups/AssetsDownloadFlow/PF_PopupAssetsDownloadErrorStorage";
	public const string PATH_ERROR_STORAGE_PERMISSION = "UI/Popups/AssetsDownloadFlow/PF_PopupAssetsDownloadErrorStoragePermission";
	public const string PATH_ERROR_GENERIC = 			"UI/Popups/AssetsDownloadFlow/PF_PopupAssetsDownloadErrorGeneric";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] protected bool m_update = false;
	[Comment("Optional depending on layout")]
	[SerializeField] protected Localizer m_messageText = null;
	[SerializeField] protected AssetsDownloadFlowProgressBar m_progressBar = null;

	// Internal
	protected Downloadables.Handle m_handle = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	protected void Start() {
		// Program periodic update if needed
		if(m_update) {
			InvokeRepeating("PeriodicUpdate", 0f, AssetsDownloadFlowSettings.updateInterval);
		}
	}

	/// <summary>
	/// Update at regular intervals.
	/// </summary>
	protected void PeriodicUpdate() {
		// Refresh popup's content
		Refresh();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given data.
	/// </summary>
	/// <param name="_handle">The assets group used for initialization.</param>
	public virtual void Init(Downloadables.Handle _handle) {
		// Store group
		m_handle = _handle;
	}

	/// <summary>
	/// Refresh popup's visulas.
	/// </summary>
	public virtual void Refresh() {
		// Message
		if(m_messageText != null) {
			// Depends on text to be localized (which is defined in the prefab)
			switch(m_messageText.tid) {
				case "TID_OTA_PERMISSION_POPUP_BODY": {
					m_messageText.Localize(
						m_messageText.tid,
						AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
							StringUtils.FormatFileSize(m_handle.GetTotalBytes())
						)
					);
				} break;

				case "TID_OTA_ERROR_03_BODY": {
					// Compute missing storage size
					m_messageText.Localize(
						m_messageText.tid,
						AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
							StringUtils.FormatFileSize(m_handle.GetDiskOverflowBytes())
						)
					);
				} break;

				default: {
					m_messageText.Localize(m_messageText.tid);	// No replacements
				} break;
			}
		}

		// Progress Bar
		if(m_progressBar != null) {
			m_progressBar.Refresh(m_handle);
		}
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Opens the right popup according to given handle's state.
	/// </summary>
	/// <returns>The opened popup, if any. <c>null</c> if no popup was opened.</returns>
	/// <param name="_handle">The download handle whose information we will be using.</param>
	/// <param name="_onlyIfMandatory">Only open the popup if it is mandatory. i.e. "In Progress" popup won't be triggered if this parameter is set to <c>true</c>.</param>
	public static PopupAssetsDownloadFlow OpenPopupByState(Downloadables.Handle _handle, bool _onlyIfMandatory) {
		// Ignore if handle is not valid
		if(_handle == null) return null;

		// Same if the download has finished
		if(_handle.IsAvailable()) return null;

		// Let's do it!
		string popupPath = string.Empty;

		// Has the permission been requested?
		if(_handle.NeedsToRequestPermission()) {
			// No! Open the permission request popup
			popupPath = PATH_PERMISSION;
		} else {
			// Error popups are not mandatory (so far)
			if(!_onlyIfMandatory) {
				// Yes! Check error code
				switch(_handle.GetError()) {
					case Downloadables.Handle.EError.NONE: {
						popupPath = PATH_PROGRESS;
					} break;

					case Downloadables.Handle.EError.NO_WIFI: {
						popupPath = PATH_ERROR_NO_WIFI;
					} break;

					case Downloadables.Handle.EError.NO_CONNECTION: {
						popupPath = PATH_ERROR_NO_CONNECTION;
					} break;

					case Downloadables.Handle.EError.STORAGE: {
						popupPath = PATH_ERROR_STORAGE;
					} break;

					case Downloadables.Handle.EError.STORAGE_PERMISSION: {
						popupPath = PATH_ERROR_STORAGE_PERMISSION;
					} break;

					default: {
						popupPath = PATH_ERROR_GENERIC;     // Open generic error popup
					} break;
				}
			}
		}

		// Do we have a valid popup path?
		if(!string.IsNullOrEmpty(popupPath)) {
			// Load it
			PopupController popup = PopupManager.LoadPopup(popupPath);

			// Initialize it
			PopupAssetsDownloadFlow flowPopup = popup.GetComponent<PopupAssetsDownloadFlow>();
			flowPopup.Init(_handle);

			// Enqueue popup!
			PopupManager.EnqueuePopup(popup);
			return flowPopup;
		}

		return null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dismiss button has been pressed.
	/// </summary>
	public void OnDismiss() {
		// Just close the popup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// "Wifi Only" button has been pressed.
	/// </summary>
	public void OnDenyDataPermission() {
		// Store new settings
		m_handle.SetIsPermissionRequested(true);
		m_handle.SetIsPermissionGranted(false);

		// Close Popup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// "Wifi and Mobile Data" button has been pressed.
	/// </summary>
	public void OnAllowDataPermission() {
		// Store new settings
		m_handle.SetIsPermissionRequested(true);
		m_handle.SetIsPermissionGranted(true);

		// Close Popup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// Open the device's storage settings and close the popup.
	/// </summary>
	public void OnGoToStorageSettings() {
		// Go to system permissions screen
		PermissionsManager.SharedInstance.OpenPermissionSettings();

		// Close Popup
		GetComponent<PopupController>().Close(true);
	}
}