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
	public GameObject m_mergeLayout;

	// Use this for initialization
	void Start () 
	{
		// CUSTOM SERVER
		m_loginButton.SetActive(false);
		m_actionButtons.SetActive(false);
		m_waitingText.SetActive(false);
		m_mergeLayout.SetActive(false);
		
		Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, OnSocialLog);

		// SOCIAL PLATFORM
		SocialPlatformManager.SharedInstance.Init();

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
        if (SocialPlatformManager.SharedInstance.IsLoggedIn())
        {
            GameServerManager.SharedInstance.LogInToServerThruPlatform("facebook", SocialPlatformManager.SharedInstance.GetUserId(), SocialPlatformManager.SharedInstance.GetToken(),
                delegate (Error commandError, Dictionary<string, object> response)
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
		SocialPlatformManager.SharedInstance.Login();
	}
	

	public void MergeButton()
	{
	}

	public void OnResetDeviceInfo()
	{
		PersistenceManager.Clear();
		PlayerPrefs.DeleteAll();
	}
}
