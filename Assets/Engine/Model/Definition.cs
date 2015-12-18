// Def.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Generic base class for a definition.
/// To be inherited adding as many fields as needed.
/// </summary>
[Serializable]
public class Definition {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private string m_sku = "";
	public string sku { get { return m_sku; }}
}