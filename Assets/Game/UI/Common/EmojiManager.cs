// EmojiManager.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Static class to be used as dictionary for emojis.
/// </summary>
//[CreateAssetMenu]
public class EmojiManager : SingletonScriptableObject<EmojiManager> {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[System.Serializable]
	public class EmojiData {
		public string key = "";
		public string unicodeHexCode = "0";

		/// <summary>
		/// Convert the unicode HexCode to the actual emoji string.
		/// </summary>
		/// <returns>The emoji string.</returns>
		public string GetEmojiString() {
			return EmojiManager.UnicodeToString(unicodeHexCode);
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private List<EmojiData> m_emojiDatabase = new List<EmojiData>();

	// Internal
	private Dictionary<string, EmojiData> m_dictionary = null;
	private Dictionary<string, EmojiData> dictionary {
		get {
			if(m_dictionary == null) {
				BuildDictionary();
			}
			return m_dictionary;
		}
	}

	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parse and replace emoji keys by their unicode string.
	/// </summary>
	/// <returns>The emojis.</returns>
	/// <param name="_str">String.</param>
	public static string ReplaceEmojis(string _str) {
		// Detect all keys within the string
		string key = "";
		int startIdx = 0;
		int endIdx = 0;
		HashSet<string> foundKeys = new HashSet<string>();
		do {
			// Find first opening tag
			startIdx = _str.IndexOf(':', endIdx);
			if(startIdx >= 0) {
				// Find closing tag
				endIdx = _str.IndexOf(':', startIdx + 1);
				if(endIdx >= 0) {
					// Check whehter it's a known key (could be a genuine ':' in the text)
					key = _str.Substring(startIdx, endIdx + 1 - startIdx);  // Include closing tag
					if(instance.dictionary.ContainsKey(key)) {
						// Valid key! Advance index to include closing tag
						endIdx += 1;

						// Store to found keys collection
						foundKeys.Add(key);
					}
				}
			}
		} while(startIdx >= 0);

		// Replace all found keys by their emoji string
		foreach(string k in foundKeys) {
			// No need to check if the key exists, since it has been done in the previous loop
			_str = _str.Replace(k, instance.dictionary[k].GetEmojiString());
		}

		return _str;
	}

	/// <summary>
	/// Parse a unicode hex code and converts it into a utf string.
	/// </summary>
	/// <returns>The utf string with the parsed hex code.</returns>
	/// <param name="_unicodeHexCode">The unicode hex code to be parsed.</param>
	public static string UnicodeToString(string _unicodeHexCode) {
		// Aux vars
		int emojiHex = 0;
		string emojiStr = "";
		bool error = false;

		// Parse hex value
		try {
			emojiHex = Convert.ToInt32(_unicodeHexCode, 16);
		} catch {
			error = true;
		}

		// Convert to char
		if(!error) {
			try {
				emojiStr = Char.ConvertFromUtf32(emojiHex);  // Convert to char
			} catch {
				error = true;
			}
		}

		return emojiStr;
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the dictionary for indexed access to emoji data
	/// </summary>
	private void BuildDictionary() {
		// If not created, do it now - otherwise clear existing dictionary
		if(m_dictionary == null) {
			m_dictionary = new Dictionary<string, EmojiData>();
		} else {
			m_dictionary.Clear();
		}

		// Add entries by key
		foreach(EmojiData data in m_emojiDatabase) {
			// Skip if key not valid
			if(string.IsNullOrEmpty(data.key)) continue;

			// Skip if duplicated key
			if(m_dictionary.ContainsKey(data.key)) {
				Debug.LogError(Color.red.Tag("[EmojiManager] ERROR: Duplicated key " + data.key));
				continue;
			}

			// Add it to the dictionary!
			m_dictionary.Add(data.key, data);
		}
	}

	/// <summary>
	/// A change has occurred in the inspector.
	/// </summary>
	private void OnValidate() {
		// Rebuild dictionary
		BuildDictionary();
	}
}