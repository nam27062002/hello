// PopupSummary_OLD.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System;
using UnityEngine.UI;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// End of game popup.
/// </summary>
public class PopupSummary_OLD : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	public static readonly string PATH = "Proto/Prefabs/UI/Popups/PF_PopupSummary";
	#endregion

	#region EXPOSED MEMBERS ----------------------------------------------------------------------------------------------------
	[SerializeField] NumberTextAnimator scoreAnimator = null;
	[SerializeField] Text timeLabel = null;
	[SerializeField] NumberTextAnimator coinsAnimator = null;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization
	/// </summary>
	void Start() {
		// Check required fields
		DebugUtils.Assert(scoreAnimator != null, "Required field not initialized!");
		DebugUtils.Assert(timeLabel != null, "Required field not initialized!");
		DebugUtils.Assert(coinsAnimator != null, "Required field not initialized!");

		// Initialize some of the popup's members
		// Set score
		scoreAnimator.SetValue(0, (int)App.Instance.gameLogic.score);

		// Set time - format to MM:SS
		long time = (long)App.Instance.gameLogic.elapsedSeconds;
		long iMins = time/60;
		long iSecs = time - (iMins * 60);
		timeLabel.text = String.Format("{0:00}:{1:00}", iMins, iSecs);

		// Set coins
		coinsAnimator.SetValue(0, (int)App.Instance.userData.coins);
	}
	
	/// <summary>
	/// Called every frame
	/// </summary>
	void Update() {
		
	}
	#endregion

	#region BUTTON CALLBACKS -------------------------------------------------------------------------------------------
	/// <summary
	/// The accept button has been clicked.
	/// </summary>
	public void OnAcceptClick() {
		// Go to main menu
		App.Instance.flowManager.GoToScene(FlowManager_OLD.EScenes.MAIN_MENU);
	}
	#endregion
}
#endregion
