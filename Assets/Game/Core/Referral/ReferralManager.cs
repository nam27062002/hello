// ReferralManager.cs
// Hungry Dragon
// 
// Created by Jose M. Olea 
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
public class ReferralManager
{

	//------------------------------------------------------------------------//
	// CONSTANTS                											  //
	//------------------------------------------------------------------------//

	private static readonly string GET_INFO = "/api/referral/getInfo";
	private static readonly string RECLAIM_REWARD= "/api/referral/reclaimReward";
	private static readonly string RECLAIM_ALL = "/api/referral/reclaimAll";
	private static readonly string MARK_REFERRAL = "/api/referral/markReferral";


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	// Singleton instance
	private static ReferralManager m_instance = null;

	// Communication with server
	private bool m_registered = false;
	private bool m_offlineMode = false;

	// Rewards claimed
	private Queue<OfferPackReferralReward> m_pendingRewards = new Queue<OfferPackReferralReward>();
	public Queue<OfferPackReferralReward> pendingRewards
        { get => m_pendingRewards; set => m_pendingRewards = value; }

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ReferralManager()
	{

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~ReferralManager()
	{

	}

	// Singleton
	public static ReferralManager instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new ReferralManager();
			}

			return m_instance;
		}
	}







    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//


    /// <summary>
    /// Register the endpoints 
    /// </summary>
    /// <param name="_offlineMode"></param>
    private void Initialize(bool _offlineMode = false)
	{
		if (!m_registered)
		{
			m_offlineMode = _offlineMode;

			NetworkManager.SharedInstance.RegistryEndPoint(GET_INFO, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, new int[] { 200, 404, 500, 503 }, OnGetInfoResponse);
			NetworkManager.SharedInstance.RegistryEndPoint(RECLAIM_ALL, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, new int[] { 200, 404, 500, 503 }, OnReclaimAllResponse);
			NetworkManager.SharedInstance.RegistryEndPoint(MARK_REFERRAL, NetworkManager.EPacketEncryption.E_ENCRYPTION_AES, new int[] { 200, 404, 500, 503 }, OnMarkReferralResponse);


			m_registered = true;
		}
	}

    /// <summary>
	/// Update all the referral related data from the server: amount of referrals (friends invited)
	/// and list of rewards ready to claim
	/// </summary>
	public void GetInfoFromServer ()
	{
        // Make sure the enpoints are registerd
		Initialize();

		if (!m_offlineMode)
		{
			// Dont make the request if we are not logged yet in the server
			if (GameSessionManager.SharedInstance.IsLogged())
			{

				Dictionary<string, string> kParams = new Dictionary<string, string>();
				kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
				kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();

				// Send it to the server
				ServerManager.SharedInstance.SendCommand(GET_INFO, kParams);

			}
		}
	}


    /// <summary>
	/// Tell the server that the player is claiming a reward. The server will double check if this
	/// reward has been obtained by the player.
	/// </summary>
	/// <param name="_reward">The referral reward claimed</param>
	public void ReclaimRewardFromServer(OfferPackReferralReward _reward)
	{
		// Make sure the enpoints are registerd
		Initialize();

		if (!m_offlineMode)
		{
			// Dont make the request if we are not logged yet in the server
			if (GameSessionManager.SharedInstance.IsLogged())
			{

				Dictionary<string, string> kParams = new Dictionary<string, string>();
				kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
				kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();

				JSONClass kBody = new JSONClass();
				kBody["rewardSku"] = _reward.sku;

				// Send it to the server
				ServerManager.SharedInstance.SendCommand(RECLAIM_REWARD, kParams);

			}
		}
	}


	/// <summary>
	/// Tell the server that the player is claiming all the rewards achieved. 
	/// </summary>
	/// <param name="_reward">The referral reward claimed</param>
	public void ReclaimAllFromServer()
	{
		// Make sure the enpoints are registerd
		Initialize();

		if (!m_offlineMode)
		{
			// Dont make the request if we are not logged yet in the server
			if (GameSessionManager.SharedInstance.IsLogged())
			{

				Dictionary<string, string> kParams = new Dictionary<string, string>();
				kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
				kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();

				// Send it to the server
				ServerManager.SharedInstance.SendCommand(RECLAIM_ALL, kParams);

			}
		}
	}

    /// <summary>
    /// Notify the server that the invited user has installed and open the game
    /// </summary>
    /// <param name="_userId">The id of the user that sent the invitation</param>
    public void MarkReferral (string _userId)
    {
		Initialize();

		if (!m_offlineMode)
		{
			// Dont make the request if we are not logged yet in the server
			if (GameSessionManager.SharedInstance.IsLogged())
			{

				Dictionary<string, string> kParams = new Dictionary<string, string>();
				kParams["uid"] = GameSessionManager.SharedInstance.GetUID();
				kParams["token"] = GameSessionManager.SharedInstance.GetUserToken();


				JSONClass kBody = new JSONClass();
				kBody["referredBy"] = _userId;

				// Send it to the server
				ServerManager.SharedInstance.SendCommand(MARK_REFERRAL, kParams, kBody.ToString());

			}
		}

	}

	/// <summary>
	/// Convert a list of skus in a list of rewards
	/// </summary>
	/// <param name="_skus">List of skus</param>
	/// <returns></returns>
	private List<OfferPackReferralReward> GetRewardsFromSkus (List<string> _skus)
    {
		List<OfferPackReferralReward> rewards = new List<OfferPackReferralReward>();

        foreach (string sku in _skus)
        {
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.REFERRAL_REWARDS, sku);

            if (def != null)
            {
				OfferPackReferralReward reward = new OfferPackReferralReward();
				reward.InitFromRewardDefinition(def);

				rewards.Add(reward);
            }

		}

		return rewards;            

	}


    /// <summary>
    /// Remove this reward from the unlocked rewards list and give it to the player
    /// </summary>
    /// <param name="_sku"></param>
    public void ApplyReward (string _sku)
    {
		OfferPackReferralReward reward = UsersManager.currentUser.unlockedReferralRewards.Find(r => r.sku == _sku);

        if (reward != null)
        {
            // Remove it from the unlocked rewards list
			UsersManager.currentUser.unlockedReferralRewards.Remove(reward);

            // Put the reward in the peding queue so it will delivered to the player asap
			m_pendingRewards.Enqueue(reward);

			// Notify the listeners that there is a pending reward ready
			Messenger.Broadcast(MessengerEvents.REFERRAL_REWARDS_CLAIMED, reward.sku);

		}
    }



	/// <summary>
	/// Remove this rewards from the unlocked rewards list and give them to the player
	/// </summary>
	/// <param name="sku"></param>
	public void ApplyRewards(List<String> _skus)
	{

		bool success = false;

        foreach (String sku in _skus)
        {

			OfferPackReferralReward reward = UsersManager.currentUser.unlockedReferralRewards.Find(r => r.referralRewardSku == sku);

			if (reward != null)
			{
				// Remove it from the unlocked rewards list
				UsersManager.currentUser.unlockedReferralRewards.Remove(reward);

				// Put the reward in the peding queue so it will delivered to the player asap
				m_pendingRewards.Enqueue(reward);

				success = true;

			}
		}

        if (success)
        {
			// Notify the listeners that there are pending rewards ready
			Messenger.Broadcast(MessengerEvents.REFERRAL_REWARDS_CLAIMED);
		}

	}


    /// <summary>
    /// Button invite has been pressed.
    /// </summary>
    public void InviteFriends()
    {
		string userId = UsersManager.currentUser.userId;

		// Get the link to share from firebase
		CaletyDynamicLinks.createLinkUserInvite(userId, OnShortLinkCreated);

    }


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//


	/// <summary>
	/// Delegate for receiving the referral shortlink from firebase
	/// </summary>
	/// <param name="_link"></param>
	public void OnShortLinkCreated(string _shortLink)
	{

		string title = LocalizationManager.SharedInstance.Localize("TID_REFERRAL_SHARE_TITLE");

		// Open the share dialog
		CaletyShareUtil.ShareLink(title, _shortLink);
	}


	/// <summary>
	/// Response from the server was received
	/// </summary>
	/// <param name="_strResponse">Json containing the response</param>
	/// <param name="_strCmd">The command sent</param>
	/// <param name="_reponseCode">Response code. 200 if the request was successful</param>
	/// <returns>Returns true if the response was successful</returns>
	private bool OnGetInfoResponse(string _strResponse, string _strCmd, int _reponseCode)
	{
		bool responseOk = false;

		if (_strResponse != null)
		{
			switch (_reponseCode)
			{
				case 200: // No error
					{

						JSONNode kJSON = JSON.Parse(_strResponse);
						if (kJSON != null)
						{

							if (kJSON.ContainsKey("total"))
							{

								int referrals = PersistenceUtils.SafeParse<int> ( kJSON["total"] );

                                // Store the value in user profile
								UsersManager.currentUser.totalReferrals = referrals;

							}

							if (kJSON.ContainsKey("rewards"))
							{

								List<string> skuList = new List<string>();

                                foreach (JSONNode sku in kJSON["rewards"].AsArray)
                                {
									skuList.Add(sku["sku"].Value.ToString());
                                }

								// Convert skus to actual rewards 
								List<OfferPackReferralReward> rewards = GetRewardsFromSkus(skuList);

                                // Store them in user profile
								UsersManager.currentUser.unlockedReferralRewards = rewards;
								
							}

							responseOk = true;
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



	/// <summary>
	/// Response from the server was received
	/// </summary>
	/// <param name="_strResponse">Json containing the response</param>
	/// <param name="_strCmd">The command sent</param>
	/// <param name="_reponseCode">Response code. 200 if the request was successful</param>
	/// <returns>Returns true if the response was successful</returns>
	public bool OnReclaimRewardResponse(string _strResponse, string _strCmd, int _reponseCode)
	{
		bool responseOk = false;

		if (_strResponse != null)
		{
			switch (_reponseCode)
			{
				case 200: // No error
					{

						JSONNode kJSON = JSON.Parse(_strResponse);
						if (kJSON != null)
						{
							if (kJSON.ContainsKey("result"))
							{
								if (kJSON["result"] == true)
								{
									// The reclamin operation was sucessful
									if (kJSON.ContainsKey("sku"))
									{

										// Notify that the reward has been claimed successfully
										ApplyReward (kJSON["sku"].ToString());

										responseOk = true;
									}

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


	/// <summary>
	/// Response from the server was received
	/// </summary>
	/// <param name="_strResponse">Json containing the response</param>
	/// <param name="_strCmd">The command sent</param>
	/// <param name="_reponseCode">Response code. 200 if the request was successful</param>
	/// <returns>Returns true if the response was successful</returns>
	public bool OnReclaimAllResponse(string _strResponse, string _strCmd, int _reponseCode)
	{
		bool responseOk = false;

		if (_strResponse != null)
		{
			switch (_reponseCode)
			{
				case 200: // No error
					{

						JSONNode kJSON = JSON.Parse(_strResponse);
						if (kJSON != null)
						{
							if (kJSON.ContainsKey("rewards"))
							{

								List<string> skuList = new List<string>();

								foreach (JSONNode reward in kJSON["rewards"].AsArray)
								{
									skuList.Add(reward["sku"].Value.ToString());
								}

								// Notify that the reward has been claimed successfully
								ApplyRewards(skuList);

								responseOk = true;

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


	/// <summary>
	/// Response from the server was received
	/// </summary>
	/// <param name="_strResponse">Json containing the response</param>
	/// <param name="_strCmd">The command sent</param>
	/// <param name="_reponseCode">Response code. 200 if the request was successful</param>
	/// <returns>Returns true if the response was successful</returns>
	public bool OnMarkReferralResponse(string _strResponse, string _strCmd, int _reponseCode)
	{
		bool responseOk = false;

		if (_strResponse != null)
		{
			switch (_reponseCode)
			{
				case 200: // No error
					{

						JSONNode kJSON = JSON.Parse(_strResponse);
						if (kJSON != null)
						{
							if (kJSON.ContainsKey("result"))
							{
                                if (kJSON["result"].AsBool == true)
                                {
                                    // Store the value in user profile so markReferral is not sent ever
                                    // again for this device/user
									UsersManager.currentUser.referralConfirmed = true;
                                }

								responseOk = true;

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