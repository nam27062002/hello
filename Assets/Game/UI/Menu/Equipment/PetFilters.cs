// PetsScreenFilters.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to manage pets filtering.
/// TODO:
/// - Support multiple filters per button
/// - Support multiple buttons per filter
/// - Support multiple filters per pet
/// </summary>
[RequireComponent(typeof(PetsScreenController))]
public class PetFilters : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private PetFilterButton[] m_filterButtons = new PetFilterButton[0];
	public PetFilterButton[] filterButtons {
		get { return m_filterButtons; }
	}

	// Internal references
	private PetsScreenController m_petsScreen = null;
	private PetsScreenController petsScreen {
		get {
			if(m_petsScreen == null) {
				m_petsScreen = GetComponent<PetsScreenController>();
			}
			return m_petsScreen;
		}
	}

	// Internal logic
	private Dictionary<string, bool> m_activeFilters = new Dictionary<string, bool>();	// Dictionary of <filterName, status>. If a filterName is not on the dictionary, filter is considered off.
	private bool m_dirty = false;
	private bool m_ignoreToggleEvents = false;	// Internal control var for when changing a toggle state from code
	private string m_currentFilter = "";	// Behave as tabs

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Start with all filters toggled!
		// This will also create a new entry on the dictionary for each filter button
		ResetFilters();

		// Be attentive for when a button's state changes
		for(int i = 0; i < m_filterButtons.Length; i++) {
			m_filterButtons[i].OnFilterToggled.AddListener(OnFilterButtonToggled);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Skip events
		m_ignoreToggleEvents = true;

		// Initialize buttons!
		for(int i = 0; i < m_filterButtons.Length; i++) {
			// Refresh based on current filter state
			m_filterButtons[i].toggle.isOn = CheckFilter(m_filterButtons[i].filterName);
		}

		// Apply current filters
		RefreshPills();
		m_dirty = false;
		m_ignoreToggleEvents = false;
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// If dirty, apply filters
		if(m_dirty) {
			RefreshPills();
			RefreshButtons();
			m_dirty = false;
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Ignore buttons state changes
		for(int i = 0; i < m_filterButtons.Length; i++) {
			m_filterButtons[i].OnFilterToggled.RemoveListener(OnFilterButtonToggled);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether the given pet passes current active filters or not.
	/// </summary>
	/// <returns>Whether the pet passes the current active filters.</returns>
	/// <param name="_petDef">Pet definition to be checked.</param>
	public bool CheckFilter(DefinitionNode _petDef) {
		// Definition must be valid!
		if(_petDef == null) return false;

		// [AOC] TODO!! Multiple categories per pet
		// [AOC] TODO!! Filter rarities as well?
		return CheckFilter(_petDef.Get("category"));
	}

	/// <summary>
	/// Check whether the given filter is toggled.
	/// </summary>
	/// <returns><c>true</c>, if check was filtered, <c>false</c> otherwise.</returns>
	/// <param name="_filterName">Filter name.</param>
	public bool CheckFilter(string _filterName) {
		bool isFilterActive = false;
		m_activeFilters.TryGetValue(_filterName, out isFilterActive);
		return isFilterActive;
	}

	/// <summary>
	/// Set the status of a filter.
	/// </summary>
	/// <param name="_filterName">Target filter.</param>
	/// <param name="_value">Whether to toggle filter on or off.</param>
	public void SetFilter(string _filterName, bool _value) {
		// Store new value
		m_activeFilters[_filterName] = _value;

		// Mark as dirty
		SetDirty();
	}

	/// <summary>
	/// Toggles the status of a filter.
	/// </summary>
	/// <param name="_filterName">Target filter.</param>
	public void ToggleFilter(string _filterName) {
		SetFilter(_filterName, !CheckFilter(_filterName));
	}

	/// <summary>
	/// Toggle all filters on.
	/// </summary>
	public void ResetFilters() {
		// Reset current filter
		m_currentFilter = string.Empty;

		// Toggle all values on the filters dictionary
		List<string> keys = new List<string>(m_activeFilters.Keys);		// [AOC] We can't modify the dictionary during a foreach loop (Out of Sync exception). Do this instead!
		for(int i = 0; i < keys.Count; i++) {
			m_activeFilters[keys[i]] = true;
		}

		// Iterate all buttons and make sure their filter ID is toggled on, in case it wasn't previously added to the dictionary
		// Double loop, not very efficient but not a big deal at this point
		for(int i = 0; i < m_filterButtons.Length; i++) {
			m_activeFilters[m_filterButtons[i].filterName] = true;
		}

		// Mark as dirty
		SetDirty();
	}

	/// <summary>
	/// Mark filter list as dirty. Will be refreshed the next frame.
	/// </summary>
	public void SetDirty() {
		m_dirty = true;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show/hide pills based on current filter list.
	/// </summary>
	private void RefreshPills() {
		// Iterate the pills!
		bool show = false;
		float delay = 0f;
		for(int i = 0; i < petsScreen.pills.Count; i++) {
			// Should this pill be displayed?
			show = CheckFilter(petsScreen.pills[i].def);
			if(show) {
				// Restart animation when showing
				petsScreen.pills[i].animator.tweenDelay = delay;
				petsScreen.pills[i].animator.RestartShow();
				delay += 0.05f;
			} else {
				// Don't do animation when hiding
				petsScreen.pills[i].animator.Hide(false);
			}
		}

		// Put scroll list at the start
		StartCoroutine(petsScreen.scrollList.ScrollToPositionDelayedFrames(Vector2.zero, 1));
	}

	/// <summary>
	/// Toggle buttons on/off bsed on the current active filters.
	/// </summary>
	private void RefreshButtons() {
		// Ignore toggle events, we could end up in an infinite loop!
		m_ignoreToggleEvents = true;

		// Iterate all buttons
		for(int i = 0; i < m_filterButtons.Length; i++) {
			m_filterButtons[i].toggle.isOn = CheckFilter(m_filterButtons[i].filterName);
		}

		// Restore toggle events
		m_ignoreToggleEvents = false;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A filter button has been toggled.
	/// </summary>
	/// <param name="_filterButton">The filter button that triggered the events.</param>
	private void OnFilterButtonToggled(PetFilterButton _filterButton) {
		// Nothing to do if ignore flag is on
		if(m_ignoreToggleEvents) return;
		if(!isActiveAndEnabled) return;

		// [AOC] FILTERS BEHAVIOUR
		/*
		// SPECIAL CASE 1: When all buttons are active and the button is toggled off, do the reverse action:
		// toggle the rest of buttons off and keep the toggled one active
		if(!_filterButton.toggle.isOn) {
			bool allActive = true;
			foreach(KeyValuePair<string, bool> kvp in m_activeFilters) {
				// If filter not toggled, break the loop, nothing special to do
				if(!kvp.Value) {
					allActive = false;
					break;
				}
			}

			// Toggling filter off and all filters toggled: reverse the situation
			if(allActive) {
				List<string> keys = new List<string>(m_activeFilters.Keys);		// [AOC] We can't modify the dictionary during a foreach loop (Out of Sync exception). Do this instead!
				for(int i = 0; i < keys.Count; i++) {
					// Activate only target filter 
					SetFilter(keys[i], keys[i] == _filterButton.filterName);
				}
				return;	// We're done for the special case!
			}
		}

		// SPECIAL CASE 2: When the target filter is the last active filter, toggle on all the filters
		if(!_filterButton.toggle.isOn) {
			bool allOff = true;
			foreach(KeyValuePair<string, bool> kvp in m_activeFilters) {
				// If a filter other than the target one is toggled, break the loop, nothing special to do
				if(kvp.Key != _filterButton.filterName && kvp.Value) {
					allOff = false;
					break;
				}
			}

			// Toggling filter off and all filters untoggled: toggle on all the filters
			if(allOff) {
				List<string> keys = new List<string>(m_activeFilters.Keys);		// [AOC] We can't modify the dictionary during a foreach loop (Out of Sync exception). Do this instead!
				for(int i = 0; i < keys.Count; i++) {
					// Activate them all!
					SetFilter(keys[i], true);
				}
				return;	// We're done for the special case!
			}
		}

		// NORMAL CASE: just update this filter's state
		SetFilter(_filterButton.filterName, _filterButton.toggle.isOn);
		*/

		// [AOC] TABS BEHAVIOUR
		// If the tapped button is the current filter, toggle on all filters
		if(_filterButton.filterName == m_currentFilter) {
			// Toggle all filters on
			List<string> keys = new List<string>(m_activeFilters.Keys);		// [AOC] We can't modify the dictionary during a foreach loop (Out of Sync exception). Do this instead!
			for(int i = 0; i < keys.Count; i++) {
				// Activate them all!
				SetFilter(keys[i], true);
			}

			// Reset current filter
			m_currentFilter = string.Empty;
		}

		// Otherwise make it the only active filter
		else {
			// Toggle all filters off except this one
			List<string> keys = new List<string>(m_activeFilters.Keys);		// [AOC] We can't modify the dictionary during a foreach loop (Out of Sync exception). Do this instead!
			for(int i = 0; i < keys.Count; i++) {
				// Activate only target filter 
				SetFilter(keys[i], keys[i] == _filterButton.filterName);
			}

			// Store as current filter
			m_currentFilter = _filterButton.filterName;
		}
	}
}