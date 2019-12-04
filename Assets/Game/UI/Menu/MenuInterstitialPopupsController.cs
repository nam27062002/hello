// MenuInterstitialPopupsController.cs
// Hungry Dragon
//
// Created by Alger Ortín Castellví on 22/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Centralized control to check which interstitial popups should be opened upon
/// entering the menu.
/// </summary>
public class MenuInterstitialPopupsController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string RATING_DRAGON = "dragon_crocodile";

	// Custom flags altering the standard flow
	[System.Flags]
	public enum StateFlag {
		NONE = 1 << 0,
		NEW_DRAGON_UNLOCKED = 1 << 1,
		POPUP_DISPLAYED = 1 << 2,
		WAIT_FOR_CUSTOM_POPUP = 1 << 3,
		CHECKING_CONNECTION = 1 << 4,
		COMING_FROM_A_RUN = 1 << 5,
		OPEN_OFFERS_SHOP = 1 << 6
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private StateFlag m_stateFlags = StateFlag.NONE;
	private float m_waitTimeOut;

	// Cache some data
	private IDragonData m_ratingDragonData = null;
	private MenuScreen m_previousScreen = MenuScreen.NONE;
	private MenuScreen m_currentScreen = MenuScreen.NONE;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Register to external events
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);

		// Initialize internal vars
		m_stateFlags = StateFlag.NONE;
		SetFlag(StateFlag.NEW_DRAGON_UNLOCKED, !string.IsNullOrEmpty(GameVars.unlockedDragonSku));
		m_ratingDragonData = DragonManager.GetDragonData(RATING_DRAGON);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unregister from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}
    
    /// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Customizer popup async operation+
		if (GetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP)) {
			if (!GetFlag(StateFlag.POPUP_DISPLAYED)) {
				Calety.Customiser.CustomiserPopupConfig popupConfig = HDCustomizerManager.instance.GetLastPreparedPopupConfig();
				if (popupConfig != null) {
					OpenCustomizerPopup(popupConfig);
				} else {
					m_waitTimeOut -= Time.deltaTime;
					if (m_waitTimeOut <= 0f) {
						BusyScreen.Hide(this, true);
						SetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP, false);
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Set the value of a flag.
	/// </summary>
	/// <param name="_flag">Flag.</param>
	/// <param name="_value">new value for that flag.</param>
	public void SetFlag(StateFlag _flag, bool _value) {
		// Special case for NONE
		if(_flag == StateFlag.NONE) {
			m_stateFlags = _flag;	// Clear all flags
		} else {
			if(_value) {
				m_stateFlags |= _flag;
			} else {
				m_stateFlags &= ~_flag;
			}
		}
	}

	/// <summary>
	/// Get the value of a flag.
	/// </summary>
	/// <returns>The current value of a flag.</returns>
	/// <param name="_flag">The target flag.</param>
	private bool GetFlag(StateFlag _flag) {
		// Special case for NONE
		if(_flag == StateFlag.NONE) {
			return m_stateFlags == StateFlag.NONE;  // Only if there is actually no flags active
		}

		return (m_stateFlags & _flag) != 0;
	}

	//------------------------------------------------------------------------//
	// POPUP TRIGGER METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Checks whether the Terms and Conditions popup must be opened or not and does it.
	/// </summary>
	private void CheckTermsAndConditions() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Only in the Play screen
		if(m_currentScreen != MenuScreen.PLAY) return;

		// Is the last accepted version the same as the current one?
		if(PlayerPrefs.GetInt(PopupConsentLoading.VERSION_PREFS_KEY) != PopupConsentLoading.LEGAL_VERSION) {
			PopupManager.EnqueuePopup(PopupConsentLoading.PATH);
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	/// <summary>
	/// Check Shark Pet reward popup.
	/// </summary>
    private void CheckShark()
    {
		// Don't show if a more important popup has already been displayed in this menu loop
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Nothing to do if pet is already owned
		if(UsersManager.currentUser.petCollection.IsPetUnlocked(PopupSharkPetReward.PET_SKU)) return;

		// Minimum amount of runs must be completed
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_SHARK_PET_REWARD_POPUP_AT_RUN) return;

		// Only in the right screen
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

        // Check if hungry shark is installed
        if (IsHungrySharkGameInstalled())
        {
            // Show popup
			PopupController popup = PopupManager.EnqueuePopup(PopupSharkPetReward.PATH);
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
        }
    }

	/// <summary>
	/// Is the Hungry Shark Evolution game installed?
	/// </summary>
	/// <returns>Whether Hungry Shark Evolution is installed in this device.</returns>
    private bool IsHungrySharkGameInstalled()
    {
        bool ret = false;
#if UNITY_EDITOR
		ret = true;
#elif UNITY_ANDROID
        ret = PlatformUtils.Instance.ApplicationExists("com.fgol.HungrySharkEvolution");
#elif UNITY_IOS
		ret = PlatformUtils.Instance.ApplicationExists("hungrysharkevolution://");
#endif
        return ret;
    }

	/// <summary>
	/// Check popups coming from the customizer.
	/// </summary>
	private void CheckCustomizerPopup() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Only in play screen
		if(m_currentScreen != MenuScreen.PLAY) return;

		// Enough runs have been played?
		if(UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			// Need to wait for it?
			SetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP, HDCustomizerManager.instance.IsCustomiserPopupAvailable());
			if(GetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP)) {
				m_waitTimeOut = 5f;
				BusyScreen.Show(this, false);

				string langServerCode = "en";
				DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, LocalizationManager.SharedInstance.GetCurrentLanguageSKU());
				if(langDef != null) {
					langServerCode = langDef.GetAsString("serverCode", langServerCode);
				}
				Calety.Customiser.CustomiserPopupConfig popupConfig = HDCustomizerManager.instance.GetOrRequestCustomiserPopup(langServerCode);
				if(popupConfig != null) {
					OpenCustomizerPopup(popupConfig);
				}
			}
		}
	}

	/// <summary>
	/// Popups coming from the customizer.
	/// </summary>
	/// <param name="_config">Popup configuration.</param>
	private void OpenCustomizerPopup(Calety.Customiser.CustomiserPopupConfig _config) {
		string popupPath = PopupCustomizer.PATH + "PF_PopupLayout_" + _config.m_iLayout;

		PopupController pController = PopupManager.EnqueuePopup(popupPath);
		PopupCustomizer pCustomizer = pController.GetComponent<PopupCustomizer>();
		pCustomizer.InitFromConfig(_config);

		SetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP, false);
		SetFlag(StateFlag.POPUP_DISPLAYED, true);

		BusyScreen.Hide(this, true);
	}

    /// <summary>
    /// Checks the interstitial ads.
    /// </summary>
    private void CheckInterstitialAds() {
        
        // Do the player has the Remove ads feature?
        if (UsersManager.currentUser.removeAds.IsActive)
        {
            // No ads for this user
            return;
        }

		// If coming from a run, regardles of the destination screen
		if(GetFlag(StateFlag.COMING_FROM_A_RUN)) {
			if(GameAds.instance.IsValidUserForInterstitials()) {
				StartCoroutine(LaunchInterstitial());
			}
        }
    }

	/// <summary>
	/// Open interstitial ad popup after some delay.
	/// </summary>
	/// <returns>Coroutine.</returns>
    IEnumerator LaunchInterstitial() {
		SetFlag(StateFlag.POPUP_DISPLAYED, true);
        yield return new WaitForSeconds(0.25f);
        PopupAdBlocker.LaunchAd(false, GameAds.EAdPurpose.INTERSTITIAL, OnInterstitialAdEnded);
	}

	/// <summary>
	/// Check Cross Promotion popups.
	/// </summary>
    private void CheckInterstitialCP2() {
        // CP2 interstitial has the lowest priority so if the user has already seen a popup or an ad then cp2 interstitial shouldn't be shown
		if (GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Only in the right screen
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

		// Only after a run
		if(!GetFlag(StateFlag.COMING_FROM_A_RUN)) return;

        bool checkUserRestriction = true;
        if (HDCP2Manager.Instance.CanPlayInterstitial(checkUserRestriction)) {
            PopupAdBlocker.LaunchCp2Interstitial(null);            
        }
    }    

    /// <summary>
    /// Checks whether the Rating popup must be opened or not and does it.
    /// </summary>
    private void CheckRating() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Enough runs have been played?
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) return;

		// Are we in the right screen?
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

		// Coming from game?
		if(!GetFlag(StateFlag.COMING_FROM_A_RUN)) return;

		// Is dragon unlocked?
		if(m_ratingDragonData.GetLockState() > IDragonData.LockState.LOCKED) {
			// If the dragon has been unlocked outside the menu (leveling up previous dragon),
			// don't show the popup the very first time to prevent conflict with the dragon 
			// unlock animation
			bool _checked = Prefs.GetBoolPlayer(Prefs.RATE_CHECK_DRAGON, false);
			if(_checked) {
				// Check if we need to make the player rate the game
				if(Prefs.GetBoolPlayer(Prefs.RATE_CHECK, true)) {
					string dateStr = Prefs.GetStringPlayer( Prefs.RATE_FUTURE_DATE, System.DateTime.Now.ToString());
					System.DateTime futureDate = System.DateTime.Now;
					if(!System.DateTime.TryParse(dateStr, out futureDate)) {
						futureDate = System.DateTime.Now;
					}
					if(System.DateTime.Compare(System.DateTime.Now, futureDate) > 0) {
						// Start Asking!
						if(Application.platform == RuntimePlatform.Android) {
							PopupManager.EnqueuePopup(PopupAskLikeGame.PATH);
							SetFlag(StateFlag.POPUP_DISPLAYED, true);
						} else if(Application.platform == RuntimePlatform.IPhonePlayer) {
							PopupManager.EnqueuePopup(PopupAskRateUs.PATH);
							SetFlag(StateFlag.POPUP_DISPLAYED, true);
						}
					}
				}
			} else {
				// Next time we will show the popup
				Prefs.SetBoolPlayer(Prefs.RATE_CHECK_DRAGON, true);
			}
		}
		
	}

	/// <summary>
	/// Check silent notification popups.
	/// </summary>
	private void CheckSilentNotification() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Only in the right screen
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

		// Only when coming from the play screen
		if(m_previousScreen != MenuScreen.PLAY) return;

		if(PlayerPrefs.GetInt(HDNotificationsManager.SILENT_FLAG) == 1 && !GetFlag(StateFlag.CHECKING_CONNECTION)) 
		{
			if(DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable) {
				ShowGoOnlinePopup();
				PlayerPrefs.SetInt(HDNotificationsManager.SILENT_FLAG, 0);
			} else {
				SetFlag(StateFlag.CHECKING_CONNECTION, true);
				GameServerManager.SharedInstance.CheckConnection( OnConnectionCheck );
			}
		}
	}

	/// <summary>
	/// Open a message popup to prompt the player to go online.
	/// </summary>
	private void ShowGoOnlinePopup() {
		IPopupMessage.Config config = IPopupMessage.GetConfig();
		config.TextType = IPopupMessage.Config.ETextType.DEFAULT;
		config.TitleTid = "TID_GO_ONLINE_FOR_TOURNAMENTS_TITLE";
		config.ShowTitle = true;
		config.MessageTid = "TID_GO_ONLINE_FOR_TOURNAMENTS_BODY";
		config.BackButtonStrategy = IPopupMessage.Config.EBackButtonStratety.Close;
		config.ConfirmButtonTid = "TID_GEN_OK";
		config.OnConfirm = null;
		config.ButtonMode = IPopupMessage.Config.EButtonsMode.Confirm;
		config.IsButtonCloseVisible = true;

		PopupManager.PopupMessage_Open(config);

		SetFlag(StateFlag.POPUP_DISPLAYED, true);
	}

	/// <summary>
	/// Checks whether the Survey popup must be opened or not and does it.
	/// </summary>
	private void CheckSurvey() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Only in the right screen
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

		// Only when coming from a run
		if(!GetFlag(StateFlag.COMING_FROM_A_RUN)) return;

		// Enough games have been played?
		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			if(PopupAskSurvey.CheckAndOpen() != null) {
				SetFlag(StateFlag.POPUP_DISPLAYED, true);
			}
		}
	}

	/// <summary>
	/// Checks whether the Featured Offer popup must be opened or not and does it.
	/// </summary>
	private void CheckFeaturedOffer() {
		// Minimum amount of runs must be completed
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_OFFERS_POPUPS_AT_RUN) return;

		// Nothing to show if there is no featured offer
		if(OffersManager.featuredOffer == null) return;

		// Choose a location based on current screen
		OfferPack.WhereToShow whereToShow = OfferPack.WhereToShow.SHOP_ONLY;
		switch(m_currentScreen) {
			case MenuScreen.PLAY: {
				whereToShow = OfferPack.WhereToShow.PLAY_SCREEN;
			} break;

			case MenuScreen.DRAGON_SELECTION: {
				if(GetFlag(StateFlag.COMING_FROM_A_RUN)) {
					whereToShow = OfferPack.WhereToShow.DRAGON_SELECTION_AFTER_RUN;
				} else {
					whereToShow = OfferPack.WhereToShow.DRAGON_SELECTION;
				}
			} break;
		}

		// The offer will do the rest of the checks
		PopupController popup = OffersManager.featuredOffer.EnqueuePopupIfPossible(whereToShow);
		if(popup != null) {
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	/// <summary>
	/// Check wether we need to launch the promoted IAPs popup.
	/// </summary>
    private void CheckPromotedIAPs() {
		// Only in the right screens
		if(m_currentScreen != MenuScreen.PLAY && m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

        if (GameStoreManager.SharedInstance.HavePromotedIAPs()) {
			PopupManager.EnqueuePopup(PopupPromotedIAPs.PATH);
            SetFlag(StateFlag.POPUP_DISPLAYED, true);
        }
    }

	/// <summary>
	/// Checks whether the Pre-Registration Rewards popup must be displayed or not and does it.
	/// </summary>
	private void CheckPreRegRewards() {
		// [AOC] As of version 1.12 (1st update post-WWL), don't give the pre-reg rewards anymore
		return;

	}

	/// <summary>
	/// Checks the animoji tutorial popup.
	/// </summary>
	private void CheckAnimojiTutorial() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Never if animojis not supported in this device
		if(!AnimojiScreenController.IsDeviceSupported()) return;

		// Don't if tutorial is already completed
		if(Prefs.GetBoolPlayer(PopupInfoAnimoji.ANIMOJI_TUTORIAL_KEY, false)) return;

		// Only in the right screen
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

		// Is photo feature available? (FTUX)
		if(!ShareButton.CanBeDisplayed()) return;

        // OTA: Are all the asset bundles downloaded?
        Downloadables.Handle allContentHandle  = HDAddressablesManager.Instance.GetHandleForAllDownloadables();
        if (! allContentHandle.IsAvailable())
        {
            return;
        }

        // All checks passed! Show the popup
        PopupManager.EnqueuePopup(PopupInfoAnimoji.PATH);
		SetFlag(StateFlag.POPUP_DISPLAYED, true);
	}


	/// <summary>
	/// Checks the leagues unlock popup.
	/// </summary>
	private void CheckLeaguesUnlock() {
		// Show it only in the goals section (showing missions by default)
		if(m_currentScreen != MenuScreen.MISSIONS
		&& m_currentScreen != MenuScreen.GLOBAL_EVENTS
		&& m_currentScreen != MenuScreen.LEAGUES) {
			return;
		}

        // Can we show the popup?
        PopupController popup = null;
		if(PopupLeaguesUnlocked.Check()) {
			popup = PopupManager.LoadPopup(PopupLeaguesUnlocked.PATH);
            PopupLeaguesUnlocked leaguesPopup = popup.GetComponent<PopupLeaguesUnlocked>();
            leaguesPopup.Init(m_currentScreen);
			PopupManager.EnqueuePopup(popup);
		}

		// Set flag
		if(popup != null) {
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}


    /// <summary>
	/// Checks the leagues dragons popup.
	/// </summary>
	private void CheckLegendaryDragonsUnlock()
    {

        // Show it only in the dragon selection section
        if (m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

        // Can we show the popup?
        PopupController popup = null;
        if (PopupSpecialDragonsUnlocked.Check())
        {
            popup = PopupManager.LoadPopup(PopupSpecialDragonsUnlocked.PATH);
            PopupSpecialDragonsUnlocked specialsPopup = popup.GetComponent<PopupSpecialDragonsUnlocked>();
            specialsPopup.Init(m_currentScreen);
            PopupManager.EnqueuePopup(popup);
        }

        // Set flag
        if (popup != null)
        {
            SetFlag(StateFlag.POPUP_DISPLAYED, true);
        }
    }

    /// <summary>
    /// Checks the daily rewards popup.
    /// </summary>
    private void CheckDailyRewards() {
		// Never if feature not enabled
		if(!FeatureSettingsManager.IsDailyRewardsEnabled()) return;

		// Only in the right screen
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

		// Never if daily rewards are not yet enabled
		if(!UsersManager.currentUser.HasPlayedGames(GameSettings.ENABLE_DAILY_REWARDS_AT_RUN)) return;

		// If the reward is available show the popup!
		if(UsersManager.currentUser.dailyRewards.CanCollectNextReward()) {
			PopupManager.EnqueuePopup(PopupDailyRewards.PATH);
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	/// <summary>
	/// Check whether we need to trigger any popup related to downloadable assets.
	/// </summary>
	private void CheckDownloadAssets() {
		// Get the assets download flow object from the target screen and use it to check whether a popup must be displayed or not.
		PopupAssetsDownloadFlow downloadPopup = null;
		switch(m_currentScreen) {
			case MenuScreen.DRAGON_SELECTION: {
				MenuDragonScreenController screenController = InstanceManager.menuSceneController.GetScreenData(m_currentScreen).ui.GetComponent<MenuDragonScreenController>();

                if (screenController != null &&
                    screenController.assetsDownloadFlow != null) {

                        // If the target progression point has been reached (usually Sparks), trigger download for ALL
                        if (UsersManager.currentUser.GetHighestDragon().GetOrder() == AssetsDownloadFlowSettings.autoTriggerAfterDragon)
                        {

                            screenController.assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());
                            downloadPopup = screenController.assetsDownloadFlow.OpenPopupIfNeeded(AssetsDownloadFlow.Context.PLAYER_BUYS_TRIGGER_DRAGON);

					    }
                        // The player bought a Medium or bigger tier dragon, trigger download for ALL
                        else if (UsersManager.currentUser.GetHighestDragon().GetOrder() > AssetsDownloadFlowSettings.autoTriggerAfterDragon)
                        {
                            
                            screenController.assetsDownloadFlow.InitWithHandle(HDAddressablesManager.Instance.GetHandleForAllDownloadables());
                            downloadPopup = screenController.assetsDownloadFlow.OpenPopupIfNeeded(AssetsDownloadFlow.Context.PLAYER_BUYS_NOT_DOWNLOADED_DRAGON);
                        }


                        // In case the download is completed, at this point we want to show a popup informing the player
                        float downloadProgress = HDAddressablesManager.Instance.GetHandleForAllDownloadables().Progress;
                        if (downloadProgress >= 1)
                        {
                            // Make sure we need to show the popup (the download popup was accepted at some point)
                            if (Prefs.GetBoolPlayer(AssetsDownloadFlowSettings.OTA_SHOW_DOWNLOAD_COMPLETE_POPUP, false))
                            {
                                // Force to show the popup
                                screenController.assetsDownloadFlow.OpenPopupByState(PopupAssetsDownloadFlow.PopupType.ANY, AssetsDownloadFlow.Context.NOT_SPECIFIED, true);

                            }
                        }
                    }
			} break;
		}

		// Did we open any popup?
		if(downloadPopup != null) {
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	/// <summary>
	/// Checks whether the Golden Fragments Conversion popup must be displayed or not and does it.
	/// </summary>
	private void CheckGoldenFragmentsConversion() {
		// Only in the right screen
		if(m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

		// Can we show the popup?
		PopupController popup = null;
		if(PopupGoldenFragmentConversion.Check()) {
			// Yes! Just do it, no extra initialization needed
			popup = PopupManager.EnqueuePopup(PopupGoldenFragmentConversion.PATH);
		}

		// Set flag
		if(popup != null) {
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

    /// <summary>
    /// Checks if there is a happy hour popup ready to be shown
    /// This function will only show the popup if the happy hour offer was
    /// triggered by buying the gems from the "not enough currency" popup
    /// </summary>
    private void CheckHappyHourOffer()
    {
        // Check if there is a happy hour
        if (OffersManager.instance.happyHour == null)
            return;

        HappyHourOffer happyHour = OffersManager.instance.happyHour;

        // Dont show a popup if the happy hour has already finished
        if (!happyHour.IsActive())
            return;

        // Is there a popup pending to show?
        if (!happyHour.pendingPopup)
            return;

        // All the required runs have been played?
        if (!UsersManager.currentUser.HasPlayedGames(happyHour.triggerPopupAtRun))
            return;

        // Only in the right screen
        if (m_currentScreen != MenuScreen.DRAGON_SELECTION) return;

        // Load the popup
        PopupController popup = PopupManager.LoadPopup(PopupHappyHour.PATH);
        PopupHappyHour popupHappyHour = popup.GetComponent<PopupHappyHour>();

        // Initialize the popup (set the discount %)
        popupHappyHour.Init(happyHour.lastOfferSku);

        // Show the popup
        PopupManager.EnqueuePopup(popup);

		// Set flag
		if(popup != null) {
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	/// <summary>
	/// Do we need to open the offers shop?
	/// </summary>
	private void CheckOffersShop() {
		// [AOC] TODO!!
	}

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// The menu has just switched from one screen to another.
    /// </summary>
    /// <param name="_from">Screen we're coming from.</param>
    /// <param name="_to">Screen we're going to.</param>
    private void OnMenuScreenChanged(MenuScreen _from, MenuScreen _to) {
		// Cache transition data
		//Debug.Log("Transition ended from " + Colors.coral.Tag(_from.ToString()) + " to " + Colors.aqua.Tag(_to.ToString()));
		m_previousScreen = _from;
		m_currentScreen = _to;

		// Don't show anything if a dragon has been unlocked during gameplay!
		// We never want to cover the dragon unlock animation!
		if(GetFlag(StateFlag.NEW_DRAGON_UNLOCKED)) {
			SetFlag(StateFlag.NEW_DRAGON_UNLOCKED, false);
			return;
		}

		// Similarly, don't show anything if we have pending rewards!
		if(UsersManager.currentUser.rewardStack.Count > 0) return;

		// Do we come from playing? (whetever is Classic, Lab or Tournament)
		SetFlag(StateFlag.COMING_FROM_A_RUN, _from == MenuScreen.NONE && _to != MenuScreen.PLAY);

		// Do all checks here by order!
		// Each check function should use m_previousScreen and m_currentScreen to decide
		// Use PopupManager.OpenPopup to force a popup open or PopupManager.EnqueuePopup to put it in a sequence
		// Check other flags (StateFlag) for other state conditions
		CheckPromotedIAPs();
		CheckInterstitialAds();
		//CheckTermsAndConditions();
		CheckGoldenFragmentsConversion();
		CheckCustomizerPopup();
		CheckDailyRewards();
		CheckPreRegRewards();
		CheckShark();
		CheckAnimojiTutorial();
        CheckLegendaryDragonsUnlock();
        CheckLeaguesUnlock();
		CheckRating();
		CheckSurvey();
		CheckSilentNotification();
		CheckOffersShop();
		CheckFeaturedOffer();
		CheckInterstitialCP2();
		CheckDownloadAssets();
        CheckHappyHourOffer();
    }

	/// <summary>
	/// A dragon has been acquired.
	/// </summary>
	/// <param name="_dragonData">The dragon that has been acquired.</param>
	private void OnDragonAcquired(IDragonData _dragonData) {
		// If unlocking the required dragon for the rating popup, mark the popup as it can be displayed
		if(m_ratingDragonData.GetLockState() > IDragonData.LockState.LOCKED) {
			Prefs.SetBoolPlayer(Prefs.RATE_CHECK_DRAGON, true);
		}
	}

	/// <summary>
	/// Server connection check callback.
	/// </summary>
	/// <param name="_error">Error code.</param>
	private void OnConnectionCheck(FGOL.Server.Error _error) {
		if(_error != null) {
			// if there was a connection error show popup
			ShowGoOnlinePopup();
		}
		PlayerPrefs.SetInt(HDNotificationsManager.SILENT_FLAG, 0);
		SetFlag(StateFlag.CHECKING_CONNECTION, false);
	}



	/// <summary>
	/// The interstitial ad has finished.
	/// </summary>
	/// <param name="rewardGiven">Wether a reward was given or not.</param>
	private void OnInterstitialAdEnded(bool rewardGiven) {
		if(rewardGiven) {
			GameAds.instance.ResetIntersitialCounter();
		}
	}
}
