using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TournamentInfoScreen : MonoBehaviour {
	//----------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 1f;	// Seconds

	//----------------------------------------------------------------//

	[SerializeField] private TextMeshProUGUI m_timerText;

	[SeparatorAttribute("Goal")]
	[SerializeField] private Localizer m_goalText;
	[SerializeField] private Image m_goalIcon;

	[SeparatorAttribute("Modifiers")]
	[SerializeField] private ModifierIcon[] m_modifier;

	[SeparatorAttribute("Location")]
	[SerializeField] private GameObject m_mapContainer;
	[SerializeField] private TextMeshProUGUI m_areaText;
	[SerializeField] private Image m_areaIcon;

	[SeparatorAttribute("Leaderboard")]
	[SerializeField] private TournamentLeaderboardView m_leaderboard;

	[SeparatorAttribute("Rewards")]
	[SerializeField] private GameObject m_rewardsRoot = null;
	[SerializeField] private Transform m_rewardsContainer = null;
	[SerializeField] private GameObject m_rewardPrefab = null;


	//----------------------------------------------------------------//
	private HDTournamentManager m_tournament;
	private HDTournamentDefinition m_definition;
	private bool m_waitingRewardsData = false;


	//----------------------------------------------------------------//


	//TEMP
	void Refresh() {
		m_tournament = HDLiveEventsManager.instance.m_tournament;
		m_definition = m_tournament.data.definition as HDTournamentDefinition;

		if (m_definition != null) {
			//GOALS
			m_goalText.Localize(m_definition.m_goal.m_desc);
			m_goalIcon.sprite = Resources.Load<Sprite>(UIConstants.LIVE_EVENTS_ICONS_PATH + m_definition.m_goal.m_icon);

			//MODIFIERS
			List<IModifierDefinition> mods = new List<IModifierDefinition>();
			for (int i = 0; i < m_definition.m_dragonMods.Count; ++i) {
				mods.Add(m_definition.m_dragonMods[i]);
			}

			for (int i = 0; i < m_definition.m_otherMods.Count; ++i) {
				mods.Add(m_definition.m_otherMods[i]);
			}

			for (int i = 0; i < m_modifier.Length; ++i) {
				if (i < mods.Count) {
					m_modifier[i].InitFromDefinition(mods[i]);
				} else {
					m_modifier[i].gameObject.SetActive(false);
				}
			}

			//MAP
			m_mapContainer.SetActive(!string.IsNullOrEmpty(m_definition.m_goal.m_area));
			m_areaText.text = m_definition.m_goal.m_area;

			//LEADERBOARD
			if (m_tournament.data.m_state <= HDLiveEventData.State.NOT_JOINED) {
				m_leaderboard.gameObject.SetActive(false);
			} else {
				m_leaderboard.gameObject.SetActive(true);
			}

			//REWARDS
			if(m_tournament.data.m_state == HDLiveEventData.State.NOT_JOINED) {
				m_rewardsRoot.SetActive(true);

				// Clear any existing reward view
				m_rewardsContainer.DestroyAllChildren(false);

				// Instantiate and initialize rewards views
				for(int i = 0; i < m_definition.m_rewards.Count; ++i) {
					GameObject newInstance = Instantiate<GameObject>(m_rewardPrefab, m_rewardsContainer, false);
					TournamentRewardView view = newInstance.GetComponent<TournamentRewardView>();
					view.InitFromReward(m_definition.m_rewards[i]);
				}
			} else {
				m_rewardsRoot.SetActive(false);
			}

			//TIMER
			UpdatePeriodic();
		}
	}

	// Update timers periodically
	void UpdatePeriodic() {
		if (m_definition != null && !m_definition.m_refund) {
			double seconds = m_definition.timeToEnd.TotalSeconds;
			if (seconds <= 0f) {
				seconds = 0f;

				if (!m_waitingRewardsData) {
					Messenger.AddListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);

					// Request rewards data and wait for it to be loaded
					m_tournament.RequestRewards();

					// Show busy screen
					BusyScreen.Setup(true, LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_LOADING"));
					BusyScreen.Show(this);

					m_waitingRewardsData = true;
					CancelInvoke();
				}
			}

			m_timerText.text = LocalizationManager.SharedInstance.Localize(
				"TID_TOURNAMENT_ICON_ENDS_IN", 
				TimeUtils.FormatTime(
					System.Math.Max(0, seconds), // Just in case, never go negative
					TimeUtils.EFormat.ABBREVIATIONS,
					4
				)
			);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The next screen button has been pressed.
	/// </summary>
	public void OnNextButton() {
		// Send Tracking event
		HDTrackingManager.Instance.Notify_TournamentClickOnNextOnDetailsScreen(m_definition.m_name);

		// [AOC] TODO!! Select fixed or flexible build screen!
		InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_DRAGON_SETUP);
	}

	/// <summary>
	/// Force a refresh every time we enter the tab!
	/// </summary>
	public void OnShowPreAnimation() {
		Refresh();

		m_waitingRewardsData = false;

		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);
	}

	public void OnHidePreAnimation() {
		Messenger.RemoveListener<int, HDLiveEventsManager.ComunicationErrorCodes>(MessengerEvents.LIVE_EVENT_REWARDS_RECEIVED, OnRewardsResponse);
	}

	/// <summary>
	/// We got a response on the rewards request.
	/// </summary>
	private void OnRewardsResponse(int _eventId, HDLiveEventsManager.ComunicationErrorCodes _errorCode) {
		// Ignore if we weren't waiting for rewards!
		if(!m_waitingRewardsData) return;
		m_waitingRewardsData = false;

		// Hide busy screen
		BusyScreen.Hide(this);

		// Success?
		if(_errorCode == HDLiveEventsManager.ComunicationErrorCodes.NO_ERROR) {
			// Go to tournament rewards screen!
			TournamentRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.TOURNAMENT_REWARD).ui.GetComponent<TournamentRewardScreen>();
			scr.StartFlow();
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.TOURNAMENT_REWARD);
		} else {
			// Show error message
			UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_TOURNAMENT_REWARDS_ERROR"),
				new Vector2(0.5f, 0.33f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
			text.text.color = UIConstants.ERROR_MESSAGE_COLOR;
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.PLAY);
		}
	}
}
