// DragonXPBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to control and setup an XP bar with a dragon's data.
/// </summary>
public class DragonXPBar : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const int SEPARATORS_POOL_SIZE = 10;

	// Auxiliar class to display disguises unlocking
	protected class DisguiseInfo {
		public DefinitionNode def = null;
		public float delta = 0f;
		public DragonXPBarSkinMarker barMarker = null;
		public bool unlocked = false;
		public ResultsScreenDisguiseFlag flag = null;
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Setup
	[InfoBox("All parameters except XP Bar are optional.")]
	[SerializeField] protected bool m_linear = true;	// [AOC] Levels are not uniform (same xp per level), but we want the separators at regular intervals, so we need to do the correction

	// Sliders
	[SerializeField] protected Slider m_xpBar;
	public Slider xpBar {
		get { return m_xpBar; }
	}

	[SerializeField] protected Slider m_auxBar;
	public Slider auxBar {
		get { return m_auxBar; }
	}

	// Textfields
	[Separator]
	[SerializeField] protected Localizer m_levelText;
	public Localizer levelText {
		get { return m_levelText; }
	}



	// Bar separators prefab
	[Separator]
	[SerializeField] protected GameObject m_barSeparatorPrefab = null;
	[SerializeField] protected RectTransform m_barSeparatorsParent = null;
	[SerializeField] protected GameObject m_disguiseMarkerPrefab = null;
    [SerializeField] protected UITooltip m_levelToolTip = null;

    // Internal
    protected DragonDataClassic m_dragonData = null;    // Last used dragon data
    protected List<DragonXPBarSeparator> m_barSeparators = new List<DragonXPBarSeparator>();
	protected List<DisguiseInfo> m_disguises = new List<DisguiseInfo>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {

        // Create, initialize and instantiate a pool of bar separators
        ResizeSeparatorsPool(SEPARATORS_POOL_SIZE);

	}

	/// <summary>
	/// Refresh with data from a target dragon.
	/// </summary>
	/// <param name="_sku">The sku of the dragon whose data we want to use to initialize the bar.</param>
	/// <param name="_delay">Optional delay before refreshing the data. Useful to sync with other UI animations.</param>
	public void Refresh(string _sku, float _delay = -1f) {

        // Only show classic dragons bar
        bool classic = DragonManager.GetDragonData(_sku).type == IDragonData.Type.CLASSIC;
        gameObject.SetActive(classic);

        // Nope
        if (!classic) return;


        // Ignore delay if disabled (coroutines can't be started with the component disabled)
        if (isActiveAndEnabled && _delay > 0) {
			// Start internal coroutine
			StartCoroutine(RefreshDelayed(_sku, _delay));
		} else {
			// Get new dragon's data from the dragon manager and do the refresh logic
			Refresh(DragonManager.GetDragonData(_sku) as DragonDataClassic);
		}
	}

    /// <summary>
	/// Update all fields with given dragon data.
	/// Heirs should call the base when overriding.
	/// </summary>
	/// <param name="_data">Dragon data.</param>
	public void Refresh(DragonDataClassic _data)
    {

        // Store new dragon data
        m_dragonData = _data;

        // XP Bar value
        if (m_xpBar != null)
        {
            m_xpBar.minValue = 0;
            m_xpBar.maxValue = 1;
            if (m_linear)
            {
                float deltaPerLevel = 1f / (_data.progression.maxLevel);
                m_xpBar.value = _data.progression.progressByLevel + Mathf.Lerp(0f, deltaPerLevel, _data.progression.progressCurrentLevel);  // [AOC] This should do it!
            }
            else
            {
                m_xpBar.value = _data.progression.progressByXp;
            }
        }

        // Level Text
        RefreshLevelText(_data.progression.level, _data.progression.maxLevel, false);

        // Bar separators and markers
        InitSeparators(_data);

    }


    //------------------------------------------------------------------------//
    // INTERNAL METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Creates and initialize as many separators as desired to the pool.
    /// </summary>
    private void ResizeSeparatorsPool(int _capacity) {
		// Ignore if separator is null
		if(m_barSeparatorPrefab == null) return;

		// If positive, add to the pool
		if(_capacity > m_barSeparators.Count) {
			GameObject obj = null;
			DragonXPBarSeparator separator = null;
			Transform separatorsParent = m_barSeparatorsParent == null ? m_xpBar.transform : m_barSeparatorsParent;
			for(int i = m_barSeparators.Count; i < _capacity; i++) {
				// Attach separator to the slider and start disabled
				obj = (GameObject)GameObject.Instantiate(m_barSeparatorPrefab, separatorsParent, false);
				obj.transform.SetAsFirstSibling();	// At the bottom
				separator = obj.GetComponent<DragonXPBarSeparator>();
				separator.AttachToSlider(m_xpBar, 0f);
				obj.SetActive(false);
				m_barSeparators.Add(separator);
			}
		}
	}

	/// <summary>
	/// Delayed refresh.
	/// </summary>
	/// <param name="_sku">The sku of the dragon whose data we want to use to initialize the bar.</param>
	/// <param name="_delay">Optional delay before refreshing the data. Useful to sync with other UI animations.</param>
	private IEnumerator RefreshDelayed(string _sku, float _delay = -1f) {
		// If there is a delay, respect it
		if(_delay > 0f) {
			yield return new WaitForSeconds(_delay);
		}

		// Get new dragon's data from the dragon manager
		DragonDataClassic data = DragonManager.GetDragonData(_sku) as DragonDataClassic;
		if(data == null) yield break;

		// Call virtual method
		Refresh(data);
	}

	
	/// <summary>
	/// Refresh the level text with the given values. Optionally launch a level up animation.
	/// </summary>
	/// <param name="_currentLevel">Dragon's current level [0..N-1].</param>
	/// <param name="_maxLevel">Dragon's max level [0..N-1].</param>
	/// <param name="_animate">Whether to launch a level up animation or not.</param>
	protected void RefreshLevelText(int _currentLevel, int _maxLevel, bool _animate) {
		// Ignore if level text is not defined
		if(m_levelText == null) return;

		// Update text
		m_levelText.Localize(
			"TID_LEVEL_ABBR",
			LocalizationManager.SharedInstance.Localize(
				"TID_FRACTION", 
				StringUtils.FormatNumber(_currentLevel + 1), 
				StringUtils.FormatNumber(_maxLevel + 1)
			)
		);

		// Animate?
		if(_animate) {
			m_levelText.transform.DOScale(1.5f, 0.15f).SetLoops(2, LoopType.Yoyo);
		}
	}

	/// <summary>
	/// Initialize the separators with the progression data of the given dragon.
	/// </summary>
	/// <param name="_data">Dragon data to be considered.</param>
	private void InitSeparators(DragonDataClassic _data) {
		// Check params
		if(_data == null) {
			for(int i = 0; i < m_barSeparators.Count; i++) {
				m_barSeparators[i].gameObject.SetActive(false);
			}

			for(int i = 0; i < m_disguises.Count; i++) {
				m_disguises[i].barMarker.gameObject.SetActive(false);
			}
			return;
		}

		// Bar separators
		if(m_barSeparatorPrefab != null) {
			// Make sure we have enough separators
			ResizeSeparatorsPool(_data.progression.maxLevel - 1);

			// Put separators into position
			float delta = 0f;
			int level = 0;
			int maxLevel = _data.progression.maxLevel;
			for(int i = 0; i < m_barSeparators.Count; i++) {
				// Show?
				// [AOC] Exclude first level (no separator at the start!)
				level = i + 1;
				if(level < maxLevel) {
					// Initialize and show it
					if(m_linear) {
						delta = Mathf.InverseLerp(0, maxLevel, level);
					} else {
						// Mark the start of the level
						delta = _data.progression.GetXpRangeForLevel(level).min/_data.progression.xpRange.max;
					}

					// Activate and put into position
					m_barSeparators[i].gameObject.SetActive(true);
					m_barSeparators[i].SetDelta(delta);
				} else {
					// Hide separator
					m_barSeparators[i].gameObject.SetActive(false);
				}
			}
		}

		// Do the same with the disguises unlock markers
		// Only if prefab is defined
		if(m_disguiseMarkerPrefab != null) {
			// Get all disguises for this dragon and sort them by unlockLevel property
			Transform markersParent = m_barSeparatorsParent == null ? m_xpBar.transform : m_barSeparatorsParent;
			List<DefinitionNode> defList = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", _data.def.sku);
			DefinitionsManager.SharedInstance.SortByProperty(ref defList, "unlockLevel", DefinitionsManager.SortType.NUMERIC);
			int slotIdx = 0;
			for(int i = 0; i < defList.Count; i++) {
                
                // Skip if unlockLevel is 0 (default skin)
                int unlockLevel = defList[i].GetAsInt("unlockLevel");
                if (unlockLevel <= 0) {
                    
                    continue;
                }

				// Can we reuse info object?
				DisguiseInfo info = null;
				if(slotIdx < m_disguises.Count) {
					info = m_disguises[slotIdx];
				} else {
					info = new DisguiseInfo();
					m_disguises.Add(info);
				}


                // Initialize info
                info.def = defList[i];


                // Compute delta corresponding to this disguise unlock level
                info.delta = Mathf.InverseLerp(0, _data.progression.maxLevel, unlockLevel);
				info.unlocked = (info.delta <= m_xpBar.normalizedValue);    // Use current var value to quickly determine initial state
				info.unlocked |= UsersManager.currentUser.wardrobe.GetSkinState(info.def.sku) == Wardrobe.SkinState.OWNED;	// Also unlocked if previously owned (i.e. via offer pack)

				// Create a new bar marker or reuse an existing one
				if(info.barMarker == null) {
					GameObject markerObj = (GameObject)GameObject.Instantiate(m_disguiseMarkerPrefab, markersParent, false);
					markerObj.transform.SetAsLastSibling();	// Make sure it shows at the top
                    info.barMarker = markerObj.GetComponent<DragonXPBarSkinMarker>();
                    info.barMarker.skinSku = defList[i].sku;
                    info.barMarker.Definition = defList[i];
					info.barMarker.AttachToSlider(m_xpBar, info.delta);
                    if (m_levelToolTip != null)
                    {
                        markerObj.GetComponent<UITooltipTrigger>().tooltip = m_levelToolTip;
                    }
                } else {
					info.barMarker.skinSku = defList[i].sku;
                    info.barMarker.Definition = defList[i];
                    info.barMarker.SetDelta(info.delta);
					info.barMarker.gameObject.SetActive(true);
				}

                slotIdx++;
            }

			// Reset the rest of info objects
			for(int i = slotIdx; i < m_disguises.Count; i++) {
				m_disguises[i].def = null;
				m_disguises[i].barMarker.gameObject.SetActive(false);
			}
		}
	}
}
