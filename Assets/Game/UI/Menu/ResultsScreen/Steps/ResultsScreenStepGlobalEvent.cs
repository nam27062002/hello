// ResultsScreenStepGlobalEvent.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepGlobalEvent : ResultsScreenStep {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const int MAX_SUBMIT_ATTEMPTS = 2;

	private enum Panel {
		OFFLINE,
		ACTIVE
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ShowHideAnimator m_offlineGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_activeGroupAnim = null;
	[Space]
	[SerializeField] private Localizer m_tapToContinueText = null;
	[SerializeField] private ShowHideAnimator m_tapToContinueAnim = null;

	[Separator("Title Panel")]
	[SerializeField] private TextMeshProUGUI m_descriptionText = null;
	[SerializeField] private Image m_eventIcon = null;

	[Separator("Active Panel")]
	[SerializeField] private NumberTextAnimator m_scoreText = null;	
	[SerializeField] private Localizer m_bonusDragonInfoText = null;
	[SerializeField] private TextMeshProUGUI m_bonusDragonText = null;
	[SerializeField] private NumberTextAnimator m_finalScoreText = null;	
	[Space]
	[SerializeField] private TextMeshProUGUI m_keysBonusLabelText = null;
	[Space]
	[SerializeField] private CurrencyButton m_buyKeyPCButton = null;
	[SerializeField] private ShowHideAnimator m_buyKeyAdButtonAnim = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_keysBonusText = null;
	[SerializeField] private Localizer m_keysBonusInfoText = null;
	[SerializeField] private ShowHideAnimator m_keysBonusTextAnim = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_scoreOrnamentAnim = null;
	[SerializeField] private ShowHideAnimator m_scoreGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_bonusDragonOrnamentAnim = null;
	[SerializeField] private ShowHideAnimator m_bonusDragonGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_keyOrnamentAnim = null;
	[SerializeField] private ShowHideAnimator m_keyBonusGroupAnim = null;
	[Space]
	[SerializeField] private float m_rowDelay = 1f;

	[Space]
	[SerializeField] private TweenSequence m_sequence = null;

	// Internal logic
	private GlobalEvent m_event = null;
	private Panel m_activePanel = Panel.OFFLINE;
	private Sequence m_activePanelSequence = null;

	private long m_finalScore = 0;
	private int m_submitAttempts = 0;
	private bool m_continueEnabled = false;

	private bool m_bonusDragon = false;
	private bool m_keyBonus = false;
	private bool m_keyPurchased = false;
	private bool m_keyFromAds = false;
	private DefinitionNode m_keyShopPackDef = null;
	
	//------------------------------------------------------------------------//
	// ResultsScreenStep IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed() {
		// Never during FTUX
		if(UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_GLOBAL_EVENTS_AT_RUN) return false;

		// Is there a valid current event to display? Check error codes to know so.
		GlobalEventManager.ErrorCode canContribute = GlobalEventManager.CanContribute();
		if((canContribute == GlobalEventManager.ErrorCode.NONE
			|| canContribute == GlobalEventManager.ErrorCode.OFFLINE
			|| canContribute == GlobalEventManager.ErrorCode.NOT_LOGGED_IN)
			&& GlobalEventManager.currentEvent != null
			&& GlobalEventManager.currentEvent.objective.enabled
			&& GlobalEventManager.currentEvent.remainingTime.TotalSeconds > 0	// We check event hasn't finished while playing
		) {	// [AOC] This will cover cases where the event is active but not enabled for this player (i.e. during the tutorial).
			// In addition to all that, check that we actually have a score to register
			if(GlobalEventManager.currentEvent.objective.currentValue > 0) {
				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Init this step.
	/// </summary>
	override protected void DoInit() {
		// Get event data!
		m_event = GlobalEventManager.currentEvent;

		// Initialize some vars
		m_keyShopPackDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, "shop_pack_keys_0");

		// Listen to sequence ending
		m_sequence.OnFinished.AddListener(OnHidePostAnimation);
	}

	/// <summary>
	/// Launch this step.
	/// </summary>
	override protected void DoLaunch() {
		// Make sure we have the latest event data
		m_event = GlobalEventManager.currentEvent;

		// Subscribe to external events
		Messenger.AddListener<bool>(MessengerEvents.GLOBAL_EVENT_SCORE_REGISTERED, OnContributionConfirmed);

		// Reset local vars
		m_submitAttempts = 0;

		// Was key collected during this run? We'll know it because we'll have at least one key
		if(UsersManager.currentUser.keys > 0) {
			// Yes! Use it immediately!
			ConsumeKeys((ulong)UsersManager.currentUser.keys, false);
		} else {
			m_keyBonus = false;
		}

		// Bonus dragon?
		if(m_event != null) {
			m_bonusDragon = DragonManager.currentDragon.def.sku == m_event.bonusDragonSku;
		} else {
			m_bonusDragon = false;
		}

		// Initialize static stuff
		if(m_event != null && m_event.objective != null) {
			// Objective image
			m_eventIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + m_event.objective.icon);

			// Event description
			m_descriptionText.text = m_event.objective.GetDescription();
		}

		// Do a first refresh
		InitPanel(false, true);

		// Don't allow continue
		m_continueEnabled = false;

		// Launch sequence!
		m_sequence.Launch();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh data based on current event state.
	/// </summary>
	/// <param name="_animate">Whether to animate or do an instant refresh.</param>
	private void InitPanel(bool _animate, bool _resetValues) {
		// Select visible panel based on event state
		GlobalEventManager.ErrorCode error = GlobalEventManager.CanContribute();
		switch(error) {
			case GlobalEventManager.ErrorCode.NONE:				m_activePanel = Panel.ACTIVE;	break;
			case GlobalEventManager.ErrorCode.OFFLINE:			m_activePanel = Panel.OFFLINE;	break;
		}

		// Initialize active panel
		switch(m_activePanel) {
			case Panel.ACTIVE: {
				if(_resetValues) {
					m_scoreText.SetValue(0, false);
					m_finalScoreText.SetValue(0, false);
					RefreshKeysField(_animate);

					// Bonus dragon info
					DefinitionNode bonusDragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, m_event.bonusDragonSku);
					if ( bonusDragonDef != null ){
						m_bonusDragonInfoText.Localize("TID_EVENT_RESULTS_BONUS_DRAGON_INFO", bonusDragonDef.GetLocalized("tidName"));
					}

					// Bonus dragon text
					if(m_event.bonusDragonSku == DragonManager.currentDragon.def.sku) {
						m_bonusDragonText.text = "x2";
					} else {
						m_bonusDragonText.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_BONUS_DRAGON_NOT_APPLIED");
					}

					// Hide everything (prepare for anim)
					m_scoreOrnamentAnim.Hide(false);
					m_scoreGroupAnim.Hide(false);
					m_bonusDragonOrnamentAnim.Hide(false);
					m_bonusDragonGroupAnim.Hide(false);
					m_keyOrnamentAnim.Hide(false);
					m_keyBonusGroupAnim.Hide(false);
				}
				m_tapToContinueText.Localize("TID_RESULTS_TAP_TO_CONTINUE");
			} break;

			case Panel.OFFLINE: {
				// Nothing to do!
				m_tapToContinueText.Localize("TID_RESULTS_TAP_TO_SKIP");
			} break;
		}

		// Set panels visibility
		m_activeGroupAnim.Set(m_activePanel == Panel.ACTIVE, _animate);
		m_offlineGroupAnim.Set(m_activePanel == Panel.OFFLINE, _animate);
	}

	/// <summary>
	/// Shows up the proper asset in the keys field.
	/// </summary>
	private void RefreshKeysField(bool _animate) {
		// Key found in-game or obtained using other methods (ads, PC)
		if(m_keyBonus) {
			m_keysBonusLabelText.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_KEY_BONUS_USED");
			m_keysBonusText.text = "x2";
			m_keysBonusInfoText.Localize("");
		} else {
			m_keysBonusLabelText.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_KEY_BONUS_NOT_FOUND", "x2");
		}

		// Select what to choose - depends on whether we have already used a key and whether we have enough keys
		m_buyKeyPCButton.animator.Set(!m_keyBonus, _animate);
		m_buyKeyAdButtonAnim.Set(!m_keyBonus, _animate);
		m_keysBonusTextAnim.Set(m_keyBonus, _animate);

		// Set up double up price tag
		m_buyKeyPCButton.SetAmount(m_keyShopPackDef.GetAsFloat("price"), UserProfile.Currency.HARD);
	}

	/// <summary>
	/// Trigger the active panel animation.
	/// </summary>
	private void LaunchActivePanelAnimation() {
		// Kill any existing tween
		string tweenId = "PopupGlobalEventContribution.ActivePanel";
		DOTween.Kill(tweenId);

		// Init some stuff
		m_finalScoreText.SetValue(m_finalScore, false);
		m_finalScore = 0;

		m_continueEnabled = false;
		m_tapToContinueAnim.Hide();

		RefreshKeysField(true);

		// Hide everything
		m_scoreOrnamentAnim.Hide(false);
		m_scoreGroupAnim.Hide(false);
		m_bonusDragonOrnamentAnim.Hide(false);
		m_bonusDragonGroupAnim.Hide(false);
		m_keyOrnamentAnim.Hide(false);
		m_keyBonusGroupAnim.Hide(false);

		// Sequentially update values
		m_activePanelSequence = DOTween.Sequence()
			.SetId(tweenId)

			// Base score
			.AppendCallback(() => {
				m_scoreOrnamentAnim.Show();
				m_scoreGroupAnim.Show();
			})
			.AppendInterval(m_scoreGroupAnim.tweenDelay + m_scoreGroupAnim.tweenDuration)
			.AppendCallback(() => {
				m_finalScore = (long)m_event.objective.currentValue;
				m_scoreText.SetValue(m_finalScore, true);
				m_finalScoreText.SetValue(m_finalScore, true);
			})
			.AppendInterval(m_rowDelay)

			// Bonus Dragon
			.AppendCallback(() => {
				m_bonusDragonOrnamentAnim.Show();
				m_bonusDragonGroupAnim.Show();
			})
			.AppendInterval(m_bonusDragonGroupAnim.tweenDelay + m_bonusDragonGroupAnim.tweenDuration)
			.AppendCallback(() => {
				if(m_bonusDragon) m_finalScore *= 2;
				m_finalScoreText.SetValue(m_finalScore, true);
			})
			.AppendInterval(m_rowDelay)

			// Bonus key
			.AppendCallback(() => {
				m_keyOrnamentAnim.Show();
				m_keyBonusGroupAnim.Show();
			})
			.AppendInterval(m_keyBonusGroupAnim.tweenDelay + m_keyBonusGroupAnim.tweenDuration)
			.AppendCallback(() => {
				if(m_keyBonus) m_finalScore *= 2;
				m_finalScoreText.SetValue(m_finalScore, true);
			})

			// Tap to continue
			.AppendCallback(() => {
				// Allow continue
				m_continueEnabled = true;
				m_tapToContinueAnim.Show();
				m_activePanelSequence = null;
			});
	}

	/// <summary>
	/// Discard event contribution and close the popup.
	/// </summary>
	private void CloseAndDiscard() {
		// If we purchased keys, refund
		if(m_keyPurchased) {
			UsersManager.currentUser.EarnCurrency(
				UserProfile.Currency.HARD, 
				(ulong)m_keyShopPackDef.GetAsLong("price"), 
				true, 
				HDTrackingManager.EEconomyGroup.GLOBAL_EVENT_REFUND
			);
		}

		// Continue sequence
		m_sequence.Play();
	}

	/// <summary>
	/// Perform all required actions when using keys to double the score.
	/// </summary>
	/// <param name="_keysAmount">Amount of keys to be consumed.</param>
	/// <param name="_animate">Whether to refresh visuals or not.</param>
	private void ConsumeKeys(ulong _keysAmount, bool _updateScore) {
		// Remember decision
		m_keyBonus = true;

		// Always consume via ResourcesFlow (for tracking purposes)
		ResourcesFlow keysFlow = new ResourcesFlow();
		keysFlow.Begin(
			(long)_keysAmount, 
			UserProfile.Currency.KEYS, 
			HDTrackingManager.EEconomyGroup.GLOBAL_EVENT_BONUS,
			null
		);

		// Update score?
		if(_updateScore) {
			// Refresh visuals
			RefreshKeysField(true);

			// Update final score
			m_finalScore *= 2;
			m_finalScoreText.SetValue(m_finalScore, true);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Tap to continue has been pressed.
	/// </summary>
	public void OnTapToContinue() {
		// Depends on active panel
		switch(m_activePanel) {
			case Panel.ACTIVE: {
				// If the sequence is running, fast forward
				if(m_activePanelSequence != null) {
					// Accelerate everything
					m_activePanelSequence.timeScale = 10;
					m_scoreText.duration = 0.1f;
					m_finalScoreText.duration = 0.1f;
				}

				// Sequence has finished
				else if(m_continueEnabled) {
					// Success! Wait for the confirmation from the server
					BusyScreen.Show(this);

					// Attempt to do the contribution (we may have lost connectivity)
					GlobalEventManager.ErrorCode res = GlobalEventManager.Contribute(
						m_bonusDragon ? 2f : 1f,
						m_keyBonus ? 2f : 1f,
						m_keyPurchased,
						m_keyFromAds
					);
					if(res != GlobalEventManager.ErrorCode.NONE) {
						BusyScreen.Hide(this);
						// We can't contribute! Refresh panel
						InitPanel(true, false);

						// Reset submission attempts
						m_submitAttempts = 0;						
					}
				}
			} break;

			case Panel.OFFLINE: {
				// Discard contribution if allowed
				if(m_continueEnabled) {
					CloseAndDiscard();
				}
			} break;
		}
	}

	/// <summary>
	/// Show animation just finished.
	/// </summary>
	public void OnShowPostAnimation() {
		// Trigger animation
		if(m_activePanel == Panel.ACTIVE) {
			LaunchActivePanelAnimation();
		} else {
			// Allow continue
			m_continueEnabled = true;
		}
	}

	/// <summary>
	/// Hide animation is about to start.
	/// </summary>
	public void OnHidePreAnimation() {
		// Disable continue spamming!
		m_continueEnabled = false;

		// Remove all keys from the user. With the new design keys are no longer stockable!
		if(UsersManager.currentUser.keys > 0) {
			ResourcesFlow flow = new ResourcesFlow();
			flow.Begin(
				UsersManager.currentUser.keys, 	// Spend as many keys as we currently have
				UserProfile.Currency.KEYS, 
				HDTrackingManager.EEconomyGroup.GLOBAL_EVENT_KEYS_RESET,
				null
			);
		}
	}

	/// <summary>
	/// Hide animation has finished.
	/// </summary>
	private void OnHidePostAnimation() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(MessengerEvents.GLOBAL_EVENT_SCORE_REGISTERED, OnContributionConfirmed);

		// Clear sequence
		if(m_activePanelSequence != null) {
			m_activePanelSequence.Kill();
			m_activePanelSequence = null;
		}

		// Mark as finished
		OnFinished.Invoke();
	}

	/// <summary>
	/// Retry connection button has been pressed.
	/// </summary>
	public void OnRetryConnectionButton() {
		// Just refreshing is enough
		InitPanel(true, false);

		// If suceeded, launch intro anim
		if(m_activePanel == Panel.ACTIVE) {
			LaunchActivePanelAnimation();
		}
	}

	/// <summary>
	/// Double up button has been pressed.
	/// </summary>
	public void OnBuyKeyAdButton() {
		// Show a video ad!
		PopupAdBlocker.Launch(
			true, 
			GameAds.EAdPurpose.RESULTS_GET_KEY,
			(bool _success) => {
                if ( _success )
                {
    				m_keyFromAds = true;
    				// Add keys and consume them instantly (for tracking purposes)
    				ulong keysAmount = 1;
    				UsersManager.currentUser.EarnCurrency(
    					UserProfile.Currency.KEYS, 
    					keysAmount, 
    					true, 
    					HDTrackingManager.EEconomyGroup.REWARD_AD
    				);
    				ConsumeKeys(keysAmount, true);
                }
			}
		);
	}

	/// <summary>
	/// Buy more keys button has been pressed.
	/// </summary>
	public void OnBuyKeyPCButton() {
		// Perform transaction
		ResourcesFlow flow = new ResourcesFlow("GlobalEventBonusScore");
		flow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				// Remember decision
				m_keyPurchased = true;

				// Add keys and consume them instantly (for tracking purposes)
				ulong keysAmount = (ulong)m_keyShopPackDef.GetAsLong("amount");
				UsersManager.currentUser.EarnCurrency(
					UserProfile.Currency.KEYS, 
					keysAmount, 
					true, 
					HDTrackingManager.EEconomyGroup.SHOP_EXCHANGE
				);
				ConsumeKeys(keysAmount, true);
			}
		);
		flow.Begin(
			m_keyShopPackDef.GetAsLong("price"), 
			UserProfile.Currency.HARD, 
			HDTrackingManager.EEconomyGroup.SHOP_KEYS_PACK, 
			m_keyShopPackDef
		);
	}

	/// <summary>
	/// We've received a response from the server.
	/// </summary>
	/// <param name="_success">Was the contribute operation successful?</param>
	private void OnContributionConfirmed(bool _success) {
		// Hide busy screen
		BusyScreen.Hide(this);

		// Successful?
		if(_success) {
			// Continue sequence to close the popup
			m_sequence.Play();
		} else {
			// Something went wrong!
			m_submitAttempts++;	// Increase sumbission attempts

			// If we've reached max submission attempts, show different feedback and close popup
			if(m_submitAttempts >= MAX_SUBMIT_ATTEMPTS) {
				// Show feedback
				UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
					LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_MAX_SUBMISSIONS_ERROR"),
					new Vector2(0.5f, 0.5f),
					(RectTransform)this.GetComponentInParent<Canvas>().transform
				);
				text.text.color = Color.red;

				// Discard contribution
				CloseAndDiscard();
			} else {
				// Show feedback
				UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
					LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR"),
					new Vector2(0.5f, 0.5f),
					(RectTransform)this.GetComponentInParent<Canvas>().transform
				);
				text.text.color = Color.red;

				// Refresh info
				InitPanel(true, false);
			}
		}
	}
}