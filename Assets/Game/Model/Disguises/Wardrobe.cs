using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wardrobe 
{
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// [AOC] Disguises no longer has levels. We'll keep the int value to track owned disguises though. 0 - not owned, 1 - owned.
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

	public bool IsDisguiseOwned(string _sku) {
		if ( m_disguises != null && m_disguises.ContainsKey(_sku)) {
			return m_disguises[_sku] > 0;
		}
		return false;
	}

	public void UnlockDisguise(string _sku) {
		m_disguises[_sku] = 1;
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
