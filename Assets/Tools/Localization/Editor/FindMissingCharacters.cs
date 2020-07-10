// FindMissingCharacters.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 26/05/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using System.Text;
using System.Linq;
using TMPro;
using System.Text.RegularExpressions;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This tool finds all the characters present in the source file A but missing in
/// the file B. Them show them all in the output.
/// </summary>
[Serializable]
public class FindMissingCharacters: EditorWindow {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	const string FONT_PATH = "UI/Fonts/";
	const string LOCALIZATION_PATH = "Localization/";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	public TMP_FontAsset m_font;

	public List<LanguageSet> m_languageSets;


	[SerializeField]
	private string m_output = "Output here...";

	private SerializedObject m_findMissingChars;


	//------------------------------------------------------------------------//
	// INNER CLASSES														  //
	//------------------------------------------------------------------------//
	public class LanguageSet
	{
		public TextAsset localizationTxt;
		public TMP_FontAsset tmpFont;

        public LanguageSet(string _localizationFilePath, string _fontPath)
        {
			localizationTxt = Resources.Load<TextAsset>(_localizationFilePath);
			tmpFont = Resources.Load<TMP_FontAsset>(_fontPath);

        }
	}


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Opens the window.
	/// </summary>
	[MenuItem("Tools/Localization/Find Missing Characters")]
	public static void OpenWindow()
	{
		GetWindow<FindMissingCharacters>();
	}


    private void OnEnable()
    {
		m_languageSets = new List<LanguageSet>();

        // Define here all the paths of the font assets and the localizations files

		// English
		m_languageSets.Add(new LanguageSet(LOCALIZATION_PATH + "english", FONT_PATH + "FNT_Default/FNT_Default"));
		// Chinese
		m_languageSets.Add(new LanguageSet(LOCALIZATION_PATH + "simplified_chinese", FONT_PATH + "FNT_ZH_Simpl/FNT_ZH_Simpl"));

	}


    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI()
    {
		EditorGUILayout.LabelField("Languages", EditorStyles.boldLabel);

		foreach (LanguageSet set in m_languageSets)
        {
			GUILayout.BeginHorizontal("box");
			set.localizationTxt = (TextAsset)EditorGUILayout.ObjectField("Source File", set.localizationTxt, typeof(TextAsset), true);
            set.tmpFont = (TMP_FontAsset)EditorGUILayout.ObjectField("TMP Font", set.tmpFont, typeof(TMP_FontAsset), true);
			GUILayout.EndHorizontal();
		}

		if (GUILayout.Button("Find missing chars")) 
        {
			FindMissingChars();
        }

		EditorStyles.textField.wordWrap = true;
		EditorGUILayout.TextArea(m_output, GUILayout.Height(position.height - 20));



	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Button "Find missing characters" was pressed.
    /// </summary>
    private void FindMissingChars()
    {

        // Iterate all languages
		foreach (LanguageSet set in m_languageSets)
        {
			string dictionary = TMP_FontAsset.GetCharacters(set.tmpFont);
			string inputText = set.localizationTxt.text;

			string missingChars = "";

            // Separate the localization file in lines
			string[] lines = inputText.Split('\n');

            foreach (string line in lines)
            {
				// Separate key from value
				string[] entry = line.Split('=');

                // If there is no value in the entry, jump to next
                if (entry.Length < 2)
					continue;

				string value = entry[1];

				// Remove possible external keys in the value
				value = Regex.Replace(value, "<[^<>]+>", "");

				// Ignore the key and look for missing characters in the value
				for (int i = 0; i < value.Length; i++)
				{
					if (!dictionary.Contains(value[i].ToString()))
					{
						// Avoid duplicities
						if (!missingChars.Contains(value[i].ToString()))
							missingChars += value[i];
					}
				}
			}
           

            if (missingChars != "")
            {
                // Sort the characters
				missingChars = SortString(missingChars);

				m_output += "\nMissing chars in " + set.tmpFont.name + ":";
				m_output += "\n" + missingChars + "\n";
                

			}

		}

		
	}

	private string SortString(string input)
	{
		char[] characters = input.ToArray();
		Array.Sort(characters);
		return new string(characters);
	}

}