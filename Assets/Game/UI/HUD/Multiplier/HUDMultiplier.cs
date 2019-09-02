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
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for the score multiplier display in the hud.
/// </summary>
[RequireComponent(typeof(Animator))]
public class HUDMultiplier : IHUDCounter {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string COMBO_SFX_PATH = "ui_combo_%U0";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Exposed setup	
	[SerializeField] private Text m_maskText = null;
    [SerializeField] private TextMeshProUGUI m_overlayText = null;
	[SerializeField] private TextMeshProUGUI m_progressFillText = null;
    [SerializeField] private Image m_progressFill = null;
	[SerializeField] private ParticleSystem m_changePS = null;
    
	// Internal logic
	private int m_comboSFXIdx = 0;
    private long m_multiplierToShow;
	private Vector2 m_fillTextureOffset = GameConstants.Vector2.zero;
    
    private Material m_fontMaterial;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    protected override void Awake() {
        base.Awake();
		m_changePS.gameObject.SetActive(false);
	}

    protected override void Start() {
        base.Start();

        m_fontMaterial = m_progressFillText.fontMaterial;

        SetMultiplierToShow(RewardManager.currentScoreMultiplierData,RewardManager.currentFireRushMultiplier,true);                
		UpdateProgress();
    }

    /// <summary>
    /// The spawner has been enabled.
    /// </summary>
    protected override void OnEnable() {
		// Call parent
		base.OnEnable();

		// Subscribe to external events
		Messenger.AddListener<ScoreMultiplier, float>(MessengerEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}
	
	/// <summary>
	/// The spawner has been disabled.
	/// </summary>
	protected override void OnDisable() {
		// Call parent
		base.OnDisable();

		// Unsubscribe from external events
		Messenger.RemoveListener<ScoreMultiplier, float>(MessengerEvents.SCORE_MULTIPLIER_CHANGED, OnMultiplierChanged);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	public override void PeriodicUpdate() {
        base.PeriodicUpdate();

		// If we have a valid multiplier, show progress to reach next one
		if(RewardManager.currentScoreMultiplierData != RewardManager.defaultScoreMultiplier) {
			UpdateProgress();
		}
	}

    //------------------------------------------------------------------//
    // INTERNAL UTILS													//
    //------------------------------------------------------------------//
	private void UpdateProgress() {
		if(m_progressFill != null) {
			Vector3 scale = m_progressFill.rectTransform.localScale;
			scale.y = RewardManager.scoreMultiplierProgress;
			m_progressFill.rectTransform.localScale = scale;
		}

		if(m_progressFillText != null) {
			// The fill texture is setup in a way that the top half is transparent and the bottom half is tinted
			// When empty (value 0), map the top half to the text mesh
			// When full (value 1), map the bottom half to the text mesh
			// In between, interpolate fill texture offsetY between 0 and -0.5
			m_fillTextureOffset.y = Mathf.Lerp(0, -0.5f, RewardManager.scoreMultiplierProgress);
			m_fontMaterial.SetTextureOffset(ShaderUtilities.ID_FaceTex, m_fillTextureOffset);
		}
	}

    private void SetMultiplierToShow(ScoreMultiplier _mult, float fireRushMultiplier ,bool immediate)
    {
        // We just keep the integer part
		m_multiplierToShow = (long)(_mult.multiplier * fireRushMultiplier);

        // Do it! Except if going back to "no multiplier"
        // if (_mult != null && _mult != RewardManager.defaultScoreMultiplier)
        if ( m_multiplierToShow > 1 )
        {            
            UpdateValue(m_multiplierToShow, false, immediate);
        }
    }

    protected override string GetValueAsString() {
        return StringUtils.FormatNumber(m_multiplierToShow);
    }


    /// <summary>
    /// Updates the displayed multiplier.
    /// </summary>
    /// <param name="_mult">The multiplier we want to display.</param>
    protected override void PrintValueExtended() {
        base.PrintValueExtended();
        if (m_maskText != null)
            m_maskText.text = m_valueTxt.text;

		if(m_progressFillText != null)
			m_progressFillText.text = m_valueTxt.text;

        if (m_overlayText != null)
            m_overlayText.text = m_valueTxt.text;
    }
       
    protected override void PlayAnimExtended() {
        // Launch anim
        if (m_anim != null)
        {
            long m_defaultScoreMultiplier = (long)RewardManager.defaultScoreMultiplier.multiplier;

            // If it's the default multiplier, fade out
            if (m_multiplierToShow == m_defaultScoreMultiplier)
            {
                // Make sure "in" trigger is consumed
				m_anim.ResetTrigger( GameConstants.Animator.IN);
				m_anim.SetTrigger( GameConstants.Animator.OUT);

                // Reset combo index
                m_comboSFXIdx = 0;
            }
            else
            {
                // Make sure "out" trigger is consumed
				m_anim.ResetTrigger( GameConstants.Animator.OUT );
				m_anim.SetTrigger( GameConstants.Animator.IN );
				m_anim.SetTrigger( GameConstants.Animator.CHANGE );

                // Trigger particle effect as well
                m_changePS.gameObject.SetActive(true);
                m_changePS.Stop();
                m_changePS.Play();

                // And sound! ^^
                m_comboSFXIdx = Mathf.Min(m_comboSFXIdx + 1, 10);   // Increase combo index! Max combo audio available sounds.
                string audioPath = COMBO_SFX_PATH.Replace("%U0", m_comboSFXIdx.ToString());
                AudioController.Play(audioPath);
            }
        }
    }

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    /// <summary>
    /// Current score multiplier has changed.
    /// </summary>
    /// <param name="_newMultiplier">The new multiplier.</param>
    private void OnMultiplierChanged(ScoreMultiplier _newMultiplier, float fireRushMultiplier) {
        // Update text
		SetMultiplierToShow(_newMultiplier, fireRushMultiplier, false);
        NeedsToPlayAnim = true;
    }
}
