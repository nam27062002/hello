using UnityEngine;
using System.Collections;
using SimpleJSON;

public class PersistenceSynchManager : MonoBehaviour 
{
	//
	/*
		Idea:
			After login on external platform we ask for the saved data. And Check Last Server Saved Version

			If there is none, we send our save data

			If it's the same version we continue playing and we will save our data when needed, increasing the version

			If it's a bigger version 
				If we have a local version different from server we will ask the player what he wants to get
				If we haven't changed anything we will load this new version


			1 [synched]-------2 [synched]-------3 [synched] ----------- 3 [synched] => last server saved version is 3 and server version is bigger => load
															----------- 4 [SERVER]
			

			1 [synched]-------2 [synched]-------3 [synched] ----------- 4a [NOT synched] => last server saved version is 3, local version is 4, and server version is bigger than server saved version => ask
															----------- 4b [SERVER]

			Everytime we save we will increase version. Everytime we save we will try to synch with the server

			On Merge Question: We will ask what game the player wants and persist that info locally and on server. 
							We will show the progress and check if there was a purchase. In case of a purchase we will add the purchase from the other save data to the selected one

		Question:
			Should I try to save persystence even without having external platform? Save it first and try to associate later? Is it woth the hassle?
			We should show the client id somewhere in case we want to give something to the players? How will it work with this system?

		Notes:
			Once logged I should be able to get new info from the server. Should I be able to do it before? If yes I should log in to our server before everything else
			If I dont need to be logged in I should be able to ask for things without a user. For example: customizer or new content

			Best scenario -> I don't need to wait for the login but I can do server stuff. This way we don't bother the user with waiting times and we have help from the server

	*/

	// Use this for initialization
	void Start () 
	{
		ExternalPlatformManager.instance.OnLogin += OnExternalLogin;
		//RequestNetwork.instance.onAuthResponse += OnAuthResponse;

	}

	void OnDestroy()
	{
		ExternalPlatformManager.instance.OnLogin -= OnExternalLogin;
		//RequestNetwork.instance.onAuthResponse -= OnAuthResponse;
	}
	
	void OnExternalLogin()
	{
		// Try to sync save data
		string id = ExternalPlatformManager.instance.GetId();
		// RequestNetwork -> ask for Persistence
		//RequestNetwork.instance.Authenticate( "server", "sufix", "platformId", "_platformUserName");
	}

	void OnAuthResponse( JSONNode result )
	{
		
	}

	void OnServerPersistence()
	{
		// Check versions with current and set info
	}
}
