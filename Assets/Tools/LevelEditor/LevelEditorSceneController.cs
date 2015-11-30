// LevelEditorSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
namespace LevelEditor {
	/// <summary>
	/// Scene controller for the level editor scene.
	/// </summary>
	public class LevelEditorSceneController : SceneController {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public static readonly string NAME = "SC_LevelEditor";

		//------------------------------------------------------------------//
		// MEMBERS															//
		//------------------------------------------------------------------//
		private bool m_invulnerableBackup = false;

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		override protected void Awake() {
			// Load the dragon
			DragonManager.LoadDragon(LevelEditor.settings.testDragon);

			// Enable reward manager to see coins feedback
			RewardManager.Reset();

			// Call parent
			base.Awake();
		}

		/// <summary>
		/// Component enabled.
		/// </summary>
		void OnEnable() {
			// We don't want the dragon to die during the level testing
			m_invulnerableBackup = DebugSettings.invulnerable;
			DebugSettings.invulnerable = true;
		}
		
		/// <summary>
		/// Component disabled.
		/// </summary>
		void OnDisable() {
			// Restore invlunerability cheat
			DebugSettings.invulnerable = m_invulnerableBackup;
		}
	}
}
