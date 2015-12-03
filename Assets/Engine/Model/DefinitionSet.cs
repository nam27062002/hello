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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Generic collection of definitions.
/// To be inherited with a definition type of your own.
/// TODO:
/// 	- Store them as dictionary by sku for faster access, using a custom editor for editing
/// </summary>
public class DefinitionSet<T> : ScriptableObject where T : Definition {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Content
	[SerializeField] protected T[] m_defs;

	// Others
	public int Length { get { return m_defs.Length; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get a definition given its sku.
	/// </summary>
	/// <returns>The definition with the given sku. <c>null</c> if not found.</returns>
	/// <param name="_sku">The sku of the wanted definition.</param>
	/// <typeparam name="T">Specialization of the definition.</typeparam>
	public T GetDef<T>(string _sku) where T : Definition {
		// [AOC] Ideally we should have definitions indexed by sku in a dictionary, but the fact that Unity doesn't support dictionary serialization makes it hard
		for(int i = 0; i < m_defs.Length; i++) {
			if(m_defs[i].sku == _sku) {
				return m_defs[i] as T;
			}
		}

		// Definition not found
		return null;
	}

	/// <summary>
	/// Get the definition stored at the given index.
	/// </summary>
	/// <returns>The definition at the given index. <c>null</c> if not found or given index not valid.</returns>
	/// <param name="_idx">The index of the wanted definition.</param>
	/// <typeparam name="T">Specialization of the definition.</typeparam>
	public T GetDef<T>(int _idx) where T : Definition {
		// Check index
		if(_idx < 0 || _idx >= m_defs.Length) return null;

		// Return def at given index
		return m_defs[_idx] as T;
	}
}