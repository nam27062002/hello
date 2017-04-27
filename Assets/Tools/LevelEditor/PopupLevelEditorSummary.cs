// PopupLevelEditorSummary.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// End of game popup.
	/// </summary>
	[RequireComponent(typeof(PopupController))]
	public class PopupLevelEditorSummary : MonoBehaviour {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public const string PATH = "PF_PopupLevelEditorSummary";

		public enum Result {
			NONE,
			FINISH,
			REVIVE
		}

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		// Exposed members
		[SerializeField] TextMeshProUGUI scoreLabel = null;
		[SerializeField] TextMeshProUGUI timeLabel = null;
		[SerializeField] TextMeshProUGUI coinsLabel = null;

		// Popup result
		public Result result = Result.NONE;

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization
		/// </summary>
		void Awake() {
			// Check required fields
			DebugUtils.Assert(scoreLabel != null, "Required field not initialized!");
			DebugUtils.Assert(timeLabel != null, "Required field not initialized!");
			DebugUtils.Assert(coinsLabel != null, "Required field not initialized!");

			// Define popup controller delegates
			PopupController controller = GetComponent<PopupController>();
			DebugUtils.Assert(controller != null, "Required component!");
			controller.OnOpenPreAnimation.AddListener(OnOpenPreAnimation);

			Messenger.AddListener(GameEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPlayerPreFreeRevive);
		}

		void OnDestroy()
		{
			Messenger.RemoveListener(GameEvents.PLAYER_PET_PRE_FREE_REVIVE, OnPlayerPreFreeRevive);
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
			scoreLabel.text = StringUtils.FormatNumber(RewardManager.score);
			
			// Set time - format to MM:SS
			LevelEditorSceneController game = InstanceManager.gameSceneControllerBase as LevelEditorSceneController;
			timeLabel.text = TimeUtils.FormatTime(game.elapsedSeconds, TimeUtils.EFormat.ABBREVIATIONS, 2, TimeUtils.EPrecision.MINUTES);
			
			// Set initial coins
			coinsLabel.text = StringUtils.FormatNumber(RewardManager.coins);
		}

		//------------------------------------------------------------------//
		// CALLBACKS														//
		//------------------------------------------------------------------//
		/// <summary>
		/// The revive button has been pressed.
		/// </summary>
		public void OnReviveButton() {
			result = Result.REVIVE;
			GetComponent<PopupController>().Close(true);
		}

		/// <summary>
		/// The finish button has been pressed.
		/// </summary>
		public void OnFinishButton() {
			result = Result.FINISH;
			GetComponent<PopupController>().Close(true);
		}

		void OnPlayerPreFreeRevive()
		{
			Time.timeScale = 1;
			GetComponent<PopupController>().Close(true);
		}
	}
}
