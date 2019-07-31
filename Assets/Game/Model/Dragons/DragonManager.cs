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
	// Dictionary containing ALL dragons
	private Dictionary<string, IDragonData> m_dragonsBySku = null;
	public static Dictionary<string, IDragonData> dragonsBySku {
		get { return instance.m_dragonsBySku; }
	}

	// Specialized lists with commonly used sorting ways
	// Use GetDragonsBy* methods to access them
	private List<IDragonData> m_classicDragonsByOrder = null;
	private List<IDragonData> m_specialDragonsByOrder = null;

	// Shortcut to get the data of the currently selected dragon
	// [AOC] Adding support for different dragon types and game modes
	public static IDragonData CurrentDragon {
		get {
            return GetDragonData(instance.m_user.CurrentDragon);
        }
	}

    public static IDragonData GetClassicDragonsByOrder(int order)
    {
        return (m_instance.m_classicDragonsByOrder != null && order > -1 && order < m_instance.m_classicDragonsByOrder.Count) ? m_instance.m_classicDragonsByOrder[order] : null;
    }

    // Shortcut to get the data of the biggest owned dragon (classic ones) (following progression order)
    // Null if there are no dragons owned (should never happen)
    // [AOC] CLASSIC ONLY
    public static IDragonData biggestOwnedDragon {
		get {
			// Reverse-iterate all the dragons by order and find the biggest one owned
			for(int i = instance.m_classicDragonsByOrder.Count - 1; i >= 0; i--) {
				// Is it owned?
				if(instance.m_classicDragonsByOrder[i].isOwned) {
					// Yes! Return dragon
					return instance.m_classicDragonsByOrder[i];
				}
			}

			// No dragons owned (should never happen)
			return null;
		}
	}

    public static DragonTier maxSpecialDragonTierUnlocked {
        get {
			// Iterate all special dragons and find the one with the biggest tier
            DragonTier maxTier = DragonTier.TIER_0; // Specials start at tier 1, but we'll return 0 if the user doesn't own a dragon.
			for(int i = 0; i < instance.m_specialDragonsByOrder.Count; ++i) {
				// Only owned dragons, of course
				if(instance.m_specialDragonsByOrder[i].isOwned) {
					maxTier = (DragonTier)Mathf.Max((int)maxTier, (int)instance.m_specialDragonsByOrder[i].tier);
				}
			}
			return maxTier;
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
		m_classicDragonsByOrder = new List<IDragonData>();
		m_specialDragonsByOrder = new List<IDragonData>();
	}

	//------------------------------------------------------------------//
	// DRAGON DATA GETTERS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// All dragon types considered.
	/// </summary>
	public static bool IsDragonOwned(string _sku) {
		IDragonData data = null;
		if(instance.m_dragonsBySku.TryGetValue(_sku, out data)) {
			return data.isOwned;
		}
		return false;
	}

	/// <summary>
	/// All dragon types.
	/// </summary>
	public static bool IsFirstDragon(string _sku) {
		IDragonData data = GetDragonData(_sku);
		if(data == null) return false;

		int order = data.GetOrder();
		if (order == 0) {
			return true;
		} else {
			bool isFirst = true;
			List<IDragonData> allDragons = GetDragonsByOrder(IDragonData.Type.ALL);
			for (int i = order - 1; order >= 0 && i < allDragons.Count; --order) {
				isFirst = isFirst && (allDragons[i].GetLockState() <= IDragonData.LockState.TEASE);
			}
			return isFirst;
		}
	}

	/// <summary>
	/// All dragon types.
	/// </summary>
	public static bool IsLastDragon(string _sku) {
		IDragonData data = GetDragonData(_sku);
		if(data == null) return false;

		int order = data.GetOrder();
		List<IDragonData> allDragons = GetDragonsByOrder(IDragonData.Type.ALL);
		if (order == allDragons.Count - 1) {
			return true;
		} else {
			bool isLast = true;
			for (int i = order + 1; order < allDragons.Count && i < allDragons.Count; ++order) {
				isLast = isLast && (allDragons[i].GetLockState() <= IDragonData.LockState.TEASE);
			}
			return isLast;
		}
	}

	/// <summary>
	/// Given a dragon sku, get its current data.
	/// </summary>
	/// <returns>The data corresponding to the dragon with the given sku. Null if not found.</returns>
	/// <param name="_sku">The sku of the dragon whose data we want.</param>
	public static IDragonData GetDragonData(string _sku) {
		IDragonData data = null;
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
	public static IDragonData GetPreviousDragonData(string _sku) {
		// Just check order
		IDragonData data = GetDragonData(_sku);
        List<IDragonData> allDragons = GetDragonsByOrder(IDragonData.Type.ALL);
        int order = data.GetOrder();
		if(order > 0 && order < allDragons.Count) {	// Exclude if first dragon
			return allDragons[order - 1];
		}

		// Not found! Probably last dragon
		return null;
	}

	/// <summary>
	/// Get the data of the dragon following a specific one.
	/// </summary>
	/// <returns>The next dragon data. <c>null</c> if last dragon.</returns>
	/// <param name="_sku">Target dragon.</param>
	public static IDragonData GetNextDragonData(string _sku) {
		// Just check order
		IDragonData data = GetDragonData(_sku);
        List<IDragonData> allDragons = GetDragonsByOrder(IDragonData.Type.ALL);
        int order = data.GetOrder();
		if(order < allDragons.Count - 1) {	// Exclude if last dragon
			return allDragons[order + 1];
		}

		// Not found! Probably last dragon
		return null;
	}

	/// <summary>
	/// Get the dragons of a specific type sorted by order.
	/// </summary>
	/// <returns>The dragons of the requested type sorted by order.</returns>
	/// <param name="_type">Type filter.</param>
	/// <param name="_createNewList">Whether to create a new list or return a reference to the cached list (do not modify it!).</param>
	public static List<IDragonData> GetDragonsByOrder(IDragonData.Type _type, bool _createNewList = false) {
		List<IDragonData> matchingDragonsByOrder = null;
		switch(_type) {
			case IDragonData.Type.CLASSIC: matchingDragonsByOrder = instance.m_classicDragonsByOrder; break;

            case IDragonData.Type.SPECIAL: matchingDragonsByOrder = instance.m_specialDragonsByOrder; break;

            case IDragonData.Type.ALL:  matchingDragonsByOrder = new List<IDragonData>();
                                        matchingDragonsByOrder.AddRange(instance.m_classicDragonsByOrder);
                                        matchingDragonsByOrder.AddRange(instance.m_specialDragonsByOrder); break;
		}

		if(_createNewList && matchingDragonsByOrder != null) {
			return new List<IDragonData>(matchingDragonsByOrder);
		} else {
			return matchingDragonsByOrder;
		}
	}



    /// <summary>
    /// Get all the dragons at a given tier.
    /// </summary>
    /// <returns>The data of the dragons at the given tier.</returns>
    /// <param name="_tier">The tier to look for.</param>
    public static List<IDragonData> GetDragonsByTier(DragonTier _tier, bool _includeHidden = false, bool _includeSpecial = false) {
		// Iterate the dragons list looking for those belonging to the target tier
		List<IDragonData> list = new List<IDragonData>();
		foreach(KeyValuePair<string, IDragonData> kvp in instance.m_dragonsBySku) {
			// Does this dragon belong to the target tier?
			if(kvp.Value.tier == _tier) {
				// Must discard it if it's hidden?
				if(!_includeHidden && kvp.Value.lockState <= IDragonData.LockState.TEASE) continue;

				// Must discard it if it's special?
				if(!_includeSpecial && kvp.Value is DragonDataSpecial) continue;

				// All checkes passed, add it to the list
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
	public static List<IDragonData> GetDragonsByLockState(IDragonData.LockState _lockState) {
		// Iterate the dragons list looking for those belonging to the target tier
		List<IDragonData> list = new List<IDragonData>();
		foreach(KeyValuePair<string, IDragonData> kvp in instance.m_dragonsBySku) 
		{
			// Does this dragon match the required lockstate?
			if(kvp.Value.lockState == _lockState || _lockState == IDragonData.LockState.ANY) {
				// Yes!! Add it to the list
				list.Add(kvp.Value);
			}
		}
		return list;
	}

    /// <summary>
    /// Get the amount of dragons of a type
    /// </summary>
    /// <param name="_type">The type of the dragon</param>
    public static int GetDragonsCount (IDragonData.Type _type)
    {
        switch (_type)
        {
            case IDragonData.Type.CLASSIC: return instance.m_classicDragonsByOrder.Count; 
            case IDragonData.Type.SPECIAL: return instance.m_specialDragonsByOrder.Count; 
            case IDragonData.Type.ALL: return instance.m_classicDragonsByOrder.Count + instance.m_specialDragonsByOrder.Count; 
        }

        // Error
        return -1;
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
		// Get the data for the new dragon
		IDragonData data = DragonManager.GetDragonData(_sku);
		Debug.Assert(data != null, "Attempting to load dragon with id " + _sku + ", but the manager has no data linked to this id");

		// Load prefab for this dragon data
		LoadDragon(data);
	}


    /// <summary>
    /// Load the dragon from its dragon data
    /// </summary>
    /// <param name="_data">Data of the dragon to be loaded.</param>
    public static void LoadDragon(IDragonData _data) {
		// Destroy any previously created player
		GameObject playerObj = GameObject.Find(GameSettings.playerName);
		if(playerObj != null) {
			DestroyImmediate(playerObj);
			playerObj = null;
		}

        // Load the prefab for the dragon with the given ID
        // GameObject prefabObj = Resources.Load<GameObject>(IDragonData.GAME_PREFAB_PATH + _data.gamePrefab);
        GameObject prefabObj = HDAddressablesManager.Instance.LoadAsset<GameObject>(_data.gamePrefab);
		Debug.Assert(prefabObj != null, "The prefab defined to dragon " + _data.sku + " couldn't be found");

		// Create a new instance - will automatically be added to the InstanceManager.player property
		playerObj = Instantiate<GameObject>(prefabObj);
		playerObj.name = GameSettings.playerName;
	}


    /// <summary>
    /// 
    /// </summary>
    /// <param name="user">User.</param>
    public static void SetupUser( UserProfile user)
	{
		instance.m_user = user;
		instance.m_dragonsBySku = user.dragonsBySku;

		// Initialize ordered list
		// Classic dragons
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DRAGONS, "type", DragonDataClassic.TYPE_CODE);
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);
		instance.m_classicDragonsByOrder.Clear();
		for(int i = 0; i < defs.Count; i++) {
			instance.m_classicDragonsByOrder.Add(instance.m_dragonsBySku[defs[i].sku]);
		}

		// Special dragons
		defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DRAGONS, "type", DragonDataSpecial.TYPE_CODE);
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);
		instance.m_specialDragonsByOrder.Clear();
		for(int i = 0; i < defs.Count; i++) {
			instance.m_specialDragonsByOrder.Add(instance.m_dragonsBySku[defs[i].sku]);
		}
	}

	/// <summary>
	/// Has a user been loaded into the manager?
	/// </summary>
	public static bool IsReady() {
		return instance.m_user != null;
	}

	//------------------------------------------------------------------//
	// DEBUG METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// 
	/// </summary>
	/// <param name="_sku">Sku.</param>
	/// <param name="_tier">Tier.</param>
	/// <param name="powerLevel">Power level.</param>
	/// <param name="hpLevel">Hp level.</param>
	/// <param name="speedLevel">Speed level.</param>
	/// <param name="energyLevel">Energy level.</param>
	public static void LoadSpecialDragon_DEBUG(string _sku, DragonTier _tier, int powerLevel, int hpLevel, int speedLevel, int energyLevel) {
		// Get the data for the new dragon
		DragonDataSpecial data = DragonManager.GetDragonData(_sku) as DragonDataSpecial;
		Debug.Assert(data != null, "Attempting to load dragon with id " + _sku + ", but the manager has no data linked to this id");

		// Override stats to match debug ones
		// [AOC] Maybe create a copy to avoid overriding actual dragon stats?
		data.GetStat(DragonDataSpecial.Stat.HEALTH).level = hpLevel;
		data.GetStat(DragonDataSpecial.Stat.SPEED).level = speedLevel;
		data.GetStat(DragonDataSpecial.Stat.ENERGY).level = energyLevel;

		data.SetTier(_tier);
		data.powerLevel = powerLevel;

		/*
        Range xpRange = data.progression.xpRange;
        float xpSetup = xpRange.Lerp( (float)_tier/ (float)DragonTier.TIER_4);
        data.progression.SetXp_DEBUG(xpSetup);
        */

		// Load it!
		LoadDragon(data);
	}
}