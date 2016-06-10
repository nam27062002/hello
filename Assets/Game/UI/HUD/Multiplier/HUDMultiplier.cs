// HUDMultiplier.cs
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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for the score multiplier display in the hud.
/// </summary>
[RequireComponent(typeof(Animator))]
public class HUDMultiplier : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string COMBO_SFX_PATH = "audio/sfx/UI/ComboFX/hsx_ui_combo_%U0";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private Text m_text = null;
	[SerializeField] private Text m_maskText = null;
	[SerializeField] private Image m_progressFill = null;
	[SerializeField] private ParticleSystem m_changePS = null;

	// Other external references
	private Animator m_anim = null;

	// Internal logic
	private int m_comboSFXIdx = 0;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_anim = GetComponent<Animator>();
		m_changePS.gameObject.SetActive(false);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		UpdateText(RewardManager.currentScoreMultiplier);
	}

	/// <summary>
	/// The spawner has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<ScoreMultiplier, ScoreMultiplier>(GameEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// If we have a valid multiplier, show progress to reach next one
		if(RewardManager.currentScoreMultiplier != RewardManager.defaultScoreMultiplier) {
			Vector3 scale = m_progressFill.rectTransform.localScale;
			scale.y = RewardManager.scoreMultiplierProgress;
			m_progressFill.rectTransform.localScale = scale;
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL UTILS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Updates the displayed multiplier.
	/// </summary>
	/// <param name="_mult">The multiplier we want to display.</param>
	private void UpdateText(ScoreMultiplier _mult) {
		// Do it! Except if going back to "no multiplier"
		if(_mult != null && _mult != RewardManager.defaultScoreMultiplier) {
			m_text.text = StringUtils.FormatNumber(_mult.multiplier, 0);
			m_maskText.text = m_text.text;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Current score multiplier has changed.
	/// </summary>
	/// <param name="_oldMultiplier">The previous multiplier.</param>
	/// <param name="_newMultiplier">The new multiplier.</param>
	private void OnMultiplierChanged(ScoreMultiplier _oldMultiplier, ScoreMultiplier _newMultiplier) {
		// Update text
		UpdateText(_newMultiplier);

		// Launch anim
		if(m_anim != null) {
			// If it's the default multiplier, fade out
			if(_newMultiplier == RewardManager.defaultScoreMultiplier) {
				// Make sure "in" trigger is consumed
				m_anim.ResetTrigger("in");
				m_anim.SetTrigger("out");

				// Reset combo index
				m_comboSFXIdx = 0;
			} else {
				// Make sure "out" trigger is consumed
				m_anim.ResetTrigger("out");
				m_anim.SetTrigger("in");
				m_anim.SetTrigger("changed");

				// Trigger particle effect as well
				m_changePS.gameObject.SetActive(true);
				m_changePS.Stop();
				m_changePS.Play();

				// And sound! ^^
				m_comboSFXIdx = Mathf.Min(m_comboSFXIdx + 1, 10);	// Increase combo index! Max combo audio available sounds.
				string audioPath = COMBO_SFX_PATH.Replace("%U0", m_comboSFXIdx.ToString());
				AudioManager.instance.PlayClip(audioPath);
			}
		}
	}
}
