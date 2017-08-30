// PopupGlobalEventContribution.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/07/2017.
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
/// 
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupGlobalEventContribution : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/GlobalEvents/PF_PopupGlobalEventContribution";

	private const int MAX_SUBMIT_ATTEMPTS = 2;

	private enum Panel {
		OFFLINE,
		ACTIVE
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Panels
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
	private DefinitionNode m_keyShopPackDef = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize some vars
		m_keyShopPackDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, "shop_pack_keys_0");
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<bool>(GameEvents.GLOBAL_EVENT_SCORE_REGISTERED, OnContributionConfirmed);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(GameEvents.GLOBAL_EVENT_SCORE_REGISTERED, OnContributionConfirmed);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Clear sequence
		if(m_activePanelSequence != null) {
			m_activePanelSequence.Kill();
			m_activePanelSequence = null;
		}
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

		// Close popup
		GetComponent<PopupController>().Close(true);
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
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// If we have no event data cached, get it now
		if(m_event == null) {
			m_event = GlobalEventManager.currentEvent;	// Should never be null (we shouldn't be displaying this popup if event is null
		}

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
	}

	/// <summary>
	/// The popup has just been opened.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Trigger animation
		if(m_activePanel == Panel.ACTIVE) {
			LaunchActivePanelAnimation();
		} else {
			// Allow continue
			m_continueEnabled = true;
		}
	}

	/// <summary>
	/// The popup is about to close.
	/// </summary>
	public void OnClosePreAnimation() {
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
	/// The submit score button has been pressed.
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
					// Attempt to do the contribution (we may have lost connectivity)
					GlobalEventManager.ErrorCode res = GlobalEventManager.Contribute(
						m_bonusDragon ? 2f : 1f,
						m_keyBonus ? 2f : 1f
					);
					if(res == GlobalEventManager.ErrorCode.NONE) {
						// Success! Wait for the confirmation from the server
						BusyScreen.Show(this);
					} else {
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
	/// We've received a response from the server.
	/// </summary>
	/// <param name="_success">Was the contribute operation successful?</param>
	private void OnContributionConfirmed(bool _success) {
		// Hide busy screen
		BusyScreen.Hide(this);

		// Successful?
		if(_success) {
			// Just close the popup!
			GetComponent<PopupController>().Close(true);
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