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
using TMPro;

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
	[SerializeField] private TextMeshProUGUI m_pcText = null;
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
		Messenger.AddListener<DamageType>(GameEvents.PLAYER_KO, OnPlayerKo);
		Messenger.AddListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnPlayerRevive);
		Messenger.AddListener(GameEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPlayerPreFreeRevive);
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
		Messenger.RemoveListener<DamageType>(GameEvents.PLAYER_KO, OnPlayerKo);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnPlayerRevive);
		Messenger.RemoveListener(GameEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPlayerPreFreeRevive);

		// Restore timescale
		Time.timeScale = 1f;
	}

	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		if(!m_timer.IsStopped()) {
            m_timerText.Localize(m_timerText.tid, StringUtils.FormatNumber(Mathf.CeilToInt((float)m_timer.GetTimeLeft() / 1000.0f)));
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
	private void DoRevive( DragonPlayer.ReviveReason reason ) {
		// Revive!
		InstanceManager.player.ResetStats(true, reason);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The revive button has been clicked.
	/// </summary>
	public void OnRevive() {
		// Make sure timer hasn't finished!
		if(m_timer.IsFinished()) return;

		// Perform transaction
		// Start purchase flow
		// Use universal finish event to detect both success and failures
		ResourcesFlow purchaseFlow = new ResourcesFlow("REVIVE");
		purchaseFlow.OnFinished.AddListener(
			(ResourcesFlow _flow) => {
				// Flow successful?
				if(_flow.successful) {
					// Do it!
					m_paidReviveCount++;
					DoRevive( DragonPlayer.ReviveReason.PAYING );
					PersistenceManager.Save();
				} else {
					// Resume countdown timer!
					m_timer.Resume();
				}
			}
		);

		// Pause timer and begin flow!
		m_timer.Stop();
		long costPC = m_paidReviveCount + 1;	// [AOC] TODO!! Actual revive cost formula
		purchaseFlow.Begin((long)costPC, UserProfile.Currency.HARD, null);

		// Without resources flow:
		// If not enough funds, pause timer and open PC shop popup
		/*long costPC = m_paidReviveCount + 1;	// [AOC] TODO!! Actual revive cost formula
		if(UsersManager.currentUser.pc >= costPC) {
			// Perform transaction
			UsersManager.currentUser.AddPC(-costPC);
			PersistenceManager.Save();

			// Do it!
			m_paidReviveCount++;
			DoRevive( DragonPlayer.ReviveReason.PAYING );
		} else {
			// Currency popup / Resources flow disabled for now
            UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}*/
	}

	/// <summary>
	/// The free revive button has been clicked.
	/// </summary>
	public void OnFreeRevive() {
		// Make sure timer hasn't finished!
		if(m_timer.IsFinished()) return;

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
	private void OnPlayerKo(DamageType _type) {
		// Initialize PC cost
		if ( m_pcText != null )
			m_pcText.text = UIConstants.GetIconString((m_freeReviveCount + m_paidReviveCount) + 1, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);	// [AOC] TODO!! Actual revive cost formula

		// Free revive available?
		m_freeReviveButton.SetActive(m_minGamesBeforeFreeReviveAvailable <= UsersManager.currentUser.gamesPlayed && m_freeReviveCount < m_freeRevivesPerGame);

		// Reset timer and control vars
		m_timer.Start(m_reviveAvailableSecs * 1000);
		m_allowCurrencyPopup = true;

		// Show!
		if ( m_animator != null )
			m_animator.Show();

		// Slow motion
		Time.timeScale = 0.25f;
	}

	private void OnPlayerRevive( DragonPlayer.ReviveReason reason )
	{
		// Stop timer
		m_timer.Stop();

		// Hide
		m_animator.Hide();

		// Restore timescale
		Time.timeScale = 1f;
	}

	private void OnPlayerPreFreeRevive()
	{
		// Stop timer
		m_timer.Stop();

		// Hide
		m_animator.Hide();
	}

	/// <summary>
	/// Ad has finished, free revive!
	/// </summary>
	private void OnAdClosed() {
		// Do it!
		m_freeReviveCount++;
		DoRevive( DragonPlayer.ReviveReason.AD );
	}
}
