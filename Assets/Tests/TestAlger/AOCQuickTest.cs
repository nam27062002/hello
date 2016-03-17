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
	public ProbabilitySet m_probSet = new ProbabilitySet(
		new ProbabilitySet.Element[] {
			new ProbabilitySet.Element("el 1"),
			new ProbabilitySet.Element("el 2"),
			new ProbabilitySet.Element("el 3"),
			new ProbabilitySet.Element("el 4")
		}
	);

	public float m_testNumber = 0f;
	public Range m_range = new Range();

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
		Debug.Log(
			"MathUtils.IsBetween(" + m_testNumber + ", " + m_range.min + ", " + m_range.max + ")\n" +
			MathUtils.IsBetween(m_testNumber, m_range.min, m_range.max)
		);
		Debug.Log(
			"MathUtils.IsWithin(" + m_testNumber + ", " + m_range.min + ", " + m_range.max + ")\n" +
			MathUtils.IsWithin(m_testNumber, m_range.min, m_range.max)
		);
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}