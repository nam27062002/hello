// PopupAskSurvey.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// https://mdc-web-tomcat17.ubisoft.org/confluence/display/ubm/%5BHD%5D+In-Game+Survey
/// </summary>
public class PopupAskSurvey : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Message/PF_PopupSurvey";

	public const string SURVEY_URL = "https://ubisoft.ca1.qualtrics.com/jfe/form/SV_23Kqa1zeNPP6rT7?UID=%USER_ID%";	// Replace %USER_ID% with actual user tracking ID

	public const string PREF_CHECK = "PopupAskSurvey.Check";
	public const string PREF_LAST_DISPLAYED_SESSION = "PopupAskSurvey.LastDisplayedSession";

	public const string MIN_OWNED_DRAGON = "dragon_reptile";
	public const int MIN_RUNS = 3;
	public const int MIN_SESSIONS = 3;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the popup must be triggered considering the current profile.
	/// If all checks are passed, opens the popup.
	/// </summary>
	/// <returns>The opened popup if all conditions to display it are met. <c>null</c> otherwise.</returns>
	public static PopupController CheckAndOpen() {
		return null;	// [AOC] As of version 1.3, disable temporarily the survey popup

		// Not if already checked!
		if(!Prefs.GetBoolPlayer(PREF_CHECK, true)) return null;

		// Not if we don't have internet access!
		if(DeviceUtilsManager.SharedInstance.internetReachability == NetworkReachability.NotReachable) return null;

		// Not if we don't have a tracking ID
		#if !UNITY_EDITOR
		if(HDTrackingManager.Instance.GetDNAProfileID() == null) return null;
		#endif

		// Not if target min dragon is not properly defined
		IDragonData minDragon = DragonManager.GetDragonData(MIN_OWNED_DRAGON);
		if(minDragon == null) return null;	// Something went really wrong

		// Not if target min dragon not yet owned (or bigger one)
		// Check whether player owns a dragon bigger than the min required and has played at least MIN_RUNS with it
		int minOwnedDragonOrder = minDragon.GetOrder();
		IDragonData targetDragon = null;
		List<IDragonData> dragonsByOrder = DragonManager.GetDragonsByOrder(IDragonData.Type.CLASSIC);
		for(int i = minOwnedDragonOrder; i < dragonsByOrder.Count; ++i) {
			if(dragonsByOrder[i].isOwned && dragonsByOrder[i].gamesPlayed >= MIN_RUNS) {
				targetDragon = dragonsByOrder[i];
				break;
			}
		}
		if(targetDragon == null) return null;

		// Only after a run!
		if(GameSceneManager.prevScene != ResultsScreenController.NAME 
		&& GameSceneManager.prevScene != GameSceneController.NAME) {
			return null;
		}

		// Not if not enough sessions have passed since last time we showed the popup
		if(HDTrackingManager.Instance.TrackingPersistenceSystem.SessionCount < Prefs.GetIntPlayer(PREF_LAST_DISPLAYED_SESSION, 0) + MIN_SESSIONS) return null;

		// All checks passed! Popup can be displayed!
		return PopupManager.EnqueuePopup(PATH);
	}

	/// <summary>
	/// Sends the tracking and close the popup.
	/// </summary>
	/// <param name="_action">Tha chosen action.</param>
	private void CloseAndSendTracking(HDTrackingManager.EPopupSurveyAction _action) {
		// Tracking
		HDTrackingManager.Instance.Notify_PopupSurveyShown(_action);

		// Close the popup!
		GetComponent<PopupController>().Close(true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Poppup is about to be displayed.
	/// </summary>
	public void OnShowPreAnimation() {
		// Update tracking vars
		Prefs.SetIntPlayer(PREF_LAST_DISPLAYED_SESSION, HDTrackingManager.Instance.TrackingPersistenceSystem.SessionCount);
	}

	/// <summary>
	/// Yes button has been pressed.
	/// </summary>
	public void OnYes() {
		// Close the popup and set to never show again
		Prefs.SetBoolPlayer(PREF_CHECK, false);
		CloseAndSendTracking(HDTrackingManager.EPopupSurveyAction.Yes);

		// Open external survey
		// Add some delay to give enough time for SFX to be played before losing focus
		string url = SURVEY_URL.Replace("%USER_ID%", HDTrackingManager.Instance.GetDNAProfileID());
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				Application.OpenURL(url);
			}, 0.15f
		);
	}

	/// <summary>
	/// No button has been pressed.
	/// </summary>
	public void OnNo() {
		// Close the popup and set to never show again
		Prefs.SetBoolPlayer(PREF_CHECK, false);
		CloseAndSendTracking(HDTrackingManager.EPopupSurveyAction.No);
	}

	/// <summary>
	/// Later button has been pressed.
	/// </summary>
	public void OnLater() {
		// Close the popup
		CloseAndSendTracking(HDTrackingManager.EPopupSurveyAction.Later);
	}
}