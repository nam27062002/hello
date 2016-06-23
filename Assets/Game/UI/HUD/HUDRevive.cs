// HUDRevive.cs
// Hungry Dragon
// 
// Marc Saña Forrellach, Alger Ortín Castellví
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Revive logic and UI controller.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class HUDRevive : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Localizer m_timerText = null;
	[SerializeField] private Text m_pcText = null;
	[SerializeField] private GameObject m_freeReviveButton = null;

	// Exposed setup
	[Space]
	[SerializeField] private float m_reviveAvailableSecs = 5f;	// [AOC] TODO!! From content
	[SerializeField] private int m_freeRevivesPerGame = 2;	// [AOC] TODO!! From content
	[SerializeField] private int m_minGamesBeforeFreeReviveAvailable = 3;	// [AOC] TODO!! From content

	// Internal references
	private ShowHideAnimator m_animator = null;

	// Internal logic
	private int m_paidReviveCount = 0;
	private int m_freeReviveCount = 0;
	private DeltaTimer m_timer = new DeltaTimer();
	private bool m_allowCurrencyPopup = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Get references
		m_animator = GetComponent<ShowHideAnimator>();

		// Subscribe to external events
		Messenger.AddListener(GameEvents.PLAYER_KO, OnPlayerKo);
		m_timer.Stop();
		m_paidReviveCount = 0;
		m_freeReviveCount = 0;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		if ( m_animator != null )
			m_animator.Hide(false);	// Start hidden
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.PLAYER_KO, OnPlayerKo);

		// Restore timescale
		Time.timeScale = 1f;
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		if(!m_timer.IsStopped()) {
			m_timerText.Localize(m_timerText.tid, StringUtils.FormatNumber(Mathf.CeilToInt(m_timer.GetTimeLeft() / 1000.0f)));
			if(m_timer.IsFinished()) {
				m_timer.Stop();
				m_animator.Hide();
				Messenger.Broadcast(GameEvents.PLAYER_DIED);
			}
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Performs the revive logic
	/// </summary>
	private void DoRevive() {
		// Revive!
		bool wasStarving = InstanceManager.player.IsStarving();
		bool wasCritical = InstanceManager.player.IsCritical();
		InstanceManager.player.ResetStats(true);

		// Disable status effects if required
		if(wasStarving != InstanceManager.player.IsStarving()) {
			Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, InstanceManager.player.IsStarving());
		}
		if(wasCritical != InstanceManager.player.IsCritical()) {
			Messenger.Broadcast<bool>(GameEvents.PLAYER_CRITICAL_TOGGLED, InstanceManager.player.IsCritical());
		}

		// Stop timer
		m_timer.Stop();

		// Hide
		m_animator.Hide();

		// Restore timescale
		Time.timeScale = 1f;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The revive button has been clicked.
	/// </summary>
	public void OnRevive() {
		// Perform transaction
		// If not enough funds, pause timer and open PC shop popup
		long costPC = m_paidReviveCount + 1;	// [AOC] TODO!! Actual revive cost formula
		if(UserProfile.pc >= costPC) {
			// Perform transaction
			UserProfile.AddPC(-costPC);
			PersistenceManager.Save();

			// Do it!
			m_paidReviveCount++;
			DoRevive();
		} else {
			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(Localization.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}

		// [AOC] TEMP!! Disable currency popup
		/*
		else if(m_allowCurrencyPopup) {
			// Open PC shop popup
			PopupController popup = PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
			popup.OnClosePostAnimation.AddListener(OnRevive);	// [AOC] Quick'n'dirty: try to revive again, but don't show the popup twice if we're out of currency!
			m_allowCurrencyPopup = false;

			// Pause timer
			m_timer.Stop();
		} else {
			// Currency popup closed, no funds purchased
			// If player tries to revive again, show popup
			m_allowCurrencyPopup = true;

			// Resume timer
			m_timer.Resume();
		}*/
	}

	/// <summary>
	/// The free revive button has been clicked.
	/// </summary>
	public void OnFreeRevive() {
		// [AOC] TODO!! Show a video ad!
		// Open placeholder popup
		PopupController popup = PopupManager.OpenPopupInstant(PopupAdRevive.PATH);
		popup.OnClosePostAnimation.AddListener(OnAdClosed);

		// Pause timer
		m_timer.Stop();
	}

	/// <summary>
	/// The player is KO.
	/// </summary>
	private void OnPlayerKo() {
		// Initialize PC cost
		if ( m_pcText != null )
			m_pcText.text = StringUtils.FormatNumber((m_freeReviveCount + m_paidReviveCount) + 1);	// [AOC] TODO!! Actual revive cost formula

		// Free revive available?
		m_freeReviveButton.SetActive(m_minGamesBeforeFreeReviveAvailable <= UserProfile.gamesPlayed && m_freeReviveCount < m_freeRevivesPerGame);

		// Reset timer and control vars
		m_timer.Start(m_reviveAvailableSecs * 1000);
		m_allowCurrencyPopup = true;

		// Show!
		if ( m_animator != null )
			m_animator.Show();

		// Slow motion
		Time.timeScale = 0.25f;
	}

	/// <summary>
	/// Ad has finished, free revive!
	/// </summary>
	private void OnAdClosed() {
		// Do it!
		m_freeReviveCount++;
		DoRevive();
	}
}
