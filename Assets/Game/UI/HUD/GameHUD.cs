// GameHUD.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/12/2015.
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
/// Root controller for the in-game HUD prefab.
/// </summary>
public class GameHUD : MonoBehaviour {
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public GameObject m_speedGameObject;
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
        if (FeatureSettingsManager.IsDebugEnabled)
            Debug_Awake();        
    }

    void OnDestroy() {
        if (ApplicationManager.IsAlive && FeatureSettingsManager.IsDebugEnabled)
            Debug_OnDestroy();
    }    

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    /// <summary>
    /// Callback for the pause button.
    /// </summary>
    public void OnPauseButton() {
		// Open pause popup
		PopupController pausePopup = PopupManager.OpenPopupInstant(PopupPause.PATH);
		pausePopup.GetComponent<PopupPause>().GoToTab(PopupPause.Tabs.OPTIONS);
	}

	/// <summary>
	/// Callback for the map button.
	/// </summary>
	public void OnMapButton() {
		PopupManager.OpenPopupInstant(PopupMap.PATH);
	}

	/// <summary>
	/// Callback for the missions button.
	/// </summary>
	public void OnMissionsButton() {
		PopupController pausePopup = PopupManager.OpenPopupInstant(PopupPause.PATH);
		pausePopup.GetComponent<PopupPause>().GoToTab(PopupPause.Tabs.MISSIONS);
	}

#region debug
    private void Debug_Awake() {
        Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);

        // Enable/Disable object depending on the flag
        Debug_SetActive();
    }

    private void Debug_OnDestroy() {
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnChanged(string _id, bool _newValue) {        
        if (_id == DebugSettings.INGAME_HUD)
        {
            // Enable/Disable object
            Debug_SetActive();
        }
        else if ( _id == DebugSettings.SHOW_SPEED )
        {
			m_speedGameObject.SetActive( _newValue );
        }
    }

    private void Debug_SetActive() {
		gameObject.SetActive(Prefs.GetBoolPlayer(DebugSettings.INGAME_HUD, true));
		if(m_speedGameObject != null) m_speedGameObject.SetActive( Prefs.GetBoolPlayer(DebugSettings.SHOW_SPEED) );
    }
#endregion   
}
