// CPPropertyID.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/11/2015.
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
/// Auxiliar class to simplify control panel setup.
/// </summary>
[System.Serializable]
public class CPPropertyId {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public string id = "";

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_id">The id of this control panel property.</param>
	public CPPropertyId(string _id = "") {
		id = _id;
	}

	/// <summary>
	/// String representation.
	/// </summary>
	override public string ToString() {
		return id;
	}
}