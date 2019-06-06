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
    // ENUMS    															  //
    //------------------------------------------------------------------------//

    [System.Flags]
	public enum PopupType {
		NONE			= 1 << 0,

		MANDATORY		= 1 << 1,
		PERMISSION		= 1 << 2,
		ERROR			= 1 << 3,
		PROGRESS		= 1 << 4,

		// [AOC] Max 32 values (try inheriting from long if more are needed)
		ANY				= ~(0)      // http://stackoverflow.com/questions/7467722/how-to-set-all-bits-of-enum-flag
	}


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed members
    [SerializeField] protected bool m_update = false;

    [Separator("Popup elements")]
    [Comment("Optional depending on layout")]
    [SerializeField] protected Localizer m_titleText = null;
    [SerializeField] protected Localizer m_messageText = null;
	[SerializeField] protected AssetsDownloadFlowProgressBar m_progressBar = null;
    [SerializeField] protected GameObject m_downloadInProgressGroup;
    [SerializeField] protected GameObject m_downloadCompletedGroup;

    [Space (10)]
    [SerializeField] protected Image m_curtain = null;
	[SerializeField] protected GameObject m_teaseInfoGroup = null;

	// Internal
	protected Downloadables.Handle m_handle = null;
    protected AssetsDownloadFlow.Context m_context = AssetsDownloadFlow.Context.NOT_SPECIFIED;
    protected string m_path;

    // Public properties
    public float curtainAlpha {
		get { return m_curtain != null ? m_curtain.color.a : 0f; }
		set { if(m_curtain != null) m_curtain.color = Colors.WithAlpha(m_curtain.color, value); }
	}

	public bool showTeasingInfo {
		get { return m_teaseInfoGroup != null ? m_teaseInfoGroup.activeSelf : false; }
		set { if(m_teaseInfoGroup != null) m_teaseInfoGroup.SetActive(value); }
	}

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
	public virtual void Init(Downloadables.Handle _handle, string _path, AssetsDownloadFlow.Context _context) {

		// Store group
		m_handle = _handle;

        // Keep a record of the prefab we are opening
        m_path = _path;

        // Keep a record of the action that triggered the popup 
        m_context = _context;        

		// Perform a first refresh
		Refresh();
	}

	/// <summary>
	/// Refresh popup's visuals.
	/// </summary>
	public virtual void Refresh() {

        switch (m_path)
        {
            // We are showing a download permission popup
            case PATH_PERMISSION: 
                {
                    
                    if (m_titleText == null || m_messageText == null)
                    {
                        // This shouldnt happen
                        Debug.LogError("The popup must have a title and a body element defined");
                        return;
                    }

                    // Use specific text for each situation
                    switch (m_context)
                    {
                        case AssetsDownloadFlow.Context.PLAYER_BUYS_TRIGGER_DRAGON:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_SMALL_DRAGONS_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_SMALL_DRAGONS_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes() )));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_BUYS_NOT_DOWNLOADED_DRAGON:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_BIG_DRAGONS_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_BIG_DRAGONS_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_PET:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_PETS_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_PETS_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_TOURNAMENT:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_TOURNAMENTS_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_TOURNAMENTS_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.LOADING_SCREEN:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_MAIN_LOADING_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_MAIN_LOADING_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_SKINS:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_SKINS_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_SKINS_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_CLICKS_ANIMOJIS:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_ANIMOJIS_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_ANIMOJIS_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_CLICKS_AR:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_AR_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_AR_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_BUYS_SPECIAL_DRAGON:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_SPECIAL_DRAGONS_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_SPECIAL_DRAGONS_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        case AssetsDownloadFlow.Context.PLAYER_CLICKS_ON_UPGRADE_SPECIAL:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_UPGRADE_SPECIAL_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_UPGRADE_SPECIAL_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }

                        default:
                            {
                                m_titleText.Localize("TID_OTA_PERMISSION_POPUP_TITLE");
                                m_messageText.Localize("TID_OTA_PERMISSION_POPUP_BODY",
                                                        AssetsDownloadFlowSettings.filesizeTextHighlightColor.Tag(
                                                        StringUtils.FormatFileSize(m_handle.GetTotalBytes())));
                                break;
                            }
                    }

                    break;
                }

            case PATH_PROGRESS:
                {

                    
                    if (this.m_progressBar.m_state == AssetsDownloadFlowProgressBar.State.COMPLETED)
                    {
                        // Download succesfully completed
                        m_titleText.Localize("TID_OTA_COMPLETED_TITLE");
                        m_messageText.Localize("TID_OTA_COMPLETED_BODY");

                        // Show the proper buttons
                        m_downloadInProgressGroup.gameObject.SetActive(false);
                        m_downloadCompletedGroup.gameObject.SetActive(true);

                        // If we didnt show the "download completed" popup to the player before
                        if (!Prefs.GetBoolPlayer(AssetsDownloadFlowSettings.OTA_DOWNLOAD_COMPLETE_POPUP_SHOWN, false))
                        {
                            // Mark it as shown, so the popup wont be launched again
                            Prefs.SetBoolPlayer(AssetsDownloadFlowSettings.OTA_DOWNLOAD_COMPLETE_POPUP_SHOWN, true);

                        }

                    }
                    else {
                        // Download Still in progress
                        m_titleText.Localize("TID_OTA_PERMISSION_POPUP_TITLE");
                        m_messageText.Localize("TID_OTA_PERMISSION_POPUP_BODY");

                        // Show the proper buttons
                        m_downloadInProgressGroup.gameObject.SetActive(true);
                        m_downloadCompletedGroup.gameObject.SetActive(false);
                    }

                    break;
                }

            // The rest of the popups
            default:
                {
                    m_messageText.Localize(m_messageText.tid);	// No replacements
                    break;
                }
        }

        /*
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
		}*/

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
    /// <param name="_typeFilterMask">Popup type filter. Multiple types can be filtered using the | operator: <c>TypeMask.MANDATORY | TypeMask.ERROR</c>.</param>
    /// <param name="_context">The situation that triggered the download popup. It will show an adapted message in each case.</param>
    /// <param name="_forcePopup">If true, shows the popup even if the content is already donwloaded</param>
    public static PopupAssetsDownloadFlow OpenPopupByState(Downloadables.Handle _handle, PopupType _typeFilterMask, AssetsDownloadFlow.Context _context, bool _forcePopup = false) {
		// Ignore if handle is not valid
		if(_handle == null) return null;

		// Same if the download has finished
        // Except if we want to force the popup
		if(_handle.IsAvailable() && !_forcePopup) return null;

		// Let's do it!
		string popupPath = string.Empty;

		// Choose popup path based on handle state
		// Has the permission been requested?
		if(_handle.NeedsToRequestPermission()) {
			// No! Open the permission request popup
			popupPath = PATH_PERMISSION;
		} else {
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

		// Check if the selected popup matches the type filter mask
		if(!CheckPopupType(popupPath, _typeFilterMask)) {
			popupPath = string.Empty;
		}

		// Do we have a valid popup path?
		if(!string.IsNullOrEmpty(popupPath)) {
			// Load it
			PopupController popup = PopupManager.LoadPopup(popupPath);

			// Initialize it
			PopupAssetsDownloadFlow flowPopup = popup.GetComponent<PopupAssetsDownloadFlow>();
			flowPopup.Init(_handle, popupPath, _context);

            // Enqueue popup!
            PopupManager.EnqueuePopup(popup);
			return flowPopup;
		}

		return null;
	}

	/// <summary>
	/// Check whether the given popup path passes the given type mask.
	/// </summary>
	/// <returns><c>true</c>, if popup type passes the given type mask, <c>false</c> otherwise.</returns>
	/// <param name="_popupPath">Popup path to be checked.</param>
	/// <param name="_typeMask">Type mask to check against.</param>
	private static bool CheckPopupType(string _popupPath, PopupType _typeMask) {
		// Choose target mask based on popup path
		PopupType targetMask = PopupType.ANY;
		switch(_popupPath) {
			case PATH_PERMISSION: {
				targetMask = PopupType.MANDATORY | PopupType.PERMISSION;
			} break;

			case PATH_PROGRESS: {
				targetMask = PopupType.PROGRESS;
			} break;

			case PATH_ERROR_GENERIC:
			case PATH_ERROR_NO_WIFI:
			case PATH_ERROR_STORAGE:
			case PATH_ERROR_NO_CONNECTION:
			case PATH_ERROR_STORAGE_PERMISSION: {
				targetMask = PopupType.ERROR;
			} break;
		}

		// Compare against input mask
		return (targetMask & _typeMask) != 0;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Dismiss button has been pressed.
	/// </summary>
	public void OnDismiss() {
		// Tracking
		HDTrackingManager.Instance.Notify_PopupOTA(this.name, Downloadables.Popup.EAction.Dismiss);

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

		// Tracking
		HDTrackingManager.Instance.Notify_PopupOTA(this.name, Downloadables.Popup.EAction.Wifi_Only);

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

		// Tracking
		HDTrackingManager.Instance.Notify_PopupOTA(this.name, Downloadables.Popup.EAction.Wifi_Mobile);

		// Close Popup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// Open the device's storage settings and close the popup.
	/// </summary>
	public void OnGoToStorageSettings() {
		// Go to system permissions screen
		PermissionsManager.SharedInstance.OpenPermissionSettings();

		// Tracking
		HDTrackingManager.Instance.Notify_PopupOTA(this.name, Downloadables.Popup.EAction.View_Storage_Options);

		// Close Popup
		GetComponent<PopupController>().Close(true);
	}
}