﻿// HUDPc.cs
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
public class HUDPc : HudWidget {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

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
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.AddListener(GameEvents.INGAME_PC_FEEDBACK_END, OnPCFeedbackEnd);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
		Messenger.RemoveListener(GameEvents.INGAME_PC_FEEDBACK_END, OnPCFeedbackEnd);
	}

    protected override void Update() {
        base.Update();

		if (m_timer > 0) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				// Fade out
				m_canvasGroup.DOKill(false);
				m_canvasGroup.DOFade(0f, 2f).SetSpeedBased(true);	// Speed based because we are not necessarily at alpha 1
				m_timer = 0;
			}
		}
	}

    //------------------------------------------------------------------//
    // INTERNAL UTILS													//
    //------------------------------------------------------------------//   
    protected override string GetValueAsString() {
		return UIConstants.GetIconString(GetValue(), UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);
    }

    private long GetValue() {
        return RewardManager.pc;
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
			// Fade in
			m_canvasGroup.DOKill(false);
			m_canvasGroup.DOFade(1f, 2f).SetSpeedBased(true);	// Speed based because we are not necessarily at alpha 0
			m_timer = 4f;
		}
	}

	/// <summary>
	/// The PC feedback animation has finished, sync with this.
	/// </summary>
	private void OnPCFeedbackEnd() {
        UpdateValue(GetValue(), true);        	
	}
}

