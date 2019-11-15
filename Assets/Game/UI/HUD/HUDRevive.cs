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
    string TID_GAME_REVIVE_FREE = "TID_GAME_REVIVE_FREE";


    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
    [SerializeField] private Localizer m_timerText = null;
	[SerializeField] private TextMeshProUGUI m_pcText = null;
	[SerializeField] private GameObject m_adsReviveButton = null;
    [SerializeField] private GameObject m_freeReviveButton = null;
    [SerializeField] private GameObject m_pcReviveButton = null;

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
	private bool m_revived = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
        // Subscribe to external events
        Messenger.AddListener(MessengerEvents.PLAYER_MUMMY_REVIVE, OnPlayerStartRevie);
        Messenger.AddListener(MessengerEvents.PLAYER_FREE_REVIVE, OnPlayerStartRevie);
        Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
		Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
		Messenger.AddListener(MessengerEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPlayerPreFreeRevive);
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
        Messenger.RemoveListener(MessengerEvents.PLAYER_MUMMY_REVIVE, OnPlayerStartRevie);
        Messenger.RemoveListener(MessengerEvents.PLAYER_FREE_REVIVE, OnPlayerStartRevie);
        Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
		Messenger.RemoveListener(MessengerEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPlayerPreFreeRevive);

		// Restore timescale
		// Time.timeScale = 1f;
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
				Messenger.Broadcast(MessengerEvents.PLAYER_DIED);
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
		// Control flag
		m_revived = true;

		// Revive!
		InstanceManager.player.ResetStats(true, reason);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The revive button has been clicked.
	/// </summary>
	public void OnRevivePC() {
		// Make sure timer hasn't finished!
		if(m_timer.IsFinished()) return;

		// Prevent button spamming!
		if(m_revived) return;

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
                    PersistenceFacade.instance.Save_Request();
                } else {
					// Resume countdown timer!
					m_timer.Resume();
				}
			}
		);

		// Pause timer and begin flow!
		m_timer.Stop();
		long costPC =  RewardManager.GetReviveCost();
		purchaseFlow.Begin((long)costPC, UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.REVIVE, null);

		// Without resources flow:
		// If not enough funds, pause timer and open PC shop popup
		/*long costPC = m_paidReviveCount + 1;	// [AOC] TODO!! Actual revive cost formula
		if(UsersManager.currentUser.pc >= costPC) {
			// Perform transaction
			UsersManager.currentUser.AddCurrency(UserProfile.Currency.HARD, -costPC);
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


        // Decrement the free revives counter
        if (UsersManager.currentUser.removeAds.UseRevive())
        {
            // Revive the player
            DoRevive(DragonPlayer.ReviveReason.REMOVE_ADS);
        }

    }

    /// <summary>
    /// The free revive button has been clicked.
    /// </summary>
    public void OnAdRevive()
    {
        // Make sure timer hasn't finished!
        if (m_timer.IsFinished()) return;

        // Pause timer
        m_timer.Stop();

        // Show video ad!
        PopupAdBlocker.LaunchAd(true, GameAds.EAdPurpose.REVIVE, OnVideoRewardCallback);
    }

    void OnVideoRewardCallback( bool done ){
		if (done){
			RewardManager.freeReviveCount++;
			DoRevive( DragonPlayer.ReviveReason.AD );
		}else{
			m_timer.Resume();
		}
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
				m_pcText.text = UIConstants.GetIconString( RewardManager.GetReviveCost(), UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);
			}

            // Has the user the Remove Ads feature?
            bool removeAds = UsersManager.currentUser.removeAds.IsActive;

            // Ad revive available?
            m_adsReviveButton.SetActive(FeatureSettingsManager.AreAdsEnabled && 
                m_minGamesBeforeFreeReviveAvailable <= UsersManager.currentUser.gamesPlayed && 
                RewardManager.freeReviveCount < m_freeRevivesPerGame &&
                !removeAds);

            // Free revives available?
            bool freeReviveAvailable = false;
            if (removeAds)
            {
                freeReviveAvailable = UsersManager.currentUser.removeAds.revivesLeft > 0;
                m_freeReviveButton.SetActive(freeReviveAvailable);
                m_freeReviveButton.GetComponentInChildren<Localizer>().Localize(TID_GAME_REVIVE_FREE, freeReviveAvailable.ToString());
            }

            // If free revive is available, dont let the user pay gems to revive
            m_pcReviveButton.SetActive(!freeReviveAvailable);

			// Show!
			if(m_animator != null) m_animator.Show();
		} else {
			// Timer
			duration = m_deathAnimDuration;
		}

		// Reset timer
		m_timer.Start(duration * 1000);

		// Reset revive flag
		m_revived = false;

        // Slow motion
        // Time.timeScale = 0.25f;
        InstanceManager.timeScaleController.Dead();
	}

    private void OnPlayerStartRevie() {
        InstanceManager.timeScaleController.ReviveStart();
    }

    private void OnPlayerRevive( DragonPlayer.ReviveReason reason )
	{
		// Stop timer
		m_timer.Stop();

		// Hide
		m_animator.Hide();

        // Restore timescale
        // Time.timeScale = 1f;
        InstanceManager.timeScaleController.Revived();
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
