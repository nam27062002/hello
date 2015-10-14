// PrefsExt.cs
// 
// Created by Alger Ortín Castellví on 09/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the PlayerPrefs and EditorPrefs classes.
/// </summary>
public static class PrefsExt {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// STATIC EXTENSION METHODS											//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// OTHER STATIC METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Sets a property to the right preferences dictionary.
	/// </summary>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_value">The value to be stored.</param>
	public static void Set(string _key, int _value) {
		#if UNITY_EDITOR
		EditorPrefs.SetInt(_key, _value);
		#else
		PlayerPrefs.SetInt(_key, _value);
		#endif
	}

	/// <summary>
	/// Sets a property to the right preferences dictionary.
	/// </summary>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_value">The value to be stored.</param>
	public static void Set(string _key, string _value) {
		#if UNITY_EDITOR
		EditorPrefs.SetString(_key, _value);
		#else
		PlayerPrefs.SetString(_key, _value);
		#endif
	}

	/// <summary>
	/// Sets a property to the right preferences dictionary.
	/// </summary>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_value">The value to be stored.</param>
	public static void Set(string _key, float _value) {
		#if UNITY_EDITOR
		EditorPrefs.SetFloat(_key, _value);
		#else
		PlayerPrefs.SetFloat(_key, _value);
		#endif
	}
	
	/// <summary>
	/// Gets a property from the right preferences dictionary.
	/// </summary>
	/// <returns>The stored value of the preference with key _key. _defaultValue if not found.</returns>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_defaultValue">The value to be returned if property was not found.</param>
	public static int Get(string _key, int _defaultValue) {
		#if UNITY_EDITOR
		return EditorPrefs.GetInt(_key, _defaultValue);
		#else
		return PlayerPrefs.GetInt(_key, _defaultValue);
		#endif
	}

	/// <summary>
	/// Gets a property from the right preferences dictionary.
	/// </summary>
	/// <returns>The stored value of the preference with key _key. _defaultValue if not found.</returns>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_defaultValue">The value to be returned if property was not found.</param>
	public static string Get(string _key, string _defaultValue) {
		#if UNITY_EDITOR
		return EditorPrefs.GetString(_key, _defaultValue);
		#else
		return PlayerPrefs.GetString(_key, _defaultValue);
		#endif
	}

	/// <summary>
	/// Gets a property from the right preferences dictionary.
	/// </summary>
	/// <returns>The stored value of the preference with key _key. _defaultValue if not found.</returns>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_defaultValue">The value to be returned if property was not found.</param>
	public static float Get(string _key, float _defaultValue) {
		#if UNITY_EDITOR
		return EditorPrefs.GetFloat(_key, _defaultValue);
		#else
		return PlayerPrefs.GetFloat(_key, _defaultValue);
		#endif
	}
}
