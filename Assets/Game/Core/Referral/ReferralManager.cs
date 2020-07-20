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




	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	// Singleton instance
	private static ReferralManager m_instance = null;

	// Communication with server
	private bool m_offlineMode = false;

	// Rewards claimed
	private Queue<OfferPackReferralReward> m_pendingRewards = new Queue<OfferPackReferralReward>();
	public Queue<OfferPackReferralReward> pendingRewards
        { get => m_pendingRewards;  }

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
	/// Update all the referral related data from the server: amount of referrals (friends invited)
	/// and list of rewards ready to claim
	/// </summary>
	public void GetInfoFromServer ()
	{

		if (!m_offlineMode)
		{
            // Find the current active referral offer 
			OfferPackReferral offer = OffersManager.GetActiveReferralOffer();

			if (offer != null)
            {
				string referralSku = offer.def.sku;

				GameServerManager.SharedInstance.Referral_GetInfo(referralSku, OnGetInfoResponse);
			}			
		}
	}




	/// <summary>
	/// Tell the server that the player is claiming all the rewards achieved. 
	/// </summary>
	/// <param name="_reward">The referral reward claimed</param>
	public void ReclaimAllFromServer()
	{

		if (!m_offlineMode)
		{
			// Find the current active referral offer 
			OfferPackReferral offer = OffersManager.GetActiveReferralOffer();

			if (offer != null)
			{
				string referralSku = offer.def.sku;

				GameServerManager.SharedInstance.Referral_ReclaimAll(referralSku, OnReclaimAllResponse);
			}
		}
	}

    /// <summary>
    /// Notify the server that the invited user has installed and open the game
    /// </summary>
    /// <param name="_userId">The id of the user that sent the invitation</param>
    public void MarkReferral (string _userId)
    {
		if (!m_offlineMode)
		{ 
				GameServerManager.SharedInstance.Referral_MarkReferral(_userId, OnMarkReferralResponse );
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

        // Notify a tracking event
		HDTrackingManager.Instance.Notify_ReferralSendInvite(_shortLink, HDTrackingManager.EReferralOrigin.Popup);
	}


	/// <summary>
	/// The server answers with the information related to this user rewards:
	/// total: long - Total of users referred by current logged user
	/// reward: JSONObject - Reward to give to the user
	/// rewards: JSONArray<JSONObject>  - Array of rewards to be reclaimed sorted by referral number
	/// </summary>
	private void OnGetInfoResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
	{
		// If there was no error, update local cache
		if (_error == null && _response != null && _response.ContainsKey("response"))
		{
            if (_response["response"] != null)
            {
				JSONNode kJSON = JSON.Parse(_response["response"] as string);
				if (kJSON != null)
				{

					if (kJSON.ContainsKey("total"))
					{

						int referrals = PersistenceUtils.SafeParse<int>(kJSON["total"]);

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
				}
			}
		}
	}



	/// <summary>
	/// The server is confirming what rewards have been collected
	/// rewards: JSONArray<JSONObject>  - Array of rewards successfully claimed
	/// </summary>
	public void OnReclaimAllResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
	{
		// If there was no error, update local cache
		if (_error == null && _response != null && _response.ContainsKey("response"))
		{
			if (_response["response"] != null)
			{
				JSONNode kJSON = JSON.Parse(_response["response"] as string);
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

					}
				}	
			}
		}
	}


	/// <summary>
	/// Response from the server was received
	/// </summary>
	/// <param name="_strResponse">Json containing the response</param>
	/// <param name="_strCmd">The command sent</param>
	/// <param name="_reponseCode">Response code. 200 if the request was successful</param>
	/// <returns>Returns true if the response was successful</returns>
	public void OnMarkReferralResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response)
	{
		// If there was no error, update local cache
		if (_error == null && _response != null && _response.ContainsKey("response"))
		{
			if (_response["response"] != null)
			{

				bool success = false;

				JSONNode kJSON = JSON.Parse(_response["response"] as string);
				if (kJSON != null)
				{

					if (kJSON.ContainsKey("result"))
					{
						if (kJSON["result"].AsBool == true)
						{
							success = true;
						}
                        else
                        {
							success = false;
                        }

                        // No matter if the referral confirmation was valid or not.
                        // We mark the flag as confirmed, so this call is never made again for this user/device.
                        // This way we save a lot of unnecesary calls to the server.
						UsersManager.currentUser.referralConfirmed = true;
					}
				}

				// Send tracking information

				// TODO: find the proper linkId and reward parameters
				string linkId = null;
				string reward = null;

				HDTrackingManager.Instance.Notify_ReferralInstall(linkId, reward, success);
			}			
		}
	}
}