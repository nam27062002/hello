// DragonHealthModifier.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple class to store and manage dragon health modifiers.
/// </summary>
[System.Serializable]
public class DragonHealthModifier {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Known skus
	public const string EATMORE_SKU = "health_modifier_eatmore";
	public const string STARVING_SKU = "health_modifier_starving";
	public const string CRITICAL_SKU = "health_modifier_critical";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private float m_threshold = 0f;
	public float threshold {
		get { return m_threshold; }
	}

	private float m_modifier = 0f;
	public float modifier {
		get { return m_modifier; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public DragonHealthModifier() {

	}

	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_def">Definition to be used to initialize the modifier data.</param>
	public DragonHealthModifier(DefinitionNode _def) {
		InitFromDef(_def);
	}

	/// <summary>
	/// Initialize data with the given definition.
	/// </summary>
	/// <param name="_def">Definition to be used.</param>
	public void InitFromDef(DefinitionNode _def) {
		// Ignore if def not valid
		if(_def == null) return;

		// Store data from def
		m_def = _def;
		m_threshold = _def.GetAsFloat("threshold");
		m_modifier = _def.GetAsFloat("modifier");
	}

	//------------------------------------------------------------------------//
	// CONSTANT VALUES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Is this health modifier the "Eat More" one?
	/// </summary>
	/// <returns><c>true</c> if this health modifier corresponds to the "Eat More" state, <c>false</c> otherwise.</returns>
	public bool IsEatMore() {
		return (m_def != null && m_def.sku == EATMORE_SKU);
	}

	/// <summary>
	/// Is this health modifier the "Starving" one?
	/// </summary>
	/// <returns><c>true</c> if this health modifier corresponds to the "Starving" state, <c>false</c> otherwise.</returns>
	public bool IsStarving() {
		return (m_def != null && m_def.sku == STARVING_SKU);
	}

	/// <summary>
	/// Is this health modifier the "Critical" one?
	/// </summary>
	/// <returns><c>true</c> if this health modifier corresponds to the "Critical" state, <c>false</c> otherwise.</returns>
	public bool IsCritical() {
		return (m_def != null && m_def.sku == CRITICAL_SKU);
	}
}