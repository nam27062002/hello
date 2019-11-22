// HUDPc.cs
// Hungry Dragon
// 

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller to update a textfield with the current amount of PC of the player.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class HUDPc : IHUDCounter {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private const long VALUE_ABBREVIATION_THRESHOLD = 99999;    // Values above this will get abbreviated

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//	
	private CanvasGroup m_canvasGroup = null;
	private float m_timer;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected override void Awake() {
		base.Awake();
		m_canvasGroup = GetComponent<CanvasGroup>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	protected override void Start() {
		base.Start();
		// Start hidden
		m_canvasGroup.alpha = 0f;
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.AddListener(MessengerEvents.UI_INGAME_PC_FEEDBACK_END, OnPCFeedbackEnd);
		Messenger.AddListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);    // Show during revive
		Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
	}

	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	protected override void OnDisable() {
		// Call parent
		base.OnDisable();

		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(MessengerEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.RemoveListener(MessengerEvents.UI_INGAME_PC_FEEDBACK_END, OnPCFeedbackEnd);
		Messenger.RemoveListener<DamageType, Transform>(MessengerEvents.PLAYER_KO, OnPlayerKo);
		Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnPlayerRevive);
	}

	public override void PeriodicUpdate() {
		base.PeriodicUpdate();

		if(m_timer > 0) {
			m_timer -= Time.unscaledDeltaTime;
			if(m_timer <= 0) {
				// Fade out
				Toggle(false);
			}
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//   
	protected override string GetValueAsString() {
		// If value is bigger than a certain amount, use abbreviated format
		if(Value > VALUE_ABBREVIATION_THRESHOLD) {
			return UIConstants.GetIconString(
				StringUtils.FormatBigNumber(Value, 2, VALUE_ABBREVIATION_THRESHOLD),
				UIConstants.IconType.PC,
				UIConstants.IconAlignment.RIGHT
			);
		} else {
			return UIConstants.GetIconString(
				Value,
				UIConstants.IconType.PC,
				UIConstants.IconAlignment.RIGHT
			);
		}
	}

	/// <summary>
	/// Trigger8
	/// </summary>
	/// <param name="_show">If set to <c>true</c> show.</param>
	private void Toggle(bool _show) {
		// Kill any current animation
		m_canvasGroup.DOKill(false);

		// Figure out target alpha
		float targetAlpha = _show ? 1f : 0f;

		// Launch animation!
		m_canvasGroup.DOFade(targetAlpha, 2f)
			.SetSpeedBased(true)    // Speed based because we are not necessarily at alpha 0
			.SetUpdate(UpdateType.Normal, true);    // Not affected by slow motion

		// Clear timer if hiding
		if(!_show) m_timer = 0;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A reward has been applied, show feedback for it.
	/// </summary>
	/// <param name="_reward">The reward that has been applied.</param>
	/// <param name="_entity">The entity that triggered the reward. Can be null.</param>
	private void OnRewardApplied(Reward _reward, Transform _entity) {
		// We only care about pc rewards
		if(_reward.pc > 0) {
			// Set value before reward (the actual value will be applied on the PCFeedbackEnd callback)
			UpdateValue(UsersManager.currentUser.pc - _reward.pc, false);

			// Fade in
			Toggle(true);
			m_timer = 4f;
		}
	}

	/// <summary>
	/// The PC feedback animation has finished, sync with this.
	/// </summary>
	private void OnPCFeedbackEnd() {
		UpdateValue(UsersManager.currentUser.pc, true);
	}

	/// <summary>
	/// The player is KO.
	/// </summary>
	private void OnPlayerKo(DamageType _type, Transform _source) {
		// Show with total current amount of PC
		UpdateValue(UsersManager.currentUser.pc, false);

		// Fade in
		Toggle(true);
		m_timer = 5f;   // Sync with the HUD Revive settings
	}

	/// <summary>
	/// The player has revived.
	/// </summary>
	/// <param name="_reason">How?</param>
	private void OnPlayerRevive(DragonPlayer.ReviveReason _reason) {
		// Hide!
		Toggle(false);
	}
}

