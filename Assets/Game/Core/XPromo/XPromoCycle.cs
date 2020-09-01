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

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Represents the progression of xpromotion local rewards (they could be destinated to HD or HSE)
/// </summary>
[Serializable]
public class XPromoCycle {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//


	// Set of local x-promo rewards as defined in content
	private List<XPromo.LocalReward> m_localRewards;


	// Configuration from content
	private Boolean m_enabled;
	private DateTime m_startDate;
	private DateTime m_endDate;
	private int m_minRuns;
	private int m_cycleSize;

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
	public int nextRewardIdx
	{
		get { return m_totalNextRewardIdx % m_cycleSize; }
	}

	// Time when the player can collect the next reward
	private DateTime m_nextRewardTimestamp;
	public DateTime nextRewardTimestamp
	{
		get { return m_nextRewardTimestamp; }
	}


	// Index of the last collected reward
	private int m_lastCollectedRewardIdx;
	public int lastCollectedRewardIdx
	{
		get { return m_lastCollectedRewardIdx; }
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

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public XPromoCycle() {
		Clear();
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

	}


	/// <summary>
	/// Reset the current progression in the xpromo cycle.
	/// </summary>
	public void ResetProgression()
	{
		// Go back to the first reward
		m_totalNextRewardIdx = 0;

		// Reset the timestamp, so the first reward can be collected.
		m_nextRewardTimestamp = DateTime.MinValue;
	}


	/// <summary>
	/// Obtain the next reward to be collected.
	/// </summary>
	/// <returns>The next reward to be collected. Shouldn't be null.</returns>
	public XPromo.LocalReward GetNextReward()
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
	public void CollectReward()
	{
		// Make sure the reward is available
		if (!CanCollectNextReward())
			return;

		// Find the proper next reward (in case the player already owns it, find a replacement)
		XPromo.LocalReward reward = GetRewardByIndex(nextRewardIdx);

		if (reward is XPromo.LocalRewardHSE)
		{
			// Open the HSE link
			XPromoManager.instance.SendRewardToHSE((XPromo.LocalRewardHSE)reward);

		}
		else if (reward is XPromo.LocalRewardHD)
		{
			Metagame.Reward rewardContent = ((XPromo.LocalRewardHD)reward).reward;

			if (rewardContent is Metagame.RewardCurrency)
			{
				// Show trail of coins/gems
			}
			else if (rewardContent is Metagame.RewardDragon)
			{
				// Show new dragon in selection screen
			}
			else
			{
				// Show new item in reward screen
			}
		}

		// Move index to next item
		m_nextRewardIdx++;

		// Calculate next item timestamp
		// Reset timestamp to 00:00 of local time (but using server timezone!)
		DateTime serverTime = GameServerManager.GetEstimatedServerTime();
		TimeSpan toMidnight = DateTime.Today.AddDays(1) - DateTime.Now; // Local
		m_nextRewardTimestamp = serverTime + toMidnight;   // Local 00:00 in server timezone

		Debug.Log("CollectNextReward nextTimestamp = " + m_nextRewardTimestamp);

		// Add tracking

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

		// All checks passed. The xPromo feature is active
		return true;

	}



	/// <summary>
	/// Load all data from content tables
	/// </summary>
	public void InitFromDefinitions()
	{

		// Load settings
		DefinitionNode settingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.XPROMO_SETTINGS, "xPromoSettings");
		m_enabled = settingsDef.GetAsBool("enabled");
		m_minRuns = settingsDef.GetAsInt("minRuns");
		m_cycleSize = settingsDef.GetAsInt("cycleSize");

		if (settingsDef.Has("startDate"))
		{
			m_startDate = TimeUtils.TimestampToDate(settingsDef.GetAsLong("startDate", 0), false);
		}

		if (settingsDef.Has("endDate"))
		{
			m_endDate = TimeUtils.TimestampToDate(settingsDef.GetAsLong("endDate", 0), false);
		}


		// Load local rewards
		List<DefinitionNode> localRwdDefinitions = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.LOCAL_REWARDS);

		// Sort the rewards by day, then by priority (should be already sorted in the content, but who knows)
		localRwdDefinitions.Sort(XPromo.LocalReward.CompareDefsByPriority);
		localRwdDefinitions.Sort(XPromo.LocalReward.CompareDefsByDay);

		foreach (DefinitionNode def in localRwdDefinitions)
		{
            // Discard disabled definitions
			if (def.GetAsBool("enabled") == false)
				continue;

			XPromo.LocalReward localReward = XPromo.LocalReward.CreateLocalRewardFromDef(def);

			if (localReward != null)
			{
				// Keep a list with all the local rewards
				m_localRewards.Add(localReward);

			}
		}

		// Check settings-rewards consistency, so the designers can know if they screwed up the content tables 
		List<XPromo.LocalReward> activeRewards = m_localRewards.Where<XPromo.LocalReward>(r => r.enabled == true).ToList();
		if (activeRewards.Count != m_cycleSize)
		{
			Debug.LogError("The number of xPromo active rewards doesn´t match the xPromo cycle length! Please, fix the content tables.");
		}


	}

	/// <summary>
	/// Gets the reward in the content in a specific position in the cycle (it could not match the day)
	/// Asumes that the local rewards is already sorted by day and priority.
	/// </summary>
	/// <param name="_index">The position index in the xpromo cycle</param>
	/// <returns></returns>
	private XPromo.LocalReward GetRewardByIndex(int _index)
	{
		// Trim the index
		_index = _index % m_cycleSize;

		// Get all rewards for that day (could be more than one) 
		List<XPromo.LocalReward> rewards = m_localRewards.Where(r => r.day == _index + 1).ToList();

		// Just in case
		if (rewards.Count == 0)
			return null;

		foreach (XPromo.LocalReward reward in rewards)
		{
			// In case of an HSE reward, ignore the priorities.
			if (reward is XPromo.LocalRewardHSE)
			{
				return reward;
			}

			// In case of HD, find the first reward not owned by the player
			else if (reward is XPromo.LocalRewardHD)
			{
				if (((XPromo.LocalRewardHD)reward).reward.IsAlreadyOwned())
				{
					// The player already have this item, try the next one in priority
					continue;

				}
				else
				{
					// We have a winner!
					return reward;
				}
			}
		}

		// No reward was found :( this shouldnt happen
		Debug.Log("No reward found for day " + _index + 1);
		return null;

	}

    /// <summary>
    /// Return a list of all the rewards in the cycle, in case that there are two rewards
    /// in the same day, use the lower priority.
    /// Asumes that the local rewards is already sorted by day and priority.
    /// </summary>
    /// <returns></returns>
    public List<XPromo.LocalReward> GetCycleRewards()
    {
        List<XPromo.LocalReward> cycleRewards = new List<XPromo.LocalReward>();
        
		int dayPointer = -1;
               
        foreach (XPromo.LocalReward reward in m_localRewards)
        {
            if (reward.day == dayPointer)
            {
				// We already have this day, skip it
				continue;
            }

			cycleRewards.Add(reward);
			dayPointer = reward.day;
        }

		return cycleRewards;
    }

	//------------------------------------------------------------------------//
	// PERSISTENCE METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Constructor from json data.
	/// </summary>
	/// <param name="_data">Data to be parsed.</param>
	public void LoadData(SimpleJSON.JSONNode _data)
	{
		// Reset any existing data
		Clear();

		// Current reward index
		if (_data.ContainsKey("xPromoNextRewardIdx"))
		{
			m_totalNextRewardIdx = PersistenceUtils.SafeParse<int>(_data["xPromoNextRewardIdx"]);
		}

		// Collect timestamp
		if (_data.ContainsKey("xPromoNextRewardTimestamp"))
		{
			m_nextRewardTimestamp = PersistenceUtils.SafeParse<DateTime>(_data["xPromoNextRewardTimestamp"]);
			Debug.Log("LoadData xPromoNextRewardTimestamp = " + m_nextRewardTimestamp);
		}

	}

	/// <summary>
	/// Serialize into json.
	/// </summary>
	/// <returns>The json. Can be null if sequence has never been generated.</returns>
	public SimpleJSON.JSONClass SaveData()
	{

		// Create a new json data object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Current reward index
		data.Add("xPromoNextRewardIdx", PersistenceUtils.SafeToString(m_totalNextRewardIdx));

		// Collect timestamp
		data.Add("xPromoNextRewardTimestamp", PersistenceUtils.SafeToString(m_nextRewardTimestamp));
		Debug.Log("SaveData xPromoNextRewardTimestamp = " + m_nextRewardTimestamp);

		// Done!
		return data;
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}