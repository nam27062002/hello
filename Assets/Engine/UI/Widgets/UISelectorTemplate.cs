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
public class UISelectorTemplate<T> : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	// Change events
	public class ItemEvent : UnityEvent<T, T> {};	// _oldItem, _newItem
	public class IndexEvent : UnityEvent<int, int, bool> {};	// _oldIdx, _newIdx, _looped

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[SerializeField] protected bool m_loop = false;
	[SerializeField] [Range(0f, 1000f)] protected float m_minDragDistance = 50f;

	// Items list
	[SerializeField] protected List<T> m_items = new List<T>();
	public List<T> items {
		get { return m_items; }
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
	private bool m_enableEvents = false;
	public bool enableEvents {
		get { return m_enableEvents; }
		set { m_enableEvents = value; }
	}
	public ItemEvent OnSelectionChanged = new ItemEvent();
	public IndexEvent OnSelectionIndexChanged = new IndexEvent();

	// Internal
	private bool m_dragProcessed = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize with a new list of items. Selected item will be resetted to first item in the list.
	/// </summary>
	/// <param name="_items">New list of items.</param>
	public void Init(List<T> _items) {
		// m_items should never be null
		if(_items == null) {
			m_items = new List<T>();
		} else {
			m_items = _items;
		}

		// Reset selection
		SelectItem(0);
	}

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
		// Use internal method
		SelectItemInternal(_idx, false);
	}

	/// <summary>
	/// Select next item.
	/// </summary>
	public void SelectNextItem() {
		// Figure out next item's index
		bool looped = false;
		int newSelectedIdx = m_selectedIdx + 1;
		if(newSelectedIdx >= m_items.Count) {
			// Loop allowed?
			if(!m_loop) return;
			newSelectedIdx = 0;
			looped = true;
		}

		// Change selection
		SelectItemInternal(newSelectedIdx, looped);
	}

	/// <summary>
	/// Select previous item.
	/// </summary>
	public void SelectPreviousItem()  {
		// Figure out previous item's index
		bool looped = false;
		int newSelectedIdx = m_selectedIdx - 1;
		if(newSelectedIdx < 0) {
			// Loop allowed?
			if(!m_loop) return;
			newSelectedIdx = m_items.Count - 1;
			looped = true;
		}

		// Change selection
		SelectItemInternal(newSelectedIdx, looped);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Changes item selected to the given one, by item index.
	/// Index will be clamped to the amount of items.
	/// </summary>
	/// <param name="_idx">Index of the item to be selected.</param>
	/// <param name="_looped">Did we looped when selecting this item?</param>
	private void SelectItemInternal(int _idx, bool _looped) {
		// Component must be enabled!
		if(!this.enabled) return;

		// Clamp index
		_idx = Mathf.Clamp(_idx, 0, m_items.Count);

		// Store new selected index
		int oldIdx = m_selectedIdx;
		m_selectedIdx = _idx;

		// Notify (if enabled)
		if(enableEvents) {
			OnSelectionChanged.Invoke(GetItem(oldIdx), m_items[m_selectedIdx]);
			OnSelectionIndexChanged.Invoke(oldIdx, m_selectedIdx, _looped);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The input has started dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnBeginDrag(PointerEventData _event) {
		// Reset drag flag
		m_dragProcessed = false;
	}

	/// <summary>
	/// The input is dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event.</param>
	public void OnDrag(PointerEventData _event) {
		// Only if we haven't already changed selection in this drag iteration
		if(!m_dragProcessed) {
			// Select next/previous dragon based on drag horizontal direction and distance
			float dragDistance = _event.position.x - _event.pressPosition.x;
			if(Mathf.Abs(dragDistance) > m_minDragDistance) {
				// Determine selection direction
				if(dragDistance > 0) {
					SelectPreviousItem();
				} else {
					SelectNextItem();
				}

				// Prevent any more item changes in this drag iteration
				m_dragProcessed = true;
			}
		}
	}

	/// <summary>
	/// The input has finished dragging over this element.
	/// </summary>
	/// <param name="_event">Data related to the event</param>
	public void OnEndDrag(PointerEventData _event) {
		// Reset drag flag
		m_dragProcessed = false;
	}
}

