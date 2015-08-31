// AOCFastTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

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
	public Range m_range = new Range(0, 10);
	[Range(0, 1)] public float m_factor = 0.5f;

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
		if(Input.GetMouseButtonDown(0)) {
			Debug.Log("_____________________________");
			Debug.Log("Manual: " + (m_range.max * m_factor + m_range.min * (1f - m_factor)));
			Debug.Log("  Lerp: " + Mathf.Lerp(m_range.min, m_range.max, m_factor));
		}
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