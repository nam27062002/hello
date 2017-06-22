using FGOL.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTest : MonoBehaviour 
{
	public GameObject m_loginButton;
	public GameObject m_actionButtons;
	public GameObject m_waitingText;	

    private enum ESocialAPI
    {
        None,
        Calety,
        FGOL
    }

    private ESocialAPI socialAPI = ESocialAPI.FGOL;

	// Use this for initialization
	void Start () 
	{
		// CUSTOM SERVER
		m_loginButton.SetActive(false);
		m_actionButtons.SetActive(false);
		m_waitingText.SetActive(false);		
		
        switch (socialAPI)
        {
            case ESocialAPI.Calety:
                Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, OnSocialLog);                
                SocialPlatformManager.SharedInstance.Init();
                break;

            case ESocialAPI.FGOL:
                SocialFacade.Instance.Init();
                break;
        }		        
    }

	void OnLog( bool logged)
	{
		if ( logged )
		{
			m_loginButton.SetActive(false);
			m_waitingText.SetActive(false);
			m_actionButtons.SetActive(true);			
		}
		else
		{
			m_loginButton.SetActive(true);
			m_waitingText.SetActive(false);
			m_actionButtons.SetActive(false);
		}
	}

	void OnSocialLog(bool logged)
	{
		if (logged)
		{
            m_loginButton.SetActive(true);            
        }		
	}

	public void Login()
	{
		m_loginButton.SetActive(false);
		m_waitingText.SetActive(true);              
        if ((socialAPI == ESocialAPI.Calety && SocialPlatformManager.SharedInstance.IsLoggedIn()) ||
            (socialAPI == ESocialAPI.FGOL && SocialFacade.Instance.IsLoggedIn(SocialFacade.Network.Facebook)))
        {
            GameServerManager.SharedInstance.LogInToServerThruPlatform(FGOL.Authentication.User.LoginTypeToCaletySocialPlatform(FGOL.Authentication.User.LoginType.Facebook), SocialPlatformManager.SharedInstance.GetUserId(), SocialPlatformManager.SharedInstance.GetToken(),
                (Error commandError, GameServerManager.ServerResponse response) =>
                {
                    if (commandError == null)
                    {
                        OnLog(true);
                    }
                    else
                    {
                        OnLog(false);
                    }
                }
            );
        }
    }

	public void GetUniverse()
	{        
        GameServerManager.SharedInstance.GetPersistence(null);
    }

	public void SetUniverse()
	{     
        GameServerManager.SharedInstance.SetPersistence("{\"version\":\"0.1.1\"}", null);
    }


	// SOCIAL PLATFORM
	public void SocialLogin()
	{
        switch (socialAPI)
        {
            case ESocialAPI.Calety:
                SocialPlatformManager.SharedInstance.Login();
                break;

            case ESocialAPI.FGOL:
                SocialFacade.Instance.Login(SocialFacade.Network.Facebook, new PermissionType[] { PermissionType.Basic }, OnSocialLog);
                break;

            case ESocialAPI.None:
                Debug.Log("No social API assigned");
                break;
        }                
    }		

	public void OnResetDeviceInfo()
	{
		PersistenceManager.Clear();
		PlayerPrefs.DeleteAll();
	}
}
