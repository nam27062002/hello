// LeaguesScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// General controller for the Leagues screen.
/// </summary>
public class LeaguesScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Panel {
		OFFLINE,
		WAITING_NEW_SEASON,
		ACTIVE_SEASON,
		LOADING,
		REWARDS_RETRY,		// [AOC] Check if needed!
        REWARDS_READY,

        COUNT
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private LeaguesScreenPanel[] m_panels = new LeaguesScreenPanel[(int)Panel.COUNT];
	[SerializeField] private ShowHideAnimator m_darkScreen = null;

	// Internal
	private Panel m_activePanel = Panel.OFFLINE;
	private HDLeagueController m_league = null;
	private HDSeasonData m_season = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Make sure we have all the required panels
		// Shouldn't happen, the custom editor makes sure everyhting is ok
		Debug.Assert(m_panels.Length == (int)Panel.COUNT, "Unexpected amount of defined panels");

		// Init internal references
		m_league = HDLiveDataManager.league;
		m_season = m_league.season;

		// Init panels
		for(int i = 0; i < m_panels.Length; i++) {
			m_panels[i].panelId = (Panel)i;
			m_panels[i].anim = m_panels[i].GetComponent<ShowHideAnimator>();
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
        Messenger.AddListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventsUpdated);
    }

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
        Messenger.RemoveListener(MessengerEvents.LIVE_EVENT_STATES_UPDATED, OnEventsUpdated);
        CancelInvoke();
    }

    void UpdatePeriodic() {
        switch (m_season.state) {
            case HDSeasonData.State.TEASING: {
                if (m_season.timeToStart.TotalSeconds <= 0) {
                    m_season.UpdateState();
                    Refresh();
                }
            }
            break;

            case HDSeasonData.State.NOT_JOINED:
            case HDSeasonData.State.JOINED: {
                if (m_season.timeToClose.TotalSeconds <= 0) {
                    m_season.UpdateState();
                    Refresh();
                }
            }
            break;

            case HDSeasonData.State.PENDING_REWARDS: {
                switch (m_activePanel) {
                    case Panel.LOADING:
                    if (m_season.rewardDataState == HDLiveData.State.VALID || m_season.rewardDataState == HDLiveData.State.ERROR) {
                        Refresh();
                    }
                    break;

                    case Panel.REWARDS_RETRY:
                    if (m_season.rewardDataState == HDLiveData.State.WAITING_RESPONSE) {
                        Refresh();
                    }
                    break;

                    default:
                        Refresh();
                        break;
                }
            }
            break;

            case HDSeasonData.State.WAITING_NEW_SEASON:
                if (m_season.timeToEnd.TotalSeconds <= 0) {
                    HDLiveDataManager.instance.ForceRequestLeagues();
                    CancelInvoke();
                }
            break;
        }
    }


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Select active panel based on current league/season state.
	/// </summary>
	public void Refresh() {
        CancelInvoke();
        InvokeRepeating("UpdatePeriodic", 0f, 0.25f);

        // Select active panel
        SelectPanel();

		// Refresh its content
		m_panels[(int)m_activePanel].Refresh();
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Based on current event state, select which panel should be active.
	/// </summary>
	private void SelectPanel() {
		// Select active panel based on season/league state
		Panel targetPanel = Panel.WAITING_NEW_SEASON;

		// Check internet connectivity first
		if(Application.internetReachability == NetworkReachability.NotReachable || !GameSessionManager.SharedInstance.IsLogged()) {
			targetPanel = Panel.OFFLINE;
		} else {
			// Depends on season state
			switch(m_season.state) {
				case HDSeasonData.State.JOINED:
				case HDSeasonData.State.NOT_JOINED: {
					targetPanel = Panel.ACTIVE_SEASON;
				} break;

				case HDSeasonData.State.REWARDS_COLLECTED:
				case HDSeasonData.State.WAITING_NEW_SEASON: {
					targetPanel = Panel.WAITING_NEW_SEASON;
				} break;

				case HDSeasonData.State.PENDING_REWARDS: {
                    switch (m_season.rewardDataState) {
                        case HDLiveData.State.EMPTY:
                            m_season.RequestMyRewards();
                            targetPanel = Panel.LOADING;
                        break;
                        case HDLiveData.State.WAITING_RESPONSE:
                            targetPanel = Panel.LOADING;
                        break;
                        case HDLiveData.State.VALID:
                            targetPanel = Panel.REWARDS_READY;
                        break;
                        case HDLiveData.State.ERROR:
                            targetPanel = Panel.REWARDS_RETRY;
                        break;
                    }
                }
                break;

				case HDSeasonData.State.NONE: {
					targetPanel = Panel.OFFLINE; // Shouldn't happen
				} break;
			}
		}

		// Toggle active panel
		SetActivePanel(targetPanel);
	}

	/// <summary>
	/// Set the given panel as active one.
	/// </summary>
	/// <param name="_panel">Panel to be set as active.</param>
	/// <param name="_animate">Trigger animations?</param>
	private void SetActivePanel(Panel _panel, bool _animate = true) {
		// Store active panel
		m_activePanel = _panel;

		// Toggle active panel
		for(int i = 0; i < m_panels.Length; ++i) {
			// Use animators if available
			bool show = (i == (int)m_activePanel);
			if(m_panels[i].anim != null) {
				m_panels[i].anim.Set(show, _animate);
			} else {
				m_panels[i].gameObject.SetActive(show);
			}
		}

		// Toggle dark background
		m_darkScreen.Set(m_panels[(int)_panel].darkBackground, _animate);

		// If showing the ACTIVE panel for the first time, trigger the tutorial
		// [AOC] TODO!!
		/*
		if(m_activePanel == Panel.EVENT_ACTIVE && !UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.QUEST_INFO)) {
			// Open popup!
			string popupName = "PF_PopupInfoGlobalEvents";
			PopupManager.OpenPopupInstant("UI/Popups/Tutorial/" + popupName);

			// Mark tutorial step as completed
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.QUEST_INFO, true);

			// Tracking!
			HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");
		}
		*/
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a refresh every time we enter the tab!
	/// </summary>
	public void OnShowPreAnimation() {
		// Show loading panel and force a first refresh, both from data and visuals
		SetActivePanel(Panel.LOADING, false);
		m_season.UpdateState();
		Refresh();
	}

	/// <summary>
	/// The retry button on the offline panel has been pressed.
	/// </summary>
	public void OnOfflineRetryButton() {
		if(Application.internetReachability != NetworkReachability.NotReachable && GameSessionManager.SharedInstance.IsLogged()) {
			// Show loading and ask for my live data
			SetActivePanel(Panel.LOADING);
			if(!HDLiveDataManager.instance.RequestMyLiveData()) {
				UbiBCN.CoroutineManager.DelayedCall(Refresh, 0.5f);
			}
		} else {
			// Message no connection
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_NO_CONNECTION"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// New data has been received.
	/// </summary>
	void OnEventsUpdated() {
		Refresh();
	}

	/// <summary>
	/// Retry rewards button has been pressed.
	/// </summary>
	public void OnRetryRewardsButton() {
        m_season.RequestMyRewards();
	}

    public void OnCollectRewardsButton() {
        //Go to leagues Reward Screen
        UsersManager.currentUser.PushReward(m_season.reward);
        m_season.RequestFinalize();
        Refresh();
    }
}