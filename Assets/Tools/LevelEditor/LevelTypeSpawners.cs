// LevelTypeSpawners.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/12/2015.
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
	/// Specialization of a level.
	/// </summary>
	[ExecuteInEditMode]
	public class LevelTypeSpawners : Level {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public static readonly string DRAGON_SPAWN_POINT_NAME = "DragonSpawn";	// Concatenate DragonId for specific spawn points
		private static readonly string DRAGON_SPAWN_POINTS_CONTAINER_NAME = "DragonSpawnPoints";

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		override protected void Awake() {
			// Call parent
			base.Awake();

			// Create default point if not already done
			GetDragonSpawnPoint(DragonId.NONE, true);
		}

		//------------------------------------------------------------------//
		// OTHER METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Finds and returns the spawn point linked to a specific dragon in this level.
		/// </summary>
		/// <returns>The dragon spawn point.</returns>
		/// <param name="_id">_id.</param>
		/// <param name="_createItIfNotFound">If set to <c>true</c> _create it if not found.</param>
		public GameObject GetDragonSpawnPoint(DragonId _id = DragonId.NONE, bool _createItIfNotFound = false) {
			// Generate game object name for this dragon
			string name = DRAGON_SPAWN_POINT_NAME;
			if(_id != DragonId.NONE && _id != DragonId.COUNT) name += _id.ToString();

			// Does the level have a spawn point for this dragon?
			GameObject spawnPointObj = gameObject.FindObjectRecursive(name);
			if(spawnPointObj == null && _createItIfNotFound) {
				// No! Create one!
				// Get the spawners container object (or create it if not found)
				GameObject spawnContainerObj = gameObject.FindObjectRecursive(DRAGON_SPAWN_POINTS_CONTAINER_NAME);
				if(spawnContainerObj == null) {
					spawnContainerObj = new GameObject(DRAGON_SPAWN_POINTS_CONTAINER_NAME);
					spawnContainerObj.transform.SetParent(this.gameObject.transform, true);
				}

				// Now we can create the spawn point for that dragon
				spawnPointObj = new GameObject(name);
				spawnPointObj.transform.position = new Vector3(0, 20, 0);
				spawnPointObj.transform.SetParent(spawnContainerObj.transform, true);
			}

			return spawnPointObj;
		}
	}
}

