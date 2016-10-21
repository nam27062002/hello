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
		Messenger.AddListener<string, bool>(GameEvents.DEBUG_SETTING_CHANGED, OnDebugSettingChanged);

		// Initialize by simulating a toggle of the setting
		OnDebugSettingChanged(DebugSettings.SHOW_COLLISIONS, DebugSettings.Get(DebugSettings.SHOW_COLLISIONS));
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<string, bool>(GameEvents.DEBUG_SETTING_CHANGED, OnDebugSettingChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Restore default shader
		m_camera.ResetReplacementShader();
	}

	/// <summary>
	/// Called after camera has rendered the scene.
	/// http://docs.unity3d.com/ScriptReference/MonoBehaviour.OnRenderObject.html
	/// </summary>
	private void Update() {
		// Backup some camera settings that we don't want to override
		m_cullingMask = m_camera.cullingMask;

		// Update settings from reference camera
		m_camera.CopyFrom(m_refCamera);
		m_camera.SetReplacementShader(m_collisionReplacementShader, "ReplacementShaderID");

		// Restore some settings
		m_camera.cullingMask = m_cullingMask;
		m_camera.depth += 1;	// Render on top of the reference camera
		m_camera.clearFlags = CameraClearFlags.Depth;	// Depth-only, we want to see the reference camera
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
			this.gameObject.SetActive(_newValue);
		}
	}
}