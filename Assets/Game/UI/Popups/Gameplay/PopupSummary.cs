// PopupSummary.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// End of game popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupSummary : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Gameplay/PF_PopupSummary";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[SerializeField] NumberTextAnimator scoreAnimator = null;
	[SerializeField] Text timeLabel = null;
	[SerializeField] NumberTextAnimator coinsAnimator = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		// Check required fields
		DebugUtils.Assert(scoreAnimator != null, "Required field not initialized!");
		DebugUtils.Assert(timeLabel != null, "Required field not initialized!");
		DebugUtils.Assert(coinsAnimator != null, "Required field not initialized!");

		// Define popup controller delegates
		PopupController controller = GetComponent<PopupController>();
		DebugUtils.Assert(controller != null, "Required component!");
		controller.onOpenPreAnimationDelegate = OnOpenPreAnimation;
		controller.onOpenPostAnimationDelegate = OnOpenPostAnimation;
		controller.onClosePostAnimationDelegate = OnClosePostAnimation;
	}
	
	/// <summary>
	/// Called every frame
	/// </summary>
	void Update() {
		
	}

	//------------------------------------------------------------------//
	// DELEGATES														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Actions to perform right before the popup is opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Initialize some of the popup's members
		// Set initial score
		scoreAnimator.SetValue(0, 0);
		
		// Set time - format to MM:SS
		GameSceneController game = InstanceManager.GetSceneController<GameSceneController>();
		timeLabel.text = TimeUtils.FormatTime(game.elapsedSeconds, TimeUtils.EFormat.ABBREVIATIONS, 2, TimeUtils.EPrecision.MINUTES);
		
		// Set initial coins
		coinsAnimator.SetValue(0, 0);
	}

	/// <summary>
	/// Actions to perform right after the popup is opened.
	/// </summary>
	public void OnOpenPostAnimation() {
		// Launch number animators
		scoreAnimator.SetValue(0, (int)RewardManager.score);
		coinsAnimator.SetValue(0, (int)RewardManager.coins);
	}

	/// <summary>
	/// Actions to perform right after the popup is closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// Go back to main menu
		FlowManager.GoToMenu();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary
	/// The accept button has been clicked.
	/// </summary>
	public void OnAcceptClick() {
		// Trigger the close animation
		GetComponent<PopupController>().Close(true);
	}
}
