﻿// DefinitionSetTemplate.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Generic serializable dictionary.
/// MUST BE inherited with specific types in order to be exposed to Unity's inspector, using the [Serializable] tag.
/// <example><code>
/// [Serializable]
/// public class MyDict : SerializableDictionary<string, GameObject> { }
/// </code></example>
/// 
/// Similarly, the property drawer included must be inherited in an Editor class
/// and marked as custom property drawer for that specific SerializableDictionary specification:
/// <example><code>
/// [CustomPropertyDrawer(typeof(MyDict), true)]
/// public class MyDictEditor : SerializableDictionaryEditor { }
/// </code></example>
/// 
/// Implements ISerializationCallbackReceiver to be able to edit definitions as an array, 
/// but store them as a dictionary.
/// See http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html
/// See http://answers.unity3d.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
/// TODO:
/// 	- Store them as dictionary by sku for faster access, using a custom editor for editing
/// </summary>
[Serializable]
public class SerializableDictionary<K,T> : ISerializationCallbackReceiver {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Content
	protected Dictionary<K, T> m_dict = new Dictionary<K, T>();		// Definitions indexed by sku
	[SerializeField] private List<T> m_valueList = new List<T>();	// Only to be used for editing the values in the inspector
	[SerializeField] private List<K> m_keyList = new List<K>();		// Only to be used for editing the values in the inspector

	// Properties
	public int Count { get { return m_dict.Count; }}
	public Dictionary<K, T> dict { get { return m_dict; }}
	public List<K> keyList { get { return m_dict.Keys.ToList<K>();}}
	public List<T> valueList { get { return m_dict.Values.ToList<T>(); }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get a value given its key.
	/// </summary>
	/// <returns>The definition with the given key. <c>null</c> if not found.</returns>
	/// <param name="_key">The key of the wanted definition.</param>
	public virtual T Get(K _key) {
		// Easy! We already have it indexed by key!
		try {
			return m_dict[_key];
		} catch {
			return default(T);
		}
	}

	//------------------------------------------------------------------//
	// ISerializationCallbackReceiver IMPLEMENTATION					//
	//------------------------------------------------------------------//
	// See http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html
	// See http://answers.unity3d.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
	/// <summary>
	/// The object is about to get serialized.
	/// </summary>
	public void OnBeforeSerialize() 
	{
		m_keyList.Clear();
		m_valueList.Clear();
		foreach(KeyValuePair<K,T> kvp in m_dict)
		{
			m_keyList.Add(kvp.Key);
			m_valueList.Add(kvp.Value);
		}	
	}

	/// <summary>
	/// The object has been deserialized.
	/// </summary>
	public void OnAfterDeserialize() {
		// Save the edited List into the dictionary
		// [AOC] TODO!! Feedback for key duplicates
		m_dict = new Dictionary<K, T>();
		for(int i = 0; i < Mathf.Min( m_valueList.Count, m_keyList.Count ); i++) 
		{
			try {
				m_dict.Add( m_keyList[i], m_valueList[i]);
			} catch {
				Debug.LogError("<color=red>Element with key </color><color=white>" + m_keyList[i] + "</color><color=red> already exists!</color>");
			}
		}
	}
}