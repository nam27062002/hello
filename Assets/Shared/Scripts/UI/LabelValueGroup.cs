// LabelValueGroup.cs
// Monster
// 
// Created by Alger Ortín Castellví on 16/06/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to group a label and a value textfield pair.
/// </summary>
public class LabelValueGroup : MonoBehaviour{
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Visual elements
	public Text labelTxt = null;
	public Text valueTxt = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Start() {
		// Check required stuff
		DebugUtils.Assert(labelTxt != null, "Required member!");
		DebugUtils.Assert(valueTxt != null, "Required member!");
	}
}
