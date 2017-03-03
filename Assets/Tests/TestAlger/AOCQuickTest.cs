// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
//[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private Transform m_target = null;
	[SerializeField] private DragControl m_dragControl = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_dragControl.value = new Vector2(-m_target.localRotation.eulerAngles.y, m_target.localRotation.eulerAngles.z);
		m_dragControl.OnValueChanged.AddListener(OnDragValueChanged);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		
	}

	/// <summary>
	/// Something changed on the inspector.
	/// </summary>
	private void OnValidate() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {

	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The drag control's value has changed.
	/// </summary>
	/// <param name="_control">Control that triggered the event.</param>
	private void OnDragValueChanged(DragControl _control) {
		//Debug.Log(_control.dragging + " | " + _control.value.ToString() + " | " + _control.velocity);
		//m_target.Rotate(0f, -_control.offset.x, _control.offset.y, Space.Self);
		m_target.localRotation = Quaternion.Euler(0f, -_control.value.x, _control.value.y);
	}
}