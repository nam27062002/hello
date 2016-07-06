using UnityEngine;
using System.Collections;

public class NetTest : MonoBehaviour 
{
	public GameObject m_loginButton;
	public GameObject m_actionButtons;
	public GameObject m_waitingText;

	// Use this for initialization
	void Start () 
	{
		// CUSTOM SERVER
		m_loginButton.SetActive(true);
		m_actionButtons.SetActive(false);
		m_waitingText.SetActive(false);

		Messenger.AddListener<bool>(GameEvents.LOGGED, OnLog);

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
		}
		else
		{
			m_loginButton.SetActive(true);
			m_waitingText.SetActive(false);
			m_actionButtons.SetActive(false);
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
}
