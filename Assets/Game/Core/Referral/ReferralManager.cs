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
public class ReferralManager {
	//------------------------------------------------------------------------//
	// CONSTANTS                											  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Singleton instance
	private static ReferralManager m_instance = null;
	public static ReferralManager instance {
		get {
			if(m_instance == null) {
				m_instance = new ReferralManager();
			}
			return m_instance;
		}
	}

	// Communication with server
	private bool m_offlineMode = false;
	private bool m_waitingServerResponse = false;

	// Rewards claimed
	private Queue<OfferPackReferralReward> m_pendingRewards = new Queue<OfferPackReferralReward>();
	public Queue<OfferPackReferralReward> pendingRewards { get => m_pendingRewards; }

	// Tracking
	private HDTrackingManager.EReferralOrigin m_inviteOrigin;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ReferralManager() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~ReferralManager() {

	}

	/// <summary>
	/// To be called externally every frame, since this singleton is not a MonoBehaviour.
	/// </summary>
	public void Update() {
		// Try to mark the referral install if needed
		ConfirmReferralConversionIfPossible();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// To be call whenever the app is ready to read the referral link.
	/// </summary>
	public void ReadReferralLink() {
		// Ignore if the player has already been referred
		if(UsersManager.currentUser.referralConfirmed)
			return;

		// Ignore if we already have a referrer ID assigned
		if(!string.IsNullOrEmpty(UsersManager.currentUser.referrerUserId))
			return;

		// All good! Get the referrer ID from Calety's Deep Link system
		string referrerId = CaletyDynamicLinks.getReferrerID();
		if(!string.IsNullOrEmpty(referrerId)) {
			// Valid Id, store it
			UsersManager.currentUser.referrerUserId = referrerId;
		}
	}

	enum State {
		INIT,					// Initial state, we haven't read the deep link yet
		NOT_REFERRED,			// We've read the data from the deep link, no referrer Id was found -> user not referred
		PENDING_CONFIRMATION,	// We've read the data from the deep link, a valid referrer Id was found, pending confirmation with server
		REFERRAL_CONFIRMED		// Server confirmation received, with or without success
	}

	/// <summary>
	// The invited user has to confirm that the app has been open, so the referral user
	// increments its referal counter and can claim rewards.
	/// </summary>
	public void ConfirmReferralConversionIfPossible() {
		// Exit if the conversion has been already confirmed (with or without success)
		if(UsersManager.currentUser.referralConfirmed)
			return;

		// Nothing to do either if we don't have a valid referrer User Id
		// [AOC] TODO!! FOR SURE?
		//		 Non-referred users will be constantly performing this check (string comparison)
		//		 
		//		 WE MAY WANT TO MARK AS REFERRAL CONFIRMED (UNSUCCESSFUL) SO WE DON'T KEEP TRYING FOREVER
		//		 THE ONLY RISK HERE IS - MAY WE GET THE REFERRER USER ID AFTER ATEMPTING THE CONVERSION?
		//		 REFLEXIONEM-HI, SI US PLAU, REFLEXIONEM-HI
		if(string.IsNullOrEmpty(UsersManager.currentUser.referrerUserId))
			return;

		// We need a valid DNA Id to validate the request
		if(string.IsNullOrEmpty(HDTrackingManager.Instance.GetDNAProfileID()))
			return;

		// Skip if we are waiting for a server response
		if(m_waitingServerResponse)
			return;

		// All good! Notify the server confirming the conversion of the invited player
		if(!m_offlineMode) {
			m_waitingServerResponse = true;
			GameServerManager.SharedInstance.Referral_MarkReferral(
				UsersManager.currentUser.referrerUserId, 
				OnMarkReferralResponse
			);
		}
	}

	/// <summary>
	/// Update all the referral related data from the server: amount of referrals (friends invited)
	/// and list of rewards ready to claim
	/// </summary>
	public void GetInfoFromServer() {

		if(!m_offlineMode) {
			// Find the current active referral offer 
			OfferPackReferral offer = OffersManager.GetActiveReferralOffer();

			if(offer != null) {
				string referralSku = offer.def.sku;

				GameServerManager.SharedInstance.Referral_GetInfo(referralSku, OnGetInfoResponse);
			}
		}
	}
	
	/// <summary>
	/// Tell the server that the player is claiming all the rewards achieved. 
	/// </summary>
	/// <param name="_reward">The referral reward claimed</param>
	public void ReclaimAllFromServer() {

		if(!m_offlineMode) {
			// Find the current active referral offer 
			OfferPackReferral offer = OffersManager.GetActiveReferralOffer();

			if(offer != null) {
				string referralSku = offer.def.sku;

				GameServerManager.SharedInstance.Referral_ReclaimAll(referralSku, OnReclaimAllResponse);
			}
		}
	}

	/// <summary>
	/// Convert a list of skus in a list of rewards
	/// </summary>
	/// <param name="_skus">List of skus</param>
	/// <returns></returns>
	private List<OfferPackReferralReward> GetRewardsFromSkus(List<string> _skus) {
		List<OfferPackReferralReward> rewards = new List<OfferPackReferralReward>();
		foreach(string sku in _skus) {
			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.REFERRAL_REWARDS, sku);
			if(def != null) {
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
	public void ApplyReward(string _sku) {
		OfferPackReferralReward reward = UsersManager.currentUser.unlockedReferralRewards.Find(r => r.sku == _sku);
		if(reward != null) {
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
	public void ApplyRewards(List<String> _skus) {
		bool success = false;
		foreach(String sku in _skus) {
			OfferPackReferralReward reward = UsersManager.currentUser.unlockedReferralRewards.Find(r => r.referralRewardSku == sku);
			if(reward != null) {
				// Remove it from the unlocked rewards list
				UsersManager.currentUser.unlockedReferralRewards.Remove(reward);

				// Put the reward in the peding queue so it will delivered to the player asap
				m_pendingRewards.Enqueue(reward);
				success = true;
			}
		}

		if(success) {
			// Notify the listeners that there are pending rewards ready
			Messenger.Broadcast(MessengerEvents.REFERRAL_REWARDS_CLAIMED);
		}

	}

	/// <summary>
	/// Button invite has been pressed.
	/// </summary>
	public void InviteFriends(HDTrackingManager.EReferralOrigin _origin) {
		string userId = UsersManager.currentUser.userId;

		// Store origin for tracking purposes
		m_inviteOrigin = _origin;

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
	public void OnShortLinkCreated(string _shortLink, CaletyDynamicLinks.shortLinkResult result) {
		switch(result) {

			case CaletyDynamicLinks.shortLinkResult.OK:
				string title = LocalizationManager.SharedInstance.Localize("TID_REFERRAL_SHARE_TITLE");

				// Open the share dialog
				CaletyShareUtil.ShareLink(title, _shortLink);

				// Increment counter
				UsersManager.currentUser.invitesSent++;

				// Tracking
				HDTrackingManager.Instance.Notify_ReferralSendInvite(m_inviteOrigin);
				break;


			case CaletyDynamicLinks.shortLinkResult.CANCELLED:
			case CaletyDynamicLinks.shortLinkResult.FAULTED:
				// Show error popup in the game
				string text = LocalizationManager.SharedInstance.Localize("TID_GEN_ERROR");
				UIFeedbackText.CreateAndLaunch(
					text,
					new Vector2(0.5f, 0.33f),
					PopupManager.canvas.transform as RectTransform
				);  // Use popup's canvas

				break;
		}
	}

	/// <summary>
	/// The server answers with the information related to this user rewards:
	/// total: long - Total of users referred by current logged user
	/// reward: JSONObject - Reward to give to the user
	/// rewards: JSONArray<JSONObject>  - Array of rewards to be reclaimed sorted by referral number
	/// </summary>
	private void OnGetInfoResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// If there was no error, update local cache
		if(_error == null && _response != null && _response.ContainsKey("response")) {
			if(_response["response"] != null) {
				JSONNode kJSON = JSON.Parse(_response["response"] as string);
				if(kJSON != null) {

					if(kJSON.ContainsKey("total")) {

						int referrals = PersistenceUtils.SafeParse<int>(kJSON["total"]);

						// Store the value in user profile
						UsersManager.currentUser.totalReferrals = referrals;

					}

					if(kJSON.ContainsKey("rewards")) {

						List<string> skuList = new List<string>();

						foreach(JSONNode sku in kJSON["rewards"].AsArray) {
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
	public void OnReclaimAllResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// If there was no error, update local cache
		if(_error == null && _response != null && _response.ContainsKey("response")) {
			if(_response["response"] != null) {
				JSONNode kJSON = JSON.Parse(_response["response"] as string);
				if(kJSON != null) {
					if(kJSON.ContainsKey("rewards")) {

						List<string> skuList = new List<string>();

						foreach(JSONNode reward in kJSON["rewards"].AsArray) {
							skuList.Add(reward["sku"].Value.ToString());
						}

						// Notify that the reward has been claimed successfully
						ApplyRewards(skuList);
					}
				}
			}
		}

		// Notify the game
		Messenger.Broadcast<FGOL.Server.Error>(MessengerEvents.REFERRAL_REWARDS_CLAIM_RESPONSE_RECEIVED, _error);
	}

	/// <summary>
	/// Response from the server was received
	/// </summary>
	/// <param name="_strResponse">Json containing the response</param>
	/// <param name="_strCmd">The command sent</param>
	/// <param name="_reponseCode">Response code. 200 if the request was successful</param>
	/// <returns>Returns true if the response was successful</returns>
	public void OnMarkReferralResponse(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) {
		// No longer waiting for response
		m_waitingServerResponse = false;

		// If there was no error, update local cache
		if(_error == null && _response != null && _response.ContainsKey("response")) {
			if(_response["response"] != null) {

				bool success = false;

				JSONNode kJSON = JSON.Parse(_response["response"] as string);
				if(kJSON != null) {

					if(kJSON.ContainsKey("result")) {
						if(kJSON["result"].AsBool == true) {
							success = true;
						} else {
							success = false;
							Debug.LogError("Unsuccessful! " + kJSON["errorCode"] + ": " + kJSON["errorMsg"]);
						}

						// No matter if the referral confirmation was valid or not.
						// We mark the flag as confirmed, so this call is never made again for this user/device.
						// This way we save a lot of unnecesary calls to the server.
						UsersManager.currentUser.referralConfirmed = true;
					}
				}

				// Send tracking information
				HDTrackingManager.Instance.Notify_ReferralInstall(success, UsersManager.currentUser.referrerUserId);
			}
		}
	}
}