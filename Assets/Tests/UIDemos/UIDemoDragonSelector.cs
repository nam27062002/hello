// UIDemoDragonSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class UIDemoDragonSelector : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public GameObject[] m_dragons;
	public Text m_dragonNameText;
	public int m_selectedIdx = -1;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	private void Awake() {
		for(int i = 0; i < m_dragons.Length; i++) {
			m_dragons[i].SetActive(false);
		}
	}

	private void Start() {
		GoToDragon(0);
	}

	public void GoToDragon(int _newDragonIdx) {
		// Trigger "out" animation on current dragon
		if(m_selectedIdx >= 0) m_dragons[m_selectedIdx].GetComponent<Animator>().SetTrigger("out");
		
		// Enable new dragon and trigger "in" animation
		m_dragons[_newDragonIdx].SetActive(true);
		m_dragons[_newDragonIdx].GetComponent<Animator>().SetTrigger("in");
		
		// Update text
		m_dragonNameText.text = m_dragons[_newDragonIdx].name;
		
		// Update dragon pointer
		m_selectedIdx = _newDragonIdx;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public void OnNextDragon() {
		// Select next dragon in the list
		int nextIdx = m_selectedIdx + 1;
		if(nextIdx >= m_dragons.Length) nextIdx = 0;
		GoToDragon(nextIdx);
	}

	public void OnPrevDragon() {
		// Select next dragon in the list
		int nextIdx = m_selectedIdx - 1;
		if(nextIdx < 0) nextIdx = m_dragons.Length - 1;
		GoToDragon(nextIdx);
	}
}