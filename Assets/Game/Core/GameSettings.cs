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
				m_defaultValues[TOUCH_3D_ENABLED] = PlatformUtils.Instance.InputPressureSupprted();
				m_defaultValues[BLOOD_ENABLED] = true;

				m_defaultValues[SHOW_BIG_AMOUNT_CONFIRMATION_POPUP] = true;
				m_defaultValues[SHOW_EXIT_RUN_CONFIRMATION_POPUP] = true;
			}
			return m_defaultValues;
		}
	}

	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
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
			Messenger.Broadcast<float>(MessengerEvents.TILT_CONTROL_SENSITIVITY_CHANGED, value);
		}
	}

	// UI Settings
	public const string SHOW_BIG_AMOUNT_CONFIRMATION_POPUP = "SHOW_BIG_AMOUNT_CONFIRMATION_POPUP";	// bool, default true
	public const string SHOW_EXIT_RUN_CONFIRMATION_POPUP = "SHOW_EXIT_RUN_CONFIRMATION_POPUP";	// bool, default true

	[Serializable]
	public class ShareData {
		public string url = "";

		[FileList("Resources/UI/Menu/QR_codes", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*", false)]
		public string qrCodePath = "";
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Add here any global setup variable such as quality, server ip, debug enabled, ...
	// Versioning
	[Separator("Versioning")]
	[SerializeField] private Version m_internalVersion = new Version(0, 1, 0);
	public static Version internalVersion { get { return instance.m_internalVersion; }}
	
	// Gameplay
	[Separator("Gameplay")]
	[Comment("Name of the dragon instance on the scene")]
	[SerializeField] private string m_playerName = "Player";
	public static string playerName { get { return instance.m_playerName; }}

	// FTUX
	[Separator("FTUX")]
	[SerializeField] private int m_enableChestsAtRun = 3;
	public static int ENABLE_CHESTS_AT_RUN { get { return instance.m_enableChestsAtRun; }}

	[SerializeField] private int m_enableEggsAtRun = 2;
	public static int ENABLE_EGGS_AT_RUN { get { return instance.m_enableEggsAtRun; }}

	[SerializeField] private int m_enableMissionsAtRun = 2;
	public static int ENABLE_MISSIONS_AT_RUN { get { return instance.m_enableMissionsAtRun; }}

	[SerializeField] private int m_enableQuestsAtRun = 3;
	public static int ENABLE_QUESTS_AT_RUN { get { return instance.m_enableQuestsAtRun; }}

	[SerializeField] private int m_enableTournamentsAtRun = 3;
	public static int ENABLE_TOURNAMENTS_AT_RUN { get { return instance.m_enableTournamentsAtRun; }}

	[SerializeField] private int m_enableInterstitialPopupsAtRun = 12;
	public static int ENABLE_INTERSTITIAL_POPUPS_AT_RUN { get { return instance.m_enableInterstitialPopupsAtRun; }}

	[SerializeField] private int m_enableOffersPopupAtRun = 4;
	public static int ENABLE_OFFERS_POPUPS_AT_RUN { get { return instance.m_enableOffersPopupAtRun; }}

	[SerializeField] private int m_enablePreRegRewardsPopupAtRun = 2;
	public static int ENABLE_PRE_REG_REWARDS_POPUP_AT_RUN { get { return instance.m_enablePreRegRewardsPopupAtRun; }}

	[SerializeField] private int m_enableSharkPetRewardPopupAtRun = 3;
	public static int ENABLE_SHARK_PET_REWARD_POPUP_AT_RUN { get { return instance.m_enableSharkPetRewardPopupAtRun; }}

	[SerializeField] private int m_enableLabAtRun = 4;
	public static int ENABLE_LAB_AT_RUN { get { return instance.m_enableLabAtRun; } }

	[SerializeField] private int m_enableDailyRewardsAtRun = 2;
	public static int ENABLE_DAILY_REWARDS_AT_RUN { get { return instance.m_enableDailyRewardsAtRun; } }

	// Social
	[Separator("Social")]
	[SerializeField] private ShareData m_shareDataIOS = new ShareData();
	[SerializeField] private ShareData m_shareDataAndroid = new ShareData();
	[SerializeField] private ShareData m_shareDataChina = new ShareData();
	public static ShareData shareData {
		get {
			// Select target share data based on platform
			// Also specific data for China!
			if(PlatformUtils.Instance.IsChina()) {
				return instance.m_shareDataChina;
			} else {
#if UNITY_IOS
				return instance.m_shareDataIOS;
#else
				return instance.m_shareDataAndroid;
#endif
			}
		}
	}

	// Social Links
	[Space]
	[SerializeField] private string m_facebookURL = "https://www.facebook.com/HungryDragonGame";
	public static string FACEBOOK_URL {
		get { return instance.m_facebookURL; }
	}

	[SerializeField] private string m_twitterURL = "https://twitter.com/_HungryDragon";
	public static string TWITTER_URL {
		get { return instance.m_twitterURL; }
	}

	[SerializeField] private string m_instagramURL = "https://www.instagram.com/hungrydragongame";
	public static string INSTAGRAM_URL {
		get { return instance.m_instagramURL; }
	}

	[SerializeField] private string m_webURL = "http://hungrydragongame.com";
	[SerializeField] private string m_webChinaURL = "http://hungrydragongame.com";
	public static string WEB_URL {
		get {
			// Are we in China?
			if(PlatformUtils.Instance.IsChina()) {
				return instance.m_webChinaURL;
			} else {
				return instance.m_webURL;
			}
		}
	}

	[SerializeField] private string m_weiboURL = "https://www.weibo.com/ubichinamobile";
	public static string WEIBO_URL {
		get { return instance.m_weiboURL; }
	}

	[SerializeField] private string m_weChatURL = "";
	public static string WE_CHAT_URL {
		get { return instance.m_weChatURL; }
	}

	[Space]
	[SerializeField] private string m_privacyPolicyIOS_URL = "https://hungrydragon.ubi.com/legal/";
	public static string PRIVACY_POLICY_IOS_URL {
		// Attach standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		get { return instance.m_privacyPolicyIOS_URL + LocalizationManager.SharedInstance.Culture.Name; }
	}

    [SerializeField]
    private string m_privacyPolicyANDROID_URL = "https://legal.ubi.com/privacypolicy/";
    public static string PRIVACY_POLICY_ANDROID_URL
    {
        // Attach standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
        get { return instance.m_privacyPolicyANDROID_URL + LocalizationManager.SharedInstance.Culture.Name; }
    }


    [SerializeField] private string m_eulaURL = "https://legal.ubi.com/eula/";
	public static string EULA_URL {
		// Attach standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		get { return instance.m_eulaURL + LocalizationManager.SharedInstance.Culture.Name; }
	}

	[SerializeField] private string m_termsOfUseURL = "https://legal.ubi.com/termsofuse/";
	public static string TERMS_OF_USE_URL {
		// Attach standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		get { return instance.m_termsOfUseURL;/* + LocalizationManager.SharedInstance.Culture.Name;*/ } // Link already redirects to correct localized URL
	}

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
		Messenger.Broadcast<string, bool>(MessengerEvents.GAME_SETTING_TOGGLED, _settingId, _value);
	}

    public static void OnApplicationPause( bool _pause )
    {
        if ( _pause )
        {
            // Mute all sound when the app pauses
            instance.m_audioMixer.SetFloat("MusicVolume", -80f);
            instance.m_audioMixer.SetFloat("SfxVolume", -80f);
            instance.m_audioMixer.SetFloat("Sfx2DVolume", -80f);
        }
        else
        {
            // Restore all sound when the app resumes
            Set(SOUND_ENABLED, Get(SOUND_ENABLED));
            Set(MUSIC_ENABLED, Get(MUSIC_ENABLED));    
        }
    }

	/// <summary>
	/// Open the given URL, optionally adding a delay to it to give enough time
	/// to SFX and other stuff to be played before losing focus.
	/// </summary>
	/// <param name="_url">URL to be opened (see GameSettings constants).</param>
	/// <param name="_delay">Delay in seconds.</param>
	public static void OpenUrl(string _url, float _delay = 0.15f) {
		// Check invalid URL
		if(string.IsNullOrEmpty(_url)) {
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_COMING_SOON"),
				GameConstants.Vector2.center,
				PopupManager.canvas.transform as RectTransform
			);
			return;
		}

		// Launch delayed
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				Application.OpenURL(_url);
			}, _delay
		);
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
		double coefTier = 1f;	// [1..N]
		for(int i = 0; i < tierDefs.Count; ++i) {
			if(_coins > tierDefs[i].GetAsLong("minimumSC")) {
				// This is our tier!
				coefTier = tierDefs[i].GetAsDouble("tier");
				break;
			}
		}

		// Get constants from definitions
		DefinitionNode constantsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "formulaCalculation");
		double coefA = constantsDef.GetAsDouble("coefficientA");
		double coefB = constantsDef.GetAsDouble("coefficientB");
		double baseValue = constantsDef.GetAsDouble("scHCBaseValue");

		// Compute conversion factor
		double scPerPc = (coefA * coefTier + coefB) * baseValue;

		// Apply, round and return
		double pc = Mathf.Abs(_coins)/scPerPc;
		pc = Math.Round(pc, MidpointRounding.AwayFromZero);
		return Math.Max(1, (long)pc);	// At least 1
	}
}