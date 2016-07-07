using UnityEngine;
using System.Collections;

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
		m_loginButton.SetActive(true);
		m_actionButtons.SetActive(false);
		m_waitingText.SetActive(false);
		m_mergeLayout.SetActive(false);

		Messenger.AddListener<bool>(GameEvents.LOGGED, OnLog);
		Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, OnSocialLog);

		// SOCIAL PLATFORM
		SocialPlatformManager.SharedInstance.Init();

	}

	void OnDestroy()
	{
		Messenger.RemoveListener<bool>(GameEvents.LOGGED, OnLog);
	}

	void OnLog( bool logged)
	{
		if ( logged )
		{
			m_loginButton.SetActive(false);
			m_waitingText.SetActive(false);
			m_actionButtons.SetActive(true);
			CheckShowMerge();
		}
		else
		{
			m_loginButton.SetActive(true);
			m_waitingText.SetActive(false);
			m_actionButtons.SetActive(false);
		}
	}

	void OnSocialLog( bool logged )
	{
		if ( logged )
		{
			CheckShowMerge();
		}
		else
		{
			
		}
	}

	public void Login()
	{
		m_loginButton.SetActive(false);
		m_waitingText.SetActive(true);
		GameServerManager.SharedInstance.LoginToServer();
	}

	public void GetUniverse()
	{
		GameServerManager.SharedInstance.GetUniverse();
	}

	public void SetUniverse()
	{
		SimpleJSON.JSONClass info = PersistenceManager.LoadToObject( PersistenceManager.activeProfile );
		GameServerManager.SharedInstance.SetUniverse( info );
	}


	// SOCIAL PLATFORM
	public void SocialLogin()
	{
		SocialPlatformManager.SharedInstance.Login();
	}


	// MERGING
	private void CheckShowMerge()
	{
		// If we are logged in both platgorms and we already merged our local account with the save data
		if ( GameServerManager.SharedInstance.IsLogged() && GameServerManager.SharedInstance.saveDataRecovered && SocialPlatformManager.SharedInstance.IsLoggedIn() )
		{
			m_mergeLayout.SetActive(true);
		}
		else
		{
			m_mergeLayout.SetActive(false);
		}
	}

	public void MergeButton()
	{
		GameServerManager.SharedInstance.MergeSocialAccount();
	}
}
