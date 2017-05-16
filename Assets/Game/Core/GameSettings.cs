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
	// AUDIO SETTINGS														  //
	//------------------------------------------------------------------------//
	public const string SOUND_ENABLED = "GAME_SETTINGS_SOUND_ENABLED";	// bool, default true
	public static bool soundEnabled { 
		get { return Prefs.GetBoolPlayer(SOUND_ENABLED, true); }
		set { 
			if(instance.m_audioMixer != null) {
				instance.m_audioMixer.SetFloat("SfxVolume", value ? 0f : -80f);
				instance.m_audioMixer.SetFloat("Sfx2DVolume", value ? 0f : -80f);
			}
			Prefs.SetBoolPlayer(SOUND_ENABLED, value);
		}
	}

	public const string MUSIC_ENABLED = "GAME_SETTINGS_MUSIC_ENABLED";	// bool, default true
	public static bool musicEnabled { 
		get { return Prefs.GetBoolPlayer(MUSIC_ENABLED, true); }
		set { 
			if(instance.m_audioMixer != null) {
				instance.m_audioMixer.SetFloat("MusicVolume", value ? 0f : -80f);
			}
			Prefs.SetBoolPlayer(MUSIC_ENABLED, value);
		}
	}

	//------------------------------------------------------------------------//
	// CONTROL SETTINGS														  //
	//------------------------------------------------------------------------//
	public const string TILT_CONTROL_ENABLED = "GAME_SETTINGS_TILT_CONTROL_ENABLED";	// bool, default false
	public static bool tiltControlEnabled {
		get { return Prefs.GetBoolPlayer(TILT_CONTROL_ENABLED, false); }
		set {
			Prefs.SetBoolPlayer(TILT_CONTROL_ENABLED, value);
			Messenger.Broadcast<bool>(GameEvents.TILT_CONTROL_TOGGLE, value);
		}
	}

	public const string TILT_CONTROL_SENSITIVITY = "GAME_SETTINGS_TILT_CONTROL_SENSITIVITY";	// float [0..1], default 0.5f
	public static float tiltControlSensitivity {
		get { return Prefs.GetFloatPlayer(TILT_CONTROL_SENSITIVITY, 0.5f); }
		set { Prefs.SetFloatPlayer(TILT_CONTROL_SENSITIVITY, value); }
	}

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
		// Sound
		soundEnabled = soundEnabled;
		musicEnabled = musicEnabled;

		// Controls
		tiltControlEnabled = tiltControlEnabled;
		tiltControlSensitivity = tiltControlSensitivity;
	}

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
		// Get conversion factor from definition
		DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		double coinsToPC = gameSettingsDef.GetAsDouble("missingRessourcesPCperSC");

		// Apply, round and return
		double pc = Mathf.Abs(_coins) * coinsToPC;
		pc = Math.Round(pc, MidpointRounding.AwayFromZero);
		return Math.Max(1, (long)pc);	// At least 1
	}
}