// RewardScreenAnimationEvents.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Component to be attached to individual elements of a RewardsScreen so they are
/// able to communicate with the parent screen logic via Animation events.
/// </summary>
public class RewardScreenAnimationEvents : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private IRewardScreen m_parentScreen = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Debug.Assert(m_parentScreen != null, "Required field not initialized!");
	}

	//------------------------------------------------------------------------//
	// ANIMATION EVENTS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Tell the parent screen to throw next step.
	/// </summary>
	public void AdvanceStep() {
		m_parentScreen.AdvanceStep();
	}

	/// <summary>
	/// Tell parent screen to show Tap To Continue.
	/// </summary>
	public void ShowTapToContinue() {
		m_parentScreen.ToggleTapToContinue(true);
	}

	/// <summary>
	/// Tell parent screen to hide Tap To Continue.
	/// </summary>
	public void HideTapToContinue() {
		m_parentScreen.ToggleTapToContinue(false);
	}

	/// <summary>
	/// Tell parent screen to switch animation state.
	/// </summary>
	public void SetAnimationState(IRewardScreen.State _state) {
		m_parentScreen.SetAnimatingState(_state);
	}

	/// <summary>
	/// Tell parent screen to switch league info.
	/// </summary>
	public void SwitchLeagueInfo() {
		(m_parentScreen as LeaguesRewardScreen).OnSwitchLeagueInfo();
	}
}