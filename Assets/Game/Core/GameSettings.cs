// GameSettings.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global setup of the game and non-critical player settings stored in the device's cache.
/// </summary>
public class GameSettings : SingletonScriptableObject<GameSettings> {
	//------------------------------------------------------------------------//
	// DEFAULT VALUES														  //
	// Add here any new setting that needs initialization!					  //
	//------------------------------------------------------------------------//
	private Dictionary<string, bool> m_defaultValues = null;
	private Dictionary<string, bool> defaultValues {
		get {
			if(m_defaultValues == null) {
				m_defaultValues = new Dictionary<string, bool>();

				m_defaultValues[SOUND_ENABLED] = true;
				m_defaultValues[MUSIC_ENABLED] = true;

				m_defaultValues[TILT_CONTROL_ENABLED] = false;
				m_defaultValues[TOUCH_3D_ENABLED] = false;
				m_defaultValues[BLOOD_ENABLED] = true;

				m_defaultValues[SHOW_BIG_AMOUNT_CONFIRMATION_POPUP] = true;
				m_defaultValues[SHOW_EXIT_RUN_CONFIRMATION_POPUP] = true;

				m_defaultValues[NOTIFICATIONS_ENABLED] = true;
			}
			return m_defaultValues;
		}
	}

	// Audio Settings
	public const string SOUND_ENABLED = "GAME_SETTINGS_SOUND_ENABLED";	// bool, default true
	public const string MUSIC_ENABLED = "GAME_SETTINGS_MUSIC_ENABLED";	// bool, default true

	// Game Options Settings
	public const string TILT_CONTROL_ENABLED = "GAME_SETTINGS_TILT_CONTROL_ENABLED";	// bool, default false
	public const string TOUCH_3D_ENABLED = "GAME_SETTINGS_TOUCH_3D_ENABLED";	// bool, default false
	public const string BLOOD_ENABLED = "GAME_SETTINGS_BLOOD_ENABLED";	// bool, default true

	public const string TILT_CONTROL_SENSITIVITY = "GAME_SETTINGS_TILT_CONTROL_SENSITIVITY";	// float [0..1], default 0.5f
	public static float tiltControlSensitivity {
		get { return Prefs.GetFloatPlayer(TILT_CONTROL_SENSITIVITY, 0.5f); }
		set {
			Prefs.SetFloatPlayer(TILT_CONTROL_SENSITIVITY, value);
			Messenger.Broadcast<float>(GameEvents.TILT_CONTROL_SENSITIVITY_CHANGED, value);
		}
	}

	// UI Settings
	public const string SHOW_BIG_AMOUNT_CONFIRMATION_POPUP = "SHOW_BIG_AMOUNT_CONFIRMATION_POPUP";	// bool, default true
	public const string SHOW_EXIT_RUN_CONFIRMATION_POPUP = "SHOW_EXIT_RUN_CONFIRMATION_POPUP";	// bool, default true

	// Other settings
	public const string NOTIFICATIONS_ENABLED = "GAME_SETTINGS_NOTIFICATIONS_ENABLED";	// bool, default true

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Add here any global setup variable such as quality, server ip, debug enabled, ...
	[Comment("Name of the dragon instance on the scene")]
	[Separator("Gameplay")]
	[SerializeField] private string m_playerName = "Player";
	public static string playerName { get { return instance.m_playerName; }}

	[Separator("Versioning")]
	[Comment("Used by the development team, QC, etc. to identify each build internally.\nFormat X.Y.Z where:\n    - X: Development Stage [1..4] (1 - Preproduction, 2 - Production, 3 - Soft Launch, 4 - Worldwide Launch)\n    - Y: Sprint Number [1..N]\n    - Z: Build Number [1..N] within the sprint, increased by 1 for each new build")]
	[SerializeField] private Version m_internalVersion = new Version(0, 1, 0);
	public static Version internalVersion { get { return instance.m_internalVersion; }}

	// Internal references
	private AudioMixer m_audioMixer = null;

	//------------------------------------------------------------------//
	// SINGLETON STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization. To be called at the start of the application.
	/// </summary>
	public static void Init() {
		// Get external references
		instance.m_audioMixer = AudioController.Instance.AudioObjectPrefab.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;

		// Apply stored values
		Set(SOUND_ENABLED, Get(SOUND_ENABLED));
		Set(MUSIC_ENABLED, Get(MUSIC_ENABLED));

		Set(TILT_CONTROL_ENABLED, Get(TILT_CONTROL_ENABLED));
		Set(TOUCH_3D_ENABLED, Get(TOUCH_3D_ENABLED));
		Set(BLOOD_ENABLED, Get(BLOOD_ENABLED));

		Set(SHOW_BIG_AMOUNT_CONFIRMATION_POPUP, Get(SHOW_BIG_AMOUNT_CONFIRMATION_POPUP));
		Set(SHOW_EXIT_RUN_CONFIRMATION_POPUP, Get(SHOW_EXIT_RUN_CONFIRMATION_POPUP));
	}

	/// <summary>
	/// Get
	/// </summary>
	/// <param name="_settingId">Setting identifier.</param>
	/// <param name="_defaultValue">If set to <c>true</c> default value.</param>
	public static bool Get(string _settingId) {
		// Gather default value for this property, if any
		bool defaultValue = true;
		instance.defaultValues.TryGetValue(_settingId, out defaultValue);

		// Return persisted value
		return Prefs.GetBoolPlayer(_settingId, defaultValue);
	}

	/// <summary>
	/// Set a specific setting.
	/// </summary>
	/// <param name="_settingId">Setting identifier, typically a constant from this class.</param>
	/// <param name="_value">New value for the setting.</param>
	public static void Set(string _settingId, bool _value) {
		// Some properties need extra stuff
		switch(_settingId) {
			case MUSIC_ENABLED: {
				if(instance.m_audioMixer != null) {
					instance.m_audioMixer.SetFloat("MusicVolume", _value ? 0f : -80f);
				}
			} break;

			case SOUND_ENABLED: {
				if(instance.m_audioMixer != null) {
					instance.m_audioMixer.SetFloat("SfxVolume", _value ? 0f : -80f);
					instance.m_audioMixer.SetFloat("Sfx2DVolume", _value ? 0f : -80f);
				}
			} break;
		}

		// Persist
		Prefs.SetBoolPlayer(_settingId, _value);

		// Notify game
		Messenger.Broadcast<string, bool>(GameEvents.GAME_SETTING_TOGGLED, _settingId, _value);
	}

	//------------------------------------------------------------------//
	// OTHER STATIC METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compute the PC equivalent of a given amount of time.
	/// </summary>
	/// <returns>The amount of PC worth for <paramref name="_time"/> amount of time.</returns>
	/// <param name="_time">Amount of time to be evaluated.</param>
	public static int ComputePCForTime(TimeSpan _time) {
		// Get coeficients from definition
		DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		float timePcCoefA = gameSettingsDef.GetAsFloat("timeToPCCoefA");
		float timePcCoefB = gameSettingsDef.GetAsFloat("timeToPCCoefB");

		// Just apply Hadrian's formula
		double pc = timePcCoefA * _time.TotalMinutes + timePcCoefB;
		pc = Math.Round(pc, MidpointRounding.AwayFromZero);
		return Mathf.Max(1, (int)pc);	// At least 1
	}

	/// <summary>
	/// Compute the PC equivalent of a given amount of coins.
	/// </summary>
	/// <returns>The PC worth for <paramref name="_coins"/> amount of coins.</returns>
	/// <param name="_coins">Amount of coins to be evaluated.</param>
	public static long ComputePCForCoins(long _coins) {
		// Progressive conversion rather than linear one, so PC required for big amounts doesn't feel so punishing
		// Formula from eco design: SCperHC = (coefA * tier + coefB) * base

		// Figure out tier corresponding to the target coins amount
		List<DefinitionNode> tierDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.CURRENCY_TIERS);
		DefinitionsManager.SharedInstance.SortByProperty(ref tierDefs, "minimumSC", DefinitionsManager.SortType.NUMERIC);
		tierDefs.Reverse();	// High to low
		int selectedTier = 1;	// [1..N]
		for(int i = 0; i < tierDefs.Count; ++i) {
			if(_coins > tierDefs[i].GetAsLong("minimumSC")) {
				// This is our tier!
				selectedTier = tierDefs[i].GetAsInt("tier");
				break;
			}
		}

		// Get constants from definitions
		DefinitionNode constantsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "formulaCalculation");
		double coefA = constantsDef.GetAsDouble("coefficientA");
		double coefB = constantsDef.GetAsDouble("coefficientB");
		double baseValue = constantsDef.GetAsDouble("scHCBaseValue");

		// Compute conversion factor
		double scPerPc = (coefA * (double)selectedTier + coefB) * baseValue;

		// Apply, round and return
		double pc = Mathf.Abs(_coins)/scPerPc;
		pc = Math.Round(pc, MidpointRounding.AwayFromZero);
		return Math.Max(1, (long)pc);	// At least 1
	}
}