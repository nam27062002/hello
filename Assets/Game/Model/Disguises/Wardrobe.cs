using System;
using UnityEngine;
using System.Collections.Generic;

public class Wardrobe : Singleton<Wardrobe> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string SKIN_PATH = "Game/Equipable/Skins/";
	public static readonly string ITEM_PATH = "Game/Equipable/Items/";

	public static readonly int MAX_LEVEL = 3;

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
	private SortedList<string, int> m_disguises;

	private Dictionary<string, string> m_equiped;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		List<string> skus = Definitions.GetSkuList(Definitions.Category.DISGUISES);
		instance.m_disguises = new SortedList<string, int>();

		for (int i = 0; i < skus.Count; i++) {
			instance.m_disguises.Add(skus[i], 0);
		}

		instance.m_equiped = new Dictionary<string, string>();
	}

	public static int GetDisguiseLevel(string _sku) {
		return instance.m_disguises[_sku];
	}


	//------------------------------------------------------------------//
	// EQUIP															//
	//------------------------------------------------------------------//

	public static void Equip(string _dragonSku, string _disguiseSku) {
		string oldDisguise = "";

		if (instance.m_equiped.ContainsKey(_dragonSku)) {
			oldDisguise = instance.m_equiped[_dragonSku];
		}

		instance.m_equiped[_dragonSku] = _disguiseSku;

		if (oldDisguise != _disguiseSku) {
			Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, _dragonSku);
		}
	}

	public static string GetEquipedDisguise(string _dragonSku) {
		if (instance.m_equiped.ContainsKey(_dragonSku)) {
			return instance.m_equiped[_dragonSku];
		} 
		return "";
	}

	//------------------------------------------------------------------//
	// PERSISTENCE														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public static void Load(SaveData _data) {
		int size = _data.disguises.Length;
		IList<string> keys = instance.m_disguises.Keys;

		for (int i = 0; i < size; i++) {
			instance.m_disguises[keys[i]] = _data.disguises[i];
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public static SaveData Save() {
		IList<int> values = instance.m_disguises.Values;

		SaveData data = new SaveData();
		data.disguises = new int[values.Count];

		for (int i = 0; i < values.Count; i++) {
			data.disguises[i] = values[i];
		}

		return data;
	}
}
