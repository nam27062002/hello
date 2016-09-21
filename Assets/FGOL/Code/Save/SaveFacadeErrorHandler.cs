using FGOL.Authentication;
using FGOL.Server;
using System;
using System.Collections.Generic;

public class SaveFacadeErrorHandler
{
    public enum ErrorState
    {
        Authentication,
        WrongUser,
        Connection,
        Corruption,
        SyncFailed,
        UpgradeNeeded,
        UpgradeNeededAvailable,
        UpgradeNeededLocalCorrupt,
        UpgradeNeededAvailableLocalCorrupt,
        PermissionError,
        UploadFailed,
        SaveError,
        None
    }

    private class CloudSaveMessage
    {
        public string title = string.Empty;
        public string message
        {
            get
            {
                Globals.Platform platform = Globals.GetPlatform();
                    
                return m_messages.ContainsKey(platform) ? m_messages[platform] : (m_messages.ContainsKey(Globals.Platform.Unknown) ? m_messages[Globals.Platform.Unknown] : string.Empty);
            }
        }
        public bool upgradeNeeded = false;

        private Dictionary<Globals.Platform, string> m_messages = new Dictionary<Globals.Platform, string>();

        public CloudSaveMessage(string title, string message, bool upgradeNeeded = false)
        {
            this.title = title;
            m_messages[Globals.Platform.Unknown] = message;
            this.upgradeNeeded = upgradeNeeded;
        }

        public CloudSaveMessage(string title, Dictionary<Globals.Platform, string> messages, bool upgradeNeeded = false)
        {
            this.title = title;
            m_messages = messages;
            this.upgradeNeeded = upgradeNeeded;
        }
    }

    public Action onRetry = null;
    public Action onContinue = null;
    public Action onGoToAppStore = null;

    private Dictionary<ErrorState, CloudSaveMessage> m_errorMessages = new Dictionary<ErrorState, CloudSaveMessage>
    {
        { ErrorState.Authentication, new CloudSaveMessage("TID_SAVE_ERROR_CLOUD_LOGIN_FAILED_NAME", "TID_SAVE_ERROR_CLOUD_LOGIN_FAILED_DESC") },
        { ErrorState.WrongUser, new CloudSaveMessage("TID_SAVE_POPUP_ERROR_CLOUD_WRONG_USER_NAME", "TID_SAVE_POPUP_ERROR_CLOUD_WRONG_USER_DESC") },
        { ErrorState.Connection, new CloudSaveMessage("TID_SAVE_ERROR_CLOUD_OFFLINE_NAME", "TID_SAVE_ERROR_CLOUD_OFFLINE_DESC") },
        { ErrorState.Corruption, new CloudSaveMessage("TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME", "TID_SAVE_ERROR_LOCAL_CORRUPTED_OFFLINE_DESC") },
        { ErrorState.PermissionError, new CloudSaveMessage("TID_SAVE_ERROR_LOAD_FAILED_NAME", "TID_SAVE_ERROR_LOAD_FAILED_DESC") },
        { 
            ErrorState.SaveError, 
            new CloudSaveMessage("TID_SAVE_ERROR_FAILED_NAME", new Dictionary<Globals.Platform, string>
            {
                { Globals.Platform.Android, "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TEXT_ANDROID" },
                { Globals.Platform.iOS, "TID_SAVE_ERROR_FAILED_DESC" },
                { Globals.Platform.Amazon, "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TEXT_AMAZON" }
            })
        },
        { ErrorState.UpgradeNeeded, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_UPDATE_TITLE", "TID_SAVE_ERROR_UPDATE_DESC2") },
        { ErrorState.UpgradeNeededAvailable, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_UPDATE_TITLE", "TID_SAVE_ERROR_UPDATE_DESC1") },
        { ErrorState.UpgradeNeededAvailableLocalCorrupt, new CloudSaveMessage("TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME", "TID_SAVE_ERROR_LOCAL_CORRUPTED_UPDATE_DESC1") },
        { ErrorState.UpgradeNeededLocalCorrupt, new CloudSaveMessage("TID_SAVE_ERROR_LOCAL_CORRUPTED_NAME", "TID_SAVE_ERROR_LOCAL_CORRUPTED_UPDATE_DESC2") },
        { ErrorState.UploadFailed, new CloudSaveMessage("TID_SAVE_ERROR_CLOUD_OFFLINE_NAME", "TID_SAVE_ERROR_CLOUD_OFFLINE_DESC") },
        { ErrorState.SyncFailed, new CloudSaveMessage("TID_SAVE_POPUP_ERROR_SYNC_FAILED_NAME", "TID_SAVE_POPUP_ERROR_SYNC_FAILED_DESC") }
    };

    public void HandleError(ErrorState state, ErrorCodes code)
    {
        Debug.Log(string.Format("SaveFacadeErrorHandler (HandleError) :: Handling error {0} with code {1}", state, code));

        CloudSaveMessage message = null;

        if(m_errorMessages.ContainsKey(state))
        {
            message = m_errorMessages[state];
        }
        else
        {
            message = m_errorMessages[ErrorState.SyncFailed];
        }

		//	Send used network with the error code now that we support multiple providers
		SocialFacade.Network networkUsed = SocialManager.GetSelectedSocialNetwork ();

        if(!message.upgradeNeeded)
        {
            PopupMessage.Config config = PopupMessage.GetConfig();
            config.TitleTid = message.title;
            config.MessageTid = message.message;
            config.MessageParams = new string[] { code.ToString(), SocialFacade.GetLocalizedNetworkName(networkUsed) };
            config.ConfirmButtonTid = "TID_GEN_RETRY";
            config.CancelButtonTid = "TID_GEN_CONTINUE";
            config.OnConfirm = onRetry;
            config.OnCancel = onContinue;
            config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
            PersistenceManager.Popups_OpenMessage(config);           
        }
        else
        {
            Action onUpgradeAvailable = delegate()
            {
                PopupMessage.Config config = PopupMessage.GetConfig();
                config.TitleTid = message.title;
                config.MessageTid = message.message;                
                config.MessageParams = new string[] { code.ToString(), SocialFacade.GetLocalizedNetworkName(networkUsed) };
                config.ConfirmButtonTid = "STRING_BUTTON_UPDATE";
                config.CancelButtonTid = "TID_GEN_CONTINUE";
                config.OnConfirm = onGoToAppStore;
                config.OnCancel = onContinue;
                config.ButtonMode = PopupMessage.Config.EButtonsMode.ConfirmAndCancel;
                PersistenceManager.Popups_OpenMessage(config);                
            };

            if(state == ErrorState.UpgradeNeeded)
            {
                Authenticator.Instance.CheckGameVersion(delegate(bool upgradeAvailable)
                {
                    if(upgradeAvailable)
                    {
                        onUpgradeAvailable();
                    }
                    else
                    {
                        PopupMessage.Config config = PopupMessage.GetConfig();
                        config.TitleTid = message.title;
                        config.MessageTid = message.message;
                        config.MessageParams = new string[] { code.ToString() };
                        config.ConfirmButtonTid = "TID_GEN_CONTINUE";                        
                        config.OnConfirm = onContinue;                        
                        config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
                        PersistenceManager.Popups_OpenMessage(config);                        
                    }
                });
            }
            else if(state == ErrorState.UpgradeNeededAvailable)
            {
                onUpgradeAvailable();
            }
            else if(state == ErrorState.UpgradeNeededAvailableLocalCorrupt)
            {
                PopupMessage.Config config = PopupMessage.GetConfig();
                config.TitleTid = message.title;
                config.MessageTid = message.message;
                config.MessageParams = new string[] { code.ToString() };
                config.ConfirmButtonTid = "STRING_BUTTON_UPDATE";
                config.OnConfirm = onGoToAppStore;
                config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
                PersistenceManager.Popups_OpenMessage(config);               
            }
            else
            {
                Action noContinue = null;
                noContinue = delegate()
                {
                    PopupMessage.Config config = PopupMessage.GetConfig();
                    config.TitleTid = message.title;
                    config.MessageTid = message.message;
                    config.MessageParams = new string[] { code.ToString() };
                    config.ConfirmButtonTid = "STRING_BUTTON_UPDATE";
                    config.OnConfirm = noContinue;
                    config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
                    PersistenceManager.Popups_OpenMessage(config);                  
                };

                noContinue();
            }
        }
    }
}