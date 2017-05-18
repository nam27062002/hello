// GameCameraDebug.cs
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
/// <summary>
/// Aux camera to render the game collisions in a custom way, making use of replacement shaders.
/// See http://technology.blurst.com/2009/03/11/camera-render-with-shader/
/// See http://docs.unity3d.com/Manual/SL-ShaderReplacement.html
/// See http://docs.unity3d.com/ScriptReference/Camera.RenderWithShader.html
/// </summary>
[RequireComponent(typeof(Camera))]
public class GameCameraDebug : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] Camera m_refCamera = null;
	[SerializeField] Shader m_collisionReplacementShader = null;

	// Internal
	private Camera m_camera = null;
	int m_cullingMask = 0;

    int m_collidersMask = 0;
	
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
		Messenger.AddListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnDebugSettingChanged);


	}

    private void Start()
    {
        m_cullingMask = m_camera.cullingMask;
        m_collidersMask = LayerMask.GetMask("Ground", "GroundVisible", "Player");

        // Initialize by simulating a toggle of the setting
        OnDebugSettingChanged(DebugSettings.SHOW_COLLISIONS, Prefs.GetBoolPlayer(DebugSettings.SHOW_COLLISIONS));
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<string, bool>(GameEvents.CP_BOOL_CHANGED, OnDebugSettingChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
	}

	/// <summary>
	/// Called after camera has rendered the scene.
	/// http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnRenderObject.html
	/// </summary>
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
	/// A debug setting has been changed.
	/// </summary>
	/// <param name="_id">ID of the changed setting.</param>
	/// <param name="_newValue">New value of the setting.</param>
	private void OnDebugSettingChanged(string _id, bool _newValue) {
		// Show collisions cheat?
		if(_id == DebugSettings.SHOW_COLLISIONS) {
            // Enable/Disable object
            Debug.Log("Show Collisions : " + _newValue);
            m_camera.cullingMask = _newValue ? m_collidersMask: m_cullingMask;
        }
	}
}