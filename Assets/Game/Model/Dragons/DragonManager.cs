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
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// The data
	// The array allows us to easily setup values from inspector, while the dictionary helps us with faster searches during gameplay
	// Both contain exactly the same data, unless the array length is modified during gameplay (which shouldn't happen)
	[SerializeField] private DragonData[] m_dragons = new DragonData[(int)DragonID.COUNT];
	private Dictionary<DragonID, DragonData> m_dragonsById = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		base.Awake();

		// Keep the dragons indexed by id as well for faster searches
		m_dragonsById = new Dictionary<DragonID, DragonData>();
		for(int i = 0; i < m_dragons.Length; i++) {
			m_dragonsById[m_dragons[i].id] = m_dragons[i];
		}
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
		DragonData data = null;
		if(instance.m_dragonsById.TryGetValue(_id, data)) {
			return data;
		}
		return null;
		/*
		for(int i = 0; i < instance.m_dragons.Length; i++) {
			if(instance.m_dragons[i].id == _id) {
				return instance.m_dragons[i];
			}
		}
		return null;
		*/
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
		GameObject playerObj = GameObject.Find(GameSettings.playerName);
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
		playerObj.name = GameSettings.playerName;
		
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

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public static void Load(DragonData.SaveData[] _data) {
		// We don't trust array order, so do it by id
		for(int i = 0; i < _data.Length; i++) {
			GetDragonData(_data[i].id).Load(_data[i]);
		}
	}
	
	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public DragonData.SaveData[] Save() {
		// Create new object, initialize and return it
		DragonData.SaveData[] data = new DragonData.SaveData[instance.m_dragons.Length];
		for(int i = 0; i < instance.m_dragons.Length; i++) {
			data[i] = instance.m_dragons[i].Save();
		}
		return data;
	}
}