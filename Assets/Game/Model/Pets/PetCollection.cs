using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCollection
{
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private List<string> m_pets;

	public int unlockedPetsCount {
		get { return m_pets.Count; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public PetCollection()
	{
		m_pets = new List<string>();
	}

	/// <summary>
	/// Reset the collection to its initial state.
	/// </summary>
	public void Reset() {
		m_pets.Clear();
	}

	/// <summary>
	/// Check whether a pet is unlocked by this user.
	/// </summary>
	/// <returns><c>true</c> if the pet with the given sku has been unlocked by this user, <c>false</c> otherwise.</returns>
	/// <param name="_sku">Sku of the pet to be checked.</param>
	public bool IsPetUnlocked(string _sku) {
		if ( m_pets != null && m_pets.Contains(_sku)) {
			return true;
		}
		return false;
	}

	/// <summary>
	/// Marks the pet with the given sku as unlocked. Nothing happens if the pet was already unlocked.
	/// </summary>
	/// <param name="_sku">Sku of the pet to be unlocked.</param>
	public void UnlockPet(string _sku) {
		if ( !m_pets.Contains( _sku ) )
			m_pets.Add( _sku );
	}

	/// <summary>
	/// Remove a pet from the collection.
	/// Should only be used for debugging.
	/// </summary>
	/// <param name="_sku">Pet to be removed.</param>
	public void RemovePet(string _sku) {
		m_pets.Remove(_sku);
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
