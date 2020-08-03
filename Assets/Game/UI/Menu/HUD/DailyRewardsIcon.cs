// DailyRewardsIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the daily rewards icon in the menu HUD.
/// </summary>
public class DailyRewardsIcon : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const float UPDATE_FREQUENCY = 60f;  // Seconds. Because this feature doesn't require a constant update (no timer to show), optimize by setting a large interval.

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_rootAnim = null;
	[SerializeField] private UINotification m_notification = null;

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	protected void OnEnable() {
		// Program a periodic update
		InvokeRepeating("UpdatePeriodic", 0f, UPDATE_FREQUENCY);

		// Should we show ourselves?
		RefreshVisibility(false);

		// Show notification?
		RefreshNotification();

		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.POPUP_CLOSED, this);
		if(FeatureSettingsManager.IsControlPanelEnabled) {
			Broadcaster.AddListener(BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS, this);
		}
    }

	/// <summary>
	/// Called periodically - to avoid doing stuff every frame.
	/// </summary>
	private void UpdatePeriodic() {
		// Skip if we're not active - probably in a screen we don't belong to
		if(!isActiveAndEnabled) return;

		// Show notification?
		RefreshNotification();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected void OnDisable() {
		// Cancel periodic update
		CancelInvoke();

		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_CLOSED, this);
		Broadcaster.RemoveListener(BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS, this);
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Should we show the notification?
	/// </summary>
	private void RefreshNotification() {
		// Show only if there is a reward to collect
		if(UsersManager.currentUser != null) {
			m_notification.Set(UsersManager.currentUser.dailyRewards.CanCollectNextReward());
		} else {
			m_notification.ForceHide();
		}
	}

	/// <summary>
	/// Check whether the icon can be displayed or not.
	/// </summary>
	private void RefreshVisibility(bool _animate = true) {
		// Can it be displayed?
		// Not if featured not enabled
		if(!FeatureSettingsManager.IsDailyRewardsEnabled()) {
			m_rootAnim.ForceHide(_animate);
			return;
		}

		// Not during tutorial either
		if(!UsersManager.currentUser.HasPlayedGames(GameSettings.ENABLE_DAILY_REWARDS_AT_RUN)) {
			m_rootAnim.ForceHide(_animate);
			return;
		}

		// All checks passed! Show
		m_rootAnim.ForceShow(_animate);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The icon has been tapped.
	/// </summary>
	public void OnIconTap() {
		// Show the Daily Rewards popup
		PopupManager.OpenPopupInstant(PopupDailyRewards.PATH);
	}

	/// <summary>
	/// A global event has been sent.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Broadcast event info.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			case BroadcastEventType.POPUP_CLOSED: {
				// Is it the daily rewards popup?
				PopupManagementInfo info = (PopupManagementInfo)_broadcastEventInfo;
				if(info.popupController.GetComponent<PopupDailyRewards>() != null) {
					RefreshNotification();
				}
			} break;

			case BroadcastEventType.DEBUG_REFRESH_DAILY_REWARDS: {
				RefreshNotification();
			} break;
		}
	}
}