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

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		/*int groundWaterMask = LayerMask.GetMask("Ground", "Water");
		int objMask = gameObject.layer;
		int mix = (objMask & groundWaterMask);
		Debug.Log("\n" + Convert.ToString(objMask, 2) + "\n" + Convert.ToString(groundWaterMask, 2) + "\n" + Convert.ToString(mix, 2));*/

		//Debug.Log(Convert.ToString(LayerMask.GetMask("Default"), 2));
		Debug.Log(Convert.ToString(m_mask.value, 2));

		int objMask = (1 << gameObject.layer);
		Debug.Log(Convert.ToString(objMask, 2) + ", " + Convert.ToString(gameObject.layer, 2));

		int checkMask = LayerMask.GetMask("Ground", "Water");
		Debug.Log(Convert.ToString((1 << checkMask), 2) + ", " + Convert.ToString(checkMask, 2));

		int match = (objMask & checkMask);
		Debug.Log(Convert.ToString(match, 2));
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}