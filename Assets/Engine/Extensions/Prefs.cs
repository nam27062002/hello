// PrefsExt.cs
// 
// Created by Alger Ortín Castellví on 09/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the PlayerPrefs and EditorPrefs classes.
/// </summary>
public static class Prefs {
	//------------------------------------------------------------------//
	// CONSTANTS		 												//
	//------------------------------------------------------------------//
	public enum Mode {
		PLAYER,
		EDITOR
	}

	//------------------------------------------------------------------//
	// GENERAL METHODS	 												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Removes all keys and values from the preferences. Use with caution.
	/// </summary>
	public static void DeleteAll() {
		#if UNITY_EDITOR
		EditorPrefs.DeleteAll();
		#else
		PlayerPrefs.DeleteAll();
		#endif
	}

	//------------------------------------------------------------------//
	// GENERIC SETTERS	 												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Sets a property to the right preferences dictionary.
	/// </summary>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_value">The value to be stored.</param>
	public static void Set(string _key, string _value, Mode _mode) {
		if(_mode == Mode.EDITOR) {
			#if UNITY_EDITOR
			EditorPrefs.SetString(_key, _value);
			#endif
		} else {
			PlayerPrefs.SetString(_key, _value);
		}
	}

	public static void Set(string _key, int _value, Mode _mode) {
		if(_mode == Mode.EDITOR) {
			#if UNITY_EDITOR
			EditorPrefs.SetInt(_key, _value);
			#endif
		} else {
			PlayerPrefs.SetInt(_key, _value);
		}
	}

	public static void Set(string _key, float _value, Mode _mode) {
		if(_mode == Mode.EDITOR) {
			#if UNITY_EDITOR
			EditorPrefs.SetFloat(_key, _value);
			#endif
		} else {
			PlayerPrefs.SetFloat(_key, _value);
		}
	}

	public static void Set(string _key, bool _value, Mode _mode) {
		Set(_key, _value ? 1 : 0, _mode);
	}

	public static void Set(string _key, Vector2 _value, Mode _mode) {
		Set(_key + ".x", _value.x, _mode);
		Set(_key + ".y", _value.y, _mode);
	}

	public static void Set(string _key, Vector3 _value, Mode _mode) {
		Set(_key + ".x", _value.x, _mode);
		Set(_key + ".y", _value.y, _mode);
		Set(_key + ".z", _value.z, _mode);
	}

	public static void Set(string _key, Color _value, Mode _mode) {
		Set(_key + ".r", _value.r, _mode);
		Set(_key + ".g", _value.g, _mode);
		Set(_key + ".b", _value.b, _mode);
		Set(_key + ".a", _value.a, _mode);
	}

	public static void Set<T>(string _key, T _value, Mode _mode) {
		// [AOC] Unfortunately we can't switch a type directly, but we can compare type via an if...else collection
		// [AOC] Double cast trick to prevent compilation errors: http://stackoverflow.com/questions/4092393/value-of-type-t-cannot-be-converted-to
		Type t = typeof(T);

		// String
		if(t == typeof(string)) {
			Set(_key, (string)(object)_value, _mode);
		}

		// Float
		else if(t == typeof(float)) {
			Set(_key, (float)(object)_value, _mode);
		}

		// Int
		else if(t == typeof(int)) {
			Set(_key, (int)(object)_value, _mode);
		}

		// Bool
		else if(t == typeof(bool)) {
			Set(_key, (bool)(object)_value, _mode);
		}

		// Enum
		else if(t.IsEnum) {
			Set(_key, (int)(object)_value, _mode);
		}

		// Vector2
		else if(t == typeof(Vector2)) {
			Set(_key, (Vector2)(object)_value, _mode);
		}

		// Vector3
		else if(t == typeof(Vector3)) {
			Set(_key, (Vector3)(object)_value, _mode);
		}

		// Color
		else if(t == typeof(Color)) {
			Set(_key, (Color)(object)_value, _mode);
		}

		// Unsupported
		else {
			Debug.Log("Unsupported type!");
		}
	}

	//------------------------------------------------------------------//
	// GENERIC GETTERS	 												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets a property from the right preferences dictionary.
	/// </summary>
	/// <returns>The stored value of the preference with key _key. _defaultValue if not found.</returns>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_defaultValue">The value to be returned if property was not found.</param>
	public static string Get(string _key, string _defaultValue, Mode _mode) {
		if(_mode == Mode.EDITOR) {
			#if UNITY_EDITOR
			return EditorPrefs.GetString(_key, _defaultValue);
			#else
			return _defaultValue;
			#endif
		} else {
			return PlayerPrefs.GetString(_key, _defaultValue);
		}
	}

	public static int Get(string _key, int _defaultValue, Mode _mode) {
		if(_mode == Mode.EDITOR) {
			#if UNITY_EDITOR
			return EditorPrefs.GetInt(_key, _defaultValue);
			#else
			return _defaultValue;
			#endif
		} else {
			return PlayerPrefs.GetInt(_key, _defaultValue);
		}
	}

	public static float Get(string _key, float _defaultValue, Mode _mode) {
		if(_mode == Mode.EDITOR) {
			#if UNITY_EDITOR
			return EditorPrefs.GetFloat(_key, _defaultValue);
			#else
			return _defaultValue;
			#endif
		} else {
			return PlayerPrefs.GetFloat(_key, _defaultValue);
		}
	}

	public static bool Get(string _key, bool _defaultValue, Mode _mode) {
		int intValue = Get(_key, (int)(_defaultValue ? 1 : 0), _mode);
		return (intValue == 0) ? false : true;
	}

	public static Vector2 Get(string _key, Vector2 _defaultValue, Mode _mode) {
		Vector2 value = new Vector2();
		value.x = Get(_key + ".x", _defaultValue.x, _mode);
		value.y = Get(_key + ".y", _defaultValue.y, _mode);
		return value;
	}

	public static Vector3 Get(string _key, Vector3 _defaultValue, Mode _mode) {
		Vector3 value = new Vector3();
		value.x = Get(_key + ".x", _defaultValue.x, _mode);
		value.y = Get(_key + ".y", _defaultValue.y, _mode);
		value.z = Get(_key + ".z", _defaultValue.z, _mode);
		return value;
	}

	public static Color Get(string _key, Color _defaultValue, Mode _mode) {
		Color value = new Color();
		value.r = Get(_key + ".r", _defaultValue.r, _mode);
		value.g = Get(_key + ".g", _defaultValue.g, _mode);
		value.b = Get(_key + ".b", _defaultValue.b, _mode);
		value.a = Get(_key + ".a", _defaultValue.a, _mode);
		return value;
	}

	public static T Get<T>(string _key, T _defaultValue, Mode _mode) {
		// [AOC] Unfortunately we can't switch a type directly, but we can compare type via an if...else collection
		// [AOC] Double cast trick to prevent compilation errors: http://stackoverflow.com/questions/4092393/value-of-type-t-cannot-be-converted-to
		Type t = typeof(T);

		// String
		if(t == typeof(string)) {
			return (T)(object)Get(_key, (string)(object)_defaultValue, _mode);
		}

		// Float
		else if(t == typeof(float)) {
			return (T)(object)Get(_key, (float)(object)_defaultValue, _mode);
		}

		// Int
		else if(t == typeof(int)) {
			return (T)(object)Get(_key, (int)(object)_defaultValue, _mode);
		}

		// Bool
		else if(t == typeof(bool)) {
			return (T)(object)Get(_key, (bool)(object)_defaultValue, _mode);
		}

		// Enum
		else if(t.IsEnum) {
			return (T)(object)Get(_key, (int)(object)_defaultValue, _mode);
		}

		// Vector2
		else if(t == typeof(Vector2)) {
			return (T)(object)Get(_key, (Vector2)(object)_defaultValue, _mode);
		}

		// Vector3
		else if(t == typeof(Vector3)) {
			return (T)(object)Get(_key, (Vector3)(object)_defaultValue, _mode);
		}

		// Color
		else if(t == typeof(Color)) {
			return (T)(object)Get(_key, (Color)(object)_defaultValue, _mode);
		}

		// Unsupported
		else {
			Debug.Log("Unsupported type!");
		}
		return _defaultValue;
	}

	//------------------------------------------------------------------//
	// SETTERS BY TYPE	 												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Sets a property to the PLAYER preferences dictionary.
	/// </summary>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_value">The value to be stored.</param>
	public static void SetStringPlayer(string _key, string _value) {
		Set(_key, _value, Mode.PLAYER);
	}
	
	public static void SetIntPlayer(string _key, int _value) {
		Set(_key, _value, Mode.PLAYER);
	}
	
	public static void SetFloatPlayer(string _key, float _value) {
		Set(_key, _value, Mode.PLAYER);
	}
	
	public static void SetBoolPlayer(string _key, bool _value) {
		Set(_key, _value, Mode.PLAYER);
	}

	public static void SetVector2Player(string _key, Vector2 _value) {
		Set(_key, _value, Mode.PLAYER);
	}

	public static void SetVector3Player(string _key, Vector3 _value) {
		Set(_key, _value, Mode.PLAYER);
	}

	public static void SetColorPlayer(string _key, Color _value) {
		Set(_key, _value, Mode.PLAYER);
	}

	/// <summary>
	/// Sets a property to the EDITOR preferences dictionary.
	/// </summary>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_value">The value to be stored.</param>
	public static void SetStringEditor(string _key, string _value) {
		Set(_key, _value, Mode.EDITOR);
	}

	public static void SetIntEditor(string _key, int _value) {
		Set(_key, _value, Mode.EDITOR);
	}

	public static void SetFloatEditor(string _key, float _value) {
		Set(_key, _value, Mode.EDITOR);
	}

	public static void SetBoolEditor(string _key, bool _value) {
		Set(_key, _value, Mode.EDITOR);
	}

	public static void SetVector2Editor(string _key, Vector2 _value) {
		Set(_key, _value, Mode.EDITOR);
	}

	public static void SetVector3Editor(string _key, Vector3 _value) {
		Set(_key, _value, Mode.EDITOR);
	}

	public static void SetColorEditor(string _key, Color _value) {
		Set(_key, _value, Mode.EDITOR);
	}

	//------------------------------------------------------------------//
	// GETTERS BY TYPE	 												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets a property from the PLAYER preferences dictionary.
	/// </summary>
	/// <returns>The stored value of the preference with key _key. _defaultValue if not found.</returns>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_defaultValue">The value to be returned if property was not found.</param>
	public static string GetStringPlayer(string _key, string _defaultValue = "") {
		return Get(_key, _defaultValue, Mode.PLAYER);
	}
	
	public static int GetIntPlayer(string _key, int _defaultValue = 0) {
		return Get(_key, _defaultValue, Mode.PLAYER);
	}
	
	public static float GetFloatPlayer(string _key, float _defaultValue = 0f) {
		return Get(_key, _defaultValue, Mode.PLAYER);
	}
	
	public static bool GetBoolPlayer(string _key, bool _defaultValue = false) {
		return Get(_key, _defaultValue, Mode.PLAYER);
	}

	public static Vector2 GetVector2Player(string _key, Vector2 _defaultValue = new Vector2()) {
		return Get(_key, _defaultValue, Mode.PLAYER);
	}

	public static Vector3 GetVector3Player(string _key, Vector3 _defaultValue = new Vector3()) {
		return Get(_key, _defaultValue, Mode.PLAYER);
	}

	public static Color GetColorPlayer(string _key, Color _defaultValue = new Color()) {
		return Get(_key, _defaultValue, Mode.PLAYER);
	}

	/// <summary>
	/// Gets a property from the EDITOR preferences dictionary.
	/// </summary>
	/// <returns>The stored value of the preference with key _key. _defaultValue if not found.</returns>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_defaultValue">The value to be returned if property was not found.</param>
	public static string GetStringEditor(string _key, string _defaultValue = "") {
		return Get(_key, _defaultValue, Mode.EDITOR);
	}

	public static int GetIntEditor(string _key, int _defaultValue = 0) {
		return Get(_key, _defaultValue, Mode.EDITOR);
	}

	public static float GetFloatEditor(string _key, float _defaultValue = 0f) {
		return Get(_key, _defaultValue, Mode.EDITOR);
	}

	public static bool GetBoolEditor(string _key, bool _defaultValue = false) {
		return Get(_key, _defaultValue, Mode.EDITOR);
	}

	public static Vector2 GetVector2Editor(string _key, Vector2 _defaultValue = new Vector2()) {
		return Get(_key, _defaultValue, Mode.EDITOR);
	}

	public static Vector3 GetVector3Editor(string _key, Vector3 _defaultValue = new Vector3()) {
		return Get(_key, _defaultValue, Mode.EDITOR);
	}

	public static Color GetColorEditor(string _key, Color _defaultValue = new Color()) {
		return Get(_key, _defaultValue, Mode.EDITOR);
	}
}
