using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Localization manager is able to parse localization information from text assets.
/// Using it is simple: text = Localization.Get(key), or just add a AutoLocalize script to your labels.
/// You can switch the language by using Localization.language = "French", for example.
/// This will attempt to load the file called "Localization/French.txt" in the Resources folder.
/// It's expected that the file is full of key = value pairs, like so:
/// 
/// LABEL1 = Hello
/// LABEL2 = Music
/// Info = Localization Example
/// 
/// </summary>

public static class Localization
{
	/// <summary>
	/// Whether the localization dictionary has been loaded.
	/// </summary>
 	static public bool localizationHasBeenSet = false;

	// Key = Value dictionary (single language)
	static Dictionary<string, string> m_dictionary = new Dictionary<string, string>();

	// Currently selected language
	static string m_language;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Current localization code as defined by the C# standards
	// @see https://msdn.microsoft.com/en-us/goglobal/bb896001.aspx
	private static string _code = CultureInfo.CurrentCulture.Name;
	public static string code {
		get { return _code; }
	}


	private static CultureInfo _culture = CultureInfo.CurrentCulture;
	public static CultureInfo culture 
	{
		get { return _culture; }
	}


	static private void CheckIfLanguageLoaded()
	{
		if (!localizationHasBeenSet)
		{
			language = PlayerPrefs.GetString("Language", "lang_english");
		}
	}


	// ** NOT USED **  it is here to avoid errors with NGUI engine
	static public Dictionary<string, string[]> dictionary
	{
		get {return null;}
	}

	// ** NOT USED **  it is here to avoid errors with NGUI engine
	static public string[] knownLanguages
	{
		get {return null;}
	}


	/// <summary>
	/// Name of the currently active language.
	/// </summary>
	static public string language
	{
		get
		{
			return m_language;
		}
		set
		{
			if (m_language == null || m_language != value)
			{
				m_language = value;
				LoadDictionary(value);
			}
		}
	}
	
	static public bool ReloadDictionary()
	{
		return LoadDictionary( m_language );
	}

	/// <summary>
	/// Load the specified localization dictionary.
	/// </summary>
	static bool LoadDictionary (string languageCode)
	{
		Debug.Log("Load Language: " + languageCode);
		/*
		// DO NOT REMOVE!!
		LocalizationDef localizationDef = Rules.GetLocalizationDef (languageSku);

		if (localizationDef == null)
		{
			languageSku = "lang_english";
			localizationDef = Rules.GetLocalizationDef (languageSku);

			if (localizationDef == null)
			{
				return false;
			}
		}

		string languageTxtFilename = localizationDef.txtName;

		// Try downloaded file from assetsLUT
		string cachePath = Application.persistentDataPath + "/localization/" + languageTxtFilename + ".txt";
		if ( File.Exists(cachePath) && InstanceManager.Config.getDefinitionsFromAssetsLUT())
		{
			try
			{
				byte[] bytes = File.ReadAllBytes( cachePath );
				SetLanguage( languageSku, new ByteReader(bytes).ReadDictionary());
				return true;
			}
			catch ( Exception e )
			{
				Debug.LogError("Exception " + e);
			}
		}
		*/
		// TODO (miguel) check all the commented code and start using them
		string languageTxtFilename = "english";
		languageCode = "en";

		// Try default file in RESOURCES folder of our ipa/apk file
		TextAsset txt = Resources.Load("Localization/" + languageTxtFilename, typeof(TextAsset)) as TextAsset;
		if (txt != null)
		{
			SetLanguage(languageCode, ReadDictionary( txt.text ));
			return true;
		}
		
		return false;
	}

	private static Dictionary<string,string> ReadDictionary( string content )
	{
		Dictionary<string, string> dict = new Dictionary<string, string>();
		string[] lines = content.Split('\n');
		char[] separator = new char[] { '=' };

		for( int i = 0; i<lines.Length; i++ )
		{
			string[] split = lines[i].Split( separator, 2, System.StringSplitOptions.RemoveEmptyEntries);
			if (split.Length == 2)
			{
				string key = split[0].Trim();
				string val = split[1].Trim().Replace("\\n", "\n");
				dict[key] = val;
			}
		}
		return dict;
	}

	/// <summary>
	/// Load the specified asset and activate the localization.
	/// </summary>
	static private void SetLanguage (string languageCode, Dictionary<string, string> dictionary)
	{
		m_language = languageCode;
		// Create a new culture with the given code
		_culture = CultureInfo.CreateSpecificCulture(m_language);
		// Just in case there is any wild formatting out there, change default current culture as well
		System.Threading.Thread.CurrentThread.CurrentCulture = _culture;

		PlayerPrefs.SetString("Language", m_language);
		m_dictionary = dictionary;
		localizationHasBeenSet = true;
		Messenger.Broadcast(EngineEvents.EVENT_LANGUAGE_CHANGED);
	}
	

	// ------------------------------------------------------------------------ //


	/// <summary>
	/// Returns whether the specified key is present in the localization dictionary.
	/// </summary>
	
	public static bool Exists (string key)
	{
		// Ensure we have a language to work with
		CheckIfLanguageLoaded();
		
		return m_dictionary.ContainsKey(key);
	}


	public static string Get(string key)
	{
		return ReplaceTids(GetKeyValue(key));
	}

	/// <summary>
	/// Localize the specified value.
	/// </summary>

	private static string GetKeyValue (string key)
	{
		// Ensure we have a language to work with
		CheckIfLanguageLoaded();

		string val;
		if (m_dictionary.TryGetValue(key, out val)) return val;

#if UNITY_EDITOR
		if (Application.isPlaying)
		{
			if (string.IsNullOrEmpty(key))
				Debug.LogWarning("Localization key is empty");
			else
				Debug.LogWarning("Localization key not found: '" + key + "'");
		}
#endif
		return key;
	}


	/// <summary>
	/// Replaces the tids.
	/// This funcion search other tids inside the text and replace it for the correct text.
	/// </summary>
	/// <returns>The tids.</returns>
	/// <param name="str">String.</param>
	private static string ReplaceTids(string str)
	{
		int startIndex = str.IndexOf("<");
		
		while (startIndex > -1)
		{
			int endIndex = str.IndexOf (">");
			
			if (endIndex == -1)
			{
				Debug.LogWarning("Sentence error in '" + str + "' . The symbol > didn't find");
				startIndex = -1;
			}
			else
			{
				string tid = str.Substring(startIndex + 1, endIndex - startIndex - 1);
				string text = Get(tid);
				str = str.Replace ("<" + tid + ">", text);

				startIndex = str.IndexOf ("<");
			}
		}

		return str;
	}

	public static string Localize( string key )
	{
		return Get( key );
	}

	public static string Localize (string key, params string[] parameters)
	{		
		return (parameters == null) ? Get(key) : ReplaceParameters( key, parameters);
	}
	
	/// <summary>
	/// Replaces the parameters masked with %U inside a text.
	/// </summary>
	/// <returns>The parameters.</returns>
	/// <param name="tid">Tid.</param>
	/// <param name="parameters">The final text with parameters replaced.</param>
	public static string ReplaceParameters(string tid, string[] parameters)
	{
		string val = Get(tid);		
		int paramCount = parameters.Length;
		string textToReplaceWith;
		if (paramCount > 0)
		{
			while (val.IndexOf("%U") > -1)
			{
				int strindex = val.IndexOf("%U");
				int paramIndex = 0;
				string num = "";
				
				for (int i = strindex + 2; i < val.Length; i++)
				{
					char letter = val.ToCharArray()[i];
					if (letter >= '0' && letter <= '9')
					{
						num += letter;
					}
					else
					{
						break;
					}
				}
				
				paramIndex = int.Parse(num);
				
				textToReplaceWith = (paramIndex < paramCount) ? parameters[paramIndex] : "";				
				val = val.Replace("%U" + num, textToReplaceWith);				
			}
		}
		
		return val;
	}
	

	public static void Dump()
	{
		Debug.Log("------------------------");
		foreach( KeyValuePair<string, string> p in m_dictionary)
		{
			Debug.Log("KEY: " + p.Key + " VALUE : " + p.Value);
		}
		Debug.Log("------------------------");
	}
}
