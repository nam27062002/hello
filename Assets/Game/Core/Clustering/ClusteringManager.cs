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

    // The cluster will be calculated based on the data gathered until the end of this run
	public static int CALCULATE_CLUSTER_AFTER_RUN = 2;

	public static string CLUSTER_UNKNOW = "UNKNOWN";


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//



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

    public static string GetClusterId()
    {
		string clusterId = UsersManager.currentUser.clusterId;

		if (!string.IsNullOrEmpty(clusterId)){
            // the client already knows the cluster ID
			return clusterId;
        } else
        {
            // we dont know it yet. Request the cluster ID to the server.
			return RequestClusterIdFromServer();
        }

    }

    public static string RequestClusterIdFromServer()
    {
		string accountId = UsersManager.currentUser.userId;

		string deviceProfile = FeatureSettingsManager.instance.Device_CalculatedProfile;

		int playerProgression = UsersManager.currentUser.GetPlayerProgress();

		bool shopEntered = UsersManager.currentUser.hasEnteredShop;

		int firerushes = UsersManager.currentUser.firerushesCount;

		int gemsSpent = UsersManager.currentUser.gemsSpent;

		long score = UsersManager.currentUser.totalScore;

		int boostTime = UsersManager.currentUser.boostTime;


		// Make call to server

        Debug.Log (
            String.Format("Clustering data: accountId: {0}, deviceProfile: {1}, playerProgression: {2}, " +
            "shopEntered: {3}, firerushes: {4}, gemsSpent: {5}, score: {6}, boostTime: {7}",
            accountId, deviceProfile,playerProgression,shopEntered,firerushes,gemsSpent,score,boostTime) );

		string clusterId = CLUSTER_UNKNOW;

        if ( !string.IsNullOrEmpty(clusterId) && clusterId != CLUSTER_UNKNOW)
        {
			UsersManager.currentUser.clusterId = clusterId;
        }

		return clusterId;
        
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}