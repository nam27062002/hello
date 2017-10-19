// ResultsScreenSummary.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/10/2017.
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
/// Small summary for the results screen
/// </summary>
public class ResultsScreenSummary : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[Separator("Textfields")]
	[SerializeField] private TextMeshProUGUI m_timeText = null;
	[SerializeField] private TextMeshProUGUI m_scoreText = null;
	[SerializeField] private TextMeshProUGUI m_coinsText = null;
	[SerializeField] private TextMeshProUGUI m_chestsText = null;
	[SerializeField] private TextMeshProUGUI m_eggsText = null;
	[SerializeField] private TextMeshProUGUI m_missionsText = null;

	[Separator("Animators")]
	[SerializeField] private ShowHideAnimator m_timeAnim = null;
	[SerializeField] private ShowHideAnimator m_scoreAnim = null;
	[SerializeField] private ShowHideAnimator m_coinsAnim = null;
	[SerializeField] private ShowHideAnimator m_collectiblesAnim = null;
	[SerializeField] private ShowHideAnimator m_missionsAnim = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	public void InitSummary() {

	}

	public void ShowTime(float _timeSeconds) {

	}

	public void ShowScore(long _score) {

	}

	public void ShowCoins(long _coins) {

	}

	public void ShowCollectibles(int _chests, int _eggs) {

	}

	public void ShowMissions(int _missions) {

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}