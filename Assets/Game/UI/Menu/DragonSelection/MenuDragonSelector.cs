// MenuDragonSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Select the current dragon in the menu screen.
/// </summary>
public class MenuDragonSelector : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private int m_selectedIdx = 0;
	private List<DragonDef> m_sortedDefs = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Store a reference to all dragon defs sorted
		m_sortedDefs = DefinitionsManager.dragons.defsListByMenuOrder;

		// Figure out initial index
		string selectedSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		for(int i = 0; i < m_sortedDefs.Count; i++) {
			if(selectedSku == m_sortedDefs[i].sku) {
				m_selectedIdx = i;
				break;
			}
		}
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	/// <summary>
	/// Changes dragon selected to the given one.
	/// </summary>
	/// <param name="_sku">The sku of the dragon we want to be the current one.</param>
	public void SetSelectedDragon(string _sku) {
		// Notify game
		Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_SELECTED, _sku);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Select next dragon. To be linked with the "next" button.
	/// </summary>
	public void SelectNextDragon() {
		// Figure out next dragon's sku
		m_selectedIdx++;
		if(m_selectedIdx == m_sortedDefs.Count) m_selectedIdx = 0;

		// Change selection
		SetSelectedDragon(m_sortedDefs[m_selectedIdx].sku);
	}

	/// <summary>
	/// Select previous dragon. To be linked with the "previous" button.
	/// </summary>
	public void SelectPreviousDragon() {
		// Figure out previous dragon's sku
		m_selectedIdx--;
		if(m_selectedIdx < 0) m_selectedIdx = m_sortedDefs.Count - 1;

		// Change selection
		SetSelectedDragon(m_sortedDefs[m_selectedIdx].sku);
	}
}

