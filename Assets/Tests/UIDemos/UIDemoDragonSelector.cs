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
	public int m_initialSelectionIdx = 0;
	public int m_selectedIdx = -1;
	public bool m_useAnims = false;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		for(int i = 0; i < m_dragons.Length; i++) {
			m_dragons[i].SetActive(false);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		GoToDragon(m_initialSelectionIdx);
	}

	/// <summary>
	/// Select the dragon with the given index.
	/// </summary>
	/// <param name="_newDragonIdx">The index of the dragon to be selected.</param>
	public void GoToDragon(int _newDragonIdx) {
		// Trigger "out" animation on current dragon
		if(m_selectedIdx >= 0) {
			if(m_useAnims) {
				m_dragons[m_selectedIdx].GetComponent<Animator>().SetTrigger("out");
			} else {
				m_dragons[m_selectedIdx].SetActive(false);
			}
		}
		
		// Enable new dragon and trigger "in" animation
		m_dragons[_newDragonIdx].SetActive(true);
		if(m_useAnims) {
			m_dragons[_newDragonIdx].GetComponent<Animator>().SetTrigger("in");
		}
		
		// Update text
		m_dragonNameText.text = m_dragons[_newDragonIdx].name;
		
		// Update dragon pointer
		m_selectedIdx = _newDragonIdx;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Select next dragon.
	/// </summary>
	public void OnNextDragon() {
		// Select next dragon in the list
		int nextIdx = m_selectedIdx + 1;
		if(nextIdx >= m_dragons.Length) nextIdx = 0;
		GoToDragon(nextIdx);
	}

	/// <summary>
	/// Select previous dragon.
	/// </summary>
	public void OnPrevDragon() {
		// Select next dragon in the list
		int nextIdx = m_selectedIdx - 1;
		if(nextIdx < 0) nextIdx = m_dragons.Length - 1;
		GoToDragon(nextIdx);
	}
}