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
using FirebaseWrapper;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class ReferralManager : Singleton<ReferralManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS                											  //
	//------------------------------------------------------------------------//
	private static readonly string INVITED_BY_DLINK_PARAM = "invitedby";
	public enum State {
		// KEEP INDEXES THE SAME!
		UNKNOWN = 0,                // Initial state, we haven't read the deep link yet
		PENDING_CONFIRMATION = 1,   // We've read the data from the deep link, pending confirmation with server regardless of whether we found a valid referrer Id or not
		REFERRAL_CONFIRMED = 3      // Server confirmation received, with or without success
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Deep linking
	private bool m_deepLinkReceived = false;
	private Dictionary<string, string> m_deepLinkParams = null;

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
	#region generic_methods
	/// <summary>
	/// Default constructor.
	/// </summary>
	public ReferralManager() {
		// Subscribe to external events
		Messenger.AddListener<Dictionary<string, string>>(MessengerEvents.INCOMING_DEEPLINK_NOTIFICATION, OnDeepLinkNotification);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~ReferralManager() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Dictionary<string, string>>(MessengerEvents.INCOMING_DEEPLINK_NOTIFICATION, OnDeepLinkNotification);
	}

	/// <summary>
	/// To be called externally every frame, since this singleton is not a MonoBehaviour.
	/// </summary>
	public void Update() {
		// Try to mark the referral install if needed
		ConfirmReferralConversionIfPossible();
	}
	#endregion

	//------------------------------------------------------------------------//
	// REFERRAL DETECTION METHODS											  //
	//------------------------------------------------------------------------//
	#region referral_detection
	/// <summary>
	/// To be call whenever the app is ready to read the referral link.
	/// </summary>
	/// <returns>Whether the link was successfully read from the deep link or not.</returns>
	public bool ReadReferralLink() {
		// Only if we don't have any referral info yet
		if(UsersManager.currentUser.referralState != State.UNKNOWN)
			return false;

		// For retrocompatibility, if we have a referrer ID assigned, force a state change and return
		// [AOC] This might not be needed since we already do it on the UserProfile.Load(), but just in case
		if(!string.IsNullOrEmpty(UsersManager.currentUser.referrerUserId)) {
			UsersManager.currentUser.referralState = State.PENDING_CONFIRMATION;
			return true;
		}

		// All checks passed! Get the referrer Id
		return ReadReferralLinkInternal();
	}

	/// <summary>
	/// Try to read the referral link data withou any checks.
	/// </summary>
	/// <returns>Whether the link was successfully read from the deep link or not.</returns>
	private bool ReadReferralLinkInternal() {
		// Have we received data from the deep link system?
		if(m_deepLinkReceived) {
			// Does received data contain info about the referrer user?
			if(m_deepLinkParams != null && m_deepLinkParams.ContainsKey(INVITED_BY_DLINK_PARAM)) {
				// Read the referrer Id from the deep link
				string referrerId = m_deepLinkParams[INVITED_BY_DLINK_PARAM];

				// Change state so we don't attempt to read the link anymore!
				UsersManager.currentUser.referralState = State.PENDING_CONFIRMATION;

				// Store referrer Id (whether it's valid or not)
				UsersManager.currentUser.referrerUserId = referrerId;

				// Nothing else to do! Notify that the link was successfully read
				return true;
			}
		}

		return false;
	}

	/// <summary>
	// The invited user has to confirm that the app has been open, so the referral user
	// increments its referal counter and can claim rewards.
	/// </summary>
	public void ConfirmReferralConversionIfPossible() {
		// Exit if the conversion has been already confirmed (with or without success)
		//if(UsersManager.currentUser.referralConfirmed)
		if(UsersManager.currentUser.referralState == State.REFERRAL_CONFIRMED)
			return;

		// If we haven't yet received info from the deep link, try again
		if(UsersManager.currentUser.referralState == State.UNKNOWN) {
			// If no link was successfully read, don't do anything else
			if(!ReadReferralLinkInternal()) return;
		}

		// We need a valid DNA Id to validate the request
		if(string.IsNullOrEmpty(HDTrackingManager.Instance.GetDNAProfileID()))
			return;

		// Skip if we are already waiting for a server response
		if(m_waitingServerResponse)
			return;

		// All good! Notify the server confirming the conversion of the invited player
		m_waitingServerResponse = true;
		GameServerManager.SharedInstance.Referral_MarkReferral(
			UsersManager.currentUser.referrerUserId, 
			OnMarkReferralResponse
		);
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

		// If there was no error, mark as confirmed
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

						// No matter if the referral confirmation was valid or not, change the state to confirmed
						// so this call is never made again for this user/device.
						// This way we save a lot of unnecesary calls to the server.
						UsersManager.currentUser.referralState = State.REFERRAL_CONFIRMED;
					}
				}

				// Send tracking information
				HDTrackingManager.Instance.Notify_ReferralInstall(success, UsersManager.currentUser.referrerUserId);
			}
		}
	}
	#endregion

	//------------------------------------------------------------------------//
	// REFERRAL REWARDS METHODS												  //
	//------------------------------------------------------------------------//
	#region referral_rewards
	/// <summary>
	/// Update all the referral related data from the server: amount of referrals (friends invited)
	/// and list of rewards ready to claim
	/// </summary>
	public void GetInfoFromServer() {
		// Find the current active referral offer 
		OfferPackReferral offer = OffersManager.GetActiveReferralOffer();

		if(offer != null) {
			string referralSku = offer.def.sku;

			GameServerManager.SharedInstance.Referral_GetInfo(referralSku, OnGetInfoResponse);
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
	/// Tell the server that the player is claiming all the rewards achieved. 
	/// </summary>
	/// <param name="_reward">The referral reward claimed</param>
	public void ReclaimAllFromServer() {
		// Find the current active referral offer 
		OfferPackReferral offer = OffersManager.GetActiveReferralOffer();

		if(offer != null) {
			string referralSku = offer.def.sku;

			GameServerManager.SharedInstance.Referral_ReclaimAll(referralSku, OnReclaimAllResponse);
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
	#endregion

	//------------------------------------------------------------------------//
	// REFERRAL INVITE METHODS												  //
	//------------------------------------------------------------------------//
	#region referral_invite
	/// <summary>
	/// Button invite has been pressed.
	/// </summary>
	public void InviteFriends(HDTrackingManager.EReferralOrigin _origin) {
		string userId = UsersManager.currentUser.userId;

		// Store origin for tracking purposes
		m_inviteOrigin = _origin;


		// Do not send "local_user" if there is no user, it crashes the server
		if (userId != UserProfile.LOCAL_USER)

		{
			// Get the link to share from firebase
			DynamicLinksWrapper.createLinkUserInvite(userId, OnShortLinkCreated);
		}
	}

	/// <summary>
	/// Delegate for receiving the referral shortlink from firebase
	/// </summary>
	/// <param name="_link"></param>
	public void OnShortLinkCreated(string _shortLink, DynamicLinksWrapper.shortLinkResult result) {
		switch(result) {

			case DynamicLinksWrapper.shortLinkResult.OK:
				string title = LocalizationManager.SharedInstance.Localize("TID_REFERRAL_SHARE_TITLE");

				// Open the share dialog
//				CaletyShareUtil.ShareLink(title, _shortLink);
				PlatformUtils.Instance.ShareImage(null, _shortLink, title);

				// Increment counter
				UsersManager.currentUser.invitesSent++;

				// Tracking
				HDTrackingManager.Instance.Notify_ReferralSendInvite(m_inviteOrigin);
				break;


			case DynamicLinksWrapper.shortLinkResult.CANCELLED:
			case DynamicLinksWrapper.shortLinkResult.FAULTED:
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
	#endregion

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	#region callbacks
	/// <summary>
	/// A new deeplink notification was registered
	/// </summary>
	/// <param name="_dlinkParameters">Parameters of the deeplink.</param>
	public void OnDeepLinkNotification(Dictionary<string, string> _dlinkParameters) {
		// Store params until we can use them
		m_deepLinkReceived = true;
		m_deepLinkParams = _dlinkParameters;
	}
	#endregion
}