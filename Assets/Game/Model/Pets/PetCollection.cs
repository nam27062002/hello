using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetCollection
{
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private List<string> m_pets;		// Includes baby pets
	private List<string> m_babyPets;	// For convenience, keep baby pets list a part

	public int unlockedPetsCount {
		get { return m_pets.Count; }
	}

	public int unlockedBabyPetsCount {
		get { return m_babyPets.Count; }
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
		m_babyPets = new List<string>();
	}

	/// <summary>
	/// Reset the collection to its initial state.
	/// </summary>
	public void Reset() {
		m_pets.Clear();
		m_babyPets.Clear();
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
		if ( !m_pets.Contains( _sku ) ) {
			m_pets.Add( _sku );

			// Is it a baby pet?
			if(IsBaby(_sku)) {
				m_babyPets.Add(_sku);
			}

			// Notify game!
			Messenger.Broadcast<string>(MessengerEvents.PET_ACQUIRED, _sku);
		}
	}

	/// <summary>
	/// Remove a pet from the collection.
	/// Should only be used for debugging.
	/// </summary>
	/// <param name="_sku">Pet to be removed.</param>
	public void RemovePet(string _sku) {
		m_pets.Remove(_sku);
		m_babyPets.Remove(_sku);
	}

	//------------------------------------------------------------------//
	// STATIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Check whether the given pet is a baby pet or not.
	/// </summary>
	/// <param name="_petSku">The sku of the pet to be checked</param>
	/// <returns>Whether the pet is a baby or not.</returns>
	public static bool IsBaby(string _petSku) {
		// Get definition and check pet's category sku
		DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _petSku);
		if(petDef != null && petDef.Get("category") == "baby") {
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
		// Clear current data
		Reset();

		SimpleJSON.JSONArray petsArr = _data.AsArray;
		int petsLength = petsArr.Count;
		string petSku = "";
		for (int i = 0; i < petsLength; i++) {
			petSku = petsArr[i];
			if(!m_pets.Contains(petSku)) {
				m_pets.Add(petSku);

				// Is it a baby?
				if(IsBaby(petSku)) {
					if(!m_babyPets.Contains(petSku)) {
						m_babyPets.Add(petSku);
					}
				}
			}
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() 
	{
		SimpleJSON.JSONArray petsArr = new SimpleJSON.JSONArray();
		if(m_pets != null) {
			for (int i = 0; i<m_pets.Count; i++) {
				petsArr.Add(m_pets[i]);
			}
		}
		return petsArr;
	}
}
