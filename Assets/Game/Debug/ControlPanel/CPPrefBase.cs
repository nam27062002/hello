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
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Widget to set/get a value stored in Prefs (i.e. cheats).
/// Base class to be inherited from.
/// </summary>
public class CPPrefBase : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] protected CPPropertyId m_id = new CPPropertyId();
	public string id {
		get { return m_id.ToString(); }
		set { m_id.id = value; }
	}

	// References
	[Space(5)]
	[Comment("Label is optional.")]
	[SerializeField] protected TextMeshProUGUI m_label;
	public TextMeshProUGUI label { get { return m_label; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		
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