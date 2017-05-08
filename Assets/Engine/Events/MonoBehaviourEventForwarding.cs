// MonoBehaviourEventForwarding.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Propagate default monobehaviour's messages using unity events.
/// Feel free to add other messages (collision, input, render, etc.).
/// </summary>
[ExecuteInEditMode]
public class MonoBehaviourEventForwarding : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed events
	[Header("Initialization")]
	[SerializeField] private UnityEvent m_onAwake = null;
	[SerializeField] private UnityEvent m_onStart = null;
	[SerializeField] private UnityEvent m_onEnable = null;

	[Header("Update")]
	[SerializeField] private UnityEvent m_onUpdate = null;
	[SerializeField] private UnityEvent m_onLateUpdate = null;
	[SerializeField] private UnityEvent m_onFixedUpdate = null;

	[Header("Finalization")]
	[SerializeField] private UnityEvent m_onDisable = null;
	[SerializeField] private UnityEvent m_onDestroy = null;

	[Header("Debug")]
	[SerializeField] private UnityEvent m_onDrawGizmos = null;
	[SerializeField] private UnityEvent m_onDrawGizmosSelected = null;

	[Header("Editor")]
	[SerializeField] private UnityEvent m_onValidate = null;
	[SerializeField] private UnityEvent m_onGUI = null;

	//------------------------------------------------------------------------//
	// INITIALIZATION														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		if(m_onAwake != null && Application.isPlaying) m_onAwake.Invoke();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		if(m_onStart != null && Application.isPlaying) m_onStart.Invoke();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		if(m_onEnable != null && Application.isPlaying) m_onEnable.Invoke();
	}

	//------------------------------------------------------------------------//
	// UPDATE																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		if(m_onUpdate != null && Application.isPlaying) m_onUpdate.Invoke();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void LateUpdate() {
		if(m_onLateUpdate != null && Application.isPlaying) m_onLateUpdate.Invoke();
	}

	/// <summary>
	/// Called every fixed framerate frame.
	/// </summary>
	private void FixedUpdate() {
		if(m_onFixedUpdate != null && Application.isPlaying) m_onFixedUpdate.Invoke();
	}

	//------------------------------------------------------------------------//
	// FINALIZATION															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		if(m_onDisable != null && Application.isPlaying) m_onDisable.Invoke();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		if(m_onDestroy != null && Application.isPlaying) m_onDestroy.Invoke();
	}

	//------------------------------------------------------------------------//
	// DEBUG																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// To draw gizmos that are also pickable and always drawn.
	/// </summary>
	private void OnDrawGizmos() {
		if(m_onDrawGizmos != null) m_onDrawGizmos.Invoke();
	}

	/// <summary>
	/// To draw a gizmo if the object is selected.
	/// </summary>
	private void OnDrawGizmosSelected() {
		if(m_onDrawGizmosSelected != null) m_onDrawGizmosSelected.Invoke();
	}

	//------------------------------------------------------------------------//
	// EDITOR																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called when the script is loaded or a value is changed in the inspector (Called in the editor only).
	/// </summary>
	private void OnValidate() {
		if(m_onValidate != null) m_onValidate.Invoke();
	}

	/// <summary>
	/// Called for rendering and handling GUI events.
	/// </summary>
	private void OnGUI() {
		if(m_onGUI != null) m_onGUI.Invoke();
	}
}