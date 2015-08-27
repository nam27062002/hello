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
		float[] floats = {-250f, -100f, -35f, -4f, -1, 0f, 1f, 4f, 35f, 100f, 250f};
		foreach(float f in floats) {
			float mag = MathUtils.GetMagnitude(f);
			Debug.Log(string.Format("{0}: magnitude {1}, prev {2}, next {3}", f, mag, MathUtils.PreviousMultiple(f, mag), MathUtils.NextMultiple(f, mag)));
		}
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