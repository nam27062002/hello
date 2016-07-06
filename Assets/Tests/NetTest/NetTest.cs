using UnityEngine;
using System.Collections;

public class NetTest : MonoBehaviour 
{
	public GameObject m_loginButton;
	public GameObject m_actionButton;
	public GameObject m_waitingText;

	// Use this for initialization
	void Start () 
	{
		m_loginButton.SetActive(true);
		m_actionButton.SetActive(false);
		m_waitingText.SetActive(false);

		Messenger.AddListener<bool>(GameEvents.LOGGED, OnLog);
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
			m_actionButton.SetActive(true);
		}
		else
		{
			m_loginButton.SetActive(true);
			m_waitingText.SetActive(false);
			m_actionButton.SetActive(false);
		}
	}

	public void Login()
	{
		m_loginButton.SetActive(false);
		m_waitingText.SetActive(true);
		GameServerManager.SharedInstance.LoginToServer();
	}

	public void TestAction()
	{
		SimpleJSON.JSONClass info = PersistenceManager.LoadToObject( PersistenceManager.activeProfile );
		GameServerManager.SharedInstance.SetUniverse( info );
	}
}
