//[DGR] No support added yet
//using Definitions;
using FGOL.Authentication;
using FGOL.Server;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Sharing
{
    private enum AuthResult
    {
        OK,
        FAILED,
        CANCELLED,
        NOCONNECTION
    }

    // [DGR] No support added yet
    //private Dictionary<SocialFacade.Network, ISocialSystem> m_socialSystems;

    public Sharing(Dictionary<SocialFacade.Network, ISocialSystem> socialSystems)
    {
        // [DGR] No support added yet
        //m_socialSystems = socialSystems;
    }

    public void ShareScore(SocialFacade.Network network, int score, string levelKey, string sharkKey, Action onComplete)
    {
        /*
        // [DGR] No support added yet
        LoadingPopup.Show();

        AuthenticateSharing(network, delegate (AuthResult result)
        {
            if (result == AuthResult.OK)
            {
                bool hide = true;

                Debug.Log("Sharing (ShareScore) :: Sharing score");

                GameDB gameDB = GameDataManager.Instance.gameDB;

                GameSettings settings = gameDB.GetItem<GameSettings>(GameSettings.KeySettings);
                FacebookShareImageData shareImages = gameDB.GetItem<FacebookShareImageData>(levelKey + "Score");

                if (settings != null && shareImages != null)
                {
                    PlayerSharkData sharkData = gameDB.GetItem<PlayerSharkData>(sharkKey);

                    if (sharkData != null)
                    {
                        string shark = Localization.Get(sharkData.name);

                        LevelData levelData = gameDB.GetItem<LevelData>(levelKey);

                        if (levelData != null)
                        {
                            string level = Localization.Get(levelData.localisedName);

                            hide = false;

                            SocialFacade.Instance.Share(SocialManager.GetSelectedSocialNetwork(), settings.facebookShareUrl, Localization.Get("STRING_SNPOST_HIGHSCORE_TITLE"), Localization.Format("STRING_SNPOST_HIGHSCORE", score, shark, level), shareImages.imageUrl, delegate (bool success)
                            {
                                LoadingPopup.Hide();
                                onComplete();

                                Debug.Log("Sharing (ShareScore) :: Sharing complete - " + success);
                            });
                        }
                    }
                }

                if (hide)
                {
                    LoadingPopup.Hide();
                    onComplete();
                }
            }
            else if (result == AuthResult.NOCONNECTION)
            {
                LoadingPopup.Hide();

                MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();
                config.cancelEnabled = false;
                config.titleText = "TID_SOCIAL_ERROR_CONNECTION_NAME";
                config.messageText = "STRING_SOCIAL_ERROR_CONNECTION_INFO_GENERIC";
                config.onConfirm = onComplete;
				config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
                MessageBoxPopup.OpenMessageBox(config);
            }
            else
            {
                LoadingPopup.Hide();
                onComplete();
            }
        });
        */
    }

    public void ShareSharkPicture(SocialFacade.Network network, byte[] sharkPic, string sharkKey, Action onComplete)
    {
        /*
        //[DGR] No support added yet
        LoadingPopup.Show();

        AuthenticateSharing(network, delegate(AuthResult result){
            Action onFailed = delegate() {
                LoadingPopup.Hide();

				string networkName = SocialFacade.GetLocalizedNetworkName(network);
                MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();
                config.titleText = "STRING_SNPOST_PHOTO_FAIL_TITLE";
				config.titleArgs = new object[]{ networkName };
                config.messageText = "STRING_SNPOST_PHOTO_FAIL";
				config.messageArgs = new object[] { networkName };
                config.cancelEnabled = false;
                config.onConfirm = onComplete;
				config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;

                MessageBoxPopup.OpenMessageBox(config);
            };

            if (result == AuthResult.OK)
            {
                GameDB gameDB = GameDataManager.Instance.gameDB;
                GameSettings settings = gameDB.GetItem<GameSettings>(GameSettings.KeySettings);

                if (settings != null)
                {
                    PlayerSharkData sharkData = gameDB.GetItem<PlayerSharkData>(sharkKey);

                    if (sharkData != null)
                    {
                        string shark = Localization.Get(sharkData.name);

                        string imageName = "HungryShark.png";
						//Disable any description to pass Google Review - Neil
						string description = "";
						if (network == SocialFacade.Network.Weibo)
						{
							//weibo needs the description.
							description = Localization.Format("STRING_SNPOST_PHOTO", shark, settings.facebookShareUrl);
						}

						SocialFacade.Instance.SharePicture(network, sharkPic, imageName, description, null, delegate (bool success)
                        {
                            Debug.Log("Sharing (ShareSharkPicture) :: Sharing complete - " + success);

                            if (success)
                            {
                                LoadingPopup.Hide();

								string networkName = SocialFacade.GetLocalizedNetworkName(network);
                                MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();
                                config.titleText = "STRING_SNPOST_PHOTO_SUCCESS_TITLE";
								config.titleArgs = new object[] { networkName };
                                config.messageText = "STRING_SNPOST_PHOTO_SUCCESS";
								config.messageArgs = new object[] { networkName };
                                config.cancelEnabled = false;
								config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.confirm;
                                config.onConfirm = onComplete;

                                MessageBoxPopup.OpenMessageBox(config);
                            }
                            else
                            {
                                onFailed();
                            }
                        });
                    }
                    else
                    {
                        onFailed();
                    }
                }
                else
                {
                    onFailed();
                }
            }
            else
            {
                onFailed();
            }
        });
        */
    }

    private void AuthenticateSharing(SocialFacade.Network network, Action<AuthResult> onShare)
    {
        /*
        //[DGR] No support added yet
        HSXServer.Instance.CheckConnection(delegate (Error connectionError)
        {
            if (connectionError == null)
            {
                PermissionType[] loginPermissions = new PermissionType[] { PermissionType.Basic, PermissionType.Friends };
                PermissionType[] publishPermissions = new PermissionType[] { PermissionType.Publish };

                Action onLoggedIn = delegate ()
                {
                    Debug.Log("Sharing (AuthenticateSharing) :: Logged In");

                    if (m_socialSystems[network].IsLoggedIn(publishPermissions))
                    {
                        onShare(AuthResult.OK);
                    }
                    else
					{
						string networkName = SocialFacade.GetLocalizedNetworkName(network);
                        MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();
                        config.titleText = "STRING_SNPOST_SHAREPERMISSION_TITLE";
						config.titleArgs = new object[] { networkName };
                        config.messageText = "STRING_SNPOST_SHAREPERMISSION";
						config.messageArgs = new object[] { networkName };
                        config.confirmText = "STRING_BUTTON_YES";
                        config.cancelText = "STRING_BUTTON_NOTHANKS";
						config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.cancel;
                        config.onConfirm = delegate ()
                        {
                            Debug.Log("Sharing (AuthenticateSharing) :: No Publish Permission asking for it");

                            m_socialSystems[network].AskForPublishPermission(delegate (bool permissionGiven){
                                Util.StartCoroutineWithoutMonobehaviour("Sharing", GameUtil.Delay(0.5f, delegate () { onShare(permissionGiven ? AuthResult.OK : AuthResult.FAILED); }));
                            });
                        };
						config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.cancel;
                        config.onCancel = delegate () { onShare(AuthResult.CANCELLED); };
                        MessageBoxPopup.OpenMessageBox(config);
                    }
                };

                Action login = delegate () {
                    Debug.Log("Sharing (AuthenticateSharing) :: Logging user in");

                    SocialManager.Instance.Login(network, delegate (bool loggedIn) {
                        if (loggedIn)
                        {
                            onLoggedIn();
                        }
                        else
                        {
                            onShare(AuthResult.FAILED);
                        }
                    }, false);
                };

                if (m_socialSystems[network].IsLoggedIn(loginPermissions))
                {
                    login();
                }
                else
				{
					string networkName = SocialFacade.GetLocalizedNetworkName(network);
                    MessageBoxPopup.MessageBoxConfig config = new MessageBoxPopup.MessageBoxConfig();
                    config.titleText = "STRING_SNPOST_LOGIN_TITLE";
					config.titleArgs = new object[] { networkName };
                    config.messageText = "STRING_SNPOST_LOGIN";
					config.messageArgs = new object[] { networkName };
                    config.confirmText = "STRING_BUTTON_YES";
                    config.cancelText = "STRING_BUTTON_NOTHANKS";
					config.backButtonMode = MessageBoxPopup.MessageBoxConfig.BackButtonMode.cancel;
                    config.onConfirm = login;
                    config.onCancel = delegate () { onShare(AuthResult.CANCELLED); };
                    MessageBoxPopup.OpenMessageBox(config);
                }
            }
            else
            {
                onShare(AuthResult.NOCONNECTION);
            }
        });
        */
    }
}
