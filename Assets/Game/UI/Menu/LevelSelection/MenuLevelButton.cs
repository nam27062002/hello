// MenuLevelButton.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Interactive button on the level selection screen.
/// </summary>
public class MenuLevelButton : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[Comment("Sku of the level in the LevelDefinitions")]
	[SkuList(typeof(LevelDef), false)]
	[SerializeField] private string m_levelSku = "";

	// References
	[Comment("References")]
	[SerializeField] private GameObject m_tooltip = null;
	[SerializeField] private Text m_titleText = null;
	[SerializeField] private Text m_titleDesc = null;

	[SerializeField] private GameObject m_playerPointer = null;

	// Data
	LevelDef m_levelDef = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
		DebugUtils.Assert(m_tooltip != null, "Required reference!");
		DebugUtils.Assert(m_titleText != null, "Required reference!");
		DebugUtils.Assert(m_titleDesc != null, "Required reference!");
		DebugUtils.Assert(m_playerPointer != null, "Required reference!");

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Get level def
		m_levelDef = DefinitionsManager.levels.GetDef(m_levelSku);

		// Set name and description
		m_titleText.text = m_levelDef.tidName;
		m_titleDesc.text = m_levelDef.tidDescription;

		// Unfold if current level
		bool isCurrentLevel = (m_levelSku == UserProfile.currentLevel);
		ShowInfo(isCurrentLevel);

		// Show player pointer if current level
		m_playerPointer.SetActive(isCurrentLevel);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsusbscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_LEVEL_SELECTED, OnLevelSelected);
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Fold/unfold the level info
	/// </summary>
	/// <param name="_show">Whether to show or hide the info panel.</param>
	public void ShowInfo(bool _show) {
		// [AOC] TODO!! Animation
		m_tooltip.SetActive(_show);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	public void OnButtonClick() {
		// Try to select the level assigned to this level button
		Messenger.Broadcast<string>(GameEvents.MENU_LEVEL_SELECTED, m_levelSku);
	}

	/// <summary>
	/// A new level has been selected.
	/// </summary>
	/// <param name="_levelSku">The sku of the selected level.</param>
	public void OnLevelSelected(string _levelSku) {
		ShowInfo(_levelSku == m_levelSku);
	}
}

