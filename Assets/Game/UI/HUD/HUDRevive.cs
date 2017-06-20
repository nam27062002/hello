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

	// Other references
	[Space]
	[SerializeField] private ShowHideAnimator m_animator = null;

	// Exposed setup
	[Space]
	[SerializeField] private float m_reviveAvailableSecs = 5f;
	[SerializeField] private float m_deathAnimDuration = 4f;
	[SerializeField] private int m_freeRevivesPerGame = 2;	// [AOC] TODO!! From content
	[SerializeField] private int m_minGamesBeforeFreeReviveAvailable = 3;	// [AOC] TODO!! From content

	// Internal logic
	private DeltaTimer m_timer = new DeltaTimer();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		// Subscribe to external events
		Messenger.AddListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
		Messenger.AddListener<DragonPlayer.ReviveReason>(GameEvents.PLAYER_REVIVE, OnPlayerRevive);
		Messenger.AddListener(GameEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPlayerPreFreeRevive);
		m_timer.Stop();
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
		Messenger.RemoveListener<DamageType, Transform>(GameEvents.PLAYER_KO, OnPlayerKo);
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
					RewardManager.paidReviveCount++;
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
		long costPC = RewardManager.paidReviveCount + 1;	// [AOC] TODO!! Actual revive cost formula
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
		PopupController popup = PopupManager.OpenPopupInstant(PopupAdPlaceholder.PATH);
		popup.OnClosePostAnimation.AddListener(OnAdClosed);

		// Pause timer
		m_timer.Stop();
	}

	/// <summary>
	/// The player is KO.
	/// </summary>
	private void OnPlayerKo(DamageType _type, Transform _source) {
		// No revive available during the tutorial! Kill the dragon after some delay
		bool tutorialCompleted = UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN);

		// Init some stuff
		float duration = 0f;
		if(tutorialCompleted) {
			// Timer
			duration = m_reviveAvailableSecs;

			// Initialize PC cost
			if(m_pcText != null) {
				m_pcText.text = UIConstants.GetIconString((RewardManager.freeReviveCount + RewardManager.paidReviveCount) + 1, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);	// [AOC] TODO!! Actual revive cost formula
			}

			// Free revive available?
			m_freeReviveButton.SetActive(m_minGamesBeforeFreeReviveAvailable <= UsersManager.currentUser.gamesPlayed && RewardManager.freeReviveCount < m_freeRevivesPerGame);

			// Show!
			if(m_animator != null) m_animator.Show();
		} else {
			// Timer
			duration = m_deathAnimDuration;
		}

		// Reset timer
		m_timer.Start(duration * 1000);

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
		RewardManager.freeReviveCount++;
		DoRevive( DragonPlayer.ReviveReason.AD );
	}
}
