// CPGachaTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global class to control gacha testing features.
/// </summary>
public class CPGachaTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// REWARD CHANCE														  //
	//------------------------------------------------------------------------//
	public enum RewardChanceMode {
		DEFAULT = 0,
		SAME_PROBABILITY,
		COMMON_ONLY,
		RARE_ONLY,
		EPIC_ONLY
	};

	public enum DuplicateMode {
		DEFAULT = 0,
		ALWAYS,
		NEVER,
		RANDOM
	};

	public const string REWARD_CHANCE_MODE = "GACHA_REWARD_CHANCE_MODE";
	public static RewardChanceMode rewardChanceMode {
		get { return (RewardChanceMode)Prefs.GetIntPlayer(REWARD_CHANCE_MODE, (int)RewardChanceMode.DEFAULT); }
		set { Prefs.SetIntPlayer(REWARD_CHANCE_MODE, (int)value); }
	}

	public const string DUPLICATE_MODE = "GACHA_DUPLICATE_MODE";
	public static DuplicateMode duplicateMode {
		get { return (DuplicateMode)Prefs.GetIntPlayer(DUPLICATE_MODE, (int)DuplicateMode.DEFAULT); }
		set { Prefs.SetIntPlayer(DUPLICATE_MODE, (int)value); }
	}

	//------------------------------------------------------------------------//
	// EXPOSED MEMBERS														  //
	//------------------------------------------------------------------------//
	// Reward Chance
	[Space]
	[SerializeField] private CPEnumPref m_rewardChanceDropdown = null;
	[SerializeField] private CPEnumPref m_duplicateDropdown = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to changed events
		m_rewardChanceDropdown.InitFromEnum(REWARD_CHANCE_MODE, typeof(RewardChanceMode), 0);
		m_duplicateDropdown.InitFromEnum(DUPLICATE_MODE, typeof(DuplicateMode), 0);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure all values ar updated
		Refresh();
	}

	/// <summary>
	/// Make sure all fields have the right values.
	/// </summary>
	private void Refresh() {
		m_rewardChanceDropdown.Refresh();
		m_duplicateDropdown.Refresh();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}