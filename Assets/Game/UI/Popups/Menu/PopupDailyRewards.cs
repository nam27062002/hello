// PopupDailyRewards.cs
// 
// Created by Alger Ortín Castellví on 12/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Daily rewards popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupDailyRewards : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupDailyRewards";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private DailyRewardView[] m_rewardSlots = new DailyRewardView[DailyRewardsSequence.SEQUENCE_SIZE];
	public DailyRewardView[] rewardSlots {
		get { return m_rewardSlots; }
	}

	[Space]
	[SerializeField] private GameObject m_collectButton = null;
	[SerializeField] private GameObject m_doubleButton = null;
	[SerializeField] private GameObject m_dismissButton = null;

	// Internal logic
	private bool m_rewardsFlowPending = false;
	private bool m_rewardCollected = false;	// Prevent collect spamming

	// Cache some data
	private DailyRewardsSequence m_sequence = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Subscribe to external events
		if(FeatureSettingsManager.IsControlPanelEnabled) {
			Broadcaster.AddListener(BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS, this);
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	public void OnDisable() {
		// Unsubscribe from external events
		if(FeatureSettingsManager.IsControlPanelEnabled) {
			Broadcaster.RemoveListener(BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS, this);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with current Daily Rewards Sequence.
	/// </summary>
	/// <param name="_dismissButtonAllowed">Allow dismiss button?</param>
	private void InitWithCurrentData(bool _dismissButtonAllowed) {
		// Aux vars
		m_sequence = UsersManager.currentUser.dailyRewards;
		DailyReward currentReward = m_sequence.GetNextReward();
		bool canCollect = m_sequence.CanCollectNextReward();

		// Initialize rewards
		for(int i = 0; i < m_rewardSlots.Length; ++i) {
			// Skip if slot is not valid
			if(m_rewardSlots[i] == null) continue;

			// Hide slot if reward is not valid
			if(i >= m_sequence.rewards.Length || m_sequence.rewards[i] == null) {
				m_rewardSlots[i].gameObject.SetActive(false);
				continue;
			} else {
				m_rewardSlots[i].gameObject.SetActive(true);
			}

			// Figure out reward state
			DailyReward reward = m_sequence.rewards[i];
			DailyRewardView.State state = DailyRewardView.State.IDLE;
			if(reward.collected) {
				// Reward already collected
				state = DailyRewardView.State.COLLECTED;
			} else if(reward == currentReward) {
				// Current reward! Can it be collected?
				if(canCollect) {
					state = DailyRewardView.State.CURRENT;
				} else {
					state = DailyRewardView.State.COOLDOWN;
				}
			}

			// Initialize reward view!
			m_rewardSlots[i].InitFromData(reward, i, state);
		}

		// Initialize buttons
		m_collectButton.SetActive(canCollect);
		m_doubleButton.SetActive(canCollect && currentReward.canBeDoubled);
		m_dismissButton.SetActive(!canCollect && _dismissButtonAllowed);
	}

	/// <summary>
	/// Collects the next reward, programs the reward flow and closes the popup.
	/// </summary>
	/// <param name="_doubled">Has the reward been doubled?</param>
	private void CollectNextReward(bool _doubled) {
		// Prevent spamming
		if(m_rewardCollected) return;

		// Aux vars
		float closeDelay = 0f;

		// Make sure the reward can be collected
		if(m_sequence.CanCollectNextReward()) {
			// Aux vars
			DailyReward collectedReward = m_sequence.GetNextReward();
			DailyRewardView collectedRewardSlot = m_rewardSlots[m_sequence.rewardIdx];

			// Collect the reward! (This only pushes the reward to the stack and updates its state)
			m_sequence.CollectNextReward(_doubled);

			// Save persistence
			PersistenceFacade.instance.Save_Request();

			// Depending on reward type, either show simple feedback or trigger rewards flow
			switch(collectedReward.reward.type) {
				case Metagame.RewardEgg.TYPE_CODE:
				case Metagame.RewardPet.TYPE_CODE: {
					// Program the reward flow, will be triggered once the popup is closed
					m_rewardsFlowPending = true;
				} break;

				default: {
					// Pop the reward from the reward's stack and collect it
					Metagame.Reward reward = UsersManager.currentUser.rewardStack.Peek();
					reward.Collect();

					// Currency FX
					Transform target = InstanceManager.menuSceneController.hud.scCounter.transform;
					CurrencyTransferFX fx = CurrencyTransferFX.LoadAndLaunch(
						CurrencyTransferFX.GetDefaultPrefabPathForCurrency(reward.currency),
						this.GetComponentInParent<Canvas>().transform,
						collectedRewardSlot.transform.position + new Vector3(0f, 0f, -0.5f),       // Offset Z so the coins don't collide with the UI elements
						target.position + new Vector3(0f, 0f, -0.5f)
					);
					fx.totalDuration = 0.5f;

					// Refresh popup view
					InitWithCurrentData(false);

					// Close the popup after some delay
					closeDelay = fx.totalDuration * 0.5f;	// Give enough time to enjoy the fireworks
				} break;
			}

			// Trigger some nice VFX and SFX, regardless of the reward type
			collectedRewardSlot.LaunchCollectFX();

			// Prevent spamming
			m_rewardCollected = true;
		}

		// Close popup (with optional delay)
		if(closeDelay > 0f) {
			UbiBCN.CoroutineManager.DelayedCall(
				() => { GetComponent<PopupController>().Close(true); }
			, 1f);	
		} else {
			GetComponent<PopupController>().Close(true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Init visuals
		InitWithCurrentData(true);

		// Reset logic
		m_rewardsFlowPending = false;
	}

	/// <summary>
	/// The popup has been closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// If Rewards flow was pending, launch it now
		if(m_rewardsFlowPending) {
			PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
			scr.StartFlow(false);
			InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);
		}
	}

	/// <summary>
	/// The collect button has been pressed.
	/// </summary>
	public void OnCollectButton() {
		// Prevent spamming
		if(m_rewardCollected) return;

		// Launch the rewards flow, no doubling
		CollectNextReward(false);
	}

	/// <summary>
	/// The double button has been pressed.
	/// </summary>
	public void OnDoubleButton() {
		// Prevent spamming
		if(m_rewardCollected) return;

		// Trigger rewarded ad
		PopupAdBlocker.Launch(true, GameAds.EAdPurpose.DAILY_REWARD_DOUBLE, OnAdRewardCallback);
	}

	/// <summary>
	/// The dismiss button has been pressed.
	/// </summary>
	public void OnDismissButton() {
		// Just close the poopup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// Android back button has been pressed.
	/// </summary>
	public void OnBackButton() {
		// Simulate different behaviours depending on sequence state
		if(m_sequence.CanCollectNextReward()) {
			OnCollectButton();
		} else {
			OnDismissButton();
		}
	}

	/// <summary>
	/// A rewarded ad has finished.
	/// </summary>
	/// <param name="_success">Has the ad been successfully played?</param>
	public void OnAdRewardCallback(bool _success) {
		if(_success) {
			// Success!
			// Launch the rewards flow, double reward
			CollectNextReward(true);
		}
	}

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize with debug setup.
	/// </summary>
	/// <param name="_rewardIdx">Current reward index.</param>
	/// <param name="_canCollect">Can the reward be collected?</param>
	/// <param name="_canDouble">Can the reward be doubled? Will be ignored if _canCollect is false.</param>
	public void DEBUG_Init(int _rewardIdx, bool _canCollect, bool _canDouble) {
		// Rewards
		for(int i = 0; i < m_rewardSlots.Length; ++i) {
			// Skip if slot is not valid
			if(m_rewardSlots[i] == null) continue;

			// Figure out reward state
			DailyRewardView.State state = DailyRewardView.State.IDLE;
			if(i < _rewardIdx) {
				// Reward already collected
				state = DailyRewardView.State.COLLECTED;
			} else if(i == _rewardIdx) {
				// Current reward! Can it be collected?
				if(_canCollect) {
					state = DailyRewardView.State.CURRENT;
				} else {
					state = DailyRewardView.State.COOLDOWN;
				}
			}

			// Initialize reward view!
			m_rewardSlots[i].DEBUG_Init(state, i);
		}

		// Buttons
		m_collectButton.SetActive(_canCollect);
		m_doubleButton.SetActive(_canCollect && _canDouble);
		m_dismissButton.SetActive(!_canCollect);
	}

	/// <summary>
	/// A global event has been sent.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Broadcast event info.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			case BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS: {
				// Just reinitialize
				InitWithCurrentData(true);
			} break;
		}
	}
}