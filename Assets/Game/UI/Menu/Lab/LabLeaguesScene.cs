// LabLeaguesScene.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the Lab Leagues 3D scene.
/// </summary>
public class LabLeaguesScene : MenuScreenScene {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private MenuTrophyLoader m_trophyLoader = null;
	public MenuTrophyLoader trophyLoader {
		get { return m_trophyLoader; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Destroy any loaded trophy preview
		UnloadTrophy();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load preview of the trophy for a given league.
	/// Will unload previously loaded trophy.
	/// </summary>
	/// <param name="_sku">Sku of the league whose trophy we want to load.</param>
	public void LoadTrophy(string _leagueSku, bool _force) {
		// If required, unload previously loaded trophy
		if(_force) {
			UnloadTrophy();
        }

		// Load new trophy
		m_trophyLoader.Load(_leagueSku, _force);

		// [AOC] TODO!! Show some FX?
		// [AOC] TODO!! Trigger some SFX?
	}

	/// <summary>
	/// Unload currently loaded trophy.
	/// </summary>
	public void UnloadTrophy() {
		// Just do it
		m_trophyLoader.Unload();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}