using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetManager
{

	private List<string> m_pets;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	public PetManager()
	{
		m_pets = new List<string>();
	}


	public bool IsPetOwned(string _sku) {
		if ( m_pets != null && m_pets.Contains(_sku)) {
			return true;
		}
		return false;
	}

	public void UnlockDisguise(string _sku) {
		if ( !m_pets.Contains( _sku ) )
			m_pets.Add( _sku );
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
		m_pets.Clear();
		for (int i = 0; i < disguisesLength; i++) {
			if ( !m_pets.Contains( diguisesArr[i]) )
				m_pets.Add( diguisesArr[i] );
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() 
	{
		SimpleJSON.JSONArray diguisesArr = new SimpleJSON.JSONArray();
		if(m_pets != null) {
			for (int i = 0; i<m_pets.Count; i++) {
				diguisesArr.Add(m_pets[i]);
			}
		}
		return diguisesArr;
	}
}
