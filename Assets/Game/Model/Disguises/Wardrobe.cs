using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wardrobe : IBroadcastListener {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum SkinState {
		LOCKED = 0,
		NEW = 1,        // Same as "AVAILABLE", but showing the "new" notification
		AVAILABLE = 2,
		OWNED = 3
	};

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private Dictionary<string, SkinState> m_disguises = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public Wardrobe() {
		m_disguises = new Dictionary<string, SkinState>();

		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.SEASON_CHANGED, this);
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	~Wardrobe() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.SEASON_CHANGED, this);
	}

	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public void InitFromDefinitions() {
		Dictionary<string, DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DISGUISES);
		m_disguises.Clear();
		foreach(KeyValuePair<string, DefinitionNode> kvp in defs) {
			// Special case: if default or special skin, mark it as owned!
			if(IsDefaultSkin(kvp.Value) || IsSpecialSkin(kvp.Value)) {
				m_disguises.Add(kvp.Key, SkinState.OWNED);
			} else {
				m_disguises.Add(kvp.Key, SkinState.LOCKED);
			}
		}
	}

	/// <summary>
	/// Get the state of a specific skin.
	/// </summary>
	/// <returns>The state of the skin.</returns>
	/// <param name="_skinSku">Skin to be checked. LOCKED if sku was not found or wardrobe wasn't initialized.</param>
	public SkinState GetSkinState(string _skinSku) {
		// Is wardrobe initialized and skin sku valid?
		if(m_disguises != null && m_disguises.ContainsKey(_skinSku)) {
			return m_disguises[_skinSku];
		}
		return SkinState.LOCKED;
	}

	/// <summary>
	/// Set the state of a specific skin.
	/// </summary>
	/// <param name="_skinSku">Skin to be modified. No checks will be performed.</param>
	/// <param name="_newSkinState">New skin state. No checks will be performed.</param>
	public void SetSkinState(string _skinSku, SkinState _newSkinState) {
		// Skip if wardrobe not initialized
		if(m_disguises == null) return;

		SkinState prevState = SkinState.LOCKED;
		m_disguises.TryGetValue(_skinSku, out prevState);

		// Just do it!
		m_disguises[_skinSku] = _newSkinState;

		// If skin has been acquired, notify game
		if(_newSkinState == SkinState.OWNED && prevState != SkinState.OWNED) {
			Messenger.Broadcast<string>(MessengerEvents.SKIN_ACQUIRED, _skinSku);
		}
	}

	/// <summary>
	/// Detect unlocked skins based on given dragon data.
	/// Will check dragon's XP level and move all its locked skins matching the level
	/// to the "NEW" state.
	/// </summary>
	/// <param name="_targetDragon">Target dragon.</param>
	public void ProcessUnlockedSkins(IDragonData _targetDragon) {
		// Skip if wardrobe not initialized
		if(m_disguises == null) return;
		if(_targetDragon == null) return;

		// Get all the skins for the given dragon
		List<DefinitionNode> skinDefs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", _targetDragon.def.sku);
		for(int i = 0; i < skinDefs.Count; i++) {
			// Only check locked skins
			if(GetSkinState(skinDefs[i].sku) != SkinState.LOCKED) continue;

			// Depends on dragon type
			switch(_targetDragon.type) {
				// Classic dragons
				case IDragonData.Type.CLASSIC: {
					// Ignore skins with no unlock level defined - seasonal or default
					// Have we reached the unlock level for this skin?
					int unlockLevel = skinDefs[i].GetAsInt("unlockLevel");
					if(unlockLevel > 0 && (_targetDragon as DragonDataClassic).progression.level >= unlockLevel) {
						// Yes!! Check other unlock conditions

						// If it's a seasonal skin, check the season as well!
						string seasonSku = skinDefs[i].GetAsString("unlockSeason");
						if(!string.IsNullOrEmpty(seasonSku)) {
							// Is season active?
							if(SeasonManager.activeSeason == seasonSku) {
								// Yes!! Mark it as "NEW"
								SetSkinState(skinDefs[i].sku, SkinState.NEW);
							}
						} else {
							// Not a seasonal skin - mark it as "NEW"
							SetSkinState(skinDefs[i].sku, SkinState.NEW);
						}
					}
				} break;

				// Special dragons
				case IDragonData.Type.SPECIAL:
				default: {
					// All skins unlocked for special dragons, skip "new" state
					SetSkinState(skinDefs[i].sku, SkinState.OWNED);
				} break;
			}
		}
	}

	/// <summary>
	/// Refresh all seasonal skins lock state.
	/// </summary>
	public void ProcessSeasonalSkins() {
		// Skip if wardrobe not initialized
		if(m_disguises == null) return;

		// Get all the skins for the given dragon
		List<DefinitionNode> skinDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DISGUISES);
		for(int i = 0; i < skinDefs.Count; i++) {
			// Ignore if the skin is owned (can't change its state)
			SkinState oldState = GetSkinState(skinDefs[i].sku);
			if(oldState == SkinState.OWNED) continue;

			// Ignore if the skin is not seasonal
			string seasonSku = skinDefs[i].GetAsString("unlockSeason");
			if(string.IsNullOrEmpty(seasonSku)) continue;

			// Ignore special dragon skins
			if(IsSpecialSkin(skinDefs[i])) continue;

			// If associated season doesn't match active season, skin is locked
			if(seasonSku != SeasonManager.activeSeason) {
				SetSkinState(skinDefs[i].sku, SkinState.LOCKED);
				continue;
			}

			// Valid season! Check level requirement (if any)
			int unlockLevel = skinDefs[i].GetAsInt("unlockLevel");
			if(unlockLevel > 0) {
				// Depends on dragon type
				IDragonData dragonData = DragonManager.GetDragonData(skinDefs[i].GetAsString("dragonSku"));
				switch(dragonData.type) {
					case IDragonData.Type.CLASSIC: {
							// Do we have enough level?
							if((dragonData as DragonDataClassic).progression.level < unlockLevel) {
								// Not enough level, skin is locked
								SetSkinState(skinDefs[i].sku, SkinState.LOCKED);
								continue;
							}
						} break;
				}
			}

			// All checks passed! Skin available
			// Put the NEW flag if skin wasn't previously available
			if(oldState != SkinState.AVAILABLE) {
				SetSkinState(skinDefs[i].sku, SkinState.NEW);
			} else {
				SetSkinState(skinDefs[i].sku, SkinState.AVAILABLE);
			}
		}
	}

	/// <summary>
	/// Gets the number of acquired skins - excluding default skins and special dragons skins.
	/// </summary>
	/// <returns>The number acquired skins.</returns>
	public int GetNumAcquiredSkins() {
		int ret = 0;
		if(m_disguises != null) {
			foreach(KeyValuePair<string, SkinState> item in m_disguises) {
				if(item.Value == SkinState.OWNED) {
					// Get def
					DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, item.Key);
					if(def == null) continue;

					// Skip if it's the default skin
					if(IsDefaultSkin(def)) continue;

					// Skip if it's a special dragon skin (automatically acquired)
					if(IsSpecialSkin(def)) continue;

					// All checks passed!
					ret++;
				}
			}
		}
		return ret;
	}

	///
	/// Returns unlocked skins for dragon dragonSku from [initialLevel] to [finalLevel]
	/// 
	public List<DefinitionNode> GetUnlockedSkins(string dragonSku, int initialLevel, int finalLevel) {
		List<DefinitionNode> ret = new List<DefinitionNode>();
		// Get aLl dragon skins
		List<DefinitionNode> allSkins = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", dragonSku);
		// Sort by unlock level
		DefinitionsManager.SharedInstance.SortByProperty(ref allSkins, "unlockLevel", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < allSkins.Count; ++i) {
			// Skip if unlockLevel is 0 (default skin)
			int unlockLevel = allSkins[i].GetAsInt("unlockLevel");
			if(unlockLevel <= 0) continue;

			// Skip if already owned
			if(UsersManager.currentUser.wardrobe.GetSkinState(allSkins[i].sku) == Wardrobe.SkinState.OWNED) continue;

			// Check unlock level vs level before starting the game and level after the game
			if(unlockLevel > initialLevel && unlockLevel <= finalLevel) {
				// This skin has been unlocked during this run!
				// Add it to the list
				ret.Add(allSkins[i]);
			}
		}
		return ret;
	}



	//------------------------------------------------------------------//
	// STATIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Is the given skin the default skin for that dragon?
	/// </summary>
	/// <param name="_skinDef">The skin to be checked.</param>
	/// <returns>Whether the skin is the default one for the dragon.</returns>
	public static bool IsDefaultSkin(DefinitionNode _skinDef) {
		// It's considered the default skin if it has no unlock conditions (level 0 and no season associated)
		return _skinDef.GetAsInt("unlockLevel") <= 0 && !IsSeasonalSkin(_skinDef);
	}

	/// <summary>
	/// Is the given skin associated to a season?
	/// </summary>
	/// <param name="_skinDef">The skin to be checked.</param>
	/// <returns>Whether the skin is linked to a season or not.</returns>
	public static bool IsSeasonalSkin(DefinitionNode _skinDef) {
		return !string.IsNullOrEmpty(_skinDef.GetAsString("unlockSeason"));
	}

	/// <summary>
	/// Does this skin belong to a special dragon?
	/// </summary>
	/// <param name="_skinDef">The skin to be checked.</param>
	/// <returns>Whether the skin belongs to a special dragon or not.</returns>
	public static bool IsSpecialSkin(DefinitionNode _skinDef) {
		if(_skinDef == null) return false;
		DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(_skinDef.GetAsString("dragonSku"));
		return dragonDef != null && dragonDef.GetAsString("type") == "special";
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a json object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) 
	{
		SimpleJSON.JSONArray diguisesArr = _data.AsArray;
		int disguisesLength = diguisesArr.Count;
		for (int i = 0; i < disguisesLength; i++) {
			m_disguises[ diguisesArr[i]["disguise"] ] = (SkinState)diguisesArr[i]["level"].AsInt;
		}

		// Active season might have changed, refresh seasonal skins
		ProcessSeasonalSkins();
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() 
	{
		SimpleJSON.JSONArray diguisesArr = new SimpleJSON.JSONArray();
		if(m_disguises != null) {
			foreach (KeyValuePair<string, SkinState> pair in m_disguises) {
				// Don't store locked disguises (no need to make savefile that big!)
				if(pair.Value == SkinState.LOCKED) continue;

				// Same with special skins
				DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, pair.Key);
				if(IsSpecialSkin(def)) continue;

				// We're reusing the old "level" field ^^
				SimpleJSON.JSONClass dl = new SimpleJSON.JSONClass();
				dl.Add("disguise", pair.Key.ToString());
				dl.Add("level", ((int)pair.Value).ToString(System.Globalization.CultureInfo.InvariantCulture));
				diguisesArr.Add(dl);
			}
		}
		return diguisesArr;
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An event from the broadcaster has been received.
	/// </summary>
	/// <param name="_eventType"></param>
	/// <param name="_eventInfo"></param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _eventInfo) {
		switch(_eventType) {
			case BroadcastEventType.SEASON_CHANGED: {
				ProcessSeasonalSkins();
			} break;
		}
	}
}
