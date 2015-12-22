// DebugMenuLevelSelector.cs
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
/// Select the level in which to play in the menu screen.
/// </summary>
public class DebugMenuLevelSelector : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	public Text m_text = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required references
		DebugUtils.Assert(m_text != null, "Required component!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Update textfield
		m_text.text = LevelManager.currentLevelDef.tidName;
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	/// <summary>
	/// Changes level selected to the given one.
	/// </summary>
	/// <param name="_levelIdx">The index of the level we want to be the current one.</param>
	public void SetSelectedLevel(int _idx) {
		// Update manager
		//UserProfile.currentLevel = _idx;
		
		// Update textfield
		//m_text.text = LevelManager.GetLevelData(_idx).tidName;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Select next dragon. To be linked with the "next" button.
	/// </summary>
	public void SelectNextLevel() {
		// Figure out next level's index
		//int newIdx = UserProfile.currentLevel + 1;
		//if(newIdx == LevelManager.levels.Length) newIdx = 0;

		// Change selection
		//SetSelectedLevel(newIdx);
	}

	/// <summary>
	/// Select previous dragon. To be linked with the "previous" button.
	/// </summary>
	public void SelectPreviousLevel() {
		// Figure out previous level's index
		//int newIdx = UserProfile.currentLevel - 1;
		//if(newIdx < 0) newIdx = LevelManager.levels.Length - 1;
		
		// Change selection
		//SetSelectedLevel(newIdx);
	}
}

