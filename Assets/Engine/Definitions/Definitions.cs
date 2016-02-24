// Definitions.cs
// Hungry Dragon
// 
// Created by Miguel Angel Linares
// Refactored by Alger Ortín Castellví on 18/02/2016
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Manager for all the rules and definitions imported from content xmls.
/// </summary>
public class Definitions : Singleton<Definitions> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Category {
		UNKNOWN,
		LOCALIZATION,

		// Egg manager
		EGGS,
		EGG_REWARDS
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal data containers
	private Dictionary<Category, Dictionary<string,  DefinitionNode > > m_defsByCategoryAndSku = new Dictionary<Category, Dictionary<string, DefinitionNode> >();
	private Dictionary<Category, List<string>> m_skusByCategory = new Dictionary<Category, List<string>>();

	// Control
	private bool m_isReady = false;
	public static bool ready {
		get { return instance.m_isReady; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public Definitions() {
		// Load the rules from disk
		LoadDefinitions();
	}

	/// <summary>
	/// Load all the definitions from disk
	/// </summary>
	private void LoadDefinitions() {
		Debug.Log("LOAD DEFINITIONS");
		m_defsByCategoryAndSku.Clear();
		m_skusByCategory.Clear();

		// Settings
		LoadDefinitionsFile(Category.LOCALIZATION, "Rules/localizationDefinitions");

		// Gacha
		LoadDefinitionsFile(Category.EGGS, "Rules/eggDefinitions");
		LoadDefinitionsFile(Category.EGG_REWARDS, "Rules/eggRewardDefinitions");

		// ADD HERE ANY NEW DEFINITIONS FILE!

		// Warn all other managers and definition consumers

		// Calculate CRC?
		// calculateRulesCRC ();

        m_isReady = true;        
    }

	/// <summary>
	/// Load a single definitions file and put it into a specific category.
	/// If rules are available in cache, they will be loaded from cache instead.
	/// </summary>
	/// <param name="_category">Category where the definitions file will belong to.</param>
	/// <param name="_path">Full path from the Resources folder (without including extension) of the target xml file.</param>
	private void LoadDefinitionsFile(Category _category, string _path) {
		string fileContent = "";
		string cachePath = Application.persistentDataPath + "/" + _path + ".xml";

		string str = "Loading rules file " + _path;
		// TODO (miguel) : Check config file to know if we can load definitions from cache or directly go to resources folder
		bool fromCache = (File.Exists(cachePath) /*&& InstanceManager.Config.getDefinitionsFromAssetsLUT()*/);
		str += (fromCache) ? " from cache " + cachePath : " from resources";
		Debug.Log(str);

		XmlDocument doc = null;

		// Checks if the file is cached and the implementation for the request network object is online then the rules are taken from the server
		// otherwise they are taken from the local machine.
		if(fromCache) {          
			StreamReader sr = new StreamReader(cachePath);
			fileContent = sr.ReadToEnd();
			sr.Close();
		}

		if(!string.IsNullOrEmpty(fileContent)) {
			Debug.Log(_path + " rules file loaded from cache");

			doc = new XmlDocument();
			try {
				doc.LoadXml(fileContent);
			} catch(System.Exception) {
				doc = null;
				Debug.LogError("Exception parsing " + _path + " content: " + fileContent);
			}
		}

		// Xml inside the build is read if no file has been read yet
		if(doc == null) {           
			TextAsset textAsset = (TextAsset)Resources.Load(_path, typeof(TextAsset));
			if(textAsset == null) {
				Debug.LogError("Could not load text asset " + _path);
				return;
			}
			fileContent = textAsset.text;

			Debug.Log(_path + " rules file loaded from ipa");

			doc = new XmlDocument();
			doc.LoadXml(fileContent);
		}

		// if this rule xml file need to be inclused in the CRC to be send to server, make a copy of the source string file
		if(m_rulesListToCalculateCRC.Contains(_path)) {
			m_rulesFilesToJoinToCalculateCRC[_path] = fileContent;
		}
		
		Dictionary<string, DefinitionNode > cat;
		if(m_defsByCategoryAndSku.ContainsKey(_category)) {
			cat = m_defsByCategoryAndSku[_category];
		} else {
			cat = new Dictionary<string, DefinitionNode>();
			m_defsByCategoryAndSku.Add(_category, cat);
		}

		List<string> skuList = new List<string>();

		// Get all definitions and create nodes
		XmlNodeList list = doc.SelectNodes("//Definitions/Definition");
		foreach(XmlNode node in list) {
			DefinitionNode definitionNode = new DefinitionNode();
			definitionNode.LoadFromXml(node);
						
			if(!cat.ContainsKey(definitionNode.sku)) {
				cat.Add(definitionNode.sku, definitionNode);
				skuList.Add(definitionNode.sku);
			} else {
				Debug.LogError("This category already contains an sku : " + definitionNode.sku + " in category " + _category);
			}
		}	

		m_skusByCategory[_category] = skuList;
	}

	//------------------------------------------------------------------------//
	// DEFINITION GETTERS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the first definition matching the given sku in any category.
	/// </summary>
	/// <returns>The definition, <c>null</c> if no definition with the given sku was found.</returns>
	/// <param name="_sku">The sku of the definition to be searched.</param>
	public static DefinitionNode GetDefinition(string _sku) {
		foreach(KeyValuePair<Category, Dictionary<string,DefinitionNode >> category in instance.m_defsByCategoryAndSku) {
			Dictionary<string, DefinitionNode> cat = category.Value;
			if(cat.ContainsKey(_sku)) {
				return cat[_sku];
			}
		}
		return null;
	}

	/// <summary>
	/// Get the first definition matching the given sku in a specific category.
	/// </summary>
	/// <returns>The definition, <c>null</c> if no definition with the given sku was found.</returns>
	/// <param name="_category">The category where to search the definition.</param>
	/// <param name="_sku">The sku of the definition to be searched.</param>
	public static DefinitionNode GetDefinition(Category _category, string _sku) {
		if(instance.m_defsByCategoryAndSku.ContainsKey(_category)) {
			Dictionary<string, DefinitionNode> cat = instance.m_defsByCategoryAndSku[_category];
			if(cat.ContainsKey(_sku)) {
				return cat[_sku];
			}
		}
		return null;
	}

	/// <summary>
	/// Get all the definitions belonging to a category.
	/// </summary>
	/// <returns>The definitions belonging to the target category, not sorted.</returns>
	/// <param name="_category">The target category.</param>
	public static List<DefinitionNode> GetDefinitions(Category _category) {
		Dictionary<string, DefinitionNode> dict = GetDefinitionsDict(_category);
		if(dict != null) {
			List<DefinitionNode> list = new List<DefinitionNode>(dict.Count);
			foreach(KeyValuePair<string, DefinitionNode> kvp in dict) {
				list.Add(kvp.Value);
			}
			return list;
		}
		return null;
	}

	/// <summary>
	/// Get all the definitions belonging to a category.
	/// </summary>
	/// <returns>The definitions belonging to the target category, sorted by sku.</returns>
	/// <param name="_category">The target category.</param>
	public static Dictionary<string, DefinitionNode> GetDefinitionsDict(Category _category) {
		if(instance.m_defsByCategoryAndSku.ContainsKey(_category)) {
			return instance.m_defsByCategoryAndSku[_category];
		}
		return null;
	}

	/// <summary>
	/// Get all the definitions in a category whose sku starts by a given prefix.
	/// </summary>
	/// <returns>The list of definitions matching both search parameters.</returns>
	/// <param name="_category">The category to be searched.</param>
	/// <param name="_prefix">Sku prefix to filter definitions by.</param>
	public static List<DefinitionNode> GetDefinitionsWithPrefix(Category _category, string _prefix) {
		List<DefinitionNode> defs = new List<DefinitionNode>();
		if(instance.m_defsByCategoryAndSku.ContainsKey(_category)) {
			Dictionary<string, DefinitionNode> cat = instance.m_defsByCategoryAndSku[_category];
			foreach(KeyValuePair<string, DefinitionNode> node in cat) {
				if(node.Key.StartsWith(_prefix)) {
					defs.Add(node.Value);
				}
			}
		}
		return defs;
	}

	/// <summary>
	/// Get the first definition in a specific category whose parameter <paramref name="_variable"/> 
	/// matches <paramref name="_value"/>.
	/// </summary>
	/// <returns>The first definition matching all the search parameters.</returns>
	/// <param name="_category">Category where to look.</param>
	/// <param name="_variable">Name of the definition's parameter to filter by.</param>
	/// <param name="_value">Value of the definition's parameter to filter by.</param>
	public static DefinitionNode GetDefinitionByVariable(Category _category, string _variable, string _value) {
		if(instance.m_defsByCategoryAndSku.ContainsKey(_category)) {
			Dictionary<string, DefinitionNode> cat = instance.m_defsByCategoryAndSku[_category];
			foreach(DefinitionNode node in cat.Values) {
				if(node.Get(_variable).Equals(_value)) {
					return node;
				}
			}
		}

		return null;
	}

	/// <summary>
	/// Get all the definitions in a specific category whose parameter <paramref name="_variable"/> 
	/// matches <paramref name="_value"/>.
	/// </summary>
	/// <returns>All the definitions matching all the search parameters.</returns>
	/// <param name="_category">Category where to look.</param>
	/// <param name="_variable">Name of the definition's parameter to filter by.</param>
	/// <param name="_value">Value of the definition's parameter to filter by.</param>
	public static List<DefinitionNode> GetDefinitionsByVariable(Category _category, string _variable, string _value) {
		if(instance.m_defsByCategoryAndSku.ContainsKey(_category)) {
			List<DefinitionNode> nodes = new List<DefinitionNode>();
			Dictionary<string, DefinitionNode> cat = instance.m_defsByCategoryAndSku[_category];
			foreach(DefinitionNode node in cat.Values) {
				if(node.Get(_variable).Equals(_value)) {
					nodes.Add(node);
				}
			}
			return nodes;
		}
		return null;
	}

	//------------------------------------------------------------------------//
	// OTHER UTILS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Return a list with all the skus belonging to a specific category.
	/// </summary>
	/// <returns>The list of skus belonging to <paramref name="_category"/>.</returns>
	/// <param name="_category">The category to be searched.</param>
	public static List<string> GetSkuList(Category _category) {
		if(instance.m_skusByCategory.ContainsKey(_category)) {
			return instance.m_skusByCategory[_category];
		} else {
			return new List<string>();
		}
	}

	/// <summary>
	/// Find out the category where a definition with a given sku belongs to.
	/// </summary>
	/// <returns>The first category owning a definition with the given sku.</returns>
	/// <param name="_sku">The sku to look for.</param>
	public static Category GetCategoryForSku(string _sku) {
		foreach(KeyValuePair<Category, Dictionary<string,DefinitionNode>> category in instance.m_defsByCategoryAndSku) {
			Dictionary<string, DefinitionNode> cat = category.Value;
			if(cat.ContainsKey(_sku)) {
				return category.Key;
			}
		}
		return Category.UNKNOWN;
	}

	//------------------------------------------------------------------------//
	// CRC STUFF															  //
	// TODO!!																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// List with the xml filename if the rules in the correct order to create a CRC of all files together to be sync with server
	/// The Server get this list from "sandstorm/server/src/main/resources/rules_files.txt"
	/// </summary>
	private static List<string> m_rulesListToCalculateCRC = new List<string>() {
		"Rules/dailyMissionDefinitions",
		"Rules/distributionDefinitions",
		"Rules/edgeDefinitions",
		"Rules/featsDefinitions",
		"Rules/fleetRewardsDefinitions",
		"Rules/generalSettingsDefinitions",
		"Rules/hazardDefinitions",
		"Rules/initialValuesDefinitions",
		"Rules/itemDefinitions",
		"Rules/leagueDefinitions",
		"Rules/mainMissionDefinitions",
		"Rules/monetizationSettingsDefinitions",
		"Rules/rewardsTableDefinitions",
		"Rules/roomsDefinitions",
		"Rules/seasonDefinitions",
		"Rules/sectorDefinitions",
		"Rules/sequenceDefinitions",
		"Rules/sequenceElementDefinitions",
		"Rules/shipDefaultDefinitions",
		"Rules/shipDefinitions",
		"Rules/shipPartsDefinitions",
		"Rules/shipSkinsDefinitions",
		"Rules/shopDefinitions",
		"Rules/sideMissionDefinitions",
		"Rules/starterPackDefinitions",
	};
	
	private Dictionary<string, string> m_rulesFilesToJoinToCalculateCRC = new Dictionary<string, string>();

	private int m_rulesCRC;
	public static int rulesCRC {
		get { return instance.m_rulesCRC; }
	}

	/// <summary>
	/// Calculate and store the CRC for the definitions file set.
	/// </summary>
	private void CalculateRulesCRC() {
		string rulesFilesJoined = "";
		for(int i = 0; i < m_rulesListToCalculateCRC.Count; i++) {
			rulesFilesJoined += m_rulesFilesToJoinToCalculateCRC[m_rulesListToCalculateCRC[i]];
		}
		m_rulesCRC = CalculateRulesFilesCRC(rulesFilesJoined);
		m_rulesFilesToJoinToCalculateCRC.Clear();		
		// Debug.Log ("rulesCRC: " + m_rulesCRC);
	}

	/// <summary>
	/// Calculate the CRC code of a string.
	/// </summary>
	/// <returns>The CRC code computed from <paramref name="_input"/>.</returns>
	/// <param name="_input">The string whose CRC code we want.</param>
	private int CalculateRulesFilesCRC(string _input) {
		int h = 317;
		char[] chars = _input.ToCharArray();
		int len = chars.Length;
		for(int i = 0; i < len; i++) {
			if(chars[i] >= 0x20 && chars[i] < 0x80) {
				h = (23 * h) + chars[i];
			}
		}
		return h;
	}
}