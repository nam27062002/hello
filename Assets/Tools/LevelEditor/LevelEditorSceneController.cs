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

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		override protected void Awake() {
			// Load the dragon
			DragonManager.LoadDragon(LevelEditor.settings.testDragon);

			// We don't want the dragon to die during the level testing
			InstanceManager.player.invulnerable = true;

			// Enable reward manager to see coins feedback
			RewardManager.Reset();

			// Call parent
			base.Awake();
		}

		/// <summary>
		/// First update.
		/// </summary>
		void Start() {
		
		}
		
		/// <summary>
		/// Called every frame.
		/// </summary>
		void Update() {

		}

		/// <summary>
		/// Destructor.
		/// </summary>
		override protected void OnDestroy() {
			// Call parent
			base.OnDestroy();
		}
	}
}
