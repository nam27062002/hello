using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PetScrollRect : OptimizedScrollRect<PetPill, PetPillData> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string ALL_FILTERS = "all";

	// Events
	public class PetFiltersEvent : UnityEvent<PetScrollRect> { }
	public PetFiltersEvent OnFilterChanged = new PetFiltersEvent();

	public class PillTappedEvent : UnityEvent<PetPill> {}
	public PillTappedEvent OnPillTapped = new PillTappedEvent();

	[SerializeField] private List<GameObject> m_pillPrefabs;
	[SerializeField] private PetFilterButton[] m_filterButtons = new PetFilterButton[0];

	private bool m_dirty = false;
	private bool m_ignoreToggleEvents = false;	// Internal control var for when changing a toggle state from code

	private string m_unlockingPet = "";

	private string m_currentFilter;
	public string currentFilter { get { return m_currentFilter; } }

	private Dictionary<string, bool> m_activeFilters;
	private Dictionary<string, List<ScrollRectItemData<PetPillData>>> m_filterData;
	public List<ScrollRectItemData<PetPillData>> filteredDefs {
		get { return m_filterData[m_currentFilter]; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	// filters..
	protected override void Awake() {
		base.Awake();

		if(!Application.isPlaying) return;

		m_currentFilter = ALL_FILTERS;
		m_activeFilters = new Dictionary<string, bool>();
		m_filterData = new Dictionary<string, List<ScrollRectItemData<PetPillData>>>();

		m_filterData[ALL_FILTERS] = new List<ScrollRectItemData<PetPillData>>();

		// Be attentive for when a button's state changes
		for(int i = 0; i < m_filterButtons.Length; i++) {
			m_activeFilters.Add(m_filterButtons[i].filterName, true);
			m_filterButtons[i].OnFilterToggled.AddListener(OnFilterButtonToggled);

			m_filterData[m_filterButtons[i].filterName] = new List<ScrollRectItemData<PetPillData>>();
		}
	}

	protected override void OnEnable() {
		base.OnEnable();

		if(!Application.isPlaying) return;

		// Force a refresh
		m_dirty = true;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	protected override void OnDestroy() {
		base.OnDestroy();

		if(!Application.isPlaying) return;

		// Ignore buttons state changes
		for(int i = 0; i < m_filterButtons.Length; i++) {
			m_filterButtons[i].OnFilterToggled.RemoveListener(OnFilterButtonToggled);
		}
	}


	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		if(!Application.isPlaying) return;

		// If dirty, apply filters
		if (m_dirty) {
			ApplyFilters();
			UpdateScrollItems();
			m_dirty = false;
		}
	}


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	public void Setup(IDragonData _dragon) {
		if(!Application.isPlaying) return;

		foreach(List<ScrollRectItemData<PetPillData>> items in m_filterData.Values) {
			items.Clear();
		}

		List<DefinitionNode> m_defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		for (int i = 0; i < m_defs.Count; ++i) {
			if(!DebugSettings.showHiddenPets) {
				if (m_defs[i].GetAsBool("hidden")) {
					continue;
				}
			}

			ScrollRectItemData<PetPillData> item = new ScrollRectItemData<PetPillData>();
			PetPillData data  = new PetPillData();
			data.def = m_defs[i];
			data.dragon = _dragon;

			item.data = data;
			item.pillType = 0;

			m_filterData[ALL_FILTERS].Add(item);
			m_filterData[data.def.Get("category")].Add(item);
		}

		SortDefinitions();

		ResetFilters();

		m_dirty = true;
	}



	public void FocusOn(string _sku, bool _unlock) {
		if(!Application.isPlaying) return;

		List<ScrollRectItemData<PetPillData>> items = m_filterData[m_currentFilter];
		for (int i = 0; i < items.Count; ++i) {
			if (items[i].data.def.sku == _sku) {
				if (_unlock) {
					m_unlockingPet = _sku;
				}
				FocusOn(i, true);
				return;
			}
		}
	}

	private void SortDefinitions() {
		if(!Application.isPlaying) return;

		Dictionary<string, int> filterOrder = new Dictionary<string, int>();
		for(int i = 0; i < m_filterButtons.Length; i++) {
			filterOrder[m_filterButtons[i].filterName] = i;
		}

		// Rarity order: rarer pets first
		Dictionary<string, int> rarityOrder = new Dictionary<string, int>();
		rarityOrder[Metagame.Reward.RarityToSku(Metagame.Reward.Rarity.EPIC)] = 0;
		rarityOrder[Metagame.Reward.RarityToSku(Metagame.Reward.Rarity.RARE)] = 1;
		rarityOrder[Metagame.Reward.RarityToSku(Metagame.Reward.Rarity.COMMON)] = 2;

        // Put available pets (OTA asset bundle downloaded) first, then put owned pets at the beginning of the list, 
        // then sort by category (following filter buttons order) and finally by content order, 

        foreach (List<ScrollRectItemData<PetPillData>> items in m_filterData.Values) {
			items.Sort((ScrollRectItemData<PetPillData> _data1, ScrollRectItemData<PetPillData> _data2) => {
				DefinitionNode def1 = _data1.data.def;
				DefinitionNode def2 = _data2.data.def;

                // Sort by pet availability (OTA)
                // downloaded pets first and not downloaded last
                List<string> resource1IDs = HDAddressablesManager.Instance.GetResourceIDsForPet(def1.sku);
                bool pet1Available = HDAddressablesManager.Instance.IsResourceListAvailable(resource1IDs);
                List<string> resource2IDs = HDAddressablesManager.Instance.GetResourceIDsForPet(def2.sku);
                bool pet2Available = HDAddressablesManager.Instance.IsResourceListAvailable(resource2IDs);

                if (pet1Available && !pet2Available)
                {
                    return -1;
                }
                else if (!pet1Available && pet2Available)
                {
                    return 1;
                }
                else
                {

                    bool unlocked1 = UsersManager.currentUser.petCollection.IsPetUnlocked(def1.sku);
                    bool unlocked2 = UsersManager.currentUser.petCollection.IsPetUnlocked(def2.sku);
                    if (unlocked1 && !unlocked2)
                    {
                        return -1;
                    }
                    else if (unlocked2 && !unlocked1)
                    {
                        return 1;
                    }
                    else
                    {
                        // Both pets locked or unlocked:
                        // Sort by rarity (rarest ones first)
                        int rarityOrder1 = int.MaxValue;
                        int rarityOrder2 = int.MaxValue;
                        rarityOrder.TryGetValue(def1.Get("rarity"), out rarityOrder1);
                        rarityOrder.TryGetValue(def2.Get("rarity"), out rarityOrder2);
                        if (rarityOrder1 < rarityOrder2)
                        {
                            return -1;
                        }
                        else if (rarityOrder2 < rarityOrder1)
                        {
                            return 1;
                        }
                        else
                        {
                            // Same rarity:
                            // Sort by category (following filter buttons order)
                            int catOrder1 = int.MaxValue;
                            int catOrder2 = int.MaxValue;
                            filterOrder.TryGetValue(def1.Get("category"), out catOrder1);
                            filterOrder.TryGetValue(def2.Get("category"), out catOrder2);
                            if (catOrder1 < catOrder2)
                            {
                                return -1;
                            }
                            else if (catOrder2 < catOrder1)
                            {
                                return 1;
                            }
                            else
                            {
                                // Same category:
                                // Sort by order as defined in content
                                return def1.GetAsInt("order").CompareTo(def2.GetAsInt("order"));
                            }
                        }
                    }
                }
			});
		}
	}

	protected override void OnPillCreated(PetPill _pill) {
		_pill.OnPillTapped.AddListener(__OnPillTapped);
	}

	protected override void OnFocusFinished(PetPill _pill) {
		if (_pill.def.sku == m_unlockingPet) {
			_pill.PrepareUnlockAnim();
			_pill.LaunchUnlockAnim();
			m_unlockingPet = "";
		}
	}

	protected override void OnFocusCanceled() {
		m_unlockingPet = "";
	}

	private void __OnPillTapped(PetPill _pill) {
		FocusOn(_pill.def.sku, false);

		OnPillTapped.Invoke(_pill);
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
			m_currentFilter = ALL_FILTERS;
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
		if(!Application.isPlaying) return;

		for (int i = 0; i < m_filterButtons.Length; ++i) {			
			m_activeFilters[m_filterButtons[i].filterName] = true;
		}

		m_currentFilter = ALL_FILTERS;
	}

	private void ApplyFilters() {
		if(!Application.isPlaying) return;

		m_ignoreToggleEvents = true;	// Skip events
		for (int i = 0; i < m_filterButtons.Length; ++i) {
			PetFilterButton filter = m_filterButtons[i];
			filter.toggle.isOn = m_activeFilters[filter.filterName];
		}
		m_ignoreToggleEvents = false;

		// Notify listeners
		OnFilterChanged.Invoke(this);
	}


	private void UpdateScrollItems() {
		if(!Application.isPlaying) return;

		Setup(m_pillPrefabs, m_filterData[m_currentFilter]);
		if (string.IsNullOrEmpty(m_unlockingPet)) {			
			AnimateVisiblePills();
		}
	}
}
