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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public LayerMask m_mask;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
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

	public void OnRestart() {
		Debug.Log("RESTART");
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		//Debug.Log(System.DateTime.Now.ToString("yyyyMMdd"));
		Type enumType = typeof(ResultsSceneSetup.ChestTestMode);
		Debug.Log(enumType.AssemblyQualifiedName);
		Type t = Type.GetType("ResultsSceneSetup+ChestTestMode", false);
		Debug.Log(t == null ? "NULL" : t.Name);
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}