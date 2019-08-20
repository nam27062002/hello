// LeaguesPanelRetryRewards.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel corresponding to leagues requiring a retry on the rewards.
/// </summary>
public class LeaguesPanelError : LeaguesScreenPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
    public enum ErrorGroup {
        NETWORK = 0,
        SEASON,
        REWARDS,
        FINALIZE
    }


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed References
	[SerializeField] private Localizer m_titleText = null;
    [SerializeField] private Localizer m_messageText = null;
	[Space]
    [SerializeField] private LeaguesScreenController leaguesScreenController = null;


    private HDSeasonData m_season = null;
    private ErrorGroup m_errorGroup;
    private HDLiveDataManager.ComunicationErrorCodes m_errorCode;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize the panel with a specific error code.
    /// </summary>
    /// <param name="_group">Error group.</param>
    public void SetErrorGroup(ErrorGroup _group) {
        m_errorGroup = _group;
        m_season = HDLiveDataManager.league.season;
        m_errorCode = HDLiveDataManager.ComunicationErrorCodes.NO_ERROR;

        switch (_group) {
            case ErrorGroup.NETWORK: {
                    m_titleText.Localize("TID_LEAGUES_OFFLINE_TITLE");  // Sorry! You are offline!
                    m_messageText.Localize("TID_LEAGUES_OFFLINE_MESSAGE");  // You must be online to see and participate in the Legendary Leagues!
                }
                break;

            case ErrorGroup.SEASON: {
                    m_errorCode = m_season.liveDataError;
                    m_titleText.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR");    // Something went wrong!
                    m_messageText.Localize("TID_REWARD_AMOUNT", HDLiveDataManager.ErrorCodeEnumToInt(m_errorCode).ToString(), string.Empty);
                }
                break;

            case ErrorGroup.REWARDS: {
                    m_errorCode = m_season.rewardDataError;
                    m_titleText.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR");    // Something went wrong!
                    m_messageText.Localize("TID_REWARD_AMOUNT", HDLiveDataManager.ErrorCodeEnumToInt(m_errorCode).ToString(), string.Empty);
                }
                break;

            case ErrorGroup.FINALIZE: {
                    m_errorCode = m_season.finalizeDataError;
                    m_titleText.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR");    // Something went wrong!
                    m_messageText.Localize("TID_REWARD_AMOUNT", HDLiveDataManager.ErrorCodeEnumToInt(m_errorCode).ToString(), string.Empty);
                }
                break;
        }

        if (m_errorCode == HDLiveDataManager.ComunicationErrorCodes.NET_ERROR) {
            m_titleText.Localize("TID_LEAGUES_OFFLINE_TITLE");  // Sorry! You are offline!
            m_messageText.Localize("TID_NO_RESPONSE");  // You must be online to see and participate in the Legendary Leagues!
        }
    }


    //------------------------------------------------------------------------//
    // CALLBACK METHODS                                                       //
    //------------------------------------------------------------------------//
    public void OnRetryButton() {
        if (m_errorGroup == ErrorGroup.NETWORK || m_errorCode == HDLiveDataManager.ComunicationErrorCodes.NET_ERROR) {
            if (DeviceUtilsManager.SharedInstance.internetReachability != NetworkReachability.NotReachable) { 
                leaguesScreenController.RefreshLiveData();
            } else { // Message no connection
                UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
            }
        } else {
            switch (m_errorCode) {
                case HDLiveDataManager.ComunicationErrorCodes.OTHER_ERROR:
                case HDLiveDataManager.ComunicationErrorCodes.LDATA_NOT_FOUND:
                case HDLiveDataManager.ComunicationErrorCodes.SEASON_NOT_FOUND: {
                        leaguesScreenController.RefreshLiveData();
                    }
                    break;

                case HDLiveDataManager.ComunicationErrorCodes.NO_ERROR:
                case HDLiveDataManager.ComunicationErrorCodes.USER_IS_NOT_PENDING_REWARDS:
                case HDLiveDataManager.ComunicationErrorCodes.LEAGUEDEF_NOT_FOUND:
                case HDLiveDataManager.ComunicationErrorCodes.USER_LEAGUE_NOT_FOUND:
                case HDLiveDataManager.ComunicationErrorCodes.SEASON_IS_NOT_ACTIVE: {
                        leaguesScreenController.RefreshLiveData();
                    }
                    break;

                default: {
                        switch (m_errorGroup) {
                            case ErrorGroup.SEASON:
                            break;

                            case ErrorGroup.REWARDS:
                            leaguesScreenController.RetryRewardsButton();
                            break;

                            case ErrorGroup.FINALIZE:
                            break;
                        }
                    }
                    break;
            }
        }
    }
}