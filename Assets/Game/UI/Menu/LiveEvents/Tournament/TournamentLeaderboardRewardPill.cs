// GlobalEventsLeaderboardPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Data class.
/// </summary>
public class TournamentLeaderboardRewardPillData : TournamentLeaderboardPillBaseData {
	public HDLiveData.RankedReward reward = null;
}

/// <summary>
/// Item class.
/// </summary>
public class TournamentLeaderboardRewardPill : TournamentLeaderboardPillBase {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private RankedRewardView m_rewardView = null;

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with the given user data.
	/// </summary>
	/// <param name="_data">The user to be displayed in the pill.</param>
	public override void InitWithData(TournamentLeaderboardPillBaseData _data) {
		// Cast data
		TournamentLeaderboardRewardPillData data = _data as TournamentLeaderboardRewardPillData;
		Debug.Assert(data != null, Color.red.Tag("UNKNOWN PILL DATA FORMAT!"));

		// Initialize reward view
		m_rewardView.InitFromReward(data.reward);
	}

	public override void Animate(int _index) { }

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}