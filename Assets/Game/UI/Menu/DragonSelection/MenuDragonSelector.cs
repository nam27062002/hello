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
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Select the current dragon in the menu screen.
/// </summary>
public class MenuDragonSelector : MonoBehaviour, IBeginDragHandler, IDragHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private int m_selectedIdx = 0;
	private List<DefinitionNode> m_sortedDefs = null;

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
		m_sortedDefs = Definitions.GetDefinitions(Definitions.Category.DRAGONS);
		Definitions.SortByProperty(ref m_sortedDefs, "order", Definitions.SortType.NUMERIC);

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
	/// The selected object on the scroll list has changed.
	/// </summary>
	/// <param name="_newSelectedPoint">The new selected node object of the scrolllist.</param>
	public void OnScrollSelectedDragonChanged(ScrollRectSnapPoint _newSelectedPoint) {
		// Skip if null (shouldn't happen)
		if(_newSelectedPoint == null) return;

		// We know the new selected object must have a MenuDragonPreview component somewhere, use it to define the new selected dragon
		MenuDragonPreview dragonPreview = _newSelectedPoint.GetComponentInChildren<MenuDragonPreview>();
		if(dragonPreview != null) {
			SetSelectedDragon(dragonPreview.sku);
		}
	}

	/// <summary>
	/// Select next dragon. To be linked with the "next" button.
	/// </summary>
	/// <param name="_loop">Allow going from last to first dragon or not.</param>
	public void SelectNextDragon(bool _loop) {
		// Figure out next dragon's sku
		int newSelectedIdx = m_selectedIdx + 1;
		if(newSelectedIdx >= m_sortedDefs.Count) {
			if(!_loop) return;
			newSelectedIdx = 0;
		}

		// Change selection
		m_selectedIdx = newSelectedIdx;
		SetSelectedDragon(m_sortedDefs[m_selectedIdx].sku);
	}

	/// <summary>
	/// Select previous dragon. To be linked with the "previous" button.
	/// </summary>
	/// <param name="_loop">Allow going from first to last dragon or not.</param>
	public void SelectPreviousDragon(bool _loop) {
		// Figure out previous dragon's sku
		int newSelectedIdx = m_selectedIdx - 1;
		if(newSelectedIdx < 0) {
			if(!_loop) return;
			newSelectedIdx = m_sortedDefs.Count - 1;
		}

		// Change selection
		m_selectedIdx = newSelectedIdx;
		SetSelectedDragon(m_sortedDefs[m_selectedIdx].sku);
	}

	/// <summary>
	/// The input has started dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnBeginDrag(PointerEventData _event) {
		// Select next/previous dragon based on drag horizontal direction
		if(_event.delta.x > 0) {
			SelectPreviousDragon(false);
		} else {
			SelectNextDragon(false);
		}
	}

	/// <summary>
	/// The input is dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event.</param>
	public void OnDrag(PointerEventData _event) {
		// Nothing to do, but the OnBeginDrag event doesn't work if we don't implement the IDragHandler interface -_-
	}
}

