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
	// GENERIC SETTERS	 												//
	//------------------------------------------------------------------//
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

	public static void Set(string _key, int _value) {
		#if UNITY_EDITOR
		EditorPrefs.SetInt(_key, _value);
		#else
		PlayerPrefs.SetInt(_key, _value);
		#endif
	}

	public static void Set(string _key, float _value) {
		#if UNITY_EDITOR
		EditorPrefs.SetFloat(_key, _value);
		#else
		PlayerPrefs.SetFloat(_key, _value);
		#endif
	}

	public static void Set(string _key, bool _value) {
		#if UNITY_EDITOR
		EditorPrefs.SetInt(_key, _value ? 1 : 0);
		#else
		PlayerPrefs.SetInt(_key, _value ? 1 : 0);
		#endif
	}

	public static void Set(string _key, Vector2 _value) {
		Set(_key + ".x", _value.x);
		Set(_key + ".y", _value.y);
	}

	public static void Set(string _key, Vector3 _value) {
		Set(_key + ".x", _value.x);
		Set(_key + ".y", _value.y);
		Set(_key + ".z", _value.z);
	}

	public static void Set(string _key, Color _value) {
		Set(_key + ".r", _value.r);
		Set(_key + ".g", _value.g);
		Set(_key + ".b", _value.b);
		Set(_key + ".a", _value.a);
	}

	public static void Set<T>(string _key, T _value) {
		// [AOC] Unfortunately we can't switch a type directly, but we can compare type via an if...else collection
		// [AOC] Double cast trick to prevent compilation errors: http://stackoverflow.com/questions/4092393/value-of-type-t-cannot-be-converted-to
		Type t = typeof(T);

		// String
		if(t == typeof(string)) {
			SetString(_key, (string)(object)_value);
		}

		// Float
		else if(t == typeof(float)) {
			SetFloat(_key, (float)(object)_value);
		}

		// Int
		else if(t == typeof(int)) {
			SetInt(_key, (int)(object)_value);
		}

		// Bool
		else if(t == typeof(bool)) {
			SetBool(_key, (bool)(object)_value);
		}

		// Enum
		else if(t.IsEnum) {
			SetInt(_key, (int)(object)_value);
		}

		// Vector2
		else if(t == typeof(Vector2)) {
			SetVector2(_key, (Vector2)(object)_value);
		}

		// Vector3
		else if(t == typeof(Vector3)) {
			SetVector3(_key, (Vector3)(object)_value);
		}

		// Color
		else if(t == typeof(Color)) {
			SetColor(_key, (Color)(object)_value);
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
	public static string Get(string _key, string _defaultValue) {
		#if UNITY_EDITOR
		return EditorPrefs.GetString(_key, _defaultValue);
		#else
		return PlayerPrefs.GetString(_key, _defaultValue);
		#endif
	}

	public static int Get(string _key, int _defaultValue) {
		#if UNITY_EDITOR
		return EditorPrefs.GetInt(_key, _defaultValue);
		#else
		return PlayerPrefs.GetInt(_key, _defaultValue);
		#endif
	}

	public static float Get(string _key, float _defaultValue) {
		#if UNITY_EDITOR
		return EditorPrefs.GetFloat(_key, _defaultValue);
		#else
		return PlayerPrefs.GetFloat(_key, _defaultValue);
		#endif
	}

	public static bool Get(string _key, bool _defaultValue) {
		#if UNITY_EDITOR
		int intValue = EditorPrefs.GetInt(_key, _defaultValue ? 1 : 0);
		return (intValue == 0) ? false : true;
		#else
		int intValue = PlayerPrefs.GetInt(_key, _defaultValue ? 1 : 0);
		return (intValue == 0) ? false : true;
		#endif
	}

	public static Vector2 Get(string _key, Vector2 _defaultValue) {
		Vector2 value = new Vector2();
		value.x = Get(_key + ".x", _defaultValue.x);
		value.y = Get(_key + ".y", _defaultValue.y);
		return value;
	}

	public static Vector3 Get(string _key, Vector3 _defaultValue) {
		Vector3 value = new Vector3();
		value.x = Get(_key + ".x", _defaultValue.x);
		value.y = Get(_key + ".y", _defaultValue.y);
		value.z = Get(_key + ".z", _defaultValue.z);
		return value;
	}

	public static Color Get(string _key, Color _defaultValue) {
		Color value = new Color();
		value.r = Get(_key + ".r", _defaultValue.r);
		value.g = Get(_key + ".g", _defaultValue.g);
		value.b = Get(_key + ".b", _defaultValue.b);
		value.a = Get(_key + ".a", _defaultValue.a);
		return value;
	}

	public static T Get<T>(string _key, T _defaultValue) {
		// [AOC] Unfortunately we can't switch a type directly, but we can compare type via an if...else collection
		// [AOC] Double cast trick to prevent compilation errors: http://stackoverflow.com/questions/4092393/value-of-type-t-cannot-be-converted-to
		Type t = typeof(T);

		// String
		if(t == typeof(string)) {
			return (T)(object)GetString(_key, (string)(object)_defaultValue);
		}

		// Float
		else if(t == typeof(float)) {
			return (T)(object)GetFloat(_key, (float)(object)_defaultValue);
		}

		// Int
		else if(t == typeof(int)) {
			return (T)(object)GetInt(_key, (int)(object)_defaultValue);
		}

		// Bool
		else if(t == typeof(bool)) {
			return (T)(object)GetBool(_key, (bool)(object)_defaultValue);
		}

		// Enum
		else if(t.IsEnum) {
			return (T)(object)GetInt(_key, (int)(object)_defaultValue);
		}

		// Vector2
		else if(t == typeof(Vector2)) {
			return (T)(object)Get(_key, (Vector2)(object)_defaultValue);
		}

		// Vector3
		else if(t == typeof(Vector3)) {
			return (T)(object)Get(_key, (Vector3)(object)_defaultValue);
		}

		// Color
		else if(t == typeof(Color)) {
			return (T)(object)Get(_key, (Color)(object)_defaultValue);
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
	/// Sets a property to the right preferences dictionary.
	/// </summary>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_value">The value to be stored.</param>
	public static void SetString(string _key, string _value) {
		Set(_key, _value);
	}
	
	public static void SetInt(string _key, int _value) {
		Set(_key, _value);
	}
	
	public static void SetFloat(string _key, float _value) {
		Set(_key, _value);
	}
	
	public static void SetBool(string _key, bool _value) {
		Set(_key, _value);
	}

	public static void SetVector2(string _key, Vector2 _value) {
		Set(_key, _value);
	}

	public static void SetVector3(string _key, Vector3 _value) {
		Set(_key, _value);
	}

	public static void SetColor(string _key, Color _value) {
		Set(_key, _value);
	}

	//------------------------------------------------------------------//
	// GETTERS BY TYPE	 												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets a property from the right preferences dictionary.
	/// </summary>
	/// <returns>The stored value of the preference with key _key. _defaultValue if not found.</returns>
	/// <param name="_key">The key identifying this property.</param>
	/// <param name="_defaultValue">The value to be returned if property was not found.</param>
	public static string GetString(string _key, string _defaultValue = "") {
		return Get(_key, _defaultValue);
	}
	
	public static int GetInt(string _key, int _defaultValue = 0) {
		return Get(_key, _defaultValue);
	}
	
	public static float GetFloat(string _key, float _defaultValue = 0f) {
		return Get(_key, _defaultValue);
	}
	
	public static bool GetBool(string _key, bool _defaultValue = false) {
		return Get(_key, _defaultValue);
	}

	public static Vector2 GetVector2(string _key, Vector2 _defaultValue = new Vector2()) {
		return Get(_key, _defaultValue);
	}

	public static Vector3 GetVector3(string _key, Vector3 _defaultValue = new Vector3()) {
		return Get(_key, _defaultValue);
	}

	public static Color GetColor(string _key, Color _defaultValue = new Color()) {
		return Get(_key, _defaultValue);
	}
}
