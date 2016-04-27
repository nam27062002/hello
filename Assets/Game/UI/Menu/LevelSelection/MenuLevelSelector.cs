// MenuLevelSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

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
/// Select a level in the menu screen.
/// </summary>
public class MenuLevelSelector : MonoBehaviour, IBeginDragHandler, IDragHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Internal data
	private int m_selectedIdx = 0;
	public int selectedIdx {
		get { return m_selectedIdx; }
	}

	private List<DefinitionNode> m_sortedDefs = null;
	public List<DefinitionNode> sortedDefs {
		get { return m_sortedDefs; }
	}

	// External References
	private MenuLevelScroller3D m_scroller = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Aux vars
		MenuSceneController sceneController = InstanceManager.GetSceneController<MenuSceneController>();

		// Get external references
		m_scroller = sceneController.screensController.GetScene((int)MenuScreens.LEVEL_SELECTION).FindComponentRecursive<MenuLevelScroller3D>();
		Debug.Assert(m_scroller != null, "Required scroller not found!");

		// Store a reference to all level defs sorted
		m_sortedDefs = DefinitionsManager.GetDefinitions(DefinitionsCategory.LEVELS);
		DefinitionsManager.SortByProperty(ref m_sortedDefs, "order", DefinitionsManager.SortType.NUMERIC);

		// Figure out initial index
		string selectedSku = sceneController.selectedLevel;
		for(int i = 0; i < m_sortedDefs.Count; i++) {
			if(selectedSku == m_sortedDefs[i].sku) {
				m_selectedIdx = i;
				break;
			}
		}
	}

	//------------------------------------------------------------------//
	// LEVEL SELECTION METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Changes level selected to the given one.
	/// </summary>
	/// <param name="_sku">The sku of the level we want to select.</param>
	public void SetSelectedLevel(string _sku) {
		// Scroll without animation
		m_scroller.FocusLevel(_sku, MenuLevelScroller3D.AnimDir.NONE);

		// Notify game
		Messenger.Broadcast<string>(GameEvents.MENU_LEVEL_SELECTED, _sku);
	}

	/// <summary>
	/// Changes level selected to the given one, by level index.
	/// Index will be clamped to the amount of levels.
	/// </summary>
	/// <param name="_idx">Index of the level we want to select.</param>
	public void SetSelectedLevel(int _idx) {
		// Clamp index
		_idx = Mathf.Clamp(_idx, 0, m_sortedDefs.Count);

		// Select by sku
		SetSelectedLevel(m_sortedDefs[_idx].sku);
	}

	/// <summary>
	/// Select next level.
	/// </summary>
	/// <param name="_loop">Allow going from last to first level or not.</param>
	public void SelectNextLevel(bool _loop) {
		// Figure out next level's sku
		int newSelectedIdx = m_selectedIdx + 1;
		if(newSelectedIdx >= m_sortedDefs.Count) {
			if(!_loop) return;
			newSelectedIdx = 0;
		}

		// Change selection
		m_selectedIdx = newSelectedIdx;
		string sku = m_sortedDefs[m_selectedIdx].sku;
		
		// Scroll
		m_scroller.FocusLevel(m_sortedDefs[m_selectedIdx].sku, MenuLevelScroller3D.AnimDir.FORWARD);

		// Notify game
		Messenger.Broadcast<string>(GameEvents.MENU_LEVEL_SELECTED, sku);
	}

	/// <summary>
	/// Select previous level.
	/// </summary>
	/// <param name="_loop">Allow going from first to last level or not.</param>
	public void SelectPreviousLevel(bool _loop) {
		// Figure out previous level's sku
		int newSelectedIdx = m_selectedIdx - 1;
		if(newSelectedIdx < 0) {
			if(!_loop) return;
			newSelectedIdx = m_sortedDefs.Count - 1;
		}

		// Change selection
		m_selectedIdx = newSelectedIdx;
		string sku = m_sortedDefs[m_selectedIdx].sku;

		// Scroll
		m_scroller.FocusLevel(m_sortedDefs[m_selectedIdx].sku, MenuLevelScroller3D.AnimDir.BACKWARDS);

		// Notify game
		Messenger.Broadcast<string>(GameEvents.MENU_LEVEL_SELECTED, sku);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The input has started dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnBeginDrag(PointerEventData _event) {
		// Select next/previous level based on drag horizontal direction
		if(_event.delta.x > 0) {
			SelectPreviousLevel(true);
		} else {
			SelectNextLevel(true);
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

