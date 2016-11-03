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

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
#if !PRODUCTION
        Debug_Awake();        
#endif
    }

    void OnDestroy() {
#if !PRODUCTION
        Debug_OnDestroy();
#endif
    }    

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    /// <summary>
    /// Callback for the pause button.
    /// </summary>
    public void OnPauseButton() {
		// Open pause popup
		PopupManager.OpenPopupInstant(PopupPause.PATH);
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
    }

    private void Debug_SetActive() {
		gameObject.SetActive(Prefs.GetBoolPlayer(DebugSettings.INGAME_HUD));
    }
#endregion   
}
