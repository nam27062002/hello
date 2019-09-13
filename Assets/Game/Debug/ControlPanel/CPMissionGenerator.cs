// CPMissionsCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Allow several operations related to the mission system from the Control Panel.
/// </summary>
public class CPMissionGenerator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private TMP_Dropdown m_difficultyDropdown = null;
	[SerializeField] private TMP_Dropdown m_ownedDragonDropdown = null;
	[SerializeField] private TMP_Dropdown m_missionTypeDropdown = null;
	[SerializeField] private TMP_Dropdown m_missionSkuDropdown = null;
	[SerializeField] private Toggle m_singleRunToggle = null;
	[SerializeField] private Button m_generateNewMissionButton = null;
	[Space]
	[SerializeField] private GameObject m_blocker = null;

	// Internal logic
	private bool m_init = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Internal vars
		m_init = false;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// If not initialized, do it as soon as content is available
		if(!m_init && ContentManager.ready) {
			// Clear flag
			m_init = true;

			// Initialize and enable dropdowns
			// 1. Difficulty
			for(int i = 0; i < (int)Mission.Difficulty.COUNT; ++i) {
				m_difficultyDropdown.options.Add(
					new TMP_Dropdown.OptionData(
						((Mission.Difficulty)(i)).ToString()
					)
				);
			}
			SetSelectedOption(m_difficultyDropdown, 0);

			// 2. Dragons
			InitDropdownFromDefs(
				m_ownedDragonDropdown, 
				DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DRAGONS, "type", DragonDataClassic.TYPE_CODE)
			);

			// 3. Mission type
			RefreshAvailableTypes();

			// 4. Mission skus
			RefreshAvailableMissions();

			// 5. Single run
			RefreshSingleRun();
		}

		// If not initialized, show blocker
		m_blocker.SetActive(!m_init);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the list of available types based on selected difficulty and owned dragon.
	/// </summary>
	private void RefreshAvailableTypes() {
		if(!m_init) return;

		// Figure out max tier
		IDragonData maxOwnedDragon = DragonManager.GetDragonData(GetSelectedOption(m_ownedDragonDropdown));
		if(maxOwnedDragon == null) return;
		DragonTier maxTierUnlocked = maxOwnedDragon.tier;

		// Get candidates
		List<DefinitionNode> typeDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.MISSION_TYPES);
		typeDefs = typeDefs.FindAll(
			(DefinitionNode _def) => { 
				return (_def.GetAsInt("minTier") <= (int)maxTierUnlocked)	// Ignore mission types meant for bigger tiers
					&& (_def.GetAsInt("maxTier") >= (int)maxTierUnlocked);	// Ignore mission types meant for lower tiers
			}
		);

		// Update dropdown
		InitDropdownFromDefs(m_missionTypeDropdown, typeDefs);
	}

	/// <summary>
	/// Refresh the list of available missions based on selected difficulty, owned dragon and type.
	/// </summary>
	private void RefreshAvailableMissions() {
		if(!m_init) return;

		// Figure out max tier
		IDragonData maxOwnedDragon = DragonManager.GetDragonData(GetSelectedOption(m_ownedDragonDropdown));
		if(maxOwnedDragon == null) return;
		DragonTier maxTierUnlocked = maxOwnedDragon.tier;

		// Figure out selected type
		string selectedType = GetSelectedOption(m_missionTypeDropdown);

		// Get all mission definitions matching the selected type
		// Filter out missions based on current max dragon tier unlocked
		List<DefinitionNode> missionDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.MISSIONS, "type", selectedType);
		missionDefs = missionDefs.FindAll(
			(DefinitionNode _def) => { 
				return (_def.GetAsInt("minTier") <= (int)maxTierUnlocked)	// Ignore missions meant for bigger tiers
					&& (_def.GetAsInt("maxTier") >= (int)maxTierUnlocked);	// Ignore missions meant for lower tiers
			}
		);

		// Update dropdown
		InitDropdownFromDefs(m_missionSkuDropdown, missionDefs);
	}

	/// <summary>
	/// Refresh the single toggle availability based on selected mission.
	/// </summary>
	private void RefreshSingleRun() {
		if(!m_init) return;

		// Based on selected mission def
		DefinitionNode missionDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSIONS, GetSelectedOption(m_missionSkuDropdown));
		float singleRunChance = missionDef == null ? 0.5f : missionDef.GetAsFloat("singleRunChance");

		// Don't allow toggling if chance is either 0% or 100%
		if(singleRunChance <= 0f) {
			m_singleRunToggle.isOn = false;
			m_singleRunToggle.interactable = false;
		} else if(singleRunChance >= 1f) {
			m_singleRunToggle.isOn = true;
			m_singleRunToggle.interactable = false;
		} else {
			m_singleRunToggle.interactable = true;
		}
	}

	//------------------------------------------------------------------------//
	// DROPDOWN UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Select the target option in the given dropdown.
	/// Addresses some issues in the actual TMP_Dropdown class.
	/// </summary>
	/// <param name="_dropdown">Dropdown.</param>
	/// <param name="_idx">Index.</param>
	private void SetSelectedOption(TMP_Dropdown _dropdown, int _idx) {
		_dropdown.value = _idx;
		_dropdown.RefreshShownValue();
		/*if(_dropdown.options.Count > 0) {
			_dropdown.captionText.text = _dropdown.options[_dropdown.value].text;
		} else {
			_dropdown.captionText.text = "";
		}*/
	}

	/// <summary>
	/// Get the current selected option of a dropdown.
	/// </summary>
	/// <returns>The selected option. Empty string if none or drodpown not initialized.</returns>
	/// <param name="_dropdown">Target dropdown.</param>
	private string GetSelectedOption(TMP_Dropdown _dropdown) {
		if(_dropdown.options.Count <= 0) return string.Empty;
		if(_dropdown.value < 0 || _dropdown.value >= _dropdown.options.Count) return string.Empty;
		return _dropdown.options[_dropdown.value].text;
	}

	/// <summary>
	/// Initialize the given dropdown with the given definitions list, using skus as values.
	/// Will try to keep the current selected option.
	/// </summary>
	/// <param name="_dropdown">Target dropdown.</param>
	/// <param name="_defs">List of definitions to be displayed in the dropdown.</param>
	private void InitDropdownFromDefs(TMP_Dropdown _dropdown, List<DefinitionNode> _defs) {
		// Try to keep the same type selected if possible
		int idx = -1;
		string currentOption = GetSelectedOption(_dropdown);
		_dropdown.ClearOptions();
		for(int i = 0; i < _defs.Count; ++i) {
			_dropdown.options.Add(
				new TMP_Dropdown.OptionData(_defs[i].sku)
			);

			// Is it the previously selected option?
			if(idx == -1 && _defs[i].sku == currentOption) {
				idx = i;
			}
		}

		// Restore previous selection
		SetSelectedOption(_dropdown, idx);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Difficulty has been changed.
	/// </summary>
	/// <param name="_newValueIdx">Index of the new selected option in the dropdown.</param>
	public void OnDifficultyChanged(int _newValueIdx) {
		// Nothing to do so far
	}

	/// <summary>
	/// Owned dragon has been changed.
	/// </summary>
	/// <param name="_newValueIdx">Index of the new selected option in the dropdown.</param>
	public void OnOwnedDragonChanged(int _newValueIdx) {
		// Refresh mission types, skus and single run
		RefreshAvailableTypes();
		RefreshAvailableMissions();
		RefreshSingleRun();
	}

	/// <summary>
	/// Mission type has been changed.
	/// </summary>
	/// <param name="_newValueIdx">Index of the new selected option in the dropdown.</param>
	public void OnTypeChanged(int _newValueIdx) {
		// Refresh mission skus and single run
		RefreshAvailableMissions();
		RefreshSingleRun();
	}

	/// <summary>
	/// Mission sku has been changed.
	/// </summary>
	/// <param name="_newValueIdx">Index of the new selected option in the dropdown.</param>
	public void OnSkuChanged(int _newValueIdx) {
		// Refresh single run
		RefreshSingleRun();
	}

	/// <summary>
	/// The generate mission button has been pressed.
	/// </summary>
	public void OnGenerateMissionButton() {
		// Gather all required parameters
		Mission.Difficulty difficulty = (Mission.Difficulty)m_difficultyDropdown.value;
		DefinitionNode typeDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSION_TYPES, GetSelectedOption(m_missionTypeDropdown));
		DefinitionNode missionDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.MISSIONS, GetSelectedOption(m_missionSkuDropdown));
		string ownedDragonSku = GetSelectedOption(m_ownedDragonDropdown);
		bool singleRun = m_singleRunToggle.isOn;

		// Validate them
		if(typeDef == null || missionDef == null || string.IsNullOrEmpty(ownedDragonSku)) {
			UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
				"SOME INVALID PARAMETERS!",
				new Vector2(0.5f, 0.5f),
				ControlPanel.panel.parent as RectTransform
			);
			text.text.color = Color.red;
			return;
		}

		// Validate them
		if(InstanceManager.menuSceneController == null) {
			UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
				"ONLY IN THE MENU!",
				new Vector2(0.5f, 0.5f),
				ControlPanel.panel.parent as RectTransform
			);
			text.text.color = Color.red;
			return;
		}


		// Everything ok! Do it!
        Mission newMission = MissionManager.instance.currentModeMissions.DEBUG_GenerateNewMission(
			difficulty,
			missionDef,
			typeDef,			
			singleRun,
            ownedDragonSku
		);

		// Save persistence
		PersistenceFacade.instance.Save_Request(false);

		// Notify game
		Messenger.Broadcast(MessengerEvents.DEBUG_REFRESH_MISSION_INFO);

		// Show feedback
		if(newMission != null) {
			// Show feedback
			UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
				"SUCESS!",
				new Vector2(0.5f, 0.5f),
				ControlPanel.panel.parent as RectTransform
			);
			text.text.color = Color.green;
		}
	}
}