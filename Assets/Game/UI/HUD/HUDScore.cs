// HUDScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a score counter in the hud.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class HUDScore : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private TextMeshProUGUI m_valueTxt;
	private Animator m_anim;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_valueTxt = GetComponent<TextMeshProUGUI>();
		m_valueTxt.text = "0";

		m_anim = GetComponent<Animator>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
        LastScorePrinted = -1;
        NeedsToUpdateScore = true;
        UpdateScore();
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<Reward, Transform>(GameEvents.REWARD_APPLIED, OnRewardApplied);
	}

    private void Update() {
        if (NeedsToUpdateScore) {
            UpdateScore();
            NeedsToUpdateScore = false;
        }        
    }

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the displayed score.
	/// </summary>
	private void UpdateScore() {
        if (LastScorePrinted != RewardManager.score) {
            // Do it!
            m_valueTxt.text = StringUtils.FormatNumber(RewardManager.score);

            LastScorePrinted = RewardManager.score;
            NeedsToUpdateScore = false;
        }
    }

    private bool NeedsToUpdateScore { get; set; }
    private long LastScorePrinted { get; set; }

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A reward has been applied, show feedback for it.
	/// </summary>
	/// <param name="_reward">The reward that has been applied.</param>
	/// <param name="_entity">The entity that triggered the reward. Can be null.</param>
	private void OnRewardApplied(Reward _reward, Transform _entity) {
		// We only care about score rewards
		if(_reward.score > 0) {
            NeedsToUpdateScore = true;
			if(m_anim != null) m_anim.SetTrigger("start");
		}
	}
}
