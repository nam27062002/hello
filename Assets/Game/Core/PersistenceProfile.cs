// PersistenceProfile.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class allowing us to store a profile in a GameObject so it can be loaded into 
/// the persistence manager instead of the stored profile.
/// </summary>
public class PersistenceProfile : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string RESOURCES_FOLDER = "Debug/PersistenceProfiles";
	public const string DEFAULT_PROFILE = "Default";	// This one must always exist

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// To prevent some important profiles from being deleted
	// Don't serialize, can only be set from code or from debug inspector
	private bool m_canBeDeleted = true;	
	public bool canBeDeleted { get { return m_canBeDeleted; }}

	// The actual data
	[SerializeField] private SimpleJSON.JSONClass m_data = null;
	public SimpleJSON.JSONClass data {
		get { 
			if(m_data == null) m_data = new SimpleJSON.JSONClass();
			return m_data; 
		}
		set { m_data = value; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Nothing to do
	}
}

