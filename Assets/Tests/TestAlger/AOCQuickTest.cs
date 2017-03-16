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
using System.Text;
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
public class AOCQuickTest : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private float m_dragThreshold = 50f;
	[SerializeField] private int m_frameCountThreshold = 20;
	[SerializeField] private bool m_enableMousEvents = true;

	private bool m_clickDetection = false;
	private Vector3 m_mouseDownPos = Vector3.zero;
	private int m_downFramesCount = 0;

	[SerializeField] private PhysicsRaycaster m_raycaster = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
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
		StringBuilder sb = new StringBuilder();
		CustomInputModule inputModule = EventSystem.current.currentInputModule as CustomInputModule;

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(inputModule.lastPointerEventData, results);
		sb.AppendLine("New raycast results (" + results.Count + " results)");
		for(int i = 0; i < results.Count; i++) {
			sb.AppendLine("\t" + results[i].gameObject.name);
		}
		Debug.Log(sb.ToString());
		sb.Length = 0;

		results = inputModule.lastRaycastResults;
		sb.AppendLine("Last raycast results from custom input module (" + results.Count + " results)");
		for(int i = 0; i < results.Count; i++) {
			sb.AppendLine("\t" + results[i].gameObject.name);
		}
		Debug.Log(sb.ToString());
		sb.Length = 0;

		CustomEventSystem customEventSystem = (CustomEventSystem)EventSystem.current;
		if(customEventSystem != null) {
			results = customEventSystem.lastRaycastResults;
			sb.AppendLine("Last raycast results from custom event system! (" + results.Count + " results)");
			for(int i = 0; i < results.Count; i++) {
				sb.AppendLine("\t" + results[i].gameObject.name);
			}
		}
		Debug.Log(sb.ToString());
		sb.Length = 0;
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
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	public void OnPointerDown(PointerEventData _eventData) {
		//Debug.Log(this.name + " DOWN!");
	}

	/// <summary>
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	public void OnPointerClick(PointerEventData _eventData) {
		Debug.Log(this.name + " CLICK!");
	}

	/// <summary>
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	public void OnPointerUp(PointerEventData _eventData) {
		//Debug.Log(this.name + " UP!");
	}


	public void OnMouseDown() {
		if(!m_enableMousEvents) return;

		Debug.Log("MOUSE DOWN!");
		m_clickDetection = true;
		m_mouseDownPos = Input.mousePosition;
		m_downFramesCount = 0;
	}

	public void OnMouseDrag() {
		if(!m_enableMousEvents) return;

		Debug.Log("MOUSE DRAG! " + (Input.mousePosition - m_mouseDownPos).sqrMagnitude.ToString());
		if(m_clickDetection) {
			m_downFramesCount++;
			if((Input.mousePosition - m_mouseDownPos).sqrMagnitude > m_dragThreshold || m_downFramesCount > m_frameCountThreshold) {
				m_clickDetection = false;
			}
		}
	}

	public void OnMouseUp() {
		if(!m_enableMousEvents) return;

		Debug.Log("MOUSE UP!");
		if(m_clickDetection) {
			Debug.Log("-------------------> CLICK DETECTED!");
			m_clickDetection = false;
		}
	}

	public void OnMouseUpAsButton() {
		if(!m_enableMousEvents) return;

		Debug.Log("MOUSE CLICK!");
	}
}