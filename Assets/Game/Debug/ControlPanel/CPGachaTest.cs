// CPGachaTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/12/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Global class to control gacha testing features.
/// </summary>
public class CPGachaTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// REWARD CHANCE														  //
	//------------------------------------------------------------------------//
	public enum RewardChanceMode {
		DEFAULT = 0,
		SAME_PROBABILITY,
		COMMON_ONLY,
		RARE_ONLY,
		EPIC_ONLY,
		FORCED_PET_SKU
	};

	public enum DuplicateMode {
		DEFAULT = 0,
		ALWAYS,
		NEVER,
		RANDOM
	};

	public const string REWARD_CHANCE_MODE = "GACHA_REWARD_CHANCE_MODE";
	public static RewardChanceMode rewardChanceMode {
		get { return (RewardChanceMode)Prefs.GetIntPlayer(REWARD_CHANCE_MODE, (int)RewardChanceMode.DEFAULT); }
		set { Prefs.SetIntPlayer(REWARD_CHANCE_MODE, (int)value); }
	}

	public const string DUPLICATE_MODE = "GACHA_DUPLICATE_MODE";
	public static DuplicateMode duplicateMode {
		get { return (DuplicateMode)Prefs.GetIntPlayer(DUPLICATE_MODE, (int)DuplicateMode.DEFAULT); }
		set { Prefs.SetIntPlayer(DUPLICATE_MODE, (int)value); }
	}

	public const string FORCED_PET_SKU = "FORCED_PET_SKU";
	public static string forcedPetSku {
		get { return Prefs.GetStringPlayer(FORCED_PET_SKU, ""); }
		set { Prefs.SetStringPlayer(FORCED_PET_SKU, value); }
	}

	//------------------------------------------------------------------------//
	// EXPOSED MEMBERS														  //
	//------------------------------------------------------------------------//
	// Reward Chance
	[Space]
	[SerializeField] private CPEnumPref m_rewardChanceDropdown = null;
	[SerializeField] private CPEnumPref m_duplicateDropdown = null;
	[SerializeField] private TMP_Dropdown m_petSkuDropdown = null;

	// Internal
	private List<DefinitionNode> m_petDefs = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Init pets dropdown
		m_petSkuDropdown.onValueChanged.AddListener(OnPetValueChanged);

		// Subscribe to changed events
		m_rewardChanceDropdown.InitFromEnum(REWARD_CHANCE_MODE, typeof(RewardChanceMode), 0);
		m_duplicateDropdown.InitFromEnum(DUPLICATE_MODE, typeof(DuplicateMode), 0);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure all values are updated
		Refresh();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Only enable forced pet sku dropdown if the right mode is chosen
		m_petSkuDropdown.interactable = (rewardChanceMode == RewardChanceMode.FORCED_PET_SKU);
	}

	/// <summary>
	/// Make sure all fields have the right values.
	/// </summary>
	private void Refresh() {
		m_rewardChanceDropdown.Refresh();
		m_duplicateDropdown.Refresh();

		// Initialize pets dropdown
		m_petSkuDropdown.ClearOptions();
		m_petDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		if(m_petDefs.Count > 0) {
			// Content ready! Init dropdown
			int selectedIdx = -1;
			string currentValue = forcedPetSku;
			for(int i = 0; i < m_petDefs.Count; i++) {
				// Add poption
				m_petSkuDropdown.options.Add(
					new TMP_Dropdown.OptionData(
						m_petDefs[i].GetLocalized("tidName")
					)
				);

				// Is it the current one?
				if(m_petDefs[i].sku == currentValue) {
					selectedIdx = i;
				}
			}

			// If no pet was selected, use first one
			if(selectedIdx < 0) {
				selectedIdx = 0;
				forcedPetSku = m_petDefs[selectedIdx].sku;
			}

			// Set selection
			m_petSkuDropdown.value = selectedIdx;

			// [AOC] Dropdown seems to be bugged -_-
			if(m_petSkuDropdown.options.Count > 0) {
				m_petSkuDropdown.captionText.text = m_petSkuDropdown.options[m_petSkuDropdown.value].text;
			} else {
				m_petSkuDropdown.captionText.text = "";
			}
		} else {
			// Content not ready, try again on next "Refresh" call
			m_petDefs = null;
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		m_petSkuDropdown.onValueChanged.RemoveListener(OnPetValueChanged);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new option has been picked on the dropdown.
	/// </summary>
	public void OnPetValueChanged(int _newValueIdx) {
		if(m_petDefs != null) {
			forcedPetSku = m_petDefs[_newValueIdx].sku;
			Messenger.Broadcast<string, string>(GameEvents.CP_STRING_CHANGED, FORCED_PET_SKU, m_petDefs[_newValueIdx].sku);
			Messenger.Broadcast<string>(GameEvents.CP_PREF_CHANGED, FORCED_PET_SKU);
		}
	}
}