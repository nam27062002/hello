// DragonManagerSO.cs
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
/// Global manager of dragons. Stores current state of all dragons in the game
/// (level, stats, upgrades, etc).
/// </summary>
public class DragonManager : UbiBCN.SingletonMonoBehaviour<DragonManager> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	UserProfile m_user;

	// The data
	// We will keep it in a dictionary, but have lists with commonly used sorting ways
	private Dictionary<string, DragonData> m_dragonsBySku = null;
	public static Dictionary<string, DragonData> dragonsBySku {
		get { return instance.m_dragonsBySku; }
	}

	private List<DragonData> m_dragonsByOrder = null;
	public static List<DragonData> dragonsByOrder {
		get { return instance.m_dragonsByOrder; }
	}

	// Shortcut to get the data of the currently selected dragon
	public static DragonData currentDragon {
		get { return GetDragonData(instance.m_user.currentDragon); }
	}

	// Shortcut to get the data of the biggest owned dragon (following progression order)
	// Null if there are no dragons owned (should never happen)
	public static DragonData biggestOwnedDragon {
		get {
			// Reverse-iterate all the dragons by order and find the biggest one owned
			for(int i = instance.m_dragonsByOrder.Count - 1; i >= 0; i--) {
				// Is it owned?
				if(instance.m_dragonsByOrder[i].isOwned) {
					// Yes! Return dragon
					return instance.m_dragonsByOrder[i];
				}
			}

			// No dragons owned (should never happen)
			return null;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Create a dragon data object for every known dragon definition
		m_dragonsBySku = null;
		m_dragonsByOrder = new List<DragonData>();
	}

	//------------------------------------------------------------------//
	// DRAGON DATA GETTERS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Given a dragon sku, get its current data.
	/// </summary>
	/// <returns>The data corresponding to the dragon with the given sku. Null if not found.</returns>
	/// <param name="_sku">The sku of the dragon whose data we want.</param>
	public static DragonData GetDragonData(string _sku) {
		DragonData data = null;
		if(instance.m_dragonsBySku.TryGetValue(_sku, out data)) {
			return data;
		}
		return null;
	}

	/// <summary>
	/// Get the data of the dragon positioned before a specific one.
	/// </summary>
	/// <returns>The previous dragon data. <c>null</c> if first dragon.</returns>
	/// <param name="_sku">Target dragon.</param>
	public static DragonData GetPreviousDragonData(string _sku) {
		// Just check order
		DragonData data = GetDragonData(_sku);
		int order = data.def.GetAsInt("order");
		if(order > 0) {	// Exclude if first dragon
			return DragonManager.dragonsByOrder[order - 1];
		}

		// Not found! Probably last dragon
		return null;
	}

	/// <summary>
	/// Get the data of the dragon following a specific one.
	/// </summary>
	/// <returns>The next dragon data. <c>null</c> if last dragon.</returns>
	/// <param name="_sku">Target dragon.</param>
	public static DragonData GetNextDragonData(string _sku) {
		// Just check order
		DragonData data = GetDragonData(_sku);
		int order = data.def.GetAsInt("order");
		if(order < DragonManager.dragonsByOrder.Count - 1) {	// Exclude if last dragon
			return DragonManager.dragonsByOrder[order + 1];
		}

		// Not found! Probably last dragon
		return null;
	}

	/// <summary>
	/// Get all the dragons at a given tier.
	/// </summary>
	/// <returns>The data of the dragons at the given tier.</returns>
	/// <param name="_tier">The tier to look for.</param>
	public static List<DragonData> GetDragonsByTier(DragonTier _tier) {
		// Iterate the dragons list looking for those belonging to the target tier
		List<DragonData> list = new List<DragonData>();
		foreach(KeyValuePair<string, DragonData> kvp in instance.m_dragonsBySku) {
			// Does this dragon belong to the target tier?
			if(kvp.Value.tier == _tier) {
				// Yes!! Add it to the list
				list.Add(kvp.Value);
			}
		}
		return list;
	}

	/// <summary>
	/// Get all the dragons with a given lock state.
	/// </summary>
	/// <returns>The data of the dragons at the given lock state.</returns>
	/// <param name="_lockState">The lock state to filter by.</param>
	public static List<DragonData> GetDragonsByLockState(DragonData.LockState _lockState) {
		// Iterate the dragons list looking for those belonging to the target tier
		List<DragonData> list = new List<DragonData>();
		foreach(KeyValuePair<string, DragonData> kvp in instance.m_dragonsBySku) 
		{
			// Does this dragon match the required lockstate?
			if(kvp.Value.lockState == _lockState || _lockState == DragonData.LockState.ANY) {
				// Yes!! Add it to the list
				list.Add(kvp.Value);
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
	/// <param name="_sku">The sku of the dragon we want to instantiate on the scene.</param>
	public static void LoadDragon(string _sku) {
		// Destroy any previously created player
		GameObject playerObj = GameObject.Find(GameSettings.playerName);
		if(playerObj != null) {
			DestroyImmediate(playerObj);
			playerObj = null;
		}

		// Get the data for the new dragon
		DragonData data = DragonManager.GetDragonData(_sku);
		Debug.Assert(data != null, "Attempting to load dragon with id " + _sku + ", but the manager has no data linked to this id");

		// Load the prefab for the dragon with the given ID
		GameObject prefabObj = Resources.Load<GameObject>(DragonData.GAME_PREFAB_PATH + data.def.GetAsString("gamePrefab"));

		Debug.Assert(data != null, "The prefab defined to dragon " + _sku + " couldn't be found");

		// Create a new instance - will automatically be added to the InstanceManager.player property
		playerObj = Instantiate<GameObject>(prefabObj);
		playerObj.name = GameSettings.playerName;
			
	}

	public static void SetupUser( UserProfile user)
	{
		instance.m_user = user;
		instance.m_dragonsBySku = user.dragonsBySku;

		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DRAGONS);

		// Initialize ordered list
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);
		instance.m_dragonsByOrder.Clear();
		for(int i = 0; i < defs.Count; i++) {
			instance.m_dragonsByOrder.Add(instance.m_dragonsBySku[defs[i].sku]);
		}
	}

	/// <summary>
	/// Has a user been loaded into the manager?
	/// </summary>
	public static bool IsReady() {
		return instance.m_user != null;
	}
}