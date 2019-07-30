using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wardrobe 
{
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum SkinState {
		LOCKED = 0,
		NEW = 1,		// Same as "AVAILABLE", but showing the "new" notification
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
	public Wardrobe()
	{
		m_disguises = new Dictionary<string, SkinState>();
	}

	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public void InitFromDefinitions() 
	{
		Dictionary<string, DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.DISGUISES);
		m_disguises.Clear();
		foreach(KeyValuePair<string, DefinitionNode> kvp in defs) {
			// Special case: if unlock level is 0, mark it as owned! (probably dragon's default skins)
			if(kvp.Value.GetAsInt("unlockLevel") <= 0) {
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

		SkinState _prevState = m_disguises[_skinSku];

		// Just do it!
		m_disguises[_skinSku] = _newSkinState;

		// If skin has been acquired, notify game
		if(_newSkinState == SkinState.OWNED && _prevState != SkinState.OWNED) {
			Messenger.Broadcast<string>(MessengerEvents.SKIN_ACQUIRED, _skinSku);
		}
	}

	/// <summary>
	/// Detect unlocked sking based on given dragon data.
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
			// Is the skin locked
			if(GetSkinState(skinDefs[i].sku) == SkinState.LOCKED) {
				switch(_targetDragon.type) {
					case IDragonData.Type.CLASSIC: {
						// Have we reached the unlock level for this skin?
						if((_targetDragon as DragonDataClassic).progression.level >= skinDefs[i].GetAsInt("unlockLevel")) {
							// Yes!! Mark it as "NEW"
							SetSkinState(skinDefs[i].sku, SkinState.NEW);
						}
					} break;

					case IDragonData.Type.SPECIAL:
					default: {
						// All skins unlocked for special dragons, skip "new" state
						SetSkinState(skinDefs[i].sku, SkinState.OWNED);
					} break;
				}
				// Depends on dragon type
				if(_targetDragon is DragonDataClassic) {
					
				} else if(_targetDragon is DragonDataSpecial) {
					// All skins unlocked for special dragons, skip "new" state
					SetSkinState(skinDefs[i].sku, SkinState.OWNED);
				}
			}
		}
	}
    
    /// <summary>
    /// Gets the number owned skins.
    /// </summary>
    /// <returns>The number owned skins.</returns>
    public int GetNumOwnedSkins()
    {
        int ret = 0;
        if ( m_disguises != null )
        {
            foreach (KeyValuePair<string,SkinState> item in m_disguises)
            {
                if (item.Value == SkinState.OWNED)
                {
                    ret++;
                }
            }
        }
        return ret;
    }
    
    
    /// <summary>
    /// Gets the number of adquired skins.
    /// </summary>
    /// <returns>The number owned skins.</returns>
    public int GetNumAdquiredSkins()
    {
        int ret = 0;
        if ( m_disguises != null )
        {
            foreach (KeyValuePair<string,SkinState> item in m_disguises)
            {
                if (item.Value == SkinState.OWNED)
                {
                    // Check if it's not the default
                    DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, item.Key);
                    if ( def != null && def.GetAsInt("unlockLevel") > 0 )
                    {
                        ret++;
                    }
                }
            }
        }
        return ret;
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

				// We're reusing the old "level" field ^^
				SimpleJSON.JSONClass dl = new SimpleJSON.JSONClass();
				dl.Add("disguise", pair.Key.ToString());
				dl.Add("level", ((int)pair.Value).ToString(System.Globalization.CultureInfo.InvariantCulture));
				diguisesArr.Add(dl);
			}
		}
		return diguisesArr;
	}
}
