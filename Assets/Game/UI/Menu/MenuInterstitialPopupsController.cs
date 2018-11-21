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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private bool m_popupDisplayed = false;
    private bool m_adDisplayed = false;
    private bool m_waitForCustomPopup = false;
	private float m_waitTimeOut;

	private PopupController m_currentPopup = null;
	private bool m_checkingConnection = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Register to external events
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
		Broadcaster.AddListener(BroadcastEventType.POPUP_CLOSED, this);
		m_checkingConnection = false;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unregister from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_CLOSED, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.POPUP_CLOSED:
            {
                PopupManagementInfo info = (PopupManagementInfo)broadcastEventInfo;
                OnPopupClosed(info.popupController);
            }break;
        }
    }
    

	private void Update() {
		if (m_waitForCustomPopup) {
			if (!m_popupDisplayed) {
				CustomizerManager.CustomiserPopupConfig popupConfig = HDCustomizerManager.instance.GetLastPreparedPopupConfig();
				if (popupConfig != null) {
					OpenCustomizerPopup(popupConfig);
				} else {
					m_waitTimeOut -= Time.deltaTime;
					if (m_waitTimeOut <= 0f) {
						BusyScreen.Hide(this, true);
						m_waitForCustomPopup = false;
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Checks whether the Terms and Conditions popup must be opened or not and does it.
	/// </summary>
	private void CheckTermsAndConditions() {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

		// Is the last accepted version the same as the current one?
		if(PlayerPrefs.GetInt(PopupConsentLoading.VERSION_PREFS_KEY) != PopupConsentLoading.LEGAL_VERSION) {
			Debug.Log("<color=RED>LEGAL</color>");
			m_currentPopup = PopupManager.OpenPopupInstant(PopupConsentLoading.PATH);
			m_popupDisplayed = true;
		}
	}

	private void CheckCustomizerPopup() {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			m_waitForCustomPopup = HDCustomizerManager.instance.IsCustomiserPopupAvailable();
			if (m_waitForCustomPopup) {
				m_waitTimeOut = 5f;
				BusyScreen.Show(this, false);

				string langServerCode = "en";
				DefinitionNode langDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LOCALIZATION, LocalizationManager.SharedInstance.GetCurrentLanguageSKU());
				if(langDef != null) {
					langServerCode = langDef.GetAsString("serverCode", langServerCode);
				}
				CustomizerManager.CustomiserPopupConfig popupConfig = HDCustomizerManager.instance.GetOrRequestCustomiserPopup(langServerCode);
				if (popupConfig != null) {
					OpenCustomizerPopup(popupConfig);
				}
			}
		}
	}

    private void CheckShark()
    {
		// Don't show if a more important popup has already been displayed in this menu loop
		if(m_popupDisplayed) return;

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
                m_popupDisplayed = true;
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


	private void OpenCustomizerPopup(CustomizerManager.CustomiserPopupConfig _config) {
		string popupPath = PopupCustomizer.PATH + "PF_PopupLayout_" + _config.m_iLayout;

		PopupController pController = PopupManager.OpenPopupInstant(popupPath);
		PopupCustomizer pCustomizer = pController.GetComponent<PopupCustomizer>();
		pCustomizer.InitFromConfig(_config);

		m_waitForCustomPopup = false;
		m_popupDisplayed = true;
		m_currentPopup = pController;

		BusyScreen.Hide(this, true);
	}

    /// <summary>
    /// Checks the interstitial ads.
    /// </summary>
    private void CheckInterstitialAds() {
        if ( FeatureSettingsManager.AreAdsEnabled && GameAds.instance.IsValidUserForInterstitials() ) {
            if ( GameAds.instance.GetRunsToInterstitial() <= 0 ) {
                // Lets be loading friendly
                StartCoroutine( LaunchInterstitial() );
            } else {
                GameAds.instance.ReduceRunsToInterstitial();
            }
        }
    }

    IEnumerator LaunchInterstitial() {
        m_adDisplayed = true;
        yield return new WaitForSeconds(0.25f);
        PopupAdBlocker.Launch(false, GameAds.EAdPurpose.INTERSTITIAL, InterstitialCallback);
    }

    private void InterstitialCallback( bool rewardGiven )
    {
        if ( rewardGiven ) {
            GameAds.instance.ResetRunsToInterstitial();
        }
    }

    private void CheckInterstitialCP2() {
        // CP2 interstitial has the lowest priority so if the user has already seen a popup or an ad then cp2 interstitial shouldn't be shown
        if (m_popupDisplayed || m_adDisplayed) return;

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
		if(m_popupDisplayed) return;

		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			// Is dragon unlocked?
			IDragonData data = DragonManager.GetDragonData(RATING_DRAGON);
			if(data.GetLockState() > IDragonData.LockState.LOCKED) {
				// Don't show the popup the very first time to prevent conflict with the dragon unlock animation
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
								m_popupDisplayed = true;
							} else if(Application.platform == RuntimePlatform.IPhonePlayer) {
								m_currentPopup = PopupManager.OpenPopupInstant(PopupAskRateUs.PATH);
								m_popupDisplayed = true;
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
		if(m_popupDisplayed)
			return;

		if(PlayerPrefs.GetInt(HDNotificationsManager.SILENT_FLAG) == 1 && !m_checkingConnection) 
		{
			if(Application.internetReachability == NetworkReachability.NotReachable) {
				ShowGoOnlinePopup();
				PlayerPrefs.SetInt(HDNotificationsManager.SILENT_FLAG, 0);
			} else {
				m_checkingConnection = true;
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
		m_checkingConnection = false;
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
		if(m_popupDisplayed) return;

		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			m_currentPopup = PopupAskSurvey.Check();
			m_popupDisplayed = m_currentPopup != null;
		}
	}

	/// <summary>
	/// Checks whether the Featured 	 popup must be opened or not and does it.
	/// </summary>
	/// <param name="_whereToShow">Where are we attempting to show the popup?</param>
	private void CheckFeaturedOffer(OfferPack.WhereToShow _whereToShow) {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

		// Minimum amount of runs must be completed
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_OFFERS_POPUPS_AT_RUN) return;

		if(OffersManager.featuredOffer != null) {
			m_currentPopup = OffersManager.featuredOffer.ShowPopupIfPossible(_whereToShow);
			m_popupDisplayed = m_currentPopup != null;
		}
	}

    private void CheckPromotedIAPs() {
        if (GameStoreManager.SharedInstance.HavePromotedIAPs()) {
			m_currentPopup = PopupManager.OpenPopupInstant(PopupPromotedIAPs.PATH);
            m_popupDisplayed = true;
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
		m_popupDisplayed = true;
	}

	/// <summary>
	/// Checks the animoji tutorial popup.
	/// </summary>
	private void CheckAnimojiTutorial() {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

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
		m_popupDisplayed = true;
	}

	/// <summary>
	/// Checks the lab unlock popup.
	/// </summary>
	private void CheckLabUnlock() {
		// Because the lab popup is triggered by the Menu Dragon Screen Controller, we won't be opening it, just checking whether we can open another popup or not.
		if(PopupLabUnlocked.Check() || PopupManager.GetOpenPopup(PopupLabUnlocked.PATH) != null) {
			m_popupDisplayed = true;	// This will prevent other popups to trigger
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
		//Debug.Log("Transition ended from " + Colors.coral.Tag(_from.ToString()) + " to " + Colors.aqua.Tag(_to.ToString()));

        // if we come from playing whetever is Classic, Lab or Tournament
        if ( _from == MenuScreen.NONE && _to != MenuScreen.PLAY ) {
            CheckInterstitialAds();
        }

		switch(_to) {
			case MenuScreen.PLAY: {
                CheckPromotedIAPs();
				//CheckTermsAndConditions();
				CheckCustomizerPopup();
			} break;

		    case MenuScreen.DRAGON_SELECTION: {
				// Coming from any screen (high priority)
				CheckLabUnlock();
				CheckPreRegRewards();
				CheckShark();
				CheckAnimojiTutorial();

				// Coming from specific screens
				switch(_from) {
					// Coming from game
					case MenuScreen.NONE: {
						CheckRating();
						CheckSurvey();
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION_AFTER_RUN);
                        CheckInterstitialCP2();
					} break;

					// Coming from PLAY screen
					case MenuScreen.PLAY: {
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
		}
	}
}
