// Tab.cs
// 
// Created by Alger Ortín Castellví on 26/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Specialization of a navigation screen to work as a tab.
/// [AOC] Does nothing extra for now!!
/// </summary>
public class Tab : NavigationScreen {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private bool m_tabEnabled = true;
	public bool tabEnabled {
		get { return m_tabEnabled; }
		set { m_tabEnabled = value; }	// Don't set directly, do it via TabSystem.SetTabEnabled()
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Nothing to do, just call parent
		base.Awake();
	}
}