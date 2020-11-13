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
	[SerializeField] protected DailyRewardView[] m_rewardSlots = new DailyRewardView[DailyRewardsSequence.SEQUENCE_SIZE];
	public DailyRewardView[] rewardSlots {
		get { return m_rewardSlots; }
	}

	[Space]
	[SerializeField] protected GameObject m_collectButton = null;
	[SerializeField] protected GameObject m_doubleAdButton = null;
	[SerializeField] protected GameObject m_dismissButton = null;
    [SerializeField] protected GameObject m_doubleButton = null;

    [Space]
	[SerializeField] protected ShowHideAnimator m_currencyCounterAnim = null;
	[SerializeField] protected ProfileCurrencyCounter m_currencyCounter = null;
	[SerializeField] protected Transform m_currencyFXAnchor = null;

	// Internal logic
    protected bool m_rewardsFlowPending = false;
    protected bool m_rewardCollected = false; // Prevent collect spamming

	// Internal references
    protected ParticlesTrailFX m_currencyFX = null;

	// Cache some data
    protected DailyRewardsSequence m_sequence = null;

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
	protected virtual void InitWithCurrentData(bool _dismissButtonAllowed) {
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

        // Does the player has the Remove ads feature?
        bool removeAds = UsersManager.currentUser.removeAds.IsActive;

		// Is this the seventh day?
		bool finalRewardDay = ( m_sequence.rewardIdx == DailyRewardsSequence.SEQUENCE_SIZE - 1 );

		// Initialize buttons
		m_collectButton.SetActive(canCollect && ( !removeAds || finalRewardDay ) );
		m_doubleAdButton.SetActive(canCollect && currentReward.canBeDoubled && !removeAds );
		m_dismissButton.SetActive(!canCollect && _dismissButtonAllowed);
        m_doubleButton.SetActive(canCollect && currentReward.canBeDoubled && removeAds && !finalRewardDay);
    }

	/// <summary>
	/// Collects the next reward, programs the reward flow and closes the popup.
	/// </summary>
	/// <param name="_multiplier">The multiplier applied to the reward</param>
	protected void CollectNextReward(float _multiplier = 1) {
		// Prevent spamming
		if(m_rewardCollected) return;

		// Aux vars
		float closeDelay = 0f;

		// Make sure the reward can be collected
		if(m_sequence.CanCollectNextReward()) {
			// Aux vars
			int rewardIdx = m_sequence.rewardIdx;
			Metagame.Reward collectedReward = m_sequence.GetNextReward().reward;
			DailyRewardView collectedRewardSlot = m_rewardSlots[rewardIdx];

			// Collect the reward! (This only pushes the reward to the stack and updates its state)
			m_sequence.CollectNextReward(_multiplier);

			// Save persistence
			PersistenceFacade.instance.Save_Request();

			// Depending on reward type, either show simple feedback or trigger rewards flow
			switch(collectedReward.type) {
				case Metagame.RewardDragon.TYPE_CODE:
				case Metagame.RewardEgg.TYPE_CODE:
				case Metagame.RewardPet.TYPE_CODE: {
					// Program the reward flow, will be triggered once the popup is closed
					m_rewardsFlowPending = true;

                    // Clear the queue of pending popups so they dont interrupt the flow
                    PopupManager.ClearQueue();

				} break;

				default: {
					// Set the right currency in the counter and show it
					m_currencyCounter.SetCurrency(collectedReward.currency);

					// Show currency group
					m_currencyCounterAnim.ForceShow();

					// Currency FX
					// Make it fly to the matching currency counter
					Vector3 fromWorldPos = collectedRewardSlot.transform.position;
					Vector3 toWorldPos = m_currencyFXAnchor.transform.position;

					// Offset Z a bit so the coins don't collide with the UI elements
					// [AOC] We're assuming that UI canvases (both main and popup) are at Z0
					fromWorldPos.z = -0.5f;
					toWorldPos.z = -0.5f;

					// Ready!
					m_currencyFX = ParticlesTrailFX.LoadAndLaunch(
						ParticlesTrailFX.GetDefaultPrefabPathForCurrency(collectedReward.currency),
						this.GetComponentInParent<Canvas>().transform,
						fromWorldPos,
						toWorldPos
					);
					m_currencyFX.totalDuration = 0.3f;

					// Refresh popup view
					// If it's the last reward of the sequence, trigger nice animation for the newly generated sequence
					InitWithCurrentData(false);
					if(rewardIdx == DailyRewardsSequence.SEQUENCE_SIZE - 1) {
						for(int i = 0; i < m_rewardSlots.Length; ++i) {
							m_rewardSlots[i].GetComponent<ShowHideAnimator>().RestartShow();
						}
					}

					// Close the popup after some delay
					//closeDelay = m_currencyFX.totalDuration * 1.5f;	// Give enough time to enjoy the fireworks (if we don't, the popups canvas will be delayed and the FX will disappear :s)
					closeDelay = 1.5f;

					// Collect the reward!
					collectedReward.Collect();
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
				() => {
					GetComponent<PopupController>().Close(true);
					m_currencyCounterAnim.Hide();
				}, closeDelay);	
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

		// Hide background currency counters
		m_currencyCounterAnim.ForceHide(false);
		Messenger.Broadcast<bool>(MessengerEvents.UI_TOGGLE_CURRENCY_COUNTERS, false);
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

		// If we have a currency FX running, move it to the main Canvas so it doesn't disappear
		if(m_currencyFX != null) {
			m_currencyFX.transform.SetParent(InstanceManager.menuSceneController.GetUICanvasGO().transform, false);
			m_currencyFX = null;
		}

		// Restore background currency counters
		Messenger.Broadcast<bool>(MessengerEvents.UI_TOGGLE_CURRENCY_COUNTERS, true);
	}

	/// <summary>
	/// The collect button has been pressed.
	/// </summary>
	public void OnCollectButton() {
		// Prevent spamming
		if(m_rewardCollected) return;

		// Launch the rewards flow, no multiplier
		CollectNextReward();
	}

	/// <summary>
	/// The ad button has been pressed.
	/// </summary>
	public void OnAdButton() {
		// Prevent spamming
		if(m_rewardCollected) return;

		// Trigger rewarded ad
		PopupAdBlocker.LaunchAd(true, GameAds.EAdPurpose.DAILY_REWARD_DOUBLE, OnAdRewardCallback);
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
	public virtual void OnAdRewardCallback(bool _success) {
		if(_success) {
			// Success!
			// Launch the rewards flow, double reward
			CollectNextReward(2f);
		}
	}

    /// <summary>
    /// The player collects the double reward by using the Remove Ads feature
    /// </summary>
    public virtual void OnDoubleButton ()
    {
        // Launch the rewards flow, double reward
        CollectNextReward(2f);
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
		m_doubleAdButton.SetActive(_canCollect && _canDouble);
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