using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wardrobe 
{
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string SKIN_PATH = "Game/Equipable/Skins/";
	public static readonly string ITEM_PATH = "Game/Equipable/Items/";

	public static readonly int MAX_LEVEL = 3;

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private Dictionary<string, int> m_disguises;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	public Wardrobe()
	{
		m_disguises = new Dictionary<string, int>();
	}

	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public void InitFromDefinitions() 
	{
		List<string> skus = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.DISGUISES);
		m_disguises.Clear();
		for (int i = 0; i < skus.Count; i++) {
			m_disguises.Add(skus[i], 0);
		}
	}

	public static string GetRandomDisguise(string _dragonSku, string _rarity) {
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", _dragonSku);
		List<string> disguises = new List<string>();

		for (int i = 0; i < defs.Count; i++) {
			if (defs[i].GetAsString("rarity") == _rarity) {
				disguises.Add(defs[i].sku);
			}
		}
			
		if (disguises.Count > 0) {
			disguises.Shuffle<string>(Time.renderedFrameCount);
			return disguises.GetRandomValue();
		}

		return "";
	}


	public int GetDisguiseValue(string _sku) {
		return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _sku).GetAsInt("value");
	}

	public int GetDisguiseLevel(string _sku) {
		if ( m_disguises != null && m_disguises.ContainsKey(_sku))
			return m_disguises[_sku];
		return -1;
	}

	public bool LevelUpDisguise(string _sku) {
		if (m_disguises[_sku] < MAX_LEVEL) {
			m_disguises[_sku]++;
			return true;
		}

		return false;
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
			m_disguises[ diguisesArr[i]["disguise"] ] = diguisesArr[i]["level"].AsInt;
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
			foreach (KeyValuePair<string, int> pair in m_disguises) {
				if (pair.Value > 0) 
				{
					SimpleJSON.JSONClass dl = new SimpleJSON.JSONClass();

					dl.Add("disguise", pair.Key.ToString());
					dl.Add("level", pair.Value.ToString());

					diguisesArr.Add(dl);
				}
			}
		}
		return diguisesArr;
	}
}
