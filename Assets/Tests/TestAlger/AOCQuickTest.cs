// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
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
	public Mission m_mission = null;

	public delegate void MyDelegate(string _str);
	public MyDelegate m_myDelegateInstance = delegate(string _str) { };

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
		//m_myDelegateInstance += MyDelegate1;
		//m_myDelegateInstance += MyDelegate2;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		for(int i = 0; i < MissionManager.NUM_MISSIONS; i++) {
			Mission mission = MissionManager.GetMission(i);
			Debug.Log("Mission " + i + ": " + mission.def.sku);
			if(i == 1) m_mission = mission;
		}

		m_myDelegateInstance("Invoking MyDelegate");
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
		m_myDelegateInstance -= MyDelegate1;
		m_myDelegateInstance -= MyDelegate2;
	}

	public void MyDelegate1(string _str) {
		Debug.Log(_str);
	}

	public void MyDelegate2(string _str) {
		Debug.Log(_str + " 2!");
	}
}