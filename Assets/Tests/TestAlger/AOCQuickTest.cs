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
	public Button m_targetButton = null;
	public List<SelectableButton> m_tabButtons = new List<SelectableButton>();

	private int m_selectedIdx = -1;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		for(int i = 0; i < m_tabButtons.Count; ++i) {
			int screenIdx = i;	// Issue with lambda expressions and iterations, see http://answers.unity3d.com/questions/791573/46-ui-how-to-apply-onclick-handler-for-button-gene.html
			m_tabButtons[i].button.onClick.AddListener(() => SelectTab(screenIdx));
			m_tabButtons[i].SetSelected(false);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		SelectTab(-1);
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

	public void SelectTab(int _idx) {
		// Unselect button for the current screen
		if(m_selectedIdx >= 0) {
			// Button is disabled if tab is not enabled
			m_tabButtons[m_selectedIdx].SetSelected(false);
		}
			
		if(_idx >= 0 && _idx < m_tabButtons.Count) {
			m_selectedIdx = _idx;
		} else {
			m_selectedIdx = -1;
		}

		// Select button for newly selected screen
		if(m_selectedIdx >= 0) {
			m_tabButtons[m_selectedIdx].SetSelected(true);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public void ToggleButton(bool _toggle) {
		m_targetButton.interactable = _toggle;

		UbiBCN.CoroutineManager.DelayedCall(() => { m_targetButton.gameObject.SetActive(_toggle); }, 1);
	}

	public void OnButtonClick() {
		DebugUtils.Log("CLICK!", m_targetButton);
	}
}