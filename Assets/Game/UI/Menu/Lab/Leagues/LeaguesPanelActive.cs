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
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Panel for the leagues screen.
/// Will be displayed when the season is in the following states:
/// - Joined
/// - Not Joined
/// </summary>
public class LeaguesPanelActive : LeaguesScreenPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float PERIODIC_UPDATE_INTERVAL = 1f;	// Seconds

	[Flags]
	public enum Mode {
		JOINED = 1 << 1,
		NOT_JOINED = 1 << 2,
		TEASING = 1 << 3,
		WAITING_RESULTS = 1 << 4
	}

	[Serializable]
	public class ModeObject {
		public GameObject target = null;
		[EnumMask] public Mode showModes = (Mode)0;
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private ModeObject[] m_modeObjects = null;

	[Separator("JOINED")]
	[SerializeField] private Image m_joinedLeagueIcon = null;
	[SerializeField] private Localizer m_joinedLeagueNameText = null;

	[Separator("NOT JOINED")]
	[SerializeField] private Image m_notJoinedLeagueIcon = null;
	[SerializeField] private Localizer m_notJoinedLeagueNameText = null;

	[Separator("TEASING / WAITING")]
	[SerializeField] private Localizer m_teasingTitleText = null;
	[SerializeField] private Image m_teasingLeagueIcon = null;
	[SerializeField] private Localizer m_teasingLeagueNameText = null;
	[SerializeField] private TextMeshProUGUI m_teasingTimerText = null;
	[SerializeField] private Image m_teasingTimerBar = null;
	[SerializeField] private Range m_teasingTimeBarAngleRange = new Range(0f, 360f);

	[Separator("League Selector")]
	[SerializeField] private LeagueSelector m_leagueSelector = null;
	[SerializeField] private Localizer m_leagueSelectorNameText = null;
	[SerializeField] private ShowHideAnimator m_currentLeagueMarker = null;

	[Separator("Rewards")]
	[SerializeField] private Transform m_rewardsContainer = null;
	[SerializeField] private GameObject m_rewardPrefab = null;

	// Internal references
	private HDSeasonData m_season = null;
	private List<AnimatedRankedRewardView> m_rewardViews = new List<AnimatedRankedRewardView>();

	// Internal logic
	private Mode m_mode = Mode.JOINED;
	private bool m_refreshRewardView = false;

	// Internal properties
	private HDLeagueData defaultLeague {
		get {
			if(m_season == null) return null;

			switch(m_season.state) {
				case HDSeasonData.State.WAITING_NEW_SEASON: return m_season.nextLeague;
				default: return m_season.currentLeague;
			}
		}
	}

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
			m_leagueSelector.Init(scrollItems);

			// Setup Refresh callback
			m_leagueSelector.OnSelectionChanged.AddListener(OnSelectedLeagueChanged);
			m_leagueSelector.enableEvents = true;

			// Select current player's league
			m_leagueSelector.SelectItem(defaultLeague);
		}

		// Program periodic update call
		InvokeRepeating("UpdatePeriodic", 0f, PERIODIC_UPDATE_INTERVAL);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Leave current league as the selected one
		m_leagueSelector.SelectItem(defaultLeague);

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
		// Rewads refresh pending?
        if (m_refreshRewardView) {
            HDLeagueData leagueData = m_leagueSelector.selectedItem.leagueData;
            if (leagueData.liveDataState == HDLiveData.State.VALID) {
                RefreshRewardsInfo(leagueData);
            }
        }

		// Refresh teasing countdown timer
		if (m_mode == Mode.TEASING || m_mode == Mode.WAITING_RESULTS) {
			double remainingSeconds = 0;
			double durationSeconds = 1;	// To avoid division by 0
			if (m_season.state == HDSeasonData.State.TEASING) {
				remainingSeconds = m_season.timeToStart.TotalSeconds;
				durationSeconds = m_season.durationTeasing.TotalSeconds;
			} else if (m_season.state == HDSeasonData.State.WAITING_RESULTS) {
                remainingSeconds = m_season.timeToResuts.TotalSeconds;
                durationSeconds = m_season.durationWaitResults.TotalSeconds;
            } else {
				remainingSeconds = m_season.timeToEnd.TotalSeconds;
				durationSeconds = m_season.durationWaitNewSeason.TotalSeconds;
			}

			m_teasingTimerText.text = TimeUtils.FormatTime(
				System.Math.Max(0, remainingSeconds),	// Don't go below 0
				TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES,
				4
			);

			// Refresh progress var value
			if(m_teasingTimerBar != null) {
				// Interpolate progress between min and max angles
				float progress = 1f - (float)(remainingSeconds / durationSeconds);
				float targetAngle = m_teasingTimeBarAngleRange.Lerp(progress);
				float fillAmount = Mathf.InverseLerp(0f, 360f, targetAngle);
				m_teasingTimerBar.fillAmount = fillAmount;
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
		// Don't listen to selector events, since we will be manually refreshing the league info
		m_leagueSelector.enableEvents = false;

		// Select mode based on season state
		switch(m_season.state) {
			case HDSeasonData.State.JOINED: {
				m_mode = Mode.JOINED;
			} break;

			case HDSeasonData.State.NOT_JOINED: {
				m_mode = Mode.NOT_JOINED;
			} break;

			case HDSeasonData.State.WAITING_NEW_SEASON:
			case HDSeasonData.State.TEASING: {
				m_mode = Mode.TEASING;
			} break;

			case HDSeasonData.State.WAITING_RESULTS: {
				m_mode = Mode.WAITING_RESULTS;
			} break;
		}

		// Select info to display based on mode
		for(int i = 0; i < m_modeObjects.Length; ++i) {
			// Using Flags enum to quickly check whether this object should be displayed or not for the current mode
			m_modeObjects[i].target.SetActive(
				(m_modeObjects[i].showModes & m_mode) != 0
			);
		}

		// Refresh visuals based on active mode
		switch(m_mode) {
			case Mode.JOINED: {
				m_joinedLeagueIcon.sprite = Resources.Load<Sprite>(UIConstants.LEAGUE_ICONS_PATH + defaultLeague.icon);
				m_joinedLeagueNameText.Localize(defaultLeague.tidName);
			} break;

			case Mode.NOT_JOINED: {
				m_notJoinedLeagueIcon.sprite = Resources.Load<Sprite>(UIConstants.LEAGUE_ICONS_PATH + defaultLeague.icon);
				m_notJoinedLeagueNameText.Localize(defaultLeague.tidName);
			} break;

			case Mode.TEASING: {
				m_teasingTitleText.Localize("TID_LEAGUES_TEASING_TITLE");
				m_teasingLeagueIcon.sprite = Resources.Load<Sprite>(UIConstants.LEAGUE_ICONS_PATH + defaultLeague.icon);
				m_teasingLeagueNameText.Localize(defaultLeague.tidName);
				m_leagueSelector.SelectItem(defaultLeague); // This should update 3D trophy
			} break;

			case Mode.WAITING_RESULTS: {
				m_teasingTitleText.Localize("TID_LEAGUES_REWARDS_PROCESSING");
				m_teasingLeagueIcon.sprite = Resources.Load<Sprite>(UIConstants.LEAGUE_ICONS_PATH + defaultLeague.icon);
				m_leagueSelector.SelectItem(defaultLeague); // This should update 3D trophy and show the right rewards
			} break;
		}

		// Currently selected league info
		RefreshSelectedLeagueInfo();

		// Restore selector events
		m_leagueSelector.enableEvents = true;
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
		// Make sure 3D scene is still loaded!
		if(InstanceManager.menuSceneController != null) {
			ScreenData leaguesScreenData = InstanceManager.menuSceneController.GetScreenData(MenuScreen.LEAGUES);
			if(leaguesScreenData != null) {
				LabLeaguesScene sceneController = leaguesScreenData.scene3d as LabLeaguesScene;
				if(sceneController != null) {
					sceneController.LoadTrophy(leagueData);
				}
			}
		}

		// League Name
		if(m_leagueSelectorNameText != null) {
			m_leagueSelectorNameText.Localize(leagueData.tidName);
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

	/// <summary>
	/// 
	/// </summary>
	/// <param name="_leagueData">League data.</param>
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
	/// Selected league in the selector has changed.
	/// </summary>
	/// <param name="_oldItem"></param>
	private void OnSelectedLeagueChanged(LeagueSelectorItem _oldItem, LeagueSelectorItem _newItem) {
		// Refresh info
		RefreshSelectedLeagueInfo();
	}
}