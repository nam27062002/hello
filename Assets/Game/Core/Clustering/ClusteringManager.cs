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
	private const string GET_CLUSTER_ID = "/api/cluster/get";

	// Generic cluster ID
	public const string CLUSTER_GENERIC = "CLUSTER_GENERIC";

	// Sync
	private static readonly TimeSpan SYNC_RETRY_INTERVAL = new TimeSpan(0, 5, 0);	// 5 minutes

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Singleton instance
	private static ClusteringManager s_instance = null;
	public static ClusteringManager Instance {
		get {
			if(s_instance == null) {
				s_instance = new ClusteringManager();
			}
			return s_instance;
		}
	}

	// Communication with server
	private bool m_registered = false;
	private bool m_offlineMode = false;
	private bool m_syncingWithServer = false;   // Prevent spamming
	private DateTime m_nextSyncTimestamp = DateTime.MinValue;

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

	/// <summary>
	/// To be called externally every frame, since this singleton is not a monobehaviour.
	/// </summary>
	public void Update() {
		// Do we need to sync with the server?
		if(!UsersManager.currentUser.clusterSynced) {
			// Enough time elapsed?
			if(GameServerManager.GetEstimatedServerTime() >= m_nextSyncTimestamp) {
				// Attempt a new sync
				SyncWithServer();

				// Reset timer
				m_nextSyncTimestamp = GameServerManager.GetEstimatedServerTime() + SYNC_RETRY_INTERVAL;
			}
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
	public string GetClusterId() {

		string clusterId = UsersManager.currentUser.GetClusterId();

		if(!string.IsNullOrEmpty(clusterId)) {
			// we know the cluster ID
			return clusterId;
		} else {


			if(!ContentManager.ready) {
				// The content manager is not ready yet
				return null;
			}

			// Read config variables from the content
			DefinitionNode offerSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "offerSettings");


			// The cluster will be calculated based on the data gathered at the end of this run
			if(UsersManager.currentUser.gamesPlayed < offerSettingsDef.GetAsInt("calculateClusterAtRun")) {
				// Too early. We are not calculating the cluster ID yet.
				return null;
			}

			// Request the cluster ID to the server.
			InitializeAndSendRequest(false);

			if(UsersManager.currentUser.gamesPlayed < offerSettingsDef.GetAsInt("assignGenericClusterAtRun")) {
				// We are still waiting for the server response
				return null;
			} else {
				// The server response is taking too long. Assign the player to generic cluster.
				UsersManager.currentUser.SetClusterId(CLUSTER_GENERIC);

				Debug.Log("<color=white>Didnt get answer from server. Assigning player to generic cluster '" + CLUSTER_GENERIC + "' </color>");

				// Pending sync with server
				UsersManager.currentUser.clusterSynced = false;
				SyncWithServer();

				return CLUSTER_GENERIC;
			}
		}
	}

	/// <summary>
	/// Gather all the player variables that will be sent to the server
	/// </summary>
	private void LoadCachedValues() {

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
	private void InitializeAndSendRequest(bool _offlineMode = false) {
		if(!m_registered) {
			m_offlineMode = _offlineMode;

			NetworkManager.SharedInstance.RegistryEndPoint(GET_CLUSTER_ID, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, new int[] { 200, 404, 500, 503 }, OnGetClusterResponse);

			m_registered = true;
		}


		if(!_offlineMode) {
			// Dont make the request if we are not logged yet in the server
			if(GameSessionManager.SharedInstance.IsLogged()) {
				// Gather all the player data (dont do it before the login or accountId wont be ready)
				LoadCachedValues();

				SendRequestToServer();

			}
		}
	}

	/// <summary>
	/// Put all the variables in the payload, and send the request to the server
	/// </summary>
	private void SendRequestToServer() {

		Dictionary<string, string> kParams = new Dictionary<string, string>();
		kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
		kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();


		JSONClass kBody = new JSONClass();
		kBody["accountId"] = m_accountId;
		kBody["deviceProfile"] = m_deviceProfile;
		kBody["maxProgression"] = PersistenceUtils.SafeToString(m_playerProgression);
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

	/// <summary>
	/// Send a request to sync with the server. Only if needed, so this method can actually be spammed.
	/// </summary>
	public void SyncWithServer() {
		// Spam-preventer
		if(m_syncingWithServer) return;

		// Don't do it of not needed
		if(UsersManager.currentUser.clusterSynced) return;

		// All good, launch the request!
		m_syncingWithServer = true; // Spam-preventer
		GameServerManager.SharedInstance.Clustering_SetClusterId(UsersManager.currentUser.GetClusterId(), OnSetClusterIdResponse);
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
	private bool OnGetClusterResponse(string _strResponse, string _strCmd, int _reponseCode) {
		// Aux vars
		string currentClusterId = UsersManager.currentUser.GetClusterId();
		bool responseOk = false;

		// Parse server response
		if(_strResponse != null) {
			switch(_reponseCode) {
				case 200: // No error
				case 204: // No error, but the server doesnt know the cluster id
					{

					JSONNode kJSON = JSON.Parse(_strResponse);
					if(kJSON != null) {
						if(kJSON.ContainsKey("result")) {
							if(kJSON["result"] == true) {
								if(kJSON.ContainsKey("clusterId")) {
									// The server knows the cluster
									string clusterId = kJSON["clusterId"];

									// If the player already belongs to a non-generic cluster, ignore the response
									if(!string.IsNullOrEmpty(currentClusterId) && currentClusterId != CLUSTER_GENERIC) {
										// The server has given a different cluster Id than the one we have stored, mark it as not in sync!
										if(currentClusterId != clusterId) {
											UsersManager.currentUser.clusterSynced = false;
											SyncWithServer();
										}
									} else {
										// Store the new cluster Id
										UsersManager.currentUser.SetClusterId(clusterId);

										// Since the answer comes from the server, we're in sync!
										UsersManager.currentUser.clusterSynced = true;

										// Log
										Debug.Log("<color=white>Player assigned by server to cluster '" + clusterId + "' </color>");
									}
								}

								responseOk = true;
							}

							// else: the server doesnt know the cluster. Leave it empty.
							else {
								// Server returned an error
								Debug.LogError("Requests " + _strCmd + " returned error " +
									kJSON["errorCode"] + ": " + kJSON["errorMsg"]);

								responseOk = false;
							}
						}
					} break;
				}

				default: {
					// An error happened
					responseOk = false;
					break;
				}
			}
		}

		if(m_offlineMode) {
			return false;
		} else {
			return responseOk;
		}

	}

	/// <summary>
	/// Response from the server was received.
	/// </summary>
	/// <param name="_error">Error data.</param>
	/// <param name="_response">Response data.</param>
	public void OnSetClusterIdResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// Clear spam preventer
		m_syncingWithServer = false;

		// Parse response
		if(_error == null && _response != null && _response.ContainsKey("response") && _response["response"] != null) {
			JSONNode kJSON = JSON.Parse(_response["response"] as string);
			if(kJSON != null && kJSON.ContainsKey("result")) {
				if(kJSON["result"].AsBool == true) {
					// We're in sync!
					UsersManager.currentUser.clusterSynced = true;
					//Debug.Log("SetClusterId: Success!");
				} else {
					// Not in sync, will automatically retry after some time. Just make sure the flag is properly set.
					UsersManager.currentUser.clusterSynced = false;
					//Debug.LogError("SetClusterId: Unsuccessful! " + kJSON["errorCode"] + ": " + kJSON["errorMsg"]);
				}
			}
		} else if(_error != null) {
			//Debug.LogError("SetClusterId: Something went wrong! Error " + _error.ToString());
		} else {
			//Debug.LogError("SetClusterId: Something went wrong!");
		}
	}
}