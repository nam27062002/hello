// SelectableButtonGroup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple widget to control a group of selectable buttons.
/// </summary>
public class SelectableButtonGroup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public class SelectionChangedEvent : UnityEvent<int, int> {}
	public const int NO_SELECTION_IDX = -1;
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private List<SelectableButton> m_buttons = new List<SelectableButton>();
    public List<SelectableButton> buttons{
        get{ return m_buttons; }
        set{ m_buttons = value; }
    }
	[SerializeField] private int m_initialSelectedIdx = 0;

	// Events
	public SelectionChangedEvent OnSelectionChanged = new SelectionChangedEvent();

	// Internal
	private int m_selectedIdx = NO_SELECTION_IDX;
	public int selectedIdx {
		get { return m_selectedIdx; }
		set { SelectButton(value); }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Initialize buttons
		for(int i = 0; i < m_buttons.Count; i++) {
			// Add callback
			int buttonIdx = i;	// Issue with lambda expressions and iterations, see http://answers.unity3d.com/questions/791573/46-ui-how-to-apply-onclick-handler-for-button-gene.html
			m_buttons[i].button.onClick.AddListener(() => SelectButton(buttonIdx));
		}

		// Set initial selection
		// If a selection has been performed before, keep it
		if(m_selectedIdx == NO_SELECTION_IDX) {
			SelectButton(m_initialSelectedIdx);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the button at specific index.
	/// </summary>
	/// <returns>The button.</returns>
	/// <param name="_buttonIdx">Button index.</param>
	public SelectableButton GetButton(int _buttonIdx) {
		if(_buttonIdx < 0 || _buttonIdx >= m_buttons.Count) return null;
		return m_buttons[_buttonIdx];
	}

	/// <summary>
	/// Select the target button.
	/// Triggers OnSelectionChanged event.
	/// </summary>
	/// <param name="_buttonIdx">Button to be selected. Invalid values will unselect all buttons.</param>
	public void SelectButton(int _buttonIdx) {
		// If new index is not valid, unselect all
		if(_buttonIdx < 0 || _buttonIdx >= m_buttons.Count) {
			_buttonIdx = NO_SELECTION_IDX;
		}

		// Skip if already selected
		if(m_selectedIdx == _buttonIdx) return;

		// Skip if button can't be selected
		if(_buttonIdx != NO_SELECTION_IDX && !m_buttons[_buttonIdx].selectionAllowed) {
			return;
		}

		// Unselect previous button
		if(m_selectedIdx != NO_SELECTION_IDX) {
			m_buttons[m_selectedIdx].SetSelected(false);
		}

		// Select new button
		if(_buttonIdx != NO_SELECTION_IDX) {
			m_buttons[_buttonIdx].SetSelected(true);
		}

		// Swap indexes
		int oldIdx = m_selectedIdx;
		m_selectedIdx = _buttonIdx;

		// Trigger message
		OnSelectionChanged.Invoke(oldIdx, m_selectedIdx);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}