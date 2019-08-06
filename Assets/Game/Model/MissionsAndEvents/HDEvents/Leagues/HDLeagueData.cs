using UnityEngine;
using System;
using System.Collections.Generic;

public class HDLeagueData : IComparableWithOperators<HDLeagueData> {
	//---[Basic Data]-----------------------------------------------------------

    private readonly DefinitionNode m_def;
    private readonly string m_sku;
    private readonly string m_icon;
	private readonly string m_trophyPrefab;
    private readonly string m_tidName;
    private readonly string m_description;
    private readonly DragonTier m_minimumTier;

    private readonly int    m_order;



    //---[Extended Data]--------------------------------------------------------

	private List<HDLiveData.RankedReward> m_rewards;
    private HDLeagueLeaderboard m_leaderboard;

    public HDLiveData.State liveDataState { get; private set; }
    public HDLiveDataManager.ComunicationErrorCodes liveDataError { get; private set; }



    //---[Construction Methods]-------------------------------------------------

    public HDLeagueData(DefinitionNode _def) {
        m_def = _def;

        //Load basic data from definition
        m_sku = _def.sku;
        m_tidName = _def.Get("tidName");
        m_icon = _def.Get("icon");
		m_trophyPrefab = _def.Get("trophyPrefab");
        m_description = _def.Get("tidDesc");

        // Minimum tier definition. Get Tier enum from tier sku.
        DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.DRAGON_TIERS, 
                                                                                        "sku", _def.Get("minimumTier"));
        m_minimumTier = DragonTierGlobals.GetFromInt(def.GetAsInt("order"));


        m_order = _def.GetAsInt("order");

        m_leaderboard = new HDLeagueLeaderboard(m_sku);
    }

    public void Clean() {
		m_rewards = new List<HDLiveData.RankedReward>();
        m_leaderboard.Clean();

        liveDataState = HDLiveData.State.EMPTY;
        liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
    }

    public void WaitForData() {
        liveDataState = HDLiveData.State.WAITING_RESPONSE;
    }

    public void LoadData(SimpleJSON.JSONNode _data) {
        if (_data != null && m_sku.Equals(_data["sku"])) {
            m_rewards.Clear();
            if (_data.ContainsKey("rewards")) {
				SimpleJSON.JSONArray arr = _data["rewards"].AsArray;
				for(int i = 0; i < arr.Count; i++) {
					HDLiveData.RankedReward r = new HDLiveData.RankedReward();
					r.LoadData(arr[i], HDTrackingManager.EEconomyGroup.REWARD_LIVE_EVENT, m_tidName);
					m_rewards.Add(r);
				}

				// Since we can't assume rewards are received sorted, do it now
				m_rewards.Sort(HDLiveData.Reward.SortByTarget);   // Will be sorted by target percentage

				// Compute min rank based on previous reward
				for(int i = 1; i < m_rewards.Count; ++i) {  // Skip first reward (min is always 0)
					m_rewards[i].InitMinRankFromPreviousReward(m_rewards[i - 1]);
				}
			}

            liveDataState = HDLiveData.State.VALID;
            liveDataError = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;
        } else {
            liveDataState = HDLiveData.State.ERROR;
            liveDataError = HDLiveDataManager.ComunicationErrorCodes.OTHER_ERROR;
        }
    }


	//---[IComparable Implementation]-------------------------------------------
	/// <summary>
	/// Compare this instance with another one.
	/// </summary>
	/// <returns>The result of the comparison (-1, 0, 1).</returns>
	/// <param name="_other">Instance to be compared to.</param>
	protected override int CompareToImpl(HDLeagueData _other) {
		return this.m_order.CompareTo(_other.m_order);
	}

	/// <summary>
	/// Get the hash code corresponding to this object. Used in hashable classes such as Dictionary.
	/// </summary>
	/// <returns>The hash code corresponding to this object.</returns>
	protected override int GetHashCodeImpl() {
		return this.m_order.GetHashCode();
	}


    //---[Query Methods]--------------------------------------------------------

	public string tidName       { get { return m_tidName; } }
    public string sku           { get { return m_sku; } }
    public string icon          { get { return m_icon; } }
	public string trophyPrefab  { get { return m_trophyPrefab; } }
    public DragonTier minimumTier   { get { return m_minimumTier; } }

    public int    order         { get { return m_order; } }

    public HDLeagueLeaderboard leaderboard { get { return m_leaderboard; } }

    public Metagame.Reward GetReward(int _i) { return m_rewards[_i].reward; }
	public List<HDLiveData.RankedReward> rewards { get { return m_rewards; } }
    public Metagame.Reward GetRewardByRank(int _rank) {
        if (m_rewards.Count > 0) {
            for (int i = 0; i < m_rewards.Count; ++i) {
                if (_rank <= m_rewards[i].target) {
                    return m_rewards[i].reward;
                }
            }

            return m_rewards.Last().reward;
        }

        return null;
    }
}
