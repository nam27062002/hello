using System;
using UnityEngine;
using System.Collections.Generic;

public class Wardrobe : Singleton<Wardrobe> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private static readonly string SKIN_PATH = "Game/Equipable/Skins/";
	private static readonly string ITEM_PATH = "Game/Equipable/Items/";

	private static readonly int MAX_LEVEL = 3;

	/// <summary>
	/// Auxiliar class for persistence load/save.
	/// </summary>
	[Serializable]
	public class SaveData {
		public int[] disguises;
	}


	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private int[] m_disguises;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		int size = Definitions.GetCategoryCount(Definitions.Category.DISGUISES);
		instance.m_disguises = new int[size];

		for (int i = 0; i < size; i++) {
			instance.m_disguises[i] = 0;
		}
	}


	//------------------------------------------------------------------//
	// EQUIP															//
	//------------------------------------------------------------------//



	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public static void Load(SaveData _data) {
		int size = _data.disguises.Length;
		for (int i = 0; i < size; i++) {
			instance.m_disguises[i] = _data.disguises[i];
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public static SaveData Save() {
		// Create new object, initialize and return it
		SaveData data = new SaveData();
		data.disguises = new int[instance.m_disguises.Length];
		for (int i = 0; i < instance.m_disguises.Length; i++) {
			data.disguises[i] = instance.m_disguises[i];
		}
		return data;
	}
}
