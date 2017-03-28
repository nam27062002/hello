// ResultsScreenChestsPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Chests progress info pill for the results screen carousel.
/// </summary>
public class ResultsScreenChestsPill : ResultsScreenCarouselPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// External refs
	[SerializeField] private Localizer m_collectedText = null;
	[SerializeField] private TextMeshProUGUI m_timerText = null;
	[SerializeField] private Slider m_timerBar = null;

	// Internal logic
	private int m_initialChests = 0;
	private int m_finalChests = 0;
	private int m_processedChests = 0;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		if(!isActiveAndEnabled) return;

		// Refresh time
		RefreshTime();
	}

	//------------------------------------------------------------------------//
	// ResultsScreenCarouselPill IMPLEMENTATION								  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this pill must be displayed on the carousel or not.
	/// </summary>
	/// <returns><c>true</c> if the pill must be displayed on the carousel, <c>false</c> otherwise.</returns>
	public override bool MustBeDisplayed() {
		// Always display for now
		return true;
	}

	/// <summary>
	/// Initializes, shows and animates the pill.
	/// The <c>OnFinished</c> event will be invoked once the animation has finished.
	/// </summary>
	protected override void StartInternal() {
		// Compute required data
		InitData();

		// Initialize animation
		m_processedChests = m_initialChests;
		RefreshTexts(false);
		RefreshTime();

		// Show ourselves!
		gameObject.SetActive(true);
		animator.Show();

		// Animation is controlled from outside (to be synced with the 3D animation)
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize all the required data to be displayed.
	/// </summary>
	private void InitData() {
		// How many chests?
		// Override if cheating
		if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.NONE) {
			// Real logic
			m_initialChests = ChestManager.collectedChests;
			m_finalChests = ChestManager.collectedAndPendingChests;
		} else {
			// [AOC] DEBUG ONLY!!
			int numCollectedChests = CPResultsScreenTest.chestsMode - CPResultsScreenTest.ChestTestMode.FIXED_0;
			if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.RANDOM) {
				numCollectedChests = UnityEngine.Random.Range(0, 5);
			}

			m_initialChests = ChestManager.collectedChests;
			m_finalChests = m_initialChests + numCollectedChests;
			if(m_finalChests > ChestManager.NUM_DAILY_CHESTS) {
				m_finalChests = ChestManager.NUM_DAILY_CHESTS;
				m_initialChests = m_finalChests - numCollectedChests;	// Tweak initial chest count to avoid overflowing max
			}
		}
	}

	/// <summary>
	/// Show the pill once finished.
	/// </summary>
	public void ShowCompleted() {
		// Make sure we have the latest data
		InitData();
		
		// Set texts to final value
		m_processedChests = m_finalChests;
		RefreshTexts(false);
		RefreshTime();

		// Show ourselves!
		gameObject.SetActive(true);
		animator.Show();
	}

	/// <summary>
	/// Increses the chest count and animates the text.
	/// </summary>
	public void IncreaseChestCount() {
		// Make sure we're within limits
		if(m_processedChests < m_finalChests) {
			m_processedChests++;
			RefreshTexts(true);
		} else {
			RefreshTexts(false);
		}
	}

	/// <summary>
	/// Refresh counter with the current data.
	/// </summary>
	/// <param name="_animate">Whether to show a small animation or not.</param>
	private void RefreshTexts(bool _animate) {
		// Collected count
		if(m_collectedText != null) {
			m_collectedText.Localize("TID_CHEST_DAILY_DESC", StringUtils.FormatNumber(m_processedChests), StringUtils.FormatNumber(ChestManager.NUM_DAILY_CHESTS));

			// Animate?
			if(_animate) {
				// Play some SFX?
				m_collectedText.transform.DOKill(true);
				m_collectedText.transform.DOScale(3f, 0.15f).SetLoops(2, LoopType.Yoyo);
			}
		}
	}

	/// <summary>
	/// Refresh timer info.
	/// </summary>
	private void RefreshTime() {
		// Aux vars
		TimeSpan timeToReset = ChestManager.timeToReset;

		// Text
		if(m_timerText != null) {
			m_timerText.text = TimeUtils.FormatTime(timeToReset.TotalSeconds, TimeUtils.EFormat.DIGITS, 3, TimeUtils.EPrecision.HOURS, true);
		}

		// Bar
		if(m_timerBar != null) {
			m_timerBar.normalizedValue = (float)((ChestManager.RESET_PERIOD - timeToReset.TotalHours)/ChestManager.RESET_PERIOD);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}