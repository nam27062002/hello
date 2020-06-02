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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	public TextAsset m_sourceFile;
	public TextAsset m_currentAtlasChars;



	[SerializeField]
	private string m_missingChars = "Output here...";

	private SerializedObject m_findMissingChars;

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


	}


    /// <summary>
    /// Update the inspector window.
    /// </summary>
    public void OnGUI()
    {

		m_sourceFile = (TextAsset) EditorGUILayout.ObjectField("Source File", m_sourceFile, typeof (TextAsset), true);
		m_currentAtlasChars = (TextAsset)EditorGUILayout.ObjectField("Current atlas chars", m_currentAtlasChars, typeof(TextAsset), true);

		if (GUILayout.Button("Find missing chars")) 
        {
			FindMissingChars();
        }

		EditorStyles.textField.wordWrap = true;
		EditorGUILayout.TextArea(m_missingChars, GUILayout.Height(position.height - 20));



	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//


    private void FindMissingChars()
    {
		if (m_sourceFile == null || m_currentAtlasChars == null)
			return;

		string inputText = m_sourceFile.text;
		string dictionary = m_currentAtlasChars.text;

		string output = "";

		for (int i = 0; i < inputText.Length; i++)
		{
			if (!dictionary.Contains( inputText[i].ToString() ))
            {
				if (!output.Contains(inputText[i].ToString()))
					output += inputText[i];
            }
        }

        // Sort all chars
		output = SortString(output);

		m_missingChars = output;

    }

    private string SortString(string input)
	{
		char[] characters = input.ToArray();
		Array.Sort(characters);
		return new string(characters);
	}

}