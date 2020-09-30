// XPromoCycle.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 01/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using XPromo;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Represents the progression of xpromotion local rewards (they could be destinated to HD or HSE)
/// </summary>
[Serializable]
public class XPromoCycle {



	//------------------------------------------------------------------------//
	// ENUM															           //
	//------------------------------------------------------------------------//

    public enum ABGroup // For AB test purposes
	{
		UNDEFINED, A, B
	}

	public static string ABGroupToString(ABGroup _group) {
		switch(_group) {
			case ABGroup.UNDEFINED: return string.Empty;
			default: return _group.ToString();
		}
	}

	public static ABGroup StringToABGroup(string _str) {
		if(_str.ToLowerInvariant() == "a") {
			return ABGroup.A;
		} else if(_str.ToLowerInvariant() == "b") {
			return ABGroup.B;
		}
		return ABGroup.UNDEFINED;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//


	// Set of local x-promo rewards as defined in content
	private List<LocalReward> m_localRewards;
	public List<LocalReward> localRewards { get { return m_localRewards; } }

	// Configuration from content
	private Boolean m_enabled;
    private ABGroup m_abGroup;
    public ABGroup abGroup { get { return m_abGroup;  } }
	private DateTime m_startDate;
    private DateTime m_recruitmentLimitDate;
	private DateTime m_endDate;
	public DateTime endDate { get { return m_endDate; } }
	private int m_minRuns;
    private int m_cycleSize;
    public int cycleSize { get { return m_cycleSize;  } }

	// Rewards status:

	// Index of the next reward (counting all rewards collected)
	private int m_totalNextRewardIdx;
	public int totalNextRewardIdx
	{
		get { return m_totalNextRewardIdx; }
        set { m_totalNextRewardIdx = value; }
	}

	// Index of the next reward (in the current xpromo cycle)
	private int m_nextRewardIdx;
	public int nextRewardIdx { get { return m_totalNextRewardIdx % m_cycleSize; } }

	// Time when the player can collect the next reward
	private DateTime m_nextRewardTimestamp;
	public DateTime nextRewardTimestamp
	{
		get { return m_nextRewardTimestamp; }
		set { m_nextRewardTimestamp = value; }
	}


    // Remaining time to unlock the next reward
	public TimeSpan timeToCollection
	{
		get
		{
			// Avoid negative values
			TimeSpan remainingTime = m_nextRewardTimestamp - GameServerManager.GetEstimatedServerTime();
			if (remainingTime.TotalSeconds < 0)
			{
				return new TimeSpan(0);
			}
			else
			{
				return remainingTime;
			}
		}
	}

	// Tracking
	private string m_experimentName = "";
	public string experimentName { get { return m_experimentName; } }
	private bool m_experimentChecked = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public XPromoCycle() {

	}


	/// <summary>
	/// Custom factory method
	/// </summary>
	public static XPromoCycle CreateXPromoCycleFromDefinitions()
	{
		XPromoCycle cycle = new XPromoCycle();

		// Initialize from content
		cycle.InitFromDefinitions();

		return cycle;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Empty all the sets and restart variables
	/// </summary>
	public void Clear()
	{
		m_localRewards = new List<XPromo.LocalReward>();

		m_startDate = DateTime.MinValue;
		m_endDate = DateTime.MinValue;

		m_experimentChecked = false;
}


	/// <summary>
	/// Obtain the next reward to be collected.
	/// </summary>
	/// <returns>The next reward to be collected. Shouldn't be null.</returns>
	public LocalReward GetNextReward()
	{
		return m_localRewards[m_nextRewardIdx];    // Should always be valid
	}



	/// <summary>
	/// Wheter the user can collect the next reward, or is not yet ready
	/// </summary>
	/// <returns>Yes if we reached the unlock timestamp</returns>
	public bool CanCollectNextReward()
	{
		return GameServerManager.GetEstimatedServerTime() >= m_nextRewardTimestamp;

	}


	/// <summary>
	///  Collects the next reward (if possible) and advance the progression
	/// </summary>
	/// <param name="_index">The index of the reward in the cycle</param>
	/// <returns>Returns the collected reward</returns>
	public LocalReward CollectReward(int _index)
	{

		// Find the proper next reward (in case the player already owns it, find a replacement)
		LocalReward reward = m_localRewards [_index];

		// Prepare tracking vars
		HDTrackingManager.XPromoRewardTrackingData rewardTrackingData = new HDTrackingManager.XPromoRewardTrackingData();
		rewardTrackingData.sourceReward = reward;
		rewardTrackingData.sourceCycle = this;
		rewardTrackingData.isAltReward = false;

		// Reward for HSE?
		if (reward is LocalRewardHSE)
		{
			// An HSE reward can always be collected.
			// We make sure the player doesnt lose the reward if he fails to open the HSE app.
			LocalRewardHSE hseReward = (LocalRewardHSE)reward;

			// Open the HSE link
			XPromoManager.instance.SendRewardToHSE(hseReward);

			// Tracking data
			rewardTrackingData.sku = hseReward.rewardSku;
			rewardTrackingData.amount = hseReward.amount;
		}

		// Reward for HD?
		else if (reward is LocalRewardHD)
		{   
			// Make sure the reward is available
			if (!CanCollectNextReward())
				return null;

			// Aux vars
			Metagame.Reward finalRewardContent = ((LocalRewardHD)reward).reward;
			Metagame.Reward altRewardContent = null;

			// Is it a currency reward?
			if (finalRewardContent is Metagame.RewardCurrency)
			{
                // Give coins/gems to the user
				UsersManager.currentUser.EarnCurrency(((Metagame.RewardCurrency)finalRewardContent).currency, (ulong)finalRewardContent.amount, false,
                                                        HDTrackingManager.EEconomyGroup.REWARD_XPROMO_LOCAL);

				// Tracking data
				rewardTrackingData.InitWithReward(finalRewardContent, false, reward, this);
			}
			else  // Dragons, pets and eggs
			{
				// Does the player already have this item?
				if (!finalRewardContent.IsAlreadyOwned())
				{
					// Add the reward to the queue
					UsersManager.currentUser.PushReward(finalRewardContent);

					// Tracking data
					rewardTrackingData.InitWithReward(finalRewardContent, false, reward, this);
				}
                else
                {
					// So the player already owns the reward. We are going to give him an alternative reward
					// in form of currencies as defined in the content
					altRewardContent = XPromoManager.CreateAltReward(reward);

					// Tracking data
					rewardTrackingData.InitWithReward(altRewardContent, true, reward, this);

					// Add the reward to the queue
					UsersManager.currentUser.PushReward(altRewardContent);
				}
			}

			XPromoManager.Log("Reward " + finalRewardContent.ToString() + " collected ");

		}

		// Tracking!
		HDTrackingManager.Instance.Notify_XPromoRewardCollected(rewardTrackingData);

		// If we are collecting this reward for first time
		if (_index == nextRewardIdx)
		{
			// Unlock the next one
			UnlockNextReward();
		}

		return reward;
	}


    /// <summary>
    /// Start the countdown of the next reward in the cycle and advance the next reward pointer.
    /// </summary>
    private void UnlockNextReward ()
    {
		// Advance the index
		m_totalNextRewardIdx++;

		// Calculate the days difference between the current and next reward
		LocalReward nextReward = m_localRewards[nextRewardIdx];
		LocalReward currentReward = m_localRewards[(m_totalNextRewardIdx - 1) % m_cycleSize];
		int daysDiff = nextReward.day - currentReward.day;

		// Reset timestamp to 00:00 of local time (but using server timezone!)
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();
		TimeSpan toMidnight = DateTime.Today.AddDays(daysDiff) - DateTime.Now; // Local
		m_nextRewardTimestamp = serverTime + toMidnight;   // Local 00:00 in server timezone

		XPromoManager.Log("nextRewardTimestamp = " + m_nextRewardTimestamp);
	}


	/// <summary>
	/// Determine if the xPromo should be active at this moment
	/// </summary>
	/// <returns></returns>
	public bool IsActive()
	{
		if (m_enabled == false)
			return false;

		// The player needs to play more runs to see xPromo
		if (UsersManager.currentUser.gamesPlayed < m_minRuns)
			return false;

		// Has the xpromo already started? or is it expired?
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();
		if (serverTime < m_startDate || serverTime > m_endDate)
			return false;

        // The user is not recruited yet, and the recruitment period has ended
        if ( ! IsRecruited()  && serverTime > m_recruitmentLimitDate)
            return false;

		// Are there local rewards defined in the content?
		if (m_localRewards.Count == 0)
			return false;

        // Has the xpromo cycle been completed?
        if (m_totalNextRewardIdx >= m_cycleSize)
			return false;

		// All checks passed. The xPromo feature is active!

		// [AOC] Update tracking vars if needed
		if(!m_experimentChecked) {
			TrackingPersistenceSystem trackingPersistence = HDTrackingManager.Instance.TrackingPersistenceSystem;
			if(trackingPersistence != null) {
				// Are we starting a new experiment?
				string lastExperimentName = trackingPersistence.XPromoExperimentName;
				if(m_experimentName != lastExperimentName) {
					// Yes!! Store activation date
					trackingPersistence.XPromoActivationDate = serverTime;
				}

				// Mark as checked, we don't need to do it again
				m_experimentChecked = true;
			}
		}

		// Done!
		return true;
	}


    /// <summary>
    /// Returns true if the player already started the cycle (collected the first reward)
    /// </summary>
    /// <returns></returns>
    public bool IsRecruited(){
        return m_totalNextRewardIdx > 0;
    }


	/// <summary>
	/// Load all data from content tables
	/// </summary>
	public void InitFromDefinitions()
	{

		Clear();

		// Load settings
		DefinitionNode settingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.XPROMO_SETTINGS, "xPromoSettings");
		m_enabled = settingsDef.GetAsBool("enabled");
		m_minRuns = settingsDef.GetAsInt("minRuns");
		m_cycleSize = settingsDef.GetAsInt("cycleSize");

        
		if (settingsDef.Has("ABGroup"))
		{
            // By default everyone is in the group UNDEFINED
            string param = settingsDef.GetAsString("ABGroup", "");
			m_abGroup = StringToABGroup(param);
		}

		if (settingsDef.Has("startDate"))
		{
			m_startDate = TimeUtils.TimestampToDate(settingsDef.GetAsLong("startDate"), false);
		}

        if (settingsDef.Has("recruitingLimitDate"))
		{
			m_recruitmentLimitDate = TimeUtils.TimestampToDate(settingsDef.GetAsLong("recruitingLimitDate"), false);
		}

		if (settingsDef.Has("endDate"))
		{
			m_endDate = TimeUtils.TimestampToDate(settingsDef.GetAsLong("endDate"), false);
		}


		// Load local rewards (origin game = HD)
		List<DefinitionNode> localRwdDefinitions = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.XPROMO_REWARDS, "origin", XPromoManager.GAME_CODE_HD);

		// Sort the rewards by day (should be already sorted in the content, but who knows)
		localRwdDefinitions.Sort(LocalReward.CompareDefsByDay);

		int lastDay = -1;
		foreach (DefinitionNode def in localRwdDefinitions)
		{
            // Discard disabled definitions
			if (def.GetAsBool("enabled") == false)
				continue;

            // Discard rewards from other AB groups
            if (def.GetAsString("ABGroup") != "")
            {
                if(def.GetAsString("ABGroup").ToLower() != m_abGroup.ToString().ToLower())
					continue;
            }

			LocalReward localReward = LocalReward.CreateLocalRewardFromDef(def);

			if (localReward != null)
			{
				// Keep a list with all the local rewards
				m_localRewards.Add(localReward);
			}

            // Keep a track of the last "day" param to avoid more than one reward with the same day value in the content.
            if (lastDay != -1 && localReward.day == lastDay)
            {
				Debug.LogError("There can be only one reward per day! Please, fix the content.");
            }
			lastDay = localReward.day;

		}

		// Check settings-rewards consistency, so the designers can know if they screwed up the content tables 
        if (m_localRewards.Count != m_cycleSize)
		{
			Debug.LogError("The number of xPromo active rewards doesn´t match the xPromo cycle length! Please, fix the content tables.");
		}

		// Tracking vars
		m_experimentName = settingsDef.GetAsString("experiment");
		m_experimentChecked = false;
	}


    /// <summary>
    /// Get the index of a reward in the current xpromo cycle. This index could not match the day.
    /// </summary>
    /// <param name="_reward">The local reward</param>
    /// <returns>Index of the reward starting from 0 (first reward). Returns -1 in case this reward doesnt belong to the current xpromo cycle.</returns>
    public int GetRewardIndex (LocalReward _reward)
    {
		return m_localRewards.FindIndex( (LocalReward e) => e.sku == _reward.sku);
    }


	/// <summary>
	/// Find the state of a reward reward depending on its availability (ready to collect, already collected, still in countdown...)
	/// </summary>
	/// <returns></returns>
	public LocalReward.State GetRewardState(LocalReward _reward)
	{

        // Find the actual index of this reward in the cycle
		int index = GetRewardIndex(_reward);

		return GetRewardState(index);

	}

	/// <summary>
	/// Find the state of a reward reward depending on its availability (ready to collect, already collected, still in countdown...)
	/// </summary>
    /// <param name="_index">The index in the cycle</param>
	/// <returns></returns>
	public LocalReward.State GetRewardState(int _index)
	{


		if (_index < XPromoManager.instance.xPromoCycle.m_totalNextRewardIdx)
		{
			// This reward has been already claimed 
			return LocalReward.State.COLLECTED;
		}


		else if (_index == XPromoManager.instance.xPromoCycle.m_totalNextRewardIdx)
		{
			// The reward is the next in the cycle
			if (timeToCollection.TotalMilliseconds <= 0)
			{
				// The reward is ready to collect
				return LocalReward.State.READY;
			}
			else
			{
				// The timer countdown is not zero yet
				return LocalReward.State.COUNTDOWN;
			}

		}

		// The last case is implicit
		// else if (_index > XPromoManager.instance.xPromoCycle.m_totalNextRewardIdx + 1)
		{
			// The reward isnt unlocked yet
			return LocalReward.State.LOCKED;
		}

	}



	//------------------------------------------------------------------------//
	// CALLBACK METHODS		    											  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// The player clicked on collect reward / open HSE
	/// </summary>
	/// <returns>Returns the reward collected</returns>
	public LocalReward OnCollectReward(int _index)
	{

		return CollectReward(_index);
		
	}


	//------------------------------------------------------------------------//
	// DEBUG        		    											  //
	//------------------------------------------------------------------------//
    /// <summary>
    /// Will check all the conditions for this feature to be activated and
    /// show the output in the console for debug purposes
    /// </summary>
    public void CheckActiveConditions()
    {

		if (m_enabled == false)
        {
			XPromoManager.Log("Not active: The feature is not enabled in the settings");
			return;
        }


		// The player needs to play more runs to see xPromo
		if (UsersManager.currentUser.gamesPlayed < m_minRuns)
		{
			XPromoManager.Log("Not active: The minimum amount of runs ("+ m_minRuns + ") hasnt been reached");
			return;
		}

		// Has the xpromo already started? or is it expired?
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();
		if (serverTime < m_startDate || serverTime > m_endDate)
		{
			XPromoManager.Log("Not active: The xPromo feature has expired / not started yet");
			return;
		}

		// The user is not recruited yet, and the recruitment period has ended
		if (!IsRecruited() && serverTime > m_recruitmentLimitDate)
		{
			XPromoManager.Log("Not active: The recruiment date has finished");
			return;
		}

		// Are there local rewards defined in the content?
		if (m_localRewards.Count == 0)
		{
			XPromoManager.Log("Not active: There are no xpromo rewards defined");
			return;
		}

		// Has the xpromo cycle been completed?
		if (m_totalNextRewardIdx >= m_cycleSize)
		{
			XPromoManager.Log("Not active: The xPromo cycle has been already completed");
			return;
		}

        // Default case. Everything ok
		XPromoManager.Log("Active: all conditions met");

	}

}