// IPowerTooltip.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for power-related tooltips.
/// </summary>
public abstract class IPowerTooltip : UITooltip {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Data
	protected DefinitionNode m_powerDef = null;
	public DefinitionNode powerDef {
		get { return m_powerDef; }
	}

	protected DefinitionNode m_sourceDef = null;	// Pet, Skin, Modifier, etc.
	public DefinitionNode sourceDef {
		get { return m_sourceDef; }
	}

	// Setup
	protected PowerIcon.Mode m_powerMode = PowerIcon.Mode.PET;
	protected PowerIcon.DisplayMode m_displayMode = PowerIcon.DisplayMode.PREVIEW;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the tooltip with the given data.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	/// <param name="_sourceDef">Source of the power: Skin, pet, special dragon, etc.</param>
	/// <param name="_powerMode">Info mode for this power.</param>
	/// <param name="_displayMode">The display mode for this power.</param>
	public void InitTooltip(DefinitionNode _powerDef, DefinitionNode _sourceDef, PowerIcon.Mode _powerMode, PowerIcon.DisplayMode _displayMode) {
		// Store parameters
		m_powerDef = _powerDef;
		m_sourceDef = _sourceDef;
		m_powerMode = _powerMode;
		m_displayMode = _displayMode;

		// Call internal initializers
		Init_Internal();
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the tooltip. To be implemented by heirs.
	/// At this point, internal data variables have been initialized.
	/// </summary>
	protected abstract void Init_Internal();

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}