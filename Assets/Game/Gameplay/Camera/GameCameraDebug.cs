﻿// GameCameraDebug.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
[RequireComponent(typeof(Camera))]
public class GameCameraDebug : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
//	[SerializeField] Camera m_refCamera = null;

	// Internal
	private Camera m_camera = null;
	int m_cullingMask = 0;

    int m_collidersMask = 0;


    public Texture2D m_matCapTex = null;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
		// Get camera
		m_camera = GetComponent<Camera>();

		// Subscribe to external events
		Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnDebugSettingChangedShowCollisions);
        Messenger.AddListener<string, float>(GameEvents.CP_FLOAT_CHANGED, OnDebugSettingChangedResolutionFactor);

        Shader.SetGlobalTexture("_MatCap", m_matCapTex);
    }

    public float m_resolutionFactor = 0.25f;

    private void Start()
    {
        m_cullingMask = m_camera.cullingMask;
        m_collidersMask = LayerMask.GetMask("Ground", "GroundVisible", "Player", "AirPreys", "WaterPreys", "MachinePreys", "GroundPreys", "Mines");

        // Initialize by simulating a toggle of the setting
        OnDebugSettingChangedShowCollisions(DebugSettings.SHOW_COLLISIONS, Prefs.GetBoolPlayer(DebugSettings.SHOW_COLLISIONS));
        OnDebugSettingChangedResolutionFactor(DebugSettings.RESOLUTION_FACTOR, Prefs.GetFloatPlayer(DebugSettings.RESOLUTION_FACTOR));
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnDebugSettingChangedShowCollisions);
        Messenger.RemoveListener<string, float>(GameEvents.CP_FLOAT_CHANGED, OnDebugSettingChangedResolutionFactor);
    }

    /// <summary>
    /// Component has been disabled.
    /// </summary>
    private void OnDisable() {
	}

	private void Update() {
        // Backup some camera settings that we don't want to override

    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Show Collisions Toggle.
    /// </summary>
    /// <param name="_id">ID of the changed setting.</param>
    /// <param name="_newValue">New value of the setting.</param>
    private void OnDebugSettingChangedShowCollisions(string _id, bool _newValue) {
		// Show collisions cheat?
		if(_id == DebugSettings.SHOW_COLLISIONS) {
            // Enable/Disable object
            Debug.Log("Show Collisions : " + _newValue);
            m_camera.cullingMask = _newValue ? m_collidersMask: m_cullingMask;
        }
	}

    /// <summary>
    /// A debug setting has been changed.
    /// </summary>
    /// <param name="_id">ID of the changed setting.</param>
    /// <param name="_newValue">New value of the setting.</param>
    private float[] validFactors = new float[] { 360.0f, 720.0f, 1080.0f };
    private void OnDebugSettingChangedResolutionFactor(string _id, float _newValue)
    {
        // Show collisions cheat?
        if (_id == DebugSettings.RESOLUTION_FACTOR)
        {
            _newValue = Mathf.Clamp(_newValue, validFactors[0], validFactors[2]);
            float vf = validFactors[2];

            for (int c = 0; c < validFactors.Length; c++)
            {
                if (validFactors[c] > _newValue)
                {
                    vf = validFactors[c - 1];
                    break;
                }
            }


            float rFactor = vf / (float)Screen.height;


            int width = (int)((float)Screen.width * rFactor);
            int height = (int)((float)Screen.height * rFactor);

            Screen.SetResolution(width, height, true);

            Debug.Log("Resolution Factor = " + vf);
        }
    }


}