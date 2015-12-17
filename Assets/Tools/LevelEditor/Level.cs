// Level.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/10/2015.
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
	/// Default behaviour to be added to any editable level.
	/// </summary>
	[ExecuteInEditMode]
	public class Level : MonoBehaviour {
		//------------------------------------------------------------------//
		// CONSTANTS														//
		//------------------------------------------------------------------//
		public static readonly string DRAGON_SPAWN_POINT_NAME = "DragonSpawn";	// Concatenate DragonId for specific spawn points
		private static readonly string DRAGON_SPAWN_POINTS_CONTAINER_NAME = "DragonSpawnPoints";

		//------------------------------------------------------------------//
		// PROPERTIES														//
		//------------------------------------------------------------------//

		//------------------------------------------------------------------//
		// GENERIC METHODS													//
		//------------------------------------------------------------------//
		/// <summary>
		/// Initialization.
		/// </summary>
		private void Awake() {
			// Make ouselves static, we don't want to accidentally move the parent object
			this.gameObject.isStatic = true;

			// Create default point if not already done
			GetDragonSpawnPoint(DragonId.NONE, true);
		}

		/// <summary>
		/// First update.
		/// </summary>
		private void Start() {
		
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

