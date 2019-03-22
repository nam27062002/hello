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
public class MenuInterstitialPopupsController : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string RATING_DRAGON = "dragon_crocodile";

	// Custom flags altering the standard flow
	[System.Flags]
	private enum StateFlag {
		NONE = 1 << 0,
		NEW_DRAGON_UNLOCKED = 1 << 1,
		POPUP_DISPLAYED = 1 << 2,
		WAIT_FOR_CUSTOM_POPUP = 1 << 3,
		CHECKING_CONNECTION = 1 << 4,
		COMING_FROM_A_RUN = 1 << 5
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private StateFlag m_stateFlags = StateFlag.NONE;
	private float m_waitTimeOut;

	private PopupController m_currentPopup = null;

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
		Broadcaster.AddListener(BroadcastEventType.POPUP_CLOSED, this);

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
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_CLOSED, this);
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
	private void SetFlag(StateFlag _flag, bool _value) {
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

		// Is the last accepted version the same as the current one?
		if(PlayerPrefs.GetInt(PopupConsentLoading.VERSION_PREFS_KEY) != PopupConsentLoading.LEGAL_VERSION) {
			Debug.Log("<color=RED>LEGAL</color>");
			m_currentPopup = PopupManager.OpenPopupInstant(PopupConsentLoading.PATH);
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	private void CheckCustomizerPopup() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			SetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP, HDCustomizerManager.instance.IsCustomiserPopupAvailable());
			if (GetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP)) {
				m_waitTimeOut = 5f;
				BusyScreen.Show(this, false);

				string langServerCode = "en";
				DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, LocalizationManager.SharedInstance.GetCurrentLanguageSKU());
				if(langDef != null) {
					langServerCode = langDef.GetAsString("serverCode", langServerCode);
				}
				Calety.Customiser.CustomiserPopupConfig popupConfig = HDCustomizerManager.instance.GetOrRequestCustomiserPopup(langServerCode);
				if (popupConfig != null) {
					OpenCustomizerPopup(popupConfig);
				}
			}
		}
	}

    private void CheckShark()
    {
		// Don't show if a more important popup has already been displayed in this menu loop
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		// Minimum amount of runs must be completed
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_SHARK_PET_REWARD_POPUP_AT_RUN) return;

		string sharkPetSku = PopupSharkPetReward.PET_SKU;
        if (!UsersManager.currentUser.petCollection.IsPetUnlocked(sharkPetSku))
        {
            // Check if hungry shark is installed
            if (IsHungrySharkGameInstalled())
            {
                // Unlock pet
                UsersManager.currentUser.petCollection.UnlockPet(sharkPetSku);

                // Show popup
				PopupController popup = PopupManager.OpenPopupInstant(PopupSharkPetReward.PATH);
				SetFlag(StateFlag.POPUP_DISPLAYED, true);
            }
        }
    }

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


	private void OpenCustomizerPopup(Calety.Customiser.CustomiserPopupConfig _config) {
		string popupPath = PopupCustomizer.PATH + "PF_PopupLayout_" + _config.m_iLayout;

		PopupController pController = PopupManager.OpenPopupInstant(popupPath);
		PopupCustomizer pCustomizer = pController.GetComponent<PopupCustomizer>();
		pCustomizer.InitFromConfig(_config);

		SetFlag(StateFlag.WAIT_FOR_CUSTOM_POPUP, false);
		SetFlag(StateFlag.POPUP_DISPLAYED, true);
		m_currentPopup = pController;

		BusyScreen.Hide(this, true);
	}

    /// <summary>
    /// Checks the interstitial ads.
    /// </summary>
    private void CheckInterstitialAds() {
        if ( GameAds.instance.IsValidUserForInterstitials() ) {
            StartCoroutine( LaunchInterstitial() );
        }
    }

    IEnumerator LaunchInterstitial() {
		SetFlag(StateFlag.POPUP_DISPLAYED, true);
        yield return new WaitForSeconds(0.25f);
        PopupAdBlocker.Launch(false, GameAds.EAdPurpose.INTERSTITIAL, InterstitialCallback);
    }

    private void InterstitialCallback( bool rewardGiven )
    {
        if ( rewardGiven )
        {
            GameAds.instance.ResetIntersitialCounter();
        }
    }

    private void CheckInterstitialCP2() {
        // CP2 interstitial has the lowest priority so if the user has already seen a popup or an ad then cp2 interstitial shouldn't be shown
		if (GetFlag(StateFlag.POPUP_DISPLAYED)) return;

        bool checkUserRestriction = true;
        if (HDCP2Manager.Instance.CanPlayInterstitial(checkUserRestriction)) {
            HDCP2Manager.Instance.PlayInterstitial(checkUserRestriction);
        }
    }

    /// <summary>
    /// Checks whether the Rating popup must be opened or not and does it.
    /// </summary>
    private void CheckRating() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
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
								m_currentPopup = PopupManager.OpenPopupInstant(PopupAskLikeGame.PATH);
								SetFlag(StateFlag.POPUP_DISPLAYED, true);
							} else if(Application.platform == RuntimePlatform.IPhonePlayer) {
								m_currentPopup = PopupManager.OpenPopupInstant(PopupAskRateUs.PATH);
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
	}
	private void CheckSilentNotification() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED))
			return;

		if(PlayerPrefs.GetInt(HDNotificationsManager.SILENT_FLAG) == 1 && !GetFlag(StateFlag.CHECKING_CONNECTION)) 
		{
			if(Application.internetReachability == NetworkReachability.NotReachable) {
				ShowGoOnlinePopup();
				PlayerPrefs.SetInt(HDNotificationsManager.SILENT_FLAG, 0);
			} else {
				SetFlag(StateFlag.CHECKING_CONNECTION, true);
				GameServerManager.SharedInstance.CheckConnection( ConnectionCallback );
			}
		}
	}

	private void ConnectionCallback( FGOL.Server.Error _error) {
		if(_error != null) {
			// if there was a connection error show popup
			ShowGoOnlinePopup();			
		}
		PlayerPrefs.SetInt(HDNotificationsManager.SILENT_FLAG, 0);
		SetFlag(StateFlag.CHECKING_CONNECTION, false);
	}

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
	}

	/// <summary>
	/// Checks whether the Survey popup must be opened or not and does it.
	/// </summary>
	private void CheckSurvey() {
		// Ignore if a popup has already been displayed in this iteration
		if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;

		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			m_currentPopup = PopupAskSurvey.Check();
			if(m_currentPopup != null) {
				SetFlag(StateFlag.POPUP_DISPLAYED, true);
			}
		}
	}

	/// <summary>
	/// Checks whether the Featured 	 popup must be opened or not and does it.
	/// </summary>
	/// <param name="_whereToShow">Where are we attempting to show the popup?</param>
	private void CheckFeaturedOffer(OfferPack.WhereToShow _whereToShow) {
		// Ignore if a popup has already been displayed in this iteration
		//if(GetFlag(StateFlag.POPUP_DISPLAYED)) return;
		// Don't do it if a popup is currently open
		if(m_currentPopup != null) return;

		// Minimum amount of runs must be completed
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_OFFERS_POPUPS_AT_RUN) return;

		if(OffersManager.featuredOffer != null) {
			m_currentPopup = OffersManager.featuredOffer.ShowPopupIfPossible(_whereToShow);
			if(m_currentPopup != null) {
				SetFlag(StateFlag.POPUP_DISPLAYED, true);
			}
		}
	}

    private void CheckPromotedIAPs() {
        if (GameStoreManager.SharedInstance.HavePromotedIAPs()) {
			m_currentPopup = PopupManager.OpenPopupInstant(PopupPromotedIAPs.PATH);
            SetFlag(StateFlag.POPUP_DISPLAYED, true);
        }
    }

	/// <summary>
	/// Checks whether the Pre-Registration Rewards popup must be displayed or not and does it.
	/// </summary>
	private void CheckPreRegRewards() {
		// [AOC] As of version 1.12 (1st update post-WWL), don't give the pre-reg rewards anymore
		return;

		// Ignore if it has already been triggered
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.PRE_REG_REWARDS)) return;

		// Minimum amount of runs must be completed
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_PRE_REG_REWARDS_POPUP_AT_RUN) return;

		// Just launch the popup
		m_currentPopup = PopupManager.OpenPopupInstant(PopupPreRegRewards.PATH);
		SetFlag(StateFlag.POPUP_DISPLAYED, true);
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

		// Is photo feature available? (FTUX)
		ShowOnTutorialStep photoTutorialTrigger = InstanceManager.menuSceneController.hud.photoButton.GetComponentsInParent<ShowOnTutorialStep>(true)[0];	// [AOC] GetComponentInParent<T>() doesn't include disabled objects (and the parent object can actually be inactive triggered by the same ShowOnTutorialStep component we're looking for xD), so we're forced to use GetComponentsInParent<T>(bool includeInactive)[0] instead.
		if(photoTutorialTrigger != null) {
			if(!photoTutorialTrigger.Check()) return;
		}

		// All checks passed! Show the popup
		m_currentPopup = PopupManager.OpenPopupInstant(PopupInfoAnimoji.PATH);
		SetFlag(StateFlag.POPUP_DISPLAYED, true);
	}

	/// <summary>
	/// Checks the lab unlock popup.
	/// </summary>
	private void CheckLabUnlock() {
		// Don't display if another popup is currently open
		if(m_currentPopup != null) return;

		// Only if coming from a run or from the Play screen
		if(m_previousScreen != MenuScreen.NONE && m_previousScreen != MenuScreen.PLAY) return;

		// Can we show the popup?
		if(PopupLabUnlocked.Check()) {
			m_currentPopup = PopupManager.LoadPopup(PopupLabUnlocked.PATH);
			PopupLabUnlocked labPopup = m_currentPopup.GetComponent<PopupLabUnlocked>();
			labPopup.Init(m_currentScreen);
			PopupManager.EnqueuePopup(m_currentPopup);
		}

		// Set flag
		if(m_currentPopup != null) {
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	/// <summary>
	/// Checks the daily rewards popup.
	/// </summary>
	private void CheckDailyRewards() {
		// Don't display if another popup was opened
		if(m_currentPopup != null) return;

		// Never if feature not enabled
		if(!FeatureSettingsManager.IsDailyRewardsEnabled()) return;

		// Never if daily rewards are not yet enabled
		if(!UsersManager.currentUser.HasPlayedGames(GameSettings.ENABLE_DAILY_REWARDS_AT_RUN)) return;

		// If the reward is available show the popup!
		bool showPopup = false;
		if(UsersManager.currentUser.dailyRewards.CanCollectNextReward()) {
			showPopup = true;
		}

		// Show it?
		if(showPopup) {
			m_currentPopup = PopupManager.OpenPopupInstant(PopupDailyRewards.PATH);
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
	}

	/// <summary>
	/// Check whether we need to trigger any popup related to downloadable assets.
	/// </summary>
	private void CheckDownloadAssets() {
		// Not if another popup is open
		if(m_currentPopup != null) return;

		// Check for current screen
		PopupAssetsDownloadFlow downloadPopup = null;
		switch(m_currentScreen) {
			case MenuScreen.DRAGON_SELECTION: {
				MenuDragonScreenController screenController = InstanceManager.menuSceneController.GetScreenData(m_currentScreen).ui.GetComponent<MenuDragonScreenController>();
				if(screenController != null) {
					downloadPopup = screenController.assetsDownloadFlow.OpenPopupIfNeeded();
				}
			} break;

			case MenuScreen.LAB_DRAGON_SELECTION: {
				// TODO!!
			} break;

			case MenuScreen.TOURNAMENT_DRAGON_SETUP: {
				// TODO!!
			} break;
		}

		// Did we open any popup?
		if(downloadPopup != null) {
			m_currentPopup = downloadPopup.GetComponent<PopupController>();
			SetFlag(StateFlag.POPUP_DISPLAYED, true);
		}
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

		// Do we come from playing? (whetever is Classic, Lab or Tournament)
		SetFlag(StateFlag.COMING_FROM_A_RUN, _from == MenuScreen.NONE && _to != MenuScreen.PLAY);

		// If coming from a run, regardles of the destination screen
		if(GetFlag(StateFlag.COMING_FROM_A_RUN)) {
			// Interstitials
            CheckInterstitialAds();
        }

		// Depending on target screen
		switch(_to) {
			case MenuScreen.PLAY: {
                CheckPromotedIAPs();
				//CheckTermsAndConditions();
				CheckCustomizerPopup();
			} break;

		    case MenuScreen.DRAGON_SELECTION: {
				// Coming from any screen (high priority)
				CheckDailyRewards();
				CheckPreRegRewards();
				CheckShark();
				CheckAnimojiTutorial();

				// Coming from specific screens
				switch(_from) {
					// Coming from game
					case MenuScreen.NONE: {
						CheckLabUnlock();
						CheckRating();
						CheckSurvey();
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION_AFTER_RUN);
                        CheckInterstitialCP2();
					} break;

					// Coming from PLAY screen
					case MenuScreen.PLAY: {
						CheckLabUnlock();
						CheckSilentNotification();
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION);
					} break;
				}

                CheckPromotedIAPs();
				// Coming from any screen (low priority)
				// Nothing for now
			} break;
            case MenuScreen.TOURNAMENT_INFO:
            {
                CheckPromotedIAPs();
            }break;
		}

		// Download Assets: Always in any screen (screen will already be checked in the function)
		CheckDownloadAssets();
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
	/// A popup has been closed.
	/// </summary>
	/// <param name="_popup">Popup.</param>
	private void OnPopupClosed(PopupController _popup) {
		// Is it our current popup?
		if(_popup == m_currentPopup) {
			// Yes! Nullify current popup reference
			m_currentPopup = null;

			// Check if we need to open any other popup
			switch(m_currentScreen) {
				case MenuScreen.PLAY: {
					// Add any checks here
					CheckFeaturedOffer(OfferPack.WhereToShow.PLAY_SCREEN);
				} break;

				case MenuScreen.DRAGON_SELECTION: {
					// Always (high prio)
					CheckLabUnlock();

					// Coming from a run?
					if(GetFlag(StateFlag.COMING_FROM_A_RUN)) {
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION_AFTER_RUN);
					} else {
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION);
					}
				} break;
			}

			// Download Assets Popups: Always in any screen (screen will already be checked in the function)
			CheckDownloadAssets();
		}
	}

	/// <summary>
	/// A global event has been sent.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Broadcast event info.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			case BroadcastEventType.POPUP_CLOSED: {
				PopupManagementInfo info = (PopupManagementInfo)_broadcastEventInfo;
				OnPopupClosed(info.popupController);
			} break;
		}
	}
}
