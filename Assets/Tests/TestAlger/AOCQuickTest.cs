// AOCFastTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	//public DragonData m_dragon;
	//public DragonSkill[] m_skills;
	//public PersistenceManager.SaveData m_myData;
	//public GUIStyle m_myStlye;
	public PersistenceProfile m_selectedProfile;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//

	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		Debug.Log(PlayerPrefs.GetInt("test", -1));
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {

	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {

	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//

}