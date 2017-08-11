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
	private const long DOUBLE_UP_COST_KEYS = 1;	// [AOC] HARDCODED!! Take it from content!

	private enum Panel {
		OFFLINE,
		LOG_IN,
		ACTIVE
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Panels
	[SerializeField] private ShowHideAnimator m_offlineGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_loginGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_activeGroupAnim = null;
	[Space]
	[SerializeField] private Localizer m_tapToContinueText = null;
	[SerializeField] private ShowHideAnimator m_tapToContinueAnim = null;

	[Separator("Title Panel")]
	[SerializeField] private TextMeshProUGUI m_descriptionText = null;
	[SerializeField] private Image m_eventIcon = null;

	[Separator("Active Panel")]
	[SerializeField] private NumberTextAnimator m_scoreText = null;	
	[SerializeField] private TextMeshProUGUI m_bonusDragonText = null;
	[SerializeField] private NumberTextAnimator m_finalScoreText = null;	
	[Space]
	[SerializeField] private TextMeshProUGUI m_keysBonusLabelText = null;
	[Space]
	[SerializeField] private Localizer m_useKeysButtonText = null;
	[SerializeField] private ShowHideAnimator m_useKeysButtonAnim = null;
	[SerializeField] private ShowHideAnimator m_buyKeysButtonAnim = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_keysBonusText = null;
	[SerializeField] private ShowHideAnimator m_keysBonusTextAnim = null;
	[Space]
	[SerializeField] private ShowHideAnimator m_scoreGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_scoreOrnamentAnim = null;
	[SerializeField] private ShowHideAnimator m_bonusDragonGroupAnim = null;
	[SerializeField] private ShowHideAnimator m_bonusDragonOrnamentAnim = null;
	[SerializeField] private ShowHideAnimator m_keyBonusGroupAnim = null;
	[Space]
	[SerializeField] private float m_rowDelay = 1f;

	// Internal logic
	private Panel m_activePanel = Panel.OFFLINE;
	private GlobalEvent m_event = null;
	private int m_submitAttempts = 0;
	private bool m_usedKey = false;
	private bool m_bonusDragon = false;
	private long m_finalScore = 0;
	private bool m_continueEnabled = false;
	private Sequence m_activePanelSequence = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

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
	/// Called every frame.
	/// </summary>
	private void Update() {

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
			case GlobalEventManager.ErrorCode.OFFLINE:			m_activePanel = Panel.OFFLINE;		break;
			case GlobalEventManager.ErrorCode.NOT_LOGGED_IN:	m_activePanel = Panel.LOG_IN;		break;
		}

		// Initialize active panel
		switch(m_activePanel) {
			case Panel.ACTIVE: {
				if(_resetValues) {
					m_scoreText.SetValue(0, false);
					m_finalScoreText.SetValue(0, false);
					RefreshKeysField(_animate);

					// Bonus dragon text
					if(m_event.bonusDragonSku == DragonManager.currentDragon.def.sku) {
						m_bonusDragonText.text = "x2";
					} else {
						m_bonusDragonText.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_BONUS_DRAGON_NOT_APPLIED");
					}

					// Hide everything (prepare for anim)
					m_scoreGroupAnim.Hide(false);
					m_scoreOrnamentAnim.Hide(false);
					m_bonusDragonGroupAnim.Hide(false);
					m_bonusDragonOrnamentAnim.Hide(false);
					m_keyBonusGroupAnim.Hide(false);
				}
				m_tapToContinueText.Localize("TID_RESULTS_TAP_TO_CONTINUE");
			} break;

			case Panel.OFFLINE:
			case Panel.LOG_IN: {
				// Nothing to do!
				m_tapToContinueText.Localize("TID_RESULTS_TAP_TO_SKIP");
			} break;
		}

		// Set panels visibility
		m_activeGroupAnim.Set(m_activePanel == Panel.ACTIVE, _animate);
		m_offlineGroupAnim.Set(m_activePanel == Panel.OFFLINE, _animate);
		m_loginGroupAnim.Set(m_activePanel == Panel.LOG_IN, _animate);
	}

	/// <summary>
	/// Shows up the proper asset in the keys field.
	/// </summary>
	private void RefreshKeysField(bool _animate) {
		// Have contributed?
		if(m_usedKey) {
			m_keysBonusLabelText.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_KEY_BONUS_USED");
			m_keysBonusText.text = "x2";
		} else {
			m_keysBonusLabelText.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_KEY_BONUS", "x2");
		}

		// Select what to choose - depends on whether we have already used a key and whether we have enough keys
		m_buyKeysButtonAnim.Set(!m_usedKey && UsersManager.currentUser.keys < DOUBLE_UP_COST_KEYS, _animate);
		m_useKeysButtonAnim.Set(!m_usedKey && UsersManager.currentUser.keys >= DOUBLE_UP_COST_KEYS, _animate);
		m_keysBonusTextAnim.Set(m_usedKey, _animate);

		// Set up double up price tag
		m_useKeysButtonText.Localize("TID_EVENT_RESULTS_USE_KEY_BUTTON", StringUtils.FormatNumber(DOUBLE_UP_COST_KEYS));
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
		m_scoreGroupAnim.Hide(false);
		m_scoreOrnamentAnim.Hide(false);
		m_bonusDragonGroupAnim.Hide(false);
		m_bonusDragonOrnamentAnim.Hide(false);
		m_keyBonusGroupAnim.Hide(false);

		// Sequentially update values
		m_activePanelSequence = DOTween.Sequence()
			.SetId(tweenId)

			// Base score
			.AppendCallback(() => {
				m_scoreGroupAnim.Show();
				m_scoreOrnamentAnim.Show();
			})
			.AppendInterval(m_scoreOrnamentAnim.tweenDelay + m_scoreOrnamentAnim.tweenDuration)
			.AppendCallback(() => {
				m_finalScore = (long)m_event.objective.currentValue;
				m_scoreText.SetValue(m_finalScore, true);
				m_finalScoreText.SetValue(m_finalScore, true);
			})
			.AppendInterval(m_rowDelay)

			// Bonus Dragon
			.AppendCallback(() => {
				m_bonusDragonGroupAnim.Show();
				m_bonusDragonOrnamentAnim.Show();
			})
			.AppendInterval(m_bonusDragonOrnamentAnim.tweenDelay + m_bonusDragonOrnamentAnim.tweenDuration)
			.AppendCallback(() => {
				if(m_bonusDragon) m_finalScore *= 2;
				m_finalScoreText.SetValue(m_finalScore, true);
			})
			.AppendInterval(m_rowDelay)

			// Bonus key
			.AppendCallback(() => {
				m_keyBonusGroupAnim.Show();
			})
			.AppendInterval(m_keyBonusGroupAnim.tweenDelay + m_keyBonusGroupAnim.tweenDuration)
			.AppendCallback(() => {
				if(m_usedKey) m_finalScore *= 2;
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
		// If we spend keys, refund
		if(m_usedKey) {
			UsersManager.currentUser.EarnCurrency(UserProfile.Currency.KEYS, (ulong)DOUBLE_UP_COST_KEYS, true);	// Refund as a paid key so we don't have any issue with the limits
		}

		// Close popup
		GetComponent<PopupController>().Close(true);
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
		m_usedKey = false;
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
	/// Log In button has been pressed.
	/// </summary>
	public void OnLoginButton() {
		// [AOC] TODO!!
		UIFeedbackText.CreateAndLaunch(
			LocalizationManager.SharedInstance.Localize("TODO!!"),
			new Vector2(0.5f, 0.5f),
			(RectTransform)this.GetComponentInParent<Canvas>().transform
		);
	}

	/// <summary>
	/// Double up button has been pressed.
	/// </summary>
	public void OnUseKeyButton() {
		// Remember decision
		m_usedKey = true;

		// Refresh visuals
		RefreshKeysField(true);

		// Perform transaction
		UsersManager.currentUser.SpendCurrency(UserProfile.Currency.KEYS, (ulong)DOUBLE_UP_COST_KEYS);

		// Update final score
		m_finalScore *= 2;
		m_finalScoreText.SetValue(m_finalScore, true);
	}

	/// <summary>
	/// Buy more keys button has been pressed.
	/// </summary>
	public void OnBuyMoreKeysButton() {
		// Open the shop!
		PopupController popup = PopupManager.LoadPopup(PopupCurrencyShop.PATH);
		PopupCurrencyShop shopPopup = popup.GetComponent<PopupCurrencyShop>();
		shopPopup.Init(PopupCurrencyShop.Mode.KEYS_ONLY);
		shopPopup.closeAfterPurchase = true;
		popup.Open();

		// Refresh visuals
		RefreshKeysField(true);
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
						m_usedKey ? 2f : 1f
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

			case Panel.LOG_IN:
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