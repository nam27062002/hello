﻿// GameHUD.cs
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
	public Button m_pauseButton;

	private bool m_paused = false;
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
    /*
    // Check back button on Android
	void Update(){

    }
    */

    void OnDestroy() {
        if (ApplicationManager.IsAlive && FeatureSettingsManager.IsDebugEnabled)
            Debug_OnDestroy();
    }    

    bool CanPause(){
    	if (!m_paused){
			if (m_pauseButton.IsInteractable()){
				return m_pauseButton.IsInteractable();
			}
			return false;
    	}
    	return false;
    }

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//

    void OnApplicationPause( bool pauseStatus){
    	if ( pauseStatus && CanPause() )
    	{
			OnPauseButton();
    	}
    }

	void Unpause(){
		m_paused = false;
	}

    /// <summary>
    /// Callback for the pause button.
    /// </summary>
    public void OnPauseButton() {
		// Skip if already paused (don't stack popups pausing the game)
		// https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-619
		if(m_paused) return;

		// Open the popup
		m_paused = true;
		PopupController popupController = PopupManager.OpenPopupInstant(PopupPause.PATH);

		// Be aware for when it's closed
		// Prevent adding callback twice!
		popupController.OnClosePostAnimation.RemoveListener(Unpause);
		popupController.OnClosePostAnimation.AddListener(Unpause);
	}

	/// <summary>
	/// Callback for the map button.
	/// </summary>
	public void OnMapButton() {
		// Skip if already paused (don't stack popups pausing the game)
		// https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-619
		if(m_paused) return;

		// Open the popup
		m_paused = true;
		PopupController popupController = PopupManager.OpenPopupInstant(PopupInGameMap.PATH);

		// Be aware for when it's closed
		// Prevent adding callback twice!
		popupController.OnClosePostAnimation.RemoveListener(Unpause);
		popupController.OnClosePostAnimation.AddListener(Unpause);
	}

	/// <summary>
	/// Callback for the missions button.
	/// </summary>
	public void OnMissionsButton() {
		// Skip if already paused (don't stack popups pausing the game)
		// https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-619
		if(m_paused) return;

		// Open the popup
		m_paused = true;
		PopupController popupController = PopupManager.OpenPopupInstant(PopupInGameMissions.PATH);

		// Be aware for when it's closed
		// Prevent adding callback twice!
		popupController.OnClosePostAnimation.RemoveListener(Unpause);
		popupController.OnClosePostAnimation.AddListener(Unpause);
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

	/// <summary>
	/// Temporal cheat to activate fire rush once.
	/// </summary>
	public void Debug_ResetFireRush() {
		// That should do trigger the fire rush!
		InstanceManager.player.breathBehaviour.AddFury(InstanceManager.player.breathBehaviour.furyMax - InstanceManager.player.breathBehaviour.currentFury);
	}
#endregion   
}
