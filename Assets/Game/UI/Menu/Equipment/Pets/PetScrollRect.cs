using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetScrollRect : OptimizedScrollRect<PetPill, PetPillData> {
	private const string EMPTY = "empty";


	[SerializeField] private List<GameObject> m_pillPrefabs;
	[SerializeField] private PetFilterButton[] m_filterButtons = new PetFilterButton[0];

	private bool m_dirty = false;
	private bool m_ignoreToggleEvents = false;	// Internal control var for when changing a toggle state from code

	private string m_currentFilter;
	private Dictionary<string, bool> m_activeFilters;
	private Dictionary<string, List<ScrollRectItemData<PetPillData>>> m_filterData;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	// filters..
	public void Awake() {
		m_currentFilter = EMPTY;
		m_activeFilters = new Dictionary<string, bool>();
		m_filterData = new Dictionary<string, List<ScrollRectItemData<PetPillData>>>();

		m_filterData[EMPTY] = new List<ScrollRectItemData<PetPillData>>();

		// Be attentive for when a button's state changes
		for(int i = 0; i < m_filterButtons.Length; i++) {
			m_activeFilters.Add(m_filterButtons[i].filterName, true);
			m_filterButtons[i].OnFilterToggled.AddListener(OnFilterButtonToggled);

			m_filterData[m_filterButtons[i].filterName] = new List<ScrollRectItemData<PetPillData>>();
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


	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// If dirty, apply filters
		if (m_dirty) {
			ApplyFilters();
			RefreshButtons();
			m_dirty = false;
		}
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	public void Setup(DragonData _dragon) {
		List<DefinitionNode> m_defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);

		for (int i = 0; i < m_defs.Count; ++i) {
			ScrollRectItemData<PetPillData> item = new ScrollRectItemData<PetPillData>();
			PetPillData data  = new PetPillData();
			data.def = m_defs[i];
			data.dragon = _dragon;

			item.data = data;
			item.pillType = 0;

			m_filterData[EMPTY].Add(item);
			m_filterData[data.def.Get("category")].Add(item);
		}

		m_dirty = true;
	}

	private void OnFilterButtonToggled(PetFilterButton _filterButton) {
		if (m_ignoreToggleEvents) return;

		if(_filterButton.filterName == m_currentFilter) {
			// Toggle all filters on	
			List<string> keys = new List<string>(m_activeFilters.Keys);		// [AOC] We can't modify the dictionary during a foreach loop (Out of Sync exception). Do this instead!
			for(int i = 0; i < keys.Count; i++) {
				// Activate them all!
				m_activeFilters[keys[i]] = true;
			}

			// Reset current filter
			m_currentFilter = EMPTY;
		}

		// Otherwise make it the only active filter
		else {
			// Toggle all filters off except this one
			List<string> keys = new List<string>(m_activeFilters.Keys);		// [AOC] We can't modify the dictionary during a foreach loop (Out of Sync exception). Do this instead!
			for(int i = 0; i < keys.Count; i++) {
				// Activate only target filter 
				m_activeFilters[keys[i]] = (keys[i] == _filterButton.filterName);
			}

			// Store as current filter
			m_currentFilter = _filterButton.filterName;
		}

		m_dirty = true;
	}

	private void ResetFilters() {
		m_ignoreToggleEvents = true;	// Skip events
		for (int i = 0; i < m_filterButtons.Length; ++i) {
			m_filterButtons[i].toggle.isOn = true;
		}
		m_ignoreToggleEvents = false;

		m_currentFilter = EMPTY;
	}

	private void ApplyFilters() {
		Setup(m_pillPrefabs, m_filterData[m_currentFilter]);
	}

	private void RefreshButtons() {
		m_ignoreToggleEvents = true;	// Skip events
		for (int i = 0; i < m_filterButtons.Length; ++i) {
			PetFilterButton filter = m_filterButtons[i];
			filter.toggle.isOn = m_activeFilters[filter.filterName];
		}
		m_ignoreToggleEvents = false;
	}
}
