using UnityEngine;
using System.Collections;

public class PersystenceSynchManager : SingletonMonoBehaviour<PersystenceSynchManager>
{

	bool m_allowSynchProcess = true;
	bool m_continueSynchProcess = false;

	void Start()
	{
		Messenger.AddListener<bool>(GameEvents.LOGGED, OnLogin);
		Messenger.AddListener<bool>(GameEvents.SOCIAL_LOGGED, OnSocialLogin);
		Messenger.AddListener(GameEvents.GOOD_PLACE_TO_SYNCH, OnTrySynch);
		Messenger.AddListener(GameEvents.NO_SYNCHING, OnNoSynch);
		Messenger.AddListener(GameEvents.NEW_SAVE_DATA_FROM_SERVER, OnNewSaveDataFromServer);
	}

	void Destroy()
	{
		Messenger.RemoveListener<bool>(GameEvents.LOGGED, OnLogin);
		Messenger.RemoveListener<bool>(GameEvents.SOCIAL_LOGGED, OnSocialLogin);
		Messenger.RemoveListener(GameEvents.GOOD_PLACE_TO_SYNCH, OnTrySynch);
		Messenger.RemoveListener(GameEvents.NO_SYNCHING, OnNoSynch);
		Messenger.RemoveListener(GameEvents.NEW_SAVE_DATA_FROM_SERVER, OnNewSaveDataFromServer);
	}

	void OnTrySynch()
	{
		m_allowSynchProcess = true;
		m_continueSynchProcess = true;
	}

	void OnNoSynch()
	{
		m_allowSynchProcess = false;
	}

	void OnNewSaveDataFromServer()
	{
		m_continueSynchProcess = true;
	}

	void OnLogin( bool logged )
	{
		if ( logged )
		{
			m_continueSynchProcess = true;
		}
	}

	void OnSocialLogin( bool logged )
	{
		if ( logged )
		{
			m_continueSynchProcess = true;
		}
	}

	void Update()
	{
		if ( m_continueSynchProcess )
		{
			m_continueSynchProcess = false;
			SynchProcess();
		}
	}

	void SynchProcess()
	{
		GameServerManager gameServer = GameServerManager.SharedInstance;
		if (!gameServer.IsLogged())
		{
			gameServer.LoginToServer();	// Wait for login
		}
		else
		{
			if ( !gameServer.saveDataRecovered )
			{
				if ( gameServer.GetLastRecievedUniverse() != null )
				{
					CheckIfServerSaveDataMerge();
				}
				else
				{
					gameServer.GetUniverse();	// -> Wait for Get Universe
				}
			}
			else if ( SocialPlatformManager.SharedInstance.IsLoggedIn() )
			{
				if (!gameServer.mergedWithSocial)
				{
					 // Merge with social
					 gameServer.MergeSocialAccount();
				}
				// else -> everything is done!

			}
		}
	}

	public void CheckIfServerSaveDataMerge()
	{
		if ( !m_allowSynchProcess )
			return;
		if ( GameServerManager.SharedInstance.GetLastRecievedUniverse() != null )	// If I have recieved a universe
		{
			
			UserProfile serverData = new UserProfile();

			try
			{
				serverData.Load(GameServerManager.SharedInstance.GetLastRecievedUniverse());
			}
			catch( System.Exception e)
			{
				// Save Data not valid!
				serverData = null;
			}

			if ( serverData != null )
			{
				// Resolve Conflict!
				UsersManager.currentUser.saveCounter = Mathf.Max( UsersManager.currentUser.saveCounter, serverData.saveCounter);
				GameServerManager.SharedInstance.CleanLastRecievedUniverse();
				GameServerManager.SharedInstance.saveDataRecovered = true;
				m_continueSynchProcess = true;

				/*
				if ( UsersManager.currentUser.saveCounter < serverData.saveCounter )
				{
					// Information on server is newer -> I should get it or merge
					Messenger.Broadcast(GameEvents.MERGE_SERVER_SAVE_DATA);
					// Now we wait for the result
				}
				else
				{
					GameServerManager.SharedInstance.CleanLastRecievedUniverse();
					GameServerManager.SharedInstance.saveDataRecovered = true;
					// Lets continue Synch Process
					m_continueSynchProcess = true;
				}
				*/
			}
			else
			{
				GameServerManager.SharedInstance.CleanLastRecievedUniverse();
				GameServerManager.SharedInstance.saveDataRecovered = false;	// Maybe we need to ask again for the save data?
			}
			
		}
	}

	/*
	void OnApplicationPause(bool pause)
	{
		if ( pause )
		{
			GameServerManager.SharedInstance.saveDataRecovered = false; // ? quizas por tiempo tmb? ya veremos
		}
	}
	*/
}
