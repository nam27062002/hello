// ClusteringManager.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 29/04/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class ClusteringManager {
	//------------------------------------------------------------------------//
	// CONSTANTS                											  //
	//------------------------------------------------------------------------//
	// Server endpoint
	private static readonly string GET_CLUSTER_ID = "/api/cluster/get";

	// Generic cluster ID
	public static string CLUSTER_GENERIC = "CLUSTER_GENERIC";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Singleton instance
	private static ClusteringManager m_instance = null;

	// Communication with server
	private bool m_registered = false;
	private bool m_offlineMode = false;

	// Cached player values
	private string m_accountId;
    private int m_deviceProfile;
    private int m_playerProgression;
    private bool m_shopEntered;
    private int m_firerushes;
    private int m_gemsSpent;
    private long m_score;
    private int m_boostTime;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ClusteringManager() {


	}

	/// <summary>
	/// Destructor
	/// </summary>
	~ClusteringManager() {

	}

	// Singleton
	public static ClusteringManager Instance
	{
		get
		{
            if (m_instance == null)
            {
				m_instance = new ClusteringManager();
            }

			return m_instance;
		}
	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Returns the cluster ID assigned to this player.
    /// In case the client doesnt know the cluster, makes a request to the server,
    /// and assigns a temporary cluster ID in the meantime.
    /// </summary>
    /// <returns>The cluster ID</returns>
    public string GetClusterId( )
	{

		string clusterId = UsersManager.currentUser.GetClusterId();

		if (!string.IsNullOrEmpty(clusterId))
		{
			// we know the cluster ID
			return clusterId;
		}
		else
		{


			if (!ContentManager.ready)
			{
				// The content manager is not ready yet
				return null;
			}

			// Read config variables from the content
			DefinitionNode offerSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "offerSettings");


			// The cluster will be calculated based on the data gathered at the end of this run
            if (UsersManager.currentUser.gamesPlayed < offerSettingsDef.GetAsInt("calculateClusterAtRun"))
			{
				// Too early. We are not calculating the cluster ID yet.
				return null;
			}

			// Request the cluster ID to the server.
			InitializeAndSendRequest(false);

			if ( UsersManager.currentUser.gamesPlayed < offerSettingsDef.GetAsInt("assignGenericClusterAtRun") )
			{
				// We are still waiting for the server response
				return null;
			}
			else
			{
				// The server response is taking too long. Assign the player to generic cluster.
				UsersManager.currentUser.SetClusterId(CLUSTER_GENERIC);

				Debug.Log("<color=white>Didnt get answer from server. Assigning player to generic cluster '" + CLUSTER_GENERIC + "' </color>");

				return CLUSTER_GENERIC;
			}
		}
	}

	/// <summary>
	/// Gather all the player variables that will be sent to the server
	/// </summary>
	private void  LoadCachedValues()
	{

		m_accountId = UsersManager.currentUser.userId;
        m_deviceProfile = FeatureSettingsManager.instance.Device_CalculatedProfileOrder;
        m_playerProgression = UsersManager.currentUser.GetPlayerProgress();
        m_shopEntered = UsersManager.currentUser.hasEnteredShop;
        m_firerushes = UsersManager.currentUser.firerushesCount;
        m_gemsSpent = UsersManager.currentUser.gemsSpent;
        m_score = UsersManager.currentUser.totalScore;
        m_boostTime = UsersManager.currentUser.boostTime;

    }

    /// <summary>
    /// Check that endpoint is registered, gather all the parameters needed and the send the request to the server
    /// </summary>
    /// <param name="_offlineMode"></param>
	public void InitializeAndSendRequest(bool _offlineMode = false)
	{
		if (!m_registered)
		{
			m_offlineMode = _offlineMode;

			NetworkManager.SharedInstance.RegistryEndPoint(GET_CLUSTER_ID, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, new int[] { 200, 404, 500, 503 }, OnGetClusterResponse);

			m_registered = true;
		}


		if (!_offlineMode)
		{
			// Dont make the request if we are not logged yet in the server
			if (GameSessionManager.SharedInstance.IsLogged())
			{
				// Gather all the player data (dont do it before the login or accountId wont be ready)
				LoadCachedValues();

				SendRequestToServer();

			}
		}
	}

    /// <summary>
    /// Put all the variables in the payload, and send the request to the server
    /// </summary>
	private void SendRequestToServer()
	{

		Dictionary<string, string> kParams = new Dictionary<string, string>();
		kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
		kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();


        JSONClass kBody = new JSONClass();
		kBody["accountId"] = m_accountId;
		kBody["deviceProfile"] = m_deviceProfile;
		kBody["maxProgression"] = PersistenceUtils.SafeToString (m_playerProgression);
		kBody["fireRushes"] = PersistenceUtils.SafeToString(m_firerushes);
		kBody["shopEntered"] = PersistenceUtils.SafeToString(m_shopEntered);
		kBody["gemsSpent"] = PersistenceUtils.SafeToString(m_gemsSpent);
		kBody["score"] = PersistenceUtils.SafeToString(m_score);
		kBody["boostTime"] = PersistenceUtils.SafeToString(m_boostTime);

		// Send it to the server
		ServerManager.SharedInstance.SendCommand(GET_CLUSTER_ID, kParams, kBody.ToString());

		//Debug:
        /*
		JSONClass response = new JSONClass();
		response["clusterId"] = "cluster_5";
		int reponseCode = 200;

		UbiBCN.CoroutineManager.DelayedCall(() =>
	      {
			  OnGetClusterResponse(response.ToString(), GET_CLUSTER_ID, reponseCode);
	      }, 5);
          */
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Response from the server was received
	/// </summary>
	/// <param name="_strResponse">Json containing the cluster Id requested</param>
	/// <param name="_strCmd">The command sent</param>
	/// <param name="_reponseCode">Response code. 200 if the request was successful</param>
	/// <returns>Returns true if the response was successful</returns>
	private bool OnGetClusterResponse(string _strResponse, string _strCmd, int _reponseCode)
	{
		// If the player already belongs to a cluster (non generic) ignore the response
		string currentClusterId = UsersManager.currentUser.GetClusterId();
		if (!string.IsNullOrEmpty(currentClusterId) &&
			currentClusterId != CLUSTER_GENERIC)
        {
			return true;
		}
			

		bool responseOk = false;

		if (_strResponse != null)
		{
			switch (_reponseCode)
			{
				case 200: // No error
				case 204: // No error, but the server doesnt know the cluster id
					{

						JSONNode kJSON = JSON.Parse(_strResponse);
						if (kJSON != null)
						{
							if (kJSON.ContainsKey("result"))
							{
								if (kJSON["result"] == true)
								{
									if (kJSON.ContainsKey("clusterId"))
									{

										// The server knows the cluster
										string clusterId = kJSON["clusterId"];
										
										UsersManager.currentUser.SetClusterId(clusterId);

										Debug.Log("<color=white>Player assigned by server to cluster '" + clusterId + "' </color>");
									}
                                    // else: the server doesnt know the cluster. Leave it empty.

									responseOk = true;

								}
								else
								{
                                    // Server returned an error
									Debug.LogError("Requests " + _strCmd + " returned error " +
										kJSON["errorCode"] + ": " + kJSON["errorMsg"]);

									responseOk = false;

								}
							}
						}

						break;
					}

				default: 
					{
                        // An error happened
						responseOk = false;
						break;
					}
			}
		}



		if (m_offlineMode)
		{
			return false;
		}
		else
		{
			return responseOk;
		}

	}
}