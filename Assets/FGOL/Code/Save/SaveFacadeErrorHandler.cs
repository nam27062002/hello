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
        { ErrorState.Authentication, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_CLOUD_LOGIN_FAILED_TITLE", "STRING_SAVE_POPUP_ERROR_CLOUD_LOGIN_FAILED_TEXT") },
        { ErrorState.WrongUser, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_CLOUD_WRONG_USER_TITLE", "STRING_SAVE_POPUP_ERROR_CLOUD_WRONG_USER_TEXT") },
        { ErrorState.Connection, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TITLE", "STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TEXT") },
        { ErrorState.Corruption, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TITLE", "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TEXT_OFFLINE") },
        { ErrorState.PermissionError, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_LOAD_FAILED_TITLE", "STRING_SAVE_POPUP_ERROR_LOAD_FAILED_TEXT") },
        { 
            ErrorState.SaveError, 
            new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TITLE", new Dictionary<Globals.Platform, string>
            {
                { Globals.Platform.Android, "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TEXT_ANDROID" },
                { Globals.Platform.iOS, "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TEXT_IOS" },
                { Globals.Platform.Amazon, "STRING_SAVE_POPUP_ERROR_SAVE_FAILED_TEXT_AMAZON" }
            })
        },
        { ErrorState.UpgradeNeeded, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_UPDATE_TITLE", "STRING_SAVE_POPUP_ERROR_UPDATE_TEXT2") },
        { ErrorState.UpgradeNeededAvailable, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_UPDATE_TITLE", "STRING_SAVE_POPUP_ERROR_UPDATE_TEXT1") },
        { ErrorState.UpgradeNeededAvailableLocalCorrupt, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TITLE", "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TEXT_UPDATE1") },
        { ErrorState.UpgradeNeededLocalCorrupt, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TITLE", "STRING_SAVE_POPUP_ERROR_LOCAL_SAVE_CORRUPTED_TEXT_UPDATE2") },
        { ErrorState.UploadFailed, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TITLE", "STRING_SAVE_POPUP_ERROR_CLOUD_OFFLINE_TEXT") },
        { ErrorState.SyncFailed, new CloudSaveMessage("STRING_SAVE_POPUP_ERROR_SYNC_FAILED_TITLE", "STRING_SAVE_POPUP_ERROR_SYNC_FAILED_TEXT") }
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
            config.ConfirmButtonTid = "STRING_BUTTON_RETRY";
            config.CancelButtonTid = "STRING_BUTTON_CONTINUE";
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
                config.CancelButtonTid = "STRING_BUTTON_CONTINUE";
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
                        config.ConfirmButtonTid = "STRING_BUTTON_CONTINUE";                        
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