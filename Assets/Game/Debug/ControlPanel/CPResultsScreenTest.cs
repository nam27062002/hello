// CPResultsScreenTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
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
/// Global class to control results screen testing features.
/// </summary>
public class CPResultsScreenTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// GLOBAL																  //
	//------------------------------------------------------------------------//
	public const string TEST_ENABLED = "RESULTS_TEST_ENABLED";
	public static bool testEnabled {
		get { return Prefs.GetBoolPlayer(TEST_ENABLED, false); }
		set { Prefs.SetBoolPlayer(TEST_ENABLED, value); }
	}

	//------------------------------------------------------------------------//
	// CHESTS																  //
	//------------------------------------------------------------------------//
	public enum ChestTestMode {
		NONE = 0,
		FIXED_0,
		FIXED_1,
		FIXED_2,
		FIXED_3,
		FIXED_4,
		FIXED_5,
		RANDOM
	};

	public const string CHESTS_MODE = "RESULTS_CHESTS_MODE";
	public static ChestTestMode chestsMode {
		get {
			if(!testEnabled) return ChestTestMode.NONE;
			return (ChestTestMode)Prefs.GetIntPlayer(CHESTS_MODE, (int)ChestTestMode.NONE); 
		}
		set { Prefs.SetIntPlayer(CHESTS_MODE, (int)value); }
	}

	public enum ChestsLayout {
		ONLY_COLLECTED_CHESTS,
		FULL_PROGRESSION
	};

	public const string CHESTS_LAYOUT = "RESULTS_CHESTS_LAYOUT";
	public static ChestsLayout chestsLayout {
		get {
			return (ChestsLayout)Prefs.GetIntPlayer(CHESTS_LAYOUT, (int)ChestsLayout.FULL_PROGRESSION); 
		}
		set { Prefs.SetIntPlayer(CHESTS_LAYOUT, (int)value); }
	}

	//------------------------------------------------------------------------//
	// EGGS																	  //
	//------------------------------------------------------------------------//
	public enum EggTestMode {
		NONE,
		RANDOM,
		FOUND,
		NOT_FOUND
	};

	public const string EGG_MODE = "RESULTS_EGG_MODE";
	public static EggTestMode eggMode {
		get {
			if(!testEnabled) return EggTestMode.NONE;
			return (EggTestMode)Prefs.GetIntPlayer(EGG_MODE, (int)EggTestMode.NONE); 
		}
		set { Prefs.SetIntPlayer(EGG_MODE, (int)value); }
	}

	//------------------------------------------------------------------------//
	// MISSIONS																  //
	//------------------------------------------------------------------------//
	public enum MissionsTestMode {
		NONE,
		FIXED_0,
		FIXED_1,
		FIXED_2,
		FIXED_3
	};

	public const string MISSIONS_MODE = "RESULTS_MISSIONS_MODE";
	public static MissionsTestMode missionsMode {
		get {
			if(!testEnabled) return MissionsTestMode.NONE;
			return (MissionsTestMode)Prefs.GetIntPlayer(MISSIONS_MODE, (int)MissionsTestMode.NONE);
		}
		set { Prefs.SetIntPlayer(MISSIONS_MODE, (int)value); }
	}

	//------------------------------------------------------------------------//
	// SCORE																  //
	//------------------------------------------------------------------------//
	public const string SCORE_VALUE = "RESULTS_SCORE_VALUE";
	public static long scoreValue {
		get { return (long)Prefs.GetFloatPlayer(SCORE_VALUE, 422801f); }
		set { Prefs.SetFloatPlayer(SCORE_VALUE, value); }
	}

	public const string HIGH_SCORE_VALUE = "RESULTS_HIGH_SCORE_VALUE";
	public static long highScoreValue {
		get { return (long)Prefs.GetFloatPlayer(HIGH_SCORE_VALUE, 12664915f); }
		set { Prefs.SetFloatPlayer(HIGH_SCORE_VALUE, value); }
	}

	public const string NEW_HIGH_SCORE = "RESULTS_NEW_HIGH_SCORE";
	public static bool newHighScore {
		get { return Prefs.GetBoolPlayer(NEW_HIGH_SCORE, false); }
		set { Prefs.SetBoolPlayer(NEW_HIGH_SCORE, value); }
	}

	//------------------------------------------------------------------------//
	// REWARDS																  //
	//------------------------------------------------------------------------//
	public const string COINS_VALUE = "RESULTS_COINS_VALUE";
	public static long coinsValue {
		get { return (long)Prefs.GetFloatPlayer(COINS_VALUE, 350288f); }
		set { Prefs.SetFloatPlayer(COINS_VALUE, value); }
	}

	public const string TIME_VALUE = "RESULTS_TIME_VALUE";
	public static float timeValue {	// Total Seconds
		get { return Prefs.GetFloatPlayer(TIME_VALUE, 600f); }
		set { Prefs.SetFloatPlayer(TIME_VALUE, value); }
	}

	public const string SURVIVAL_BONUS = "RESULTS_SURVIVAL_BONUS";
	public static long survivalBonus {
		get { return (long)Prefs.GetFloatPlayer(SURVIVAL_BONUS, 10000f); }
		set { Prefs.SetFloatPlayer(SURVIVAL_BONUS, value); }
	}

	//------------------------------------------------------------------------//
	// PROGRESSION															  //
	//------------------------------------------------------------------------//
	public const string XP_INITIAL_DELTA = "RESULTS_XP_INITIAL_DELTA";
	public static float xpInitialDelta {
		get { return Prefs.GetFloatPlayer(XP_INITIAL_DELTA, 0f); }
		set { Prefs.SetFloatPlayer(XP_INITIAL_DELTA, value); }
	}

	public const string XP_FINAL_DELTA = "RESULTS_XP_FINAL_DELTA";
	public static float xpFinalDelta {
		get { return Prefs.GetFloatPlayer(XP_FINAL_DELTA, 1f); }
		set { Prefs.SetFloatPlayer(XP_FINAL_DELTA, value); }
	}

	public const string NEXT_DRAGON_LOCKED = "RESULTS_NEXT_DRAGON_LOCKED";
	public static bool nextDragonLocked {
		get { return Prefs.GetBoolPlayer(NEXT_DRAGON_LOCKED, true); }
		set { Prefs.SetBoolPlayer(NEXT_DRAGON_LOCKED, value); }
	}

	//------------------------------------------------------------------------//
	// EXPOSED MEMBERS														  //
	//------------------------------------------------------------------------//
	// Global
	[Space]
	[SerializeField] private Toggle m_testEnabledToggle = null;
	[SerializeField] private CanvasGroup m_canvasGroup = null;

	// Chests/Egg/Missions
	[Space]
	[SerializeField] private CPEnumPref m_chestsModeDropdown = null;
	[SerializeField] private CPEnumPref m_eggModeDropdown = null;
	[SerializeField] private CPEnumPref m_missionsModeDropdown = null;

	// Visuals
	[Space]
	[SerializeField] private CPEnumPref m_chestsLayoutDropdown = null;

	// Score
	[Space]
	[SerializeField] private TMP_InputField m_scoreValueInput = null;
	[SerializeField] private TMP_InputField m_highScoreValueInput = null;
	[SerializeField] private Toggle m_newHighScoreToggle = null;

	// Rewards
	[Space]
	[SerializeField] private TMP_InputField m_coinsValueInput = null;
	[SerializeField] private TMP_InputField m_timeValueInput = null;
	[SerializeField] private TMP_InputField m_survivalBonusInput = null;

	// Progression
	[Space]
	[SerializeField] private Slider m_xpInitialDeltaSlider = null;
	[SerializeField] private Slider m_xpFinalDeltaSlider = null;
	[SerializeField] private Toggle m_nextDragonLockedToggle = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to changed events
		// Lambda expressions make it so much easier
		m_testEnabledToggle.onValueChanged.AddListener(
			(bool _toggled) => {
				testEnabled = _toggled;
				m_canvasGroup.interactable = testEnabled;
			}
		);

		m_chestsModeDropdown.InitFromEnum(CHESTS_MODE, typeof(ChestTestMode), (int)ChestTestMode.RANDOM);
		m_eggModeDropdown.InitFromEnum(EGG_MODE, typeof(EggTestMode), (int)EggTestMode.RANDOM);
		m_missionsModeDropdown.InitFromEnum(MISSIONS_MODE, typeof(MissionsTestMode), (int)MissionsTestMode.NONE);

		m_chestsLayoutDropdown.InitFromEnum(CHESTS_LAYOUT, typeof(ChestsLayout), (int)ChestsLayout.FULL_PROGRESSION);

		m_scoreValueInput.onValueChanged.AddListener(_text => scoreValue = long.Parse(_text));
		m_highScoreValueInput.onValueChanged.AddListener(_text => highScoreValue = long.Parse(_text));
		m_newHighScoreToggle.onValueChanged.AddListener(_toggled => newHighScore = _toggled);

		m_coinsValueInput.onValueChanged.AddListener(_text => coinsValue = long.Parse(_text));
		m_timeValueInput.onValueChanged.AddListener(_text => timeValue = float.Parse(_text));
		m_survivalBonusInput.onValueChanged.AddListener(_text => survivalBonus = long.Parse(_text));

		m_xpInitialDeltaSlider.minValue = 0f;
		m_xpInitialDeltaSlider.maxValue = 1f;
		m_xpInitialDeltaSlider.onValueChanged.AddListener(_value => xpInitialDelta = _value);

		m_xpFinalDeltaSlider.minValue = 0f;
		m_xpFinalDeltaSlider.maxValue = 1f;
		m_xpFinalDeltaSlider.onValueChanged.AddListener(_value => xpFinalDelta = _value);

		m_nextDragonLockedToggle.onValueChanged.AddListener(_toggled => nextDragonLocked = _toggled);
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
		m_testEnabledToggle.isOn = testEnabled;

		m_chestsModeDropdown.Refresh();
		m_eggModeDropdown.Refresh();
		m_missionsModeDropdown.Refresh();

		m_chestsLayoutDropdown.Refresh();

		m_scoreValueInput.text = scoreValue.ToString();
		m_highScoreValueInput.text = highScoreValue.ToString();
		m_newHighScoreToggle.isOn = newHighScore;

		m_coinsValueInput.text = coinsValue.ToString();
		m_timeValueInput.text = timeValue.ToString();
		m_survivalBonusInput.text = survivalBonus.ToString();

		m_xpInitialDeltaSlider.value = xpInitialDelta;
		m_xpFinalDeltaSlider.value = xpFinalDelta;
		m_nextDragonLockedToggle.isOn = nextDragonLocked;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}