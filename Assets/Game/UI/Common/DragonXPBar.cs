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

	[SerializeField] protected Localizer m_dragonNameText;
	public Localizer dragonNameText {
		get { return m_dragonNameText; }
	}

	[SerializeField] protected Localizer m_dragonDescText;
	public Localizer dragonDescText {
		get { return m_dragonDescText; }
	}

	// Bar separator prefab
	[Separator]
	[SerializeField] protected GameObject m_barSeparatorPrefab = null;
	[SerializeField] protected RectTransform m_barSeparatorsParent = null;

	// Internal
	protected DragonData m_dragonData = null;	// Last used dragon data
	protected List<DragonXPBarSeparator> m_barSeparators = new List<DragonXPBarSeparator>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	virtual protected void Awake() {
		// Create, initialize and instantiate a pool of bar separators
		ExpandSeparatorsPool(SEPARATORS_POOL_SIZE);
	}

	/// <summary>
	/// Refresh with data from a target dragon.
	/// </summary>
	/// <param name="_sku">The sku of the dragon whose data we want to use to initialize the bar.</param>
	/// <param name="_delay">Optional delay before refreshing the data. Useful to sync with other UI animations.</param>
	virtual public void Refresh(string _sku, float _delay = -1f) {
		// Ignore delay if disabled (coroutines can't be started with the component disabled)
		if(isActiveAndEnabled && _delay > 0) {
			// Start internal coroutine
			StartCoroutine(RefreshDelayed(_sku, _delay));
		} else {
			// Get new dragon's data from the dragon manager and do the refresh logic
			Refresh(DragonManager.GetDragonData(_sku));
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Creates and initialize as many separators as desired to the pool.
	/// </summary>
	private void ExpandSeparatorsPool(int _amount) {
		// Ignore if separator is null
		if(m_barSeparatorPrefab == null) return;

		// If offset is negative, remove from the pool
		if(_amount < 0) {
			m_barSeparators.RemoveRange(Mathf.Max(m_barSeparators.Count, 0), _amount);
		}

		// If positive, add to the pool
		else if(_amount > 0) {
			GameObject obj = null;
			DragonXPBarSeparator separator = null;
			Transform separatorsParent = m_barSeparatorsParent == null ? m_xpBar.transform : m_barSeparatorsParent;
			for(int i = 0; i < _amount; i++) {
				// Attach separator to the slider and start disabled
				obj = (GameObject)GameObject.Instantiate(m_barSeparatorPrefab, separatorsParent, false);
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
		DragonData data = DragonManager.GetDragonData(_sku);
		if(data == null) yield break;

		// Call virtual method
		Refresh(data);
	}

	/// <summary>
	/// Update all fields with given dragon data.
	/// Heirs should call the base when overriding.
	/// </summary>
	/// <param name="_data">Dragon data.</param>
	virtual protected void Refresh(DragonData _data) {
		// Check params
		if(_data == null) return;

		// Bar value
		if(m_xpBar != null) {
			m_xpBar.minValue = 0;
			m_xpBar.maxValue = 1;
			if(m_linear) {
				float deltaPerLevel = 1f/_data.progression.numLevels;
				m_xpBar.value = _data.progression.progressByLevel + Mathf.Lerp(0f, deltaPerLevel, _data.progression.progressCurrentLevel);	// [AOC] This should do it!
			} else {
				m_xpBar.value = _data.progression.progressByXp;
			}
		}

		// Level Text
		if(m_levelText != null) {
			m_levelText.Localize("TID_LEVEL", StringUtils.FormatNumber(_data.progression.level + 1) + "/" + StringUtils.FormatNumber(_data.progression.numLevels));
		}

		// Things to update only when target dragon has changed
		if(m_dragonData != _data) {
			// Dragon Name
			if(m_dragonNameText != null) m_dragonNameText.Localize(_data.def.GetAsString("tidName"));
			if(m_dragonDescText != null) m_dragonDescText.Localize(_data.def.GetAsString("tidDesc"));
			
			// Bar separators
			if(m_barSeparatorPrefab != null) {
				InitSeparators(_data);
			}

			// Store new dragon data
			m_dragonData = _data;
		}
	}

	/// <summary>
	/// Initialize the separators with the progression data of the given dragon.
	/// </summary>
	/// <param name="_data">Dragon data to be considered.</param>
	private void InitSeparators(DragonData _data) {
		// Check params
		if(_data == null) {
			for(int i = 0; i < m_barSeparators.Count; i++) {
				m_barSeparators[i].gameObject.SetActive(false);
			}
			return;
		}

		// Make sure we have enough separators
		if(m_barSeparators.Count < _data.progression.numLevels) {
			ExpandSeparatorsPool(_data.progression.numLevels - m_barSeparators.Count);
		}

		// Put separators into position
		float delta = 0f;
		int level = 0;
		for(int i = 0; i < m_barSeparators.Count; i++) {
			// Show?
			// [AOC] Exclude first level (no separator at the start!)
			level = i + 1;
			if(level < _data.progression.numLevels) {
				// Initialize and show it
				if(m_linear) {
					delta = Mathf.InverseLerp(0, _data.progression.numLevels, level);
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
}
