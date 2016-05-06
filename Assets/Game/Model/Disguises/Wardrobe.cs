﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Wardrobe : Singleton<Wardrobe> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string SKIN_PATH = "Game/Equipable/Skins/";
	public static readonly string ITEM_PATH = "Game/Equipable/Items/";

	public static readonly int MAX_LEVEL = 3;

	[Serializable]
	public class DragonDisguise {
		public string dragon;
		public string disguise;
	}

	[Serializable]
	public class DisguiseLevel {
		public string disguise;
		public int level;
	}

	/// <summary>
	/// Auxiliar class for persistence load/save.
	/// </summary>
	[Serializable]
	public class SaveData {
		public DisguiseLevel[] disguises = new DisguiseLevel[0];
		public DragonDisguise[] equiped = new DragonDisguise[0];
	}


	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	private Dictionary<string, int> m_disguises;
	private Dictionary<string, string> m_equiped;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialize manager from definitions.
	/// Requires definitions to be loaded into the DefinitionsManager.
	/// </summary>
	public static void InitFromDefinitions() {
		List<string> skus = DefinitionsManager.GetSkuList(DefinitionsCategory.DISGUISES);

		instance.m_disguises = new Dictionary<string, int>();
		for (int i = 0; i < skus.Count; i++) {
			instance.m_disguises.Add(skus[i], 0);
		}

		instance.m_equiped = new Dictionary<string, string>();
	}

	public static string GetRandomDisguise(string _dragonSku, string _rarity) {
		List<DefinitionNode> defs = DefinitionsManager.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", _dragonSku);
		List<string> disguises = new List<string>();
		disguises.Shuffle<string>(Time.renderedFrameCount);

		for (int i = 0; i < defs.Count; i++) {
			if (defs[i].GetAsString("rarity") == _rarity) {
				disguises.Add(defs[i].sku);
			}
		}

		if (disguises.Count > 0) {
			int individualChance = 100 / disguises.Count;
			int index = UnityEngine.Random.Range(0, 100) / individualChance;

			return disguises[index];
		}

		return "";
	}

	public static int GetDisguiseValue(string _sku) {
		return DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES, _sku).GetAsInt("value");
	}

	public static int GetDisguiseLevel(string _sku) {
		if ( instance.m_disguises != null && instance.m_disguises.ContainsKey(_sku))
			return instance.m_disguises[_sku];
		return -1;
	}

	public static bool LevelUpDisguise(string _sku) {
		if (instance.m_disguises[_sku] < MAX_LEVEL) {
			instance.m_disguises[_sku]++;
			return true;
		}

		return false;
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
		if (instance.m_equiped != null && instance.m_equiped.ContainsKey(_dragonSku)) {
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
		int disguisesLength = _data.disguises.Length;
		for (int i = 0; i < disguisesLength; i++) {
			instance.m_disguises[_data.disguises[i].disguise] = _data.disguises[i].level;
		}

		int equipedLength = _data.equiped.Length;
		for (int k = 0; k < equipedLength; k++) {
			instance.m_equiped[_data.equiped[k].dragon] = _data.equiped[k].disguise;
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public static SaveData Save() {
		SaveData data = new SaveData();

		List<DisguiseLevel> saveDisguises = new List<DisguiseLevel>();
		foreach (KeyValuePair<string, int> pair in instance.m_disguises) {
			if (pair.Value > 0) {
				DisguiseLevel dl = new DisguiseLevel();

				dl.disguise = pair.Key;
				dl.level = pair.Value;

				saveDisguises.Add(dl);
			}
		}
		data.disguises = saveDisguises.ToArray();

		int k = 0;
		data.equiped = new DragonDisguise[instance.m_equiped.Count];
		foreach (KeyValuePair<string, string> pair in instance.m_equiped) {
			DragonDisguise dd = new DragonDisguise();

			dd.dragon = pair.Key;
			dd.disguise = pair.Value;

			data.equiped[k] = dd;

			k++;
		}

		return data;
	}
}
