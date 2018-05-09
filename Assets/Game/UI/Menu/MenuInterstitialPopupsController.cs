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
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Register to external events
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unregister from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnMenuScreenChanged);
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
		if(PlayerPrefs.GetInt(PopupTermsAndConditions.KEY) != PopupTermsAndConditions.LEGAL_VERSION) {
			Debug.Log("<color=RED>LEGAL</color>");
			PopupManager.OpenPopupInstant(PopupTermsAndConditions.PATH);
			HDTrackingManager.Instance.Notify_Calety_Funnel_Load(FunnelData_Load.Steps._03_terms_and_conditions);
			m_popupDisplayed = true;
		}
	}

	private void CheckCusotmizerPopup() {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

		CustomizerManager.CustomiserPopupConfig popupConfig = HDCustomizerManager.instance.GetCustomiserPopup(LocalizationManager.SharedInstance.Culture.TwoLetterISOLanguageName);
		if(popupConfig != null) {
			string popupPath = PopupCustomizer.PATH + "PF_PopupLayout" + popupConfig.m_iLayout;

			PopupController pController = PopupManager.OpenPopupInstant(popupPath);
			PopupCustomizer pCustomizer = pController.GetComponent<PopupCustomizer>();
			pCustomizer.InitFromConfig(popupConfig);

			m_popupDisplayed = true;
		}
	}

	/// <summary>
	/// Checks whether the Rating popup must be opened or not and does it.
	/// </summary>
	private void CheckRating() {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

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
							PopupManager.OpenPopupInstant(PopupAskLikeGame.PATH);
							m_popupDisplayed = true;
						} else if(Application.platform == RuntimePlatform.IPhonePlayer) {
							PopupManager.OpenPopupInstant(PopupAskRateUs.PATH);
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

	/// <summary>
	/// Checks whether the Survey popup must be opened or not and does it.
	/// </summary>
	private void CheckSurvey() {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;
		m_popupDisplayed = PopupAskSurvey.Check();
	}

	/// <summary>
	/// Checks whether the Featured Offer popup must be opened or not and does it.
	/// </summary>
	/// <param name="_whereToShow">Where are we attempting to show the popup?</param>
	private void CheckFeaturedOffer(OfferPack.WhereToShow _whereToShow) {
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;

		if(OffersManager.featuredOffer != null) {
			m_popupDisplayed = OffersManager.featuredOffer.ShowPopupIfPossible(_whereToShow);
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
		// Ignore if a popup has already been displayed in this iteration
		if(m_popupDisplayed) return;
		//Debug.Log("Transition ended from " + Colors.coral.Tag(_from.ToString()) + " to " + Colors.aqua.Tag(_to.ToString()));

		switch(_to) {
			case MenuScreen.PLAY: {
				// 1. Terms and Conditions
				CheckTermsAndConditions();

				CheckCusotmizerPopup();
			} break;

		case MenuScreen.DRAGON_SELECTION: {
				CheckCusotmizerPopup();

				switch(_from) {
					// Coming from game
					case MenuScreen.NONE: {
						// 1. Rating
						CheckRating();

						// 2. Survey
						CheckSurvey();

						// 3. Featured Offer
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION_AFTER_RUN);
					} break;

					// Coming from PLAY screen
					case MenuScreen.PLAY: {
						// 1. Featured Offer
						CheckFeaturedOffer(OfferPack.WhereToShow.DRAGON_SELECTION);
					} break;
				}
			} break;
		}
	}
}