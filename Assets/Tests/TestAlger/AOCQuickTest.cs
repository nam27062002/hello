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

	public float m_newValue = 0.25f;
	public bool m_resetProbSet = false;
	public bool m_redistribute = false;

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
		if(m_resetProbSet) {
			m_probSet = new ProbabilitySet();
			m_resetProbSet = false;
		}

		m_probSet.AddElement("New Element " + m_probSet.numElements);
		m_probSet.SetProbability(m_probSet.numElements - 1, m_newValue, false);

		if(m_redistribute) {
			m_probSet.Redistribute();
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}