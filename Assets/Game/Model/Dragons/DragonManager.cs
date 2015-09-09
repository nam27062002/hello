// DragonManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/01/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Global manager of dragons. Contains the definitions from all the dragons in
/// the game, as well as storing their current state (level, stats, upgrades, etc).
/// Has its own prefab in the Resources/Singletons folder, all content must be
/// initialized there.
/// </summary>
public class DragonManager : Singleton<DragonManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// The data
	[SerializeField] private DragonData[] m_dragons;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		base.Awake();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	override protected void Start() {
		base.Start();
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	override protected void Update() {
		base.Update();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		base.OnDestroy();
	}

	//------------------------------------------------------------------//
	// DRAGON DATA GETTERS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given a dragon ID, get its current data.
	/// </summary>
	/// <returns>The data corresponding to the dragon with the given ID. Null if not found.</returns>
	/// <param name="_id">The ID of the dragon whose data we want.</param>
	public static DragonData GetDragonData(DragonID _id) {
		for(int i = 0; i < instance.m_dragons.Length; i++) {
			if(instance.m_dragons[i].id == _id) {
				return instance.m_dragons[i];
			}
		}
		return null;
	}

	/// <summary>
	/// Get all the dragons at a given tier.
	/// </summary>
	/// <returns>The data of the dragons at the given tier.</returns>
	/// <param name="_tier">The tier to look for.</param>
	public static List<DragonData> GetDragonsByTier(int _tier) {
		// Iterate the dragons list looking for those belonging to the target tier
		List<DragonData> list = new List<DragonData>();
		for(int i = 0; i < instance.m_dragons.Length; i++) {
			// Does this dragon belong to the target tier?
			if(instance.m_dragons[i].tier == _tier) {
				// Yes!! Add it to the list
				list.Add(instance.m_dragons[i]);
			}
		}
		return list;
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Creates an instance of the dragon with the given ID and replaces the current
	/// player in the scene.
	/// The newly created instance can be accessed through the InstanceManager.player property.
	/// </summary>
	/// <param name="_id">The ID of the dragon we want to instantiate on the scene.</param>
	public static void LoadDragon(DragonID _id) {
		// Destroy any previously created player
		GameObject playerObj = GameObject.Find(GameSettings.PLAYER_NAME);
		if(playerObj != null) {
			DestroyImmediate(playerObj);
			playerObj = null;
		}

		// Get the data for the new dragon
		DragonData data = DragonManager.GetDragonData(_id);
		DebugUtils.SoftAssert(data != null, "Attempting to load dragon with id " + _id + ", but the manager has no data linked to this id");

		// Load the prefab for the dragon with the given ID
		GameObject prefabObj = Resources.Load<GameObject>(data.prefabPath);
		DebugUtils.SoftAssert(data != null, "The prefab defined to dragon " + _id + " couldn't be found");

		// Create a new instance
		playerObj = Instantiate<GameObject>(prefabObj);
		playerObj.name = GameSettings.PLAYER_NAME;
		
		// Look for a default spawn point for this dragon type in the scene and move the dragon there
		GameObject spawnPointObj = GameObject.Find("PlayerSpawn" + _id);
		if(spawnPointObj == null) {
			// We couldn't find a spawn point for this specific type, try to find a generic one
			spawnPointObj = GameObject.Find("PlayerSpawn");
		}
		if(spawnPointObj != null) {
			playerObj.transform.position = spawnPointObj.transform.position;
		}
	}
}