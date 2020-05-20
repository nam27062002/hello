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
	// FTUX
	public const string FTUX_SKU = "ftuxSettings";

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
	public const string SHOW_EXIT_RUN_CONFIRMATION_POPUP = "SHOW_EXIT_RUN_CONFIRMATION_POPUP";  // bool, default true
	public const string UI_SETTINGS_SKU = "UISettings";

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
	private int m_enableChestsAtRun = 3;
	public static int ENABLE_CHESTS_AT_RUN { get { return instance.m_enableChestsAtRun; }}

	private int m_enableEggsAtRun = 2;
	public static int ENABLE_EGGS_AT_RUN { get { return instance.m_enableEggsAtRun; }}

	private int m_enableMissionsAtRun = 2;
	public static int ENABLE_MISSIONS_AT_RUN { get { return instance.m_enableMissionsAtRun; }}

	private int m_enableQuestsAtRun = 3;
	public static int ENABLE_QUESTS_AT_RUN { get { return instance.m_enableQuestsAtRun; }}

	private int m_enableTournamentsAtRun = 3;
	public static int ENABLE_TOURNAMENTS_AT_RUN { get { return instance.m_enableTournamentsAtRun; }}

	private int m_enableInterstitialPopupsAtRun = 12;
	public static int ENABLE_INTERSTITIAL_POPUPS_AT_RUN { get { return instance.m_enableInterstitialPopupsAtRun; }}

	private int m_enableOffersPopupAtRun = 4;
	public static int ENABLE_OFFERS_POPUPS_AT_RUN { get { return instance.m_enableOffersPopupAtRun; }}

	private int m_enablePreRegRewardsPopupAtRun = 2;
	public static int ENABLE_PRE_REG_REWARDS_POPUP_AT_RUN { get { return instance.m_enablePreRegRewardsPopupAtRun; }}

	private int m_enableSharkPetRewardPopupAtRun = 3;
	public static int ENABLE_SHARK_PET_REWARD_POPUP_AT_RUN { get { return instance.m_enableSharkPetRewardPopupAtRun; }}

	private int m_enableLeaguesAtRun = 4;
	public static int ENABLE_LEAGUES_AT_RUN { get { return instance.m_enableLeaguesAtRun; } }

	private int m_enableDailyRewardsAtRun = 2;
	public static int ENABLE_DAILY_REWARDS_AT_RUN { get { return instance.m_enableDailyRewardsAtRun; } }

	private int m_enableShareButtonsAtRun = 2;
	public static int ENABLE_SHARE_BUTTONS_AT_RUN { get { return instance.m_enableShareButtonsAtRun; } }

	private int m_enablePassiveEventsAtRun = 4;
	public static int ENABLE_PASSIVE_EVENTS_AT_RUN { get { return instance.m_enablePassiveEventsAtRun; } }

	private int m_enableHappyHourAtRun = 4;
	public static int ENABLE_HAPPY_HOUR_AT_RUN { get { return instance.m_enableHappyHourAtRun; } }


	// FTUX
	[Separator("UISettings")]
	private bool m_showNextDragonInXpBar = false;
	public static bool SHOW_NEXT_DRAGON_IN_XP_BAR { get { return instance.m_showNextDragonInXpBar; } }

	private bool m_showUnlockProgressionText = false;
	public static bool SHOWN_UNLOCK_PROGRESSION_TEXT { get { return instance.m_showUnlockProgressionText; } }

	private bool m_mapAsButton = false;
	public static bool MAP_AS_BUTTON { get { return instance.m_mapAsButton; } }

	// Social
	[Separator("Social")]
	[SerializeField] private ShareData m_shareDataIOS = new ShareData();
	[SerializeField] private ShareData m_shareDataAndroid = new ShareData();
	[SerializeField] private ShareData m_shareDataChina = new ShareData();
	public static ShareData shareData {
		get
        {
			Flavour currentFlavour = FlavourManager.Instance.GetCurrentFlavour();
			return currentFlavour.ShareData;
		}
	}

	public ShareData ShareDataIOS
	{
		get { return m_shareDataIOS; }
	}

	public ShareData ShareDataAndroid
	{
		get { return m_shareDataAndroid; }
	}

	public ShareData ShareDataChina
	{
		get { return m_shareDataChina; }
	}

    public string GameWebsiteUrl
    {
        get { return m_webURL; }
    }

    public string GetWebsiteUrlChina
	{
        get { return m_webURLChina; }
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
	[SerializeField] private string m_webURLChina = "https://www.ubisoft.com.cn/games/hd";
	public static string WEB_URL
	{
		get
		{
			Flavour currentFlavour = FlavourManager.Instance.GetCurrentFlavour();
			return currentFlavour.GameWebsiteUrl;
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
		// instance.m_audioMixer = AudioController.Instance.AudioObjectPrefab.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
		instance.m_audioMixer = InstanceManager.masterMixer;

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
	/// Read some initial values from content.
	/// To be called at the start of the application.
	/// Content Manager needs to be ready and definitions loaded.
	/// Call again if definitions are reloaded (i.e. customization applied).
	/// </summary>
	public static void InitFromDefinitions() {
		// Make sure content manager is ready
		Debug.Assert(ContentManager.ready, "ERROR: Trying to initialize Game Settings but content is not ready yet!");

		// Gather definition
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, FTUX_SKU);
		if(def == null) return;	// Just in case

		// Cache data. If not found, use default value.
		instance.m_enableChestsAtRun = def.GetAsInt("enableChestsAtRun", instance.m_enableChestsAtRun);
		instance.m_enableEggsAtRun = def.GetAsInt("enableEggsAtRun", instance.m_enableEggsAtRun);
		instance.m_enableMissionsAtRun = def.GetAsInt("enableMissionsAtRun", instance.m_enableMissionsAtRun);
		instance.m_enableQuestsAtRun = def.GetAsInt("enableQuestsAtRun", instance.m_enableQuestsAtRun);
		instance.m_enableTournamentsAtRun = def.GetAsInt("enableTournamentsAtRun", instance.m_enableTournamentsAtRun);
		instance.m_enableInterstitialPopupsAtRun = def.GetAsInt("enableInterstitialPopupsAtRun", instance.m_enableInterstitialPopupsAtRun);
		instance.m_enableOffersPopupAtRun = def.GetAsInt("enableOffersPopupAtRun", instance.m_enableOffersPopupAtRun);
		instance.m_enableSharkPetRewardPopupAtRun = def.GetAsInt("enableSharkPetRewardPopupAtRun", instance.m_enableSharkPetRewardPopupAtRun);
		instance.m_enableLeaguesAtRun = def.GetAsInt("enableLeaguesAtRun", instance.m_enableLeaguesAtRun);
		instance.m_enableDailyRewardsAtRun = def.GetAsInt("enableDailyRewardsAtRun", instance.m_enableDailyRewardsAtRun);
		instance.m_enableShareButtonsAtRun = def.GetAsInt("enableShareButtonsAtRun", instance.m_enableShareButtonsAtRun);
		instance.m_enablePassiveEventsAtRun = def.GetAsInt("enablePassiveEventsAtRun", instance.m_enablePassiveEventsAtRun);
		instance.m_enableHappyHourAtRun = def.GetAsInt("enableHappyHourAtRun", instance.m_enableHappyHourAtRun);


		// UI Settings:
		def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, UI_SETTINGS_SKU);
		if (def == null) return;    // Just in case

		instance.m_showNextDragonInXpBar = def.GetAsBool("showNextDragonInXpBar", instance.m_showNextDragonInXpBar);
		instance.m_showUnlockProgressionText = def.GetAsBool("showUnlockProgressionText", instance.m_showUnlockProgressionText);
		instance.m_mapAsButton = def.GetAsBool("mapAsButton", instance.m_mapAsButton);
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
			if(instance.m_audioMixer != null) 
			{
				instance.m_audioMixer.SetFloat("MusicVolume", -80f);
				instance.m_audioMixer.SetFloat("SfxVolume", -80f);
				instance.m_audioMixer.SetFloat("Sfx2DVolume", -80f);
			}
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