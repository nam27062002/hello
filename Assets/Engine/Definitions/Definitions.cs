using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class Definitions : Singleton<Definitions>
{
	public enum Category
	{
		UNKNOWN,
		LOCALIZATION,
	};
	
	private Dictionary<Category, Dictionary<string,  DefinitionNode > > definitonsMap;

	private Dictionary<Category, List<string>> categoryMap_SkuList = new Dictionary<Category, List<string>>();

    private bool isReady = false;

	public Definitions()
	{
		definitonsMap = new Dictionary<Category, Dictionary<string, DefinitionNode> >();
		LoadDefinitions();
	}

	public void LoadDefinitions()
	{
		Debug.Log("LOAD DEFINITIONS");
		definitonsMap.Clear();
		categoryMap_SkuList.Clear();

		// Settings
		LoadDefinitionsFile( Category.LOCALIZATION, "rules/localizationDefinitions");


		// Warn all other managers and definition consumers

		// Calculate CRC?
		// calculateRulesCRC ();

        isReady = true;        
    }
	
	private void LoadDefinitionsFile( Category category, string filename )
	{
		string fileContent = "";
		string cachePath = Application.persistentDataPath + "/" + filename + ".xml";

		string str = "Loading rules file " + filename;
		// TODO (miguel) : Check config file to know if we can load definitions from cache or directly go to resources folder
		bool fromCache = ( File.Exists(cachePath) /*&& InstanceManager.Config.getDefinitionsFromAssetsLUT()*/);
		str += (fromCache) ? " from cache " + cachePath : " from resources";
		Debug.Log(str);

		XmlDocument doc = null;

        // Checks if the file is cached and the implementation for the request network object is online then the rules are taken from the server
        // otherwise they are taken from the local machine.
        if ( fromCache )
		{          
			StreamReader sr = new StreamReader( cachePath );
			fileContent = sr.ReadToEnd();
			sr.Close();
		}

		if (!string.IsNullOrEmpty (fileContent)) 
		{
			Debug.Log(filename + " rules file loaded from cache");

			doc = new XmlDocument ();
			try
			{
				doc.LoadXml (fileContent);
			}
			catch (System.Exception)
			{
				doc = null;
				Debug.LogError("Exception parsing " + filename + " content: " + fileContent);
			}
		}

		// Xml inside the build is read if no file has been read yet
		if (doc == null)
		{           
			TextAsset textAsset  = (TextAsset) Resources.Load( filename, typeof(TextAsset));
			if ( textAsset == null)
			{
				Debug.LogError("Could not load text asset " + filename);
				return;
			}
			fileContent = textAsset.text;

			Debug.Log(filename + " rules file loaded from ipa");

			doc = new XmlDocument();
			doc.LoadXml( fileContent );
		}

		// if this rule xml file need to be inclused in the CRC to be send to server, make a copy of the source string file
		if (rulesListToCalculateCRC.Contains(filename))
		{
			rulesFilesToJoinToCalculateCRC[filename] = fileContent;
		}
		
		Dictionary<string, DefinitionNode > cat;
		if ( definitonsMap.ContainsKey(category) )
		{
			cat = definitonsMap[ category ];
		}
		else
		{
			cat = new Dictionary<string, DefinitionNode>();
			definitonsMap.Add( category, cat);
		}

		List<string> skuList = new List<string> ();

		// Get all definitions and create nodes
		XmlNodeList list = doc.SelectNodes("//Definitions/Definition");
		foreach (XmlNode node in list)
		{
			DefinitionNode definitionNode = new DefinitionNode();
			definitionNode.LoadFromXml( node );
			string sku = definitionNode.Get("sku");
						
			if (!cat.ContainsKey( sku ))
			{
				cat.Add( sku, definitionNode );
				skuList.Add(sku);
			}
			else
			{
				Debug.LogError("This category already contains an sku : " + sku + " in category " + category);
			}
      	}	

		categoryMap_SkuList[category] = skuList;
	}
	
    public bool IsReady()
    {
        return isReady;
    }

	public DefinitionNode GetDefinition(string sku)
	{
		foreach(  KeyValuePair<Category, Dictionary<string,DefinitionNode >> category in definitonsMap )
		{
			Dictionary<string, DefinitionNode> cat = category.Value;
			if ( cat.ContainsKey( sku ) )
			{
				return cat[ sku ];
			}
		}
		return null;
	}

	public List<string> GetSkuList(Category category)
	{
		if (categoryMap_SkuList.ContainsKey(category))
		{
			return categoryMap_SkuList [category];
		} else {
			return new List<string>();
		}
	}


	public DefinitionNode GetDefinitionFromCategory( Category category, string sku)
	{
		if (definitonsMap.ContainsKey(category))
		{
			Dictionary<string, DefinitionNode> cat = definitonsMap[category];
			if ( cat.ContainsKey( sku ) )
			{
				return cat[ sku ];
			}
		}
		return null;
	}

	public List<DefinitionNode> GetDefinitionsWithPrefix(Category category, string prefix)
	{
		List<DefinitionNode> defs = new List<DefinitionNode>();
		
		if ( definitonsMap.ContainsKey( category ))
		{
			Dictionary<string, DefinitionNode> cat = definitonsMap[category];
			foreach( KeyValuePair<string, DefinitionNode> node in cat)
			{
				if ( node.Key.StartsWith( prefix ) )
				{
					defs.Add( node.Value );
				}
			}
		}
		
		return defs;
	}
	
	
	public Category GetCategoryForSku( string sku )
	{
		foreach(  KeyValuePair<Category, Dictionary<string,DefinitionNode>> category in definitonsMap )
		{
			Dictionary<string, DefinitionNode> cat = category.Value;
			if ( cat.ContainsKey( sku ) )
			{
				return category.Key;
			}
		}
		return Category.UNKNOWN;
	}
	
	private string GetSkuPrefx( string sku )
	{
		int index = sku.LastIndexOf('_');
		if (index != -1)
			return sku.Substring(0, index);
		else
			return sku;
	}
	
	public Dictionary<string, DefinitionNode> GetDefinitions( Category category )
	{
		if (definitonsMap.ContainsKey(category))
		{
			return definitonsMap[category];
		}
		return null;
	}
	
	public List<DefinitionNode> GetDefinitionsWithType( Category category, string type ) 
	{
		List<DefinitionNode> defs = new List<DefinitionNode>();
		
		if ( definitonsMap.ContainsKey( category ))
		{
			Dictionary<string, DefinitionNode> cat = definitonsMap[category];
			foreach( DefinitionNode node in cat.Values)
			{
				if ( node.Get("type").Equals( type ) )
				{
					defs.Add( node );
				}
			}
		}
		
		return defs;
	}
	
	public DefinitionNode GetMaxLevelByType( Category category, string type )
	{
		DefinitionNode returnValue = null;
		List<DefinitionNode> defs = GetDefinitionsWithType( category, type);
		
		foreach( DefinitionNode d in defs)
		{
			if ( returnValue == null || returnValue.GetAsInt("levelId") < d.GetAsInt("levelId") )
			{
				returnValue = d;
			}
		}
		
		return returnValue;
	}

	public DefinitionNode GetDefinitionByVariable(Category category, string variable, string value)
	{
		if ( definitonsMap.ContainsKey( category ))
		{
			Dictionary<string, DefinitionNode> cat = definitonsMap[category];
			foreach( DefinitionNode node in cat.Values)
			{
				if ( node.Get(variable).Equals( value ) )
				{
					return node;
				}
			}
		}
		
		return null;
	}

	public List<DefinitionNode> GetDefinitionsListByVariable(Category category, string variable, string value)
	{
		if ( definitonsMap.ContainsKey( category ))
		{
			List<DefinitionNode> nodes = new List<DefinitionNode>();

			Dictionary<string, DefinitionNode> cat = definitonsMap[category];
			foreach( DefinitionNode node in cat.Values)
			{
				if ( node.Get(variable).Equals( value ) )
				{
					nodes.Add(node);
				}
			}

			return nodes;
		}

		return null;


	}


	// ------------------------------------------------------------------ //

	/**
	 *  List with the xml filename if the rules in the correct order to create a CRC of all files together to be sync with server
	 *  The Server get this list from "sandstorm/server/src/main/resources/rules_files.txt"
	 */
	private static List<string> rulesListToCalculateCRC = new List<string>()
	{
		"rules/dailyMissionDefinitions",
		"rules/distributionDefinitions",
		"rules/edgeDefinitions",
		"rules/featsDefinitions",
		"rules/fleetRewardsDefinitions",
		"rules/generalSettingsDefinitions",
		"rules/hazardDefinitions",
		"rules/initialValuesDefinitions",
		"rules/itemDefinitions",
		"rules/leagueDefinitions",
		"rules/mainMissionDefinitions",
		"rules/monetizationSettingsDefinitions",
		"rules/rewardsTableDefinitions",
		"rules/roomsDefinitions",
		"rules/seasonDefinitions",
		"rules/sectorDefinitions",
		"rules/sequenceDefinitions",
		"rules/sequenceElementDefinitions",
		"rules/shipDefaultDefinitions",
		"rules/shipDefinitions",
		"rules/shipPartsDefinitions",
		"rules/shipSkinsDefinitions",
		"rules/shopDefinitions",
		"rules/sideMissionDefinitions",
		"rules/starterPackDefinitions",
	};
	
	private Dictionary<string, string> rulesFilesToJoinToCalculateCRC = new Dictionary<string, string>();

	private int rulesCRC;

	public int getRulesCRC()
	{
		return rulesCRC;
	}

	private void calculateRulesCRC()
	{
		string rulesFilesJoined = "";
		for (int i=0 ; i<rulesListToCalculateCRC.Count ; i++)
		{
			rulesFilesJoined += rulesFilesToJoinToCalculateCRC[rulesListToCalculateCRC[i]];
		}
		rulesCRC = calculateRulesFilesCRC (rulesFilesJoined);

//		Debug.Log ("rulesCRC: " + rulesCRC);

		rulesFilesToJoinToCalculateCRC.Clear ();		
	}


	private int calculateRulesFilesCRC(string input)
	{
		int h = 317;
		char[] chars = input.ToCharArray ();
		int len = chars.Length;
		for (int i = 0; i < len; i++)
		{
			if (chars[i] >= 0x20 && chars[i] < 0x80)
			{
				h = (23 * h) + chars[i];
			}
		}
		return h;
	}

}
