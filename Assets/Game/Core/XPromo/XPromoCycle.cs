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
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//


	// Set of local x-promo rewards as defined in content
	private List<XPromo.LocalReward> m_localRewards;
	public List<XPromo.LocalReward> localRewards { get { return m_localRewards; } }

	// Configuration from content
	private Boolean m_enabled;
	private DateTime m_startDate;
	private DateTime m_endDate;
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
	public int nextRewardIdx
	{
		get { return m_totalNextRewardIdx % m_cycleSize; }
       
	}

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

	// Cache
	private List<XPromo.LocalReward> m_cycleRewards; // List of the the final local rewards in this cycle (without priority duplicities)
    public List<LocalReward> cycleRewards
    {
        get
        {
			// If not cached yet
			if (m_cycleRewards == null)
            {
                // Obtain the rewards in the cycle and cache them
				m_cycleRewards = GetCycleRewards();
		    }
			return m_cycleRewards;
		}
    }

	private int[] m_daysWithRewards;

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
	/// <param name="_index">The index of the reward in the cycle</param>
	/// <returns>Returns the collected reward</returns>
	public LocalReward CollectReward(int _index)
	{

		// Find the proper next reward (in case the player already owns it, find a replacement)
		LocalReward reward = GetRewardByIndex(_index);

        // A HSE reward can always be collected.
        // We make sure the player doesnt lose the reward if he fails to open the HSE app.
		if (reward is LocalRewardHSE)
		{
			// Open the HSE link
			XPromoManager.instance.SendRewardToHSE((LocalRewardHSE)reward);

		}
		else if (reward is LocalRewardHD)
		{   
			// Make sure the reward is available
			if (!CanCollectNextReward())
				return null;

            Metagame.Reward rewardContent = ((LocalRewardHD)reward).reward;

			if (rewardContent is Metagame.RewardCurrency)
			{
                // Give coins/gems to the user
				UsersManager.currentUser.EarnCurrency(((Metagame.RewardCurrency)rewardContent).currency, (ulong)rewardContent.amount, false,
                                                        HDTrackingManager.EEconomyGroup.REWARD_XPROMO_LOCAL);

			}
			else  // Dragons, pets and eggs
			{
				// Add this reward to the queue
				UsersManager.currentUser.PushReward(rewardContent);
			}

			XPromoManager.Log("Reward " + rewardContent.ToString() + " collected ");

		}


		// If we are collecting this reward for first time
		if (_index == nextRewardIdx)
		{
			// Unlock the next one
			UnlockNextReward();
		}

		// Add tracking

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
		LocalReward nextReward = m_cycleRewards[nextRewardIdx];
		LocalReward currentReward = m_cycleRewards[(m_totalNextRewardIdx - 1) % m_cycleSize];
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

        // Has the xpromo cycle been completed?
        if (m_totalNextRewardIdx >= m_cycleSize)
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
		m_daysWithRewards = m_localRewards.Select(r => r.day).Distinct().ToArray<int>();
        if (m_daysWithRewards.Length != m_cycleSize)
		{
			Debug.LogError("The number of xPromo active rewards doesn´t match the xPromo cycle length! Please, fix the content tables.");
		}

		// Cache the rewards in a cycle
		m_cycleRewards = GetCycleRewards();



	}

	/// <summary>
	/// Gets the reward in the content in a specific position in the cycle (it could not match the day)
	/// Asumes that the local rewards list is already sorted by day and priority.
	/// </summary>
	/// <param name="_index">The position index in the xpromo cycle</param>
	/// <returns></returns>
	private XPromo.LocalReward GetRewardByIndex(int _index)
	{
		// Trim the index
		_index = _index % m_cycleSize;

		// Get all rewards for that day (could be more than one) 
		List<XPromo.LocalReward> rewards = m_localRewards.Where(r => r.day == m_daysWithRewards[_index]).ToList();

		// Just in case
		if (rewards.Count == 0)
			return null;

		LocalReward candidate = null;
        
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
				candidate = reward;

                // The player doesnt have this item yet?
				if (! ((XPromo.LocalRewardHD)reward).reward.IsAlreadyOwned())
				{
					// We have a winner!
					return candidate;
				}
			}
		}

        if (candidate == null) {
			// No reward was found :( this shouldnt happen
			XPromoManager.Log("No reward found for day " + _index + 1);
		}
        
		return candidate;

	}


    /// <summary>
    /// Return a list of all the rewards in a cycle.
    /// Will avoid using rewards that are already owned by the player.
    /// </summary>
    /// <returns></returns>
    public List<LocalReward> GetCycleRewards()
    {
        List<LocalReward> cycleRewards = new List<XPromo.LocalReward>();

        for (int i = 0; i < cycleSize ; i++)
        {
			cycleRewards.Add( GetRewardByIndex(i) );
        }
        
		return cycleRewards;
    }


    /// <summary>
    /// Get the index of a reward in the current xpromo cycle. This index could not match the day.
    /// </summary>
    /// <param name="_reward">The local reward</param>
    /// <returns>Index of the reward starting from 0 (first reward). Returns -1 in case this reward doesnt belong to the current xpromo cycle.</returns>
    public int GetRewardIndex (LocalReward _reward)
    {
		return cycleRewards.FindIndex( (LocalReward e) => e.sku == _reward.sku);
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
			XPromoManager.Log("LoadData xPromoNextRewardTimestamp = " + m_nextRewardTimestamp);
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
		XPromoManager.Log("SaveData xPromoNextRewardTimestamp = " + m_nextRewardTimestamp);

		// Done!
		return data;
	}



}