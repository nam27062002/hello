// DefCollection.cs
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
/// Generic collection of definitions.
/// To be inherited with a definition type of your own.
/// Implements ISerializationCallbackReceiver to be able to edit definitions as an array, 
/// but store them as a dictionary.
/// See http://docs.unity3d.com/ScriptReference/ISerializationCallbackReceiver.OnBeforeSerialize.html
/// See http://answers.unity3d.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
/// TODO:
/// 	- Store them as dictionary by sku for faster access, using a custom editor for editing
/// </summary>
[Serializable]
public class DefinitionSet<T> : ScriptableObject, ISerializationCallbackReceiver where T : Definition {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Content
	protected Dictionary<string, T> m_defsDict = new Dictionary<string, T>();	// Definitions indexed by sku
	[SerializeField] private List<T> m_defsList = new List<T>();				// Only to be used for editing the values in the inspector

	// Properties
	public int Count { get { return m_defsDict.Count; }}
	public Dictionary<string, T> defs { get { return m_defsDict; }}
	public List<string> skus { get { return m_defsDict.Keys.ToList<string>(); }}
	public List<T> defsList { get { return m_defsDict.Values.ToList<T>(); }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get a definition given its sku.
	/// </summary>
	/// <returns>The definition with the given sku. <c>null</c> if not found.</returns>
	/// <param name="_sku">The sku of the wanted definition.</param>
	public virtual T GetDef(string _sku) {
		// Easy! We already have it indexed by sku!
		try {
			return m_defsDict[_sku];
		} catch {
			return null;
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
	public void OnBeforeSerialize() {
		// We don't need to do anything, since all the definitions are already 
		// stored and updated in the list
	}

	/// <summary>
	/// The object has been deserialized.
	/// </summary>
	public void OnAfterDeserialize() {
		// Save the edited List into the dictionary
		// [AOC] TODO!! Feedback for key duplicates
		m_defsDict = new Dictionary<string, T>();
		for(int i = 0; i < m_defsList.Count; i++) {
			// Use sku as dictionary key
			// Check for key duplicates to avoid exception!
			if(!m_defsDict.ContainsKey(m_defsList[i].sku)) {
				m_defsDict.Add(m_defsList[i].sku, m_defsList[i]);
			} else {
				Debug.LogError("Duplicated sku " + m_defsList[i].sku + "!");
			}
		}
	}
}