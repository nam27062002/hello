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
	[SerializeField] private Text m_pcText = null;
	[SerializeField] private Text m_reviveText = null;

	// Exposed setup
	[SerializeField] private float m_reviveAvailableSecs = 5f;	// [AOC] TODO!! From content

	// Internal references
	private ShowHideAnimator m_animator = null;

	// Internal logic
	private int m_reviveCount = 0;
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
		m_reviveCount = 0;
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
			m_reviveText.text = Localization.Localize("Revive %U0", StringUtils.FormatNumber(Mathf.CeilToInt(m_timer.GetTimeLeft())));	// [AOC] HARDCODED!!
			if(m_timer.Finished()) {
				m_timer.Stop();
				m_animator.Hide();
				Messenger.Broadcast(GameEvents.PLAYER_DIED);
			}
		}
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
		long costPC = m_reviveCount + 1;	// [AOC] TODO!! Actual revive cost formula
		if(UserProfile.pc >= costPC) {
			// Revive!
			m_reviveCount++;
			InstanceManager.player.ResetStats(true);
			m_timer.Stop();

			// Perform transaction
			UserProfile.AddPC(-costPC);
			PersistenceManager.Save();

			// Hide button
			m_animator.Hide();

			// Restore timescale
			Time.timeScale = 1f;
		} else if(m_allowCurrencyPopup) {
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
		}
	}

	/// <summary>
	/// The player is KO.
	/// </summary>
	private void OnPlayerKo() {
		if ( m_pcText != null )
			m_pcText.text = StringUtils.FormatNumber(m_reviveCount + 1);	// [AOC] TODO!! Actual revive cost formula

		m_timer.Start(m_reviveAvailableSecs);
		m_allowCurrencyPopup = true;

		if ( m_animator != null )
			m_animator.Show();

		// Slow motion
		Time.timeScale = 0.25f;
	}
}
