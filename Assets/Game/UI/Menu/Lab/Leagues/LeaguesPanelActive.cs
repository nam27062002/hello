// LeaguesPanelActive.cs
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
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel for the leagues screen.
/// </summary>
public class LeaguesPanelActive : LeaguesScreenPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float PERIODIC_UPDATE_INTERVAL = 1f;	// Seconds
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private bool m_updateLeagueState = false;
	[Space]
	[SerializeField] private Image m_leagueIcon = null;
	[Space]
	[SerializeField] private LeagueSelector m_leagueSelector = null;
	[SerializeField] private Localizer m_leagueNameText = null;
	[SerializeField] private ShowHideAnimator m_currentLeagueMarker = null;
	[Space]
	[SerializeField] private GameObject m_rewardsRoot = null;
	[SerializeField] private Transform m_rewardsContainer = null;
	[SerializeField] private GameObject m_rewardPrefab = null;

	// Internal references
	private HDSeasonData m_season = null;
	private List<AnimatedRankedRewardView> m_rewardViews = new List<AnimatedRankedRewardView>();
    private bool m_refreshRewardView = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Init internal references
		m_season = HDLiveDataManager.league.season;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Initialize league selector
		if(m_leagueSelector != null) {
			// Init list of leagues to scroll around
			HDLeagueController leaguesManager = HDLiveDataManager.league;
			List<LeagueSelectorItem> scrollItems = new List<LeagueSelectorItem>(leaguesManager.leaguesCount);
			for(int i = 0; i < leaguesManager.leaguesCount; ++i) {
				// Create a new selectable item
				LeagueSelectorItem item = new LeagueSelectorItem(leaguesManager.GetLeagueData(i));
				scrollItems.Add(item);
			}

			// Reverse order so we go from worst to best league			
			m_leagueSelector.Init(scrollItems);

			// Setup Refresh callback
			m_leagueSelector.OnSelectionChanged.AddListener(OnSelectedLeagueChanged);
			m_leagueSelector.enableEvents = true;

			// Select current player's league
			m_leagueSelector.SelectItem(m_season.currentLeague);
		}

		// Program periodic update call
		InvokeRepeating("UpdatePeriodic", 0f, PERIODIC_UPDATE_INTERVAL);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Leave current league as the selected one
		m_leagueSelector.SelectItem(m_season.currentLeague);

		// Clear selector's Refresh setup
		m_leagueSelector.enableEvents = false;
		m_leagueSelector.OnSelectionChanged.RemoveListener(OnSelectedLeagueChanged);

		// Clear periodic update call
		CancelInvoke();
	}

	/// <summary>
	/// Called periodically.
	/// </summary>
	private void UpdatePeriodic() {
		// Just in case
		if(!m_season.IsRunning()) return;

		// If enabled, update league state
		if(m_updateLeagueState) {
			// Update season state if time is up
			double remainingTime = System.Math.Max(0, HDLiveDataManager.league.season.timeToClose.TotalSeconds);
			if(remainingTime <= 0) {
				m_season.UpdateState();
			}

			// If season is over, notify state change
			if(!m_season.IsRunning()) {
				Messenger.Broadcast(MessengerEvents.LIVE_EVENT_STATES_UPDATED);
			}
		}

        if (m_refreshRewardView) {
            HDLeagueData leagueData = m_leagueSelector.selectedItem.leagueData;
            if (leagueData.liveDataState == HDLiveData.State.VALID) {
                RefreshRewardsInfo(leagueData);
            }
        }
    }

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh displayed data.
	/// </summary>
	override public void Refresh() {
		// Nothing to do if we don't have a valid season
		if(!m_season.IsRunning()) return;

		// Initialize visuals
		// Player's league icon
		if(m_leagueIcon != null) {
			m_leagueIcon.sprite = Resources.Load<Sprite>(UIConstants.LEAGUE_ICONS_PATH + m_season.currentLeague.icon);
		}

		// Currently selected league info
		RefreshSelectedLeagueInfo();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the info of the selected league.
	/// </summary>
	private void RefreshSelectedLeagueInfo() {
		// [AOC] TODO!! Some nice animations / FX

		// Aux vars
		HDLeagueData leagueData = m_leagueSelector.selectedItem.leagueData;
		Debug.Assert(leagueData != null, "LEAGUE DATA IS NOT VALID!!");

		// 3D Trophy Preview
		LabLeaguesScene sceneController = InstanceManager.menuSceneController.GetScreenData(MenuScreen.LAB_LEAGUES).scene3d as LabLeaguesScene;
		if(sceneController != null) {
			sceneController.LoadTrophy(leagueData);
		}

		// League Name
		if(m_leagueNameText != null) {
			m_leagueNameText.Localize(leagueData.tidName);
		}

        // Current league marker
        if (m_currentLeagueMarker != null) {
            m_currentLeagueMarker.Set(leagueData == m_season.currentLeague);
        }

        if (leagueData.liveDataState == HDLiveData.State.VALID) {
            RefreshRewardsInfo(leagueData);
        } else {
            for (int i = 0; i < m_rewardViews.Count; ++i) {
                m_rewardViews[i].gameObject.SetActive(false);
            }
            m_refreshRewardView = true;
        }
    }

    private void RefreshRewardsInfo(HDLeagueData _leagueData) {
        // Rewards
        if (m_rewardsContainer != null) {
            // Reuse existing reward views when possible
            AnimatedRankedRewardView rewardView = null;
            for (int i = 0; i < _leagueData.rewards.Count; ++i) {
                // Reuse a view if possible, otherwise instantiate a new one
                if (i < m_rewardViews.Count) {
                    rewardView = m_rewardViews[i];
                } else {
                    GameObject newInstance = Instantiate<GameObject>(m_rewardPrefab, m_rewardsContainer, false);
                    rewardView = newInstance.GetComponent<AnimatedRankedRewardView>();
                    m_rewardViews.Add(rewardView);
                }

                if (_leagueData.liveDataState == HDLiveData.State.VALID) {
                    // Initialize with reward data
                    rewardView.InitFromReward(_leagueData.rewards[i]);

                    // Make sure it's active
                    rewardView.gameObject.SetActive(true);

                    // Add a short delay to the animation (so rewards appear sequentially) and restart it
                    rewardView.animator.tweenDelay = 0.1f * i;
                    rewardView.animator.RestartShow();
                } else {
                    rewardView.gameObject.SetActive(false);
                }
            }

            // Hide non-used views
            for (int i = _leagueData.rewards.Count; i < m_rewardViews.Count; ++i) {
                m_rewardViews[i].gameObject.SetActive(false);
            }
        }

        m_refreshRewardView = false;
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// We have new data concerning the event.
    /// </summary>
    /// <param name="_requestType">Request type.</param>
    private void OnEventDataUpdated() {
		// Nothing to do if disabled
		if(!isActiveAndEnabled) return;

		Refresh();
	}

	/// <summary>
	/// Selected league in the selector has changed.
	/// </summary>
	/// <param name="_oldItem"></param>
	private void OnSelectedLeagueChanged(LeagueSelectorItem _oldItem, LeagueSelectorItem _newItem) {
		// Refresh info
		RefreshSelectedLeagueInfo();
	}
}