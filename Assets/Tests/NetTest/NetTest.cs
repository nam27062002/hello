﻿using FGOL.Server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetTest : MonoBehaviour 
{
	public GameObject m_loginButton;
	public GameObject m_actionButtons;
	public GameObject m_waitingText;	
            
	// Use this for initialization
	void Start () 
	{
		// CUSTOM SERVER
		m_loginButton.SetActive(false);
		m_actionButtons.SetActive(false);
		m_waitingText.SetActive(false);

        Messenger.AddListener<bool>(MessengerEvents.SOCIAL_LOGGED, OnSocialLog);
        SocialPlatformManager.SharedInstance.Init(false);        
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
        if (SocialPlatformManager.SharedInstance.CurrentPlatform_IsLoggedIn())            
        {
            GameServerManager.SharedInstance.Auth(
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
		List<SocialUtils.EPlatform> platformIds = SocialPlatformManager.SharedInstance.GetSupportedPlatformIds();
		SocialUtils.EPlatform platformId = SocialUtils.EPlatform.None;
		int count = platformIds.Count;
		for (int i = 0; i < count && platformId == SocialUtils.EPlatform.None; i++) 
		{
			platformId = platformIds[i];
		}

		if (platformId == SocialUtils.EPlatform.None) 
		{
			Debug.LogError("No valid social platform found");
		} 
		else 
		{
			SocialPlatformManager.SharedInstance.Login(platformId, true, true, null);                       
		}
    }		

	public void OnResetDeviceInfo()
	{		
		PlayerPrefs.DeleteAll();
	}
}
