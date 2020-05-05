// ClusteringManager.cs
// Hungry Dragon
// 
// Created by  on 29/04/2020.
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

	private static readonly string GET_CLUSTER_ID = "/api/cluster/get";

	// The cluster will be calculated based on the data gathered until the end of this run
	public static int CALCULATE_CLUSTER_AFTER_RUN = 2;

	public static string CLUSTER_UNKNOWN = "UNKNOWN";


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	// Singleton instance
	private static ClusteringManager m_instance = null;

    // Communication with server
	private bool m_initialised = false;
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

	public string GetClusterId()
	{
		string clusterId = UsersManager.currentUser.clusterId;

		if (!string.IsNullOrEmpty(clusterId))
		{
			// the client already knows the cluster ID
			return clusterId;
		}
		else
		{
			// we dont know it yet. Request the cluster ID to the server.
			Initialise(false);

			// In the meantime, return something
			return CLUSTER_UNKNOWN;
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


	public void Initialise(bool _offlineMode = false)
	{
		if (!m_initialised)
		{
			m_offlineMode = _offlineMode;

			LoadCachedValues();

			NetworkManager.SharedInstance.RegistryEndPoint(GET_CLUSTER_ID, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, new int[] { 200, 404, 500, 503 }, OnGetClusterResponse);

			m_initialised = true;
		}


		if (!_offlineMode)
		{
			SendRequestToServer();
		}
	}

    /// <summary>
    /// Put all the variables in the payload, and send the request to the server
    /// </summary>
	private void SendRequestToServer()
	{
        // Dont make the request if the session is not created
        if (! GameSessionManager.SharedInstance.IsLogged())
        {
			return;
        }

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
		ServerManager.SharedInstance.SendCommand(GET_CLUSTER_ID, kParams.ToString(), kBody);

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
    /// <param name="_strCmd"></param>
    /// <param name="_reponseCode">Response code. 200 if the request was successful</param>
    /// <returns>Returns true if the response was successful</returns>
	private bool OnGetClusterResponse(string _strResponse, string _strCmd, int _reponseCode)
	{
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
								if (kJSON["result"] == "true")
								{
									if (kJSON.ContainsKey("clusterId"))
									{
                                        // The server knows the cluster
										UsersManager.currentUser.clusterId = kJSON["clusterId"];
										
									}
                                    // else: the server doesnt know the cluster. Leave it empty.

									responseOk = true;

								}
								else
								{
                                    // Server returned an error
									Debug.LogError("Requests " + _strCmd + " returned error " +
										kJSON["errorCode"] + ": " + kJSON["errorMsg"]);

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