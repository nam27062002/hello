using System;
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
		List<string> skus = DefinitionsManager.SharedInstance.GetSkuList(DefinitionsCategory.DISGUISES);

		instance.m_disguises = new Dictionary<string, int>();
		for (int i = 0; i < skus.Count; i++) {
			instance.m_disguises.Add(skus[i], 0);
		}

		instance.m_equiped = new Dictionary<string, string>();
	}

	public static string GetRandomDisguise(string _dragonSku, string _rarity) {
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", _dragonSku);
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
		return DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _sku).GetAsInt("value");
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
	/// Load state from a json object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public static void Load(SimpleJSON.JSONNode _data) 
	{
		SimpleJSON.JSONArray diguisesArr = _data["disguises"].AsArray;
		int disguisesLength = diguisesArr.Count;
		for (int i = 0; i < disguisesLength; i++) {
			instance.m_disguises[ diguisesArr[i]["disguise"] ] = diguisesArr[i]["level"].AsInt;
		}

		SimpleJSON.JSONArray equipedArr = _data["equiped"].AsArray;
		int equipedLength = equipedArr.Count;
		for (int k = 0; k < equipedLength; k++) {
			instance.m_equiped[ equipedArr[k]["dragon"] ] = equipedArr[k]["disguise"];
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public static SimpleJSON.JSONNode Save() {
		SimpleJSON.JSONNode data = new SimpleJSON.JSONNode();

		SimpleJSON.JSONArray diguisesArr = new SimpleJSON.JSONArray();
		if(instance.m_disguises != null) {
			foreach (KeyValuePair<string, int> pair in instance.m_disguises) {
				if (pair.Value > 0) 
				{
					SimpleJSON.JSONNode dl = new SimpleJSON.JSONNode();

					dl.Add("disguise", pair.Key.ToString());
					dl.Add("level", pair.Value.ToString());

					diguisesArr.Add(dl);
				}
			}
		}
		data["disguises"] = diguisesArr;

		SimpleJSON.JSONArray equipedArr = new SimpleJSON.JSONArray();
		if(instance.m_equiped != null) 
		{
			foreach (KeyValuePair<string, string> pair in instance.m_equiped) 
			{
				SimpleJSON.JSONNode dd = new SimpleJSON.JSONNode();

				dd.Add("dragon",pair.Key);
				dd.Add("disguise", pair.Value);

				equipedArr.Add(dd);
			}
		} 
		 
		data.Add("equiped",equipedArr);
		

		return data;
	}
}
