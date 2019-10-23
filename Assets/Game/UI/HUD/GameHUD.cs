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
using InControl;
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
	public GameObject m_mapButtonGodRays;
	public Button m_pauseButton;
    public GameObject m_miscGroup;
    public Animator m_fireRushGroup;

	[Space]
	public Component[] m_toAutoDestroy = new Component[0];
	[Min(0)]
	public int m_autoDestroyDelayFrames = 1;

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
        InstanceManager.gameHUD = this;
        Messenger.AddListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
    }

    void OnDestroy() {
        if (ApplicationManager.IsAlive && FeatureSettingsManager.IsDebugEnabled)
            Debug_OnDestroy();
        InstanceManager.gameHUD = null;
        Messenger.RemoveListener<DragonPlayer.ReviveReason>(MessengerEvents.PLAYER_REVIVE, OnRevive);
    }    

    public bool CanPause(){		
    	if (!m_paused){
			if (m_pauseButton.IsInteractable()){
				if (InstanceManager.gameSceneController.state == GameSceneController.EStates.RUNNING) {
					if (!InstanceManager.player.changingArea) {
						return m_pauseButton.IsInteractable();
					}
				}
			}
			return false;
    	}
    	return false;
    }

    private void Update() {
		// Check for auto-destruction components
		if(m_autoDestroyDelayFrames >= 0) {
			m_autoDestroyDelayFrames--;
			if(m_autoDestroyDelayFrames <= 0) {
				m_autoDestroyDelayFrames = -1;  // Don't trigger again
				for(int i = 0; i < m_toAutoDestroy.Length; ++i) {
					Destroy(m_toAutoDestroy[i]);
					m_toAutoDestroy[i] = null;
				}
			}
		}

		// Update input
		if (!m_paused) {
            InputDevice device = InputManager.ActiveDevice;

            if (device != null && device.IsActive) {
                if (device.CommandIsPressed) {
                    OnPauseButton();
                } else if (device.Action4.WasPressed) {
                    OnMapButton();
                }
            }
        }
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

		m_mapButtonGodRays.SetActive(false);

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

    void OnRevive(DragonPlayer.ReviveReason _reason) {
        m_fireRushGroup.SetBool(GameConstants.Animator.ENABLED, _reason != DragonPlayer.ReviveReason.MUMMY);
    }

#region debug
    private void Debug_Awake() {
        Messenger.AddListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, Debug_OnChanged);

        // Enable/Disable object depending on the flag
        Debug_SetActive();
    }

    private void Debug_OnDestroy() {
		Messenger.RemoveListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, Debug_OnChanged);
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
		gameObject.SetActive(DebugSettings.ingameHud);
		if(m_speedGameObject != null) m_speedGameObject.SetActive( DebugSettings.showSpeed );
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
