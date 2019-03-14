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
public class AssetsDownloadFlowPopup : MonoBehaviour {
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
	[Comment("Optional depending on layout")]
	[SerializeField] private Localizer m_messageText = null;
	[SerializeField] private AssetsDownloadFlowProgressBar m_progressBar = null;

	// Internal
	private TMP_AssetsGroupData m_group = null;

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given data.
	/// </summary>
	/// <param name="_group">The assets group used for initialization.</param>
	public virtual void Init(TMP_AssetsGroupData _group) {
		// Store group
		m_group = _group;
	}

	public virtual void Refresh() {
		// Message
		if(m_messageText != null) {
			// Depends on text to be localized (which is defined in the prefab)
			switch(m_messageText.tid) {
				case "TID_OTA_PERMISSION_POPUP_BODY": {
					m_messageText.Localize(
						m_messageText.tid,
						AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
							StringUtils.FormatFileSize(m_group.totalBytes)
						)
					);
				} break;

				case "TID_OTA_ERROR_03_BODY": {
					// Compute missing storage size
					float bytesToFree = m_group.totalBytes - m_group.downloadedBytes;
					m_messageText.Localize(
						m_messageText.tid,
						AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
							StringUtils.FormatFileSize(bytesToFree)
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
			m_progressBar.Refresh(m_group);
		}
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Opens the right popup according to given group's state.
	/// </summary>
	/// <returns></returns>
	public static AssetsDownloadFlowPopup OpenPopupByState(TMP_AssetsGroupData _group) {
		// Ignore if group is not valid
		if(_group == null) return null;

		// Same if the group is ready
		if(_group.isDone) return null;

		// Let's do it!
		string popupPath = string.Empty;

		// Has the permission been requested?
		if(!_group.dataPermissionRequested) {
			// No! Open the permission request popup
			popupPath = PATH_PERMISSION;
			_group.dataPermissionRequested = true;	// Clear flag
		} else {
			// Yes! Check error code
			switch(_group.error) {
				case TMP_AssetsGroupData.Error.NONE: {
					// No error! Show progress popup
					popupPath = PATH_PROGRESS;
				} break;

				case TMP_AssetsGroupData.Error.NO_WIFI: {
					popupPath = PATH_ERROR_NO_WIFI;
				} break;

				case TMP_AssetsGroupData.Error.NO_CONNECTION: {
					popupPath = PATH_ERROR_NO_CONNECTION;
				} break;

				case TMP_AssetsGroupData.Error.STORAGE: {
					popupPath = PATH_ERROR_STORAGE;
				} break;

				case TMP_AssetsGroupData.Error.STORAGE_PERMISSION: {
					popupPath = PATH_ERROR_STORAGE_PERMISSION;
				} break;

				default: {
					// Open generic error popup
					popupPath = PATH_ERROR_STORAGE;
				} break;
			}
		}

		// Do we have a valid popup path?
		if(!string.IsNullOrEmpty(popupPath)) {
			// Load it
			PopupController popup = PopupManager.LoadPopup(popupPath);

			// Initialize it
			AssetsDownloadFlowPopup flowPopup = popup.GetComponent<AssetsDownloadFlowPopup>();
			flowPopup.Init(_group);

			// Open it and return!
			popup.Open();
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
		m_group.dataPermissionGranted = true;

		// Close Popup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// "Wifi and Mobile Data" button has been pressed.
	/// </summary>
	public void OnAllowDataPermission() {
		// Store new settings
		m_group.dataPermissionGranted = false;

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