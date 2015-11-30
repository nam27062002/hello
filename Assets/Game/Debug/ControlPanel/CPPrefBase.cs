// CPPrefBase.cs
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
/// Widget to set/get a value stored in Prefs (i.e. cheats).
/// Base class to be inherited from.
/// </summary>
public class CPPrefBase : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[InfoBox("Don't forget to connect callbacks to this component!")]
	[SerializeField] protected CPPropertyId m_id = new CPPropertyId();

	// References
	[Space(5)]
	[Comment("Label is optional.")]
	[SerializeField] protected Text m_label;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// Shortcut to get the property id as string
	protected string id { get { return m_id.ToString(); }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Check requirements
		DebugUtils.Assert(m_id.id != "", "Pref ID not set!");
	}

	/// <summary>
	/// The component has been enabled.
	/// </summary>
	protected void OnEnable() {
		// Intitialize toggle
		Refresh();
	}

	/// <summary>
	/// Refresh value.
	/// </summary>
	public virtual void Refresh() {
		if(m_label != null) m_label.text = id;
	}
}