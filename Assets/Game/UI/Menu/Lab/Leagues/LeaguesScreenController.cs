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
	private const float PERIODIC_UPDATE_INTERVAL = 0.25f;  // Seconds

	public enum Panel {
		ERROR,
		ACTIVE_SEASON,
		LOADING,

        COUNT
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private LeaguesScreenPanel[] m_panels = new LeaguesScreenPanel[(int)Panel.COUNT];
	[SerializeField] private ShowHideAnimator m_darkScreen = null;

	// Internal
	private Panel m_activePanel = Panel.ERROR;
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

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
    private void UpdatePeriodic() {
        if (m_season.liveDataState == HDLiveData.State.ERROR) {
            Refresh();
            CancelInvoke();
        } else if (m_season.liveDataState == HDLiveData.State.VALID) {
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
                        } else if (m_activePanel != Panel.ACTIVE_SEASON) {
                            Refresh();
                        }
                    }
                    break;

                case HDSeasonData.State.WAITING_RESULTS: {
                        if (m_season.timeToResuts.TotalSeconds <= 0) {
                            m_season.UpdateState();
                            Refresh();
                        } else if (m_activePanel != Panel.ACTIVE_SEASON) {
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

                            case Panel.ERROR:
                            if (m_season.rewardDataState == HDLiveData.State.WAITING_RESPONSE) {
                                Refresh();
                            }
                            break;

                            default: {
                                    Refresh();
                                }
                                break;
                        }
                    }
                    break;

                case HDSeasonData.State.WAITING_NEW_SEASON: {
                        if (m_season.timeToEnd.TotalSeconds <= 0) {
                            HDLiveDataManager.instance.ForceRequestLeagues();
                            CancelInvoke();
                        } else if (m_activePanel != Panel.ACTIVE_SEASON) {
                            Refresh();
                        }
                    }
                    break;

                case HDSeasonData.State.NONE: {
                        Refresh();  // Keep refreshing until season state is valid
                    }
                    break;
            }
        }
    }


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Select active panel based on current league/season state.
	/// </summary>
	public void Refresh() {
		// Doing this for the cases where the periodic update has been canceled (i.e. WAITING_NEW_SEASON timer ended).
        CancelInvoke();
		InvokeRepeating("UpdatePeriodic", 0f, PERIODIC_UPDATE_INTERVAL);

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
		Panel targetPanel = Panel.LOADING;
        LeaguesPanelError errorPanel = m_panels[(int)Panel.ERROR] as LeaguesPanelError;

		// Check internet connectivity first
		if(Application.internetReachability == NetworkReachability.NotReachable || !GameSessionManager.SharedInstance.IsLogged()) {
			targetPanel = Panel.ERROR;
            errorPanel.SetErrorGroup(LeaguesPanelError.ErrorGroup.NETWORK);
		} else if (m_season.liveDataState == HDLiveData.State.ERROR) {
            targetPanel = Panel.ERROR;
            errorPanel.SetErrorGroup(LeaguesPanelError.ErrorGroup.SEASON);
        } else { 
            // Depends on season state
            switch (m_season.state) {
                case HDSeasonData.State.NONE: {
                        targetPanel = Panel.ERROR; // Shouldn't happen
                        errorPanel.SetErrorGroup(LeaguesPanelError.ErrorGroup.SEASON);
                    }
                    break;

                case HDSeasonData.State.TEASING:
                case HDSeasonData.State.NOT_JOINED:
                case HDSeasonData.State.JOINED:				
                case HDSeasonData.State.WAITING_RESULTS:
				case HDSeasonData.State.REWARDS_COLLECTED: {
                        targetPanel = Panel.ACTIVE_SEASON;
                    }
                    break;

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
                                InstanceManager.menuSceneController.GetScreenData(MenuScreen.LEAGUES_REWARD).ui.GetComponent<LeaguesRewardScreen>().StartFlow();
                                InstanceManager.menuSceneController.GoToScreen(MenuScreen.LEAGUES_REWARD, true);
                            break;
                            case HDLiveData.State.ERROR:
                            targetPanel = Panel.ERROR;
                            errorPanel.SetErrorGroup(LeaguesPanelError.ErrorGroup.REWARDS);
                            break;
                        }
                    }
                    break;

                case HDSeasonData.State.WAITING_NEW_SEASON: {
                        if (m_season.liveDataState == HDLiveData.State.ERROR) {
                            targetPanel = Panel.ERROR;
                            errorPanel.SetErrorGroup(LeaguesPanelError.ErrorGroup.FINALIZE);
                        } else {
                            targetPanel = Panel.ACTIVE_SEASON;
                        }
                    }
                    break;
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
		m_darkScreen.ForceSet(m_panels[(int)_panel].darkBackground, false);

		// If showing the ACTIVE panel for the first time, trigger the tutorial
		if(m_activePanel == Panel.ACTIVE_SEASON && !UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.LEAGUES_INFO)) {
			// Open popup!
			PopupManager.OpenPopupInstant(PopupInfoLeagues.PATH);

			// Mark tutorial step as completed
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.LEAGUES_INFO, true);

			// Tracking!
			string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoLeagues.PATH);
			HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");
		}
	}


	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

    public void OnHidePreAnimation() {
        CancelInvoke();
    }

    /// <summary>
    /// Force a refresh every time we enter the tab!
    /// </summary>
    public void OnShowPreAnimation() {
		// Show loading panel and force a first refresh, both from data and visuals
		SetActivePanel(Panel.LOADING, false);
		m_season.UpdateState();
		Refresh();
	}

    public void RefreshLiveData() {
        SetActivePanel(Panel.LOADING);
        HDLiveDataManager.instance.RequestMyLiveData(true);
        UbiBCN.CoroutineManager.DelayedCall(Refresh, 0.5f);
    }

    public void RefreshSeasonData() {
        SetActivePanel(Panel.LOADING);
        UbiBCN.CoroutineManager.DelayedCall(Refresh, 0.5f);
    }

    /// <summary>
    /// Retry rewards button has been pressed.
    /// </summary>
    public void RetryRewardsButton() {
        m_season.RequestMyRewards();
    }

    /// <summary>
    /// New data has been received.
    /// </summary>
    void OnEventsUpdated() {
		Refresh();
	}	
}