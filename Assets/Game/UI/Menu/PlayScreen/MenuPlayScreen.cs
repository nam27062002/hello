// MenuPlayScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuPlayScreen : MonoBehaviour {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//
    //------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES											//
    //------------------------------------------------------------------//    
    // Internal
	private static bool m_firstTimeMenu = true;
	private static bool create_mods = true;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() 
	{   
        //create modifiers HERE
		if (create_mods) {
			InstanceManager.CREATE_MODIFIERS();
			InstanceManager.APPLY_MODIFIERS();
			create_mods=false;
		}
    }

    /// <summary>
    /// Component has been enabled.
    /// </summary>
    private void OnEnable() 
	{
        HDTrackingManager.Instance.Notify_MenuLoaded();       
    }

	private void Update() {
		if (m_firstTimeMenu) {
            FeatureSettingsManager.instance.AdjustScreenResolution(FeatureSettingsManager.instance.Device_CurrentFeatureSettings);
            m_firstTimeMenu = false;
        }
    }

    /// <summary>
    /// Component has been disabled.
    /// </summary>
    private void OnDisable() {
       
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
   	
    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
	/// <summary>
	/// The Privacy Policy button has been pressed.
	/// </summary>
	public void OnPrivacyPolicyButton() {
        #if UNITY_IOS
		    GameSettings.OpenUrl(GameSettings.PRIVACY_POLICY_IOS_URL, 0.25f);
        #else
		    GameSettings.OpenUrl(GameSettings.PRIVACY_POLICY_ANDROID_URL, 0.25f);
        #endif
    }
}