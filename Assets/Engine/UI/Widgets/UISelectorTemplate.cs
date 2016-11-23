// UISelectorTemplate.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/11/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple component to select between a list of elements by sweeping with the finger.
/// Simpler implementation than a full-featured scroll list.
/// Since it's a generic class, it can't be used directly, requires an implementation for specific types.
/// </summary>
public class UISelectorTemplate<T> : MonoBehaviour, IBeginDragHandler, IDragHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public class ItemEvent : UnityEvent<T> {};
	public class IndexEvent : UnityEvent<int> {};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Items list
	[SerializeField] protected List<T> m_items = new List<T>();
	public List<T> items {
		get { return m_items; }
		set { 
			m_items = value; 
			SelectItem(m_selectedIdx);	// Will validate that the selected index is still valid
		}
	}

	// Selected index
	[SerializeField] protected int m_selectedIdx = 0;
	public int selectedIdx {
		get { return m_selectedIdx; }
		set { SelectItem(value); }
	}

	public T selectedItem {
		get { return GetItem(selectedIdx); }
	}

	// Events
	public ItemEvent OnSelectionChanged = new ItemEvent();
	public IndexEvent OnSelectionIndexChanged = new IndexEvent();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets the item at the given index.
	/// </summary>
	/// <returns>The item at the given index. <c>null</c> if index not valid.</returns>
	/// <param name="_idx">The index of the target item.</param>
	public T GetItem(int _idx) {
		// Check index
		if(_idx < 0 || _idx >= m_items.Count) return default(T);
		
		// Return item
		return m_items[_idx];
	}
	
	/// <summary>
	/// Changes the item selected to the given one.
	/// Will be ignored if given item is not in the list.
	/// </summary>
	/// <param name="_item">The item to be selected.</param>
	public void SelectItem(T _item) {
		// Is the item among the candidates?
		int idx = m_items.IndexOf(_item);
		if(idx < 0) return;

		// Select by index
		SelectItem(idx);
	}

	/// <summary>
	/// Changes item selected to the given one, by item index.
	/// Index will be clamped to the amount of items.
	/// </summary>
	/// <param name="_idx">Index of the item to be selected.</param>
	public void SelectItem(int _idx) {
		// Clamp index
		_idx = Mathf.Clamp(_idx, 0, m_items.Count);

		// Store new selected index
		m_selectedIdx = _idx;

		// Notify
		OnSelectionChanged.Invoke(m_items[m_selectedIdx]);
		OnSelectionIndexChanged.Invoke(m_selectedIdx);
	}

	/// <summary>
	/// Select next item.
	/// </summary>
	/// <param name="_loop">Allow going from last to first item or not.</param>
	public void SelectNextItem(bool _loop) {
		// Figure out next item's index
		int newSelectedIdx = m_selectedIdx + 1;
		if(newSelectedIdx >= DragonManager.dragonsByOrder.Count) {
			if(!_loop) return;
			newSelectedIdx = 0;
		}

		// Change selection
		SelectItem(newSelectedIdx);
	}

	/// <summary>
	/// Select previous item.
	/// </summary>
	/// <param name="_loop">Allow going from first to last item or not.</param>
	public void SelectPreviousItem(bool _loop)  {
		// Figure out previous item's index
		int newSelectedIdx = m_selectedIdx - 1;
		if(newSelectedIdx < 0) {
			if(!_loop) return;
			newSelectedIdx = DragonManager.dragonsByOrder.Count - 1;
		}

		// Change selection
		SelectItem(newSelectedIdx);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The input has started dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnBeginDrag(PointerEventData _event) {
		// Select next/previous dragon based on drag horizontal direction
		if(_event.delta.x > 0) {
			SelectPreviousItem(false);
		} else {
			SelectNextItem(false);
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

