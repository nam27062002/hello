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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private bool m_popupDisplayed = false;
	private bool m_waitForCustomPopup = false;
	private float m_waitTimeOut;

	private PopupController m_currentPopup = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Register to external events
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
		Messenger.AddListener<PopupController>(MessengerEvents.POPUP_CLOSED, OnPopupClosed);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unregister from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
		Messenger.RemoveListener<PopupController>(MessengerEvents.POPUP_CLOSED, OnPopupClosed);
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
		if(PlayerPrefs.GetInt(PopupTermsAndConditions.VERSION_PREFS_KEY) != PopupTermsAndConditions.LEGAL_VERSION) {
			Debug.Log("<color=RED>LEGAL</color>");
			m_currentPopup = PopupManager.OpenPopupInstant(PopupTermsAndConditions.PATH);
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

				CustomizerManager.CustomiserPopupConfig popupConfig = HDCustomizerManager.instance.GetOrRequestCustomiserPopup(LocalizationManager.SharedInstance.Culture.TwoLetterISOLanguageName);
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
	/// Checks whether the Rating popup must be opened or not and does it.
	/// </summary>
	private void CheckRating() {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

		if (UsersManager.currentUser.gamesPlayed >= GameSettings.ENABLE_INTERSTITIAL_POPUPS_AT_RUN) {
			// Is dragon unlocked?
			DragonData data = DragonManager.GetDragonData(RATING_DRAGON);
			if(data.GetLockState() > DragonData.LockState.LOCKED) {
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
	/// Checks whether the Featured Offer popup must be opened or not and does it.
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
		// Ignore if it has already been triggered
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.PRE_REG_REWARDS)) return;

		// Minimum amount of runs must be completed
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_PRE_REG_REWARDS_POPUP_AT_RUN) return;

		// Just launch the popup
		m_currentPopup = PopupManager.OpenPopupInstant(PopupPreRegRewards.PATH);
		m_popupDisplayed = true;
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

		switch(_to) {
			case MenuScreen.PLAY: {
                CheckPromotedIAPs();
				//CheckTermsAndConditions();
				CheckCustomizerPopup();
			} break;

		case MenuScreen.DRAGON_SELECTION: {
				// Coming from any screen (high priority)
				CheckPreRegRewards();
				CheckShark();

				// Coming from specific screens
				switch(_from) {
					// Coming from game
					case MenuScreen.NONE: {
						CheckRating();
						CheckSurvey();
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION_AFTER_RUN);
					} break;

					// Coming from PLAY screen
					case MenuScreen.PLAY: {
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION);
					} break;
				}

				// Coming from any screen (low priority)
				// Nothing for now
			} break;
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