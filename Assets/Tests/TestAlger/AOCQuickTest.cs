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
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public UISafeArea m_safeArea = new UISafeArea();
	public UISafeAreaSetter.Mode m_mode = UISafeAreaSetter.Mode.SIZE;
	public RectTransform m_rt = null;
	[Separator]
	public Vector2 m_anchoredPosition = new Vector2();
	public Vector2 m_sizeDelta = new Vector2();
	public Vector2 m_pivot = new Vector2();
	[Space]
	public Vector2 m_anchorMax = new Vector2();
	public Vector2 m_anchorMin = new Vector2();
	[Space]
	public Vector2 m_offsetMax = new Vector2();
	public Vector2 m_offsetMin = new Vector2();
	[Space]
	public Rect m_rect = new Rect();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		ReadProperties();	
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
		// Apply based on mode
		switch(m_mode) {
			case UISafeAreaSetter.Mode.SIZE: {
				// Adjust both offsets
				m_rt.offsetMin = new Vector2(
					m_rt.offsetMin.x + m_safeArea.left,
					m_rt.offsetMin.y + m_safeArea.bottom
				);

				m_rt.offsetMax = new Vector2(
					m_rt.offsetMax.x - m_safeArea.right,
					m_rt.offsetMax.y - m_safeArea.top
				);
			} break;

			case UISafeAreaSetter.Mode.POSITION: {
				// Select which margins to apply in each axis based on anchors, and do it
				Vector2 newAnchoredPos = m_rt.anchoredPosition;

				// [AOC] TODO!! Research interpolating offset based on actual anchor value

				// X
				if(m_anchorMin.x < 0.5f && m_anchorMax.x < 0.5f) {
					newAnchoredPos.x += m_safeArea.left;
				} else if(m_anchorMin.x > 0.5f && m_anchorMax.x > 0.5f) {
					newAnchoredPos.x -= m_safeArea.right;
				} else {
					// Don't move!
				}

				// Y
				if(m_anchorMin.y < 0.5f && m_anchorMax.y < 0.5f) {
					newAnchoredPos.y += m_safeArea.bottom;
				} else if(m_anchorMin.y > 0.5f && m_anchorMax.y > 0.5f) {
					newAnchoredPos.y -= m_safeArea.top;
				} else {
					// Don't move!!
				}

				// Apply!
				m_rt.anchoredPosition = newAnchoredPos;
			} break;
		}

		ReadProperties();
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {

	}

	private void ReadProperties() {
		m_anchoredPosition = m_rt.anchoredPosition;
		m_sizeDelta = m_rt.sizeDelta;
		m_pivot = m_rt.pivot;

		m_anchorMax = m_rt.anchorMax;
		m_anchorMin = m_rt.anchorMin;

		m_offsetMax = m_rt.offsetMax;
		m_offsetMin = m_rt.offsetMin;

		m_rect = m_rt.rect;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//

}