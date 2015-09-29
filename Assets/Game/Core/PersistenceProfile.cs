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
	public static readonly string RESOURCES_FOLDER = "Game/PersistenceProfiles/";
	public static readonly string DEFAULT_PROFILE = "PF_Default";	// This one must always exist

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// To prevent some important profiles from being deleted
	// Don't serialize, can only be set from code or from debug inspector
	private bool m_canBeDeleted = true;	
	public bool canBeDeleted { get { return m_canBeDeleted; }}

	// The actual data
	[SerializeField] private PersistenceManager.SaveData m_data = new PersistenceManager.SaveData();
	public PersistenceManager.SaveData data {
		get { return m_data; }
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

