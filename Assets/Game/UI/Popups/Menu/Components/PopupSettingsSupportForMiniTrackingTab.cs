// PopupSettingsSupportTab.cs
// Hungry Dragon
// 
// Created by David Germade on 25th May 2017.
// Copyright (c) 201y Ubisoft. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for handling the support tab in the settings popup when minitracking is enabled
/// </summary>
public class PopupSettingsSupportForMiniTrackingTab : MonoBehaviour
{      
    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    public void Awake() {	
        if (!FeatureSettingsManager.instance.IsMiniTrackingEnabled) {
            gameObject.SetActive(false);
        }	
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
	/// <summary>
	/// Mini Tracking file is sent to our server
	/// </summary>	
	public void OnSendTracking() {
        MiniTrackingEngine.SendTrackingFile(false,
           (FGOL.Server.Error error, GameServerManager.ServerResponse response) =>
           {
               if (error == null)
               {
                   Debug.Log("Play test tracking sent successfully");
                   PopupMessage.Config config = PopupMessage.GetConfig();
                   config.TitleTid = "";
                   config.MessageTid = "Tracking data sent successfully. Thanks!";
                   config.ConfirmButtonTid = "TID_GEN_OK";
                   config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;
                   PopupManager.PopupMessage_Open(config);
               }
               else
               {
                   Debug.Log("Error when sending play test tracking");

                   PopupMessage.Config config = PopupMessage.GetConfig();
                   config.TitleTid = "ERROR";
                   config.MessageTid = "Make sure your internet connection is active and try again";                   
                   config.ConfirmButtonTid = "TID_GEN_OK";
                   config.ButtonMode = PopupMessage.Config.EButtonsMode.Confirm;                                   
                   PopupManager.PopupMessage_Open(config);                       
               }               
           });        
	}
}