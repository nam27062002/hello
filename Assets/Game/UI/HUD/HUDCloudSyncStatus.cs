// HUDCloudSyncStatus.cs
// Hungry Dragon
// 
// Created by David Germade on 12nd September 2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

using UnityEngine;

/// <summary>
/// This class is responsible for handling the widget shown in hud in different screens to let the user know whether or not the local persistence is in sync with the cloud persistence
/// Possible states:
/// 1)Invisible: It means that the user hasn't logged in the social platform since she installed the game.
/// 2)Not in sync
/// 3)In sync
/// </summary>
public class HUDCloudSyncStatus : MonoBehaviour
{
    /// <summary>
    /// This variable is used to disable this widget easily. It's static in order to make it accessible easily
    /// </summary>
    private static bool sm_clickIsEnabled = true;
    private static bool ClickIsEnabled
    {
        get
        {
            return sm_clickIsEnabled;
        }

        set
        {
            sm_clickIsEnabled = value;
        }
    }

    private bool CloudSaveIsSynced { get; set; }

    private bool CloudSaveIsEnabled { get; set; }

    private bool IsPopupOpen { get; set; }

    [SerializeField]
    private GameObject m_syncSuccessSprite = null;

    [SerializeField]
    private GameObject m_syncFailedSprite = null;

    void Awake()
    {
        // It shouldn't be visible by default
        CloudSaveIsEnabled = false;
        View_UpdateCloudSaveIsEnabled(true);
        
        IsPopupOpen = false;
        SaveFacade.Instance.OnSyncStatusChanged += OnSyncChange;

        OnSyncChange(SaveFacade.Instance.synced);
    }

    void OnDestroy()
    {
        SaveFacade.Instance.OnSyncStatusChanged -= OnSyncChange;
    }

    void Update()
    {
        View_UpdateCloudSaveIsEnabled(false);
    }

    /// <summary>
    /// Called by the player when the user clicks on this widget
    /// </summary>
    public void OnIconClick()
    {
        if (ClickIsEnabled)
        {
            if (CloudSaveIsSynced)
            {
                if (!IsPopupOpen)
                {
                    IsPopupOpen = true;

                    PersistenceManager.Popups_OpenCloudSync
                    (
                        delegate ()
                        {
                            if (SaveFacade.Instance.cloudSaveEnabled)
                            {
                                SaveFacade.Instance.verboseMode = true;
                                SaveFacade.Instance.GoToSaveLoaderState();
                            }
                            else
                            {
                                IsPopupOpen = false;
                            }
                        },
                        delegate ()
                        {
                            IsPopupOpen = false;
                        }
                    );
                }
            }
            else
            {
                SaveFacade.Instance.verboseMode = true;
                SaveFacade.Instance.GoToSaveLoaderState();
            }
        }
    }

    private void OnSyncChange(bool synced)
    {
        CloudSaveIsSynced = synced;
        View_UpdateCloudSaveIsSynced();
    }

    private void View_UpdateCloudSaveIsEnabled(bool forced = false)
    {
        bool isEnabled = false;

#if CLOUD_SAVE && (FACEBOOK || WEIBO)
        isEnabled = SaveFacade.Instance.cloudSaveEnabled;
#endif

        if (isEnabled != CloudSaveIsEnabled || forced)
        {
            CloudSaveIsEnabled = isEnabled;
            this.gameObject.SetActive(CloudSaveIsEnabled);
        }
    }

    private void View_UpdateCloudSaveIsSynced()
    {
        if (CloudSaveIsSynced)
        {
            m_syncSuccessSprite.SetActive(true);
            m_syncFailedSprite.SetActive(false);
        }
        else
        {
            m_syncSuccessSprite.SetActive(false);
            m_syncFailedSprite.SetActive(true);
        }
    }
}