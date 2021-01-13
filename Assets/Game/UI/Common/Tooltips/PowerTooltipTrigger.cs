// PowerTooltipTrigger.cs
// 
// Created by Alger Ortín Castellví on 29/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom implementation of UITooltipTrigger adapted to a power.
/// Will choose the type of tooltip to display based on power type.
/// </summary>
public class PowerTooltipTrigger : UITooltipTrigger {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable] public class PowerTooltipTriggerEvent : UnityEvent<PowerTooltipTrigger> { }
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Tooltip prefab path
	[FileList("Resources/UI", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_genericPrefabPath = "";

	[FileList("Resources/UI", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_babyPetPrefabPath = "";

	// Tooltip instances
	[SerializeField] private IPowerTooltip m_genericTooltipInstance = null;
	[SerializeField] private IPowerTooltip m_babyPetTooltipInstance = null;

	// Other internal vars
	protected DefinitionNode m_powerDef = null;
	protected DefinitionNode m_sourceDef = null;    // Pet, Skin, Modifier, etc.
	protected PowerIcon.Mode m_powerMode = PowerIcon.Mode.PET;
	protected PowerIcon.DisplayMode m_displayMode = PowerIcon.DisplayMode.PREVIEW;

	// Events
	/// <summary>
	/// Tooltip data getter. Will be called before opening the tooltip to decide which tooltip layout to use.
	/// The listeners should use the SetTooltipData() method to initialize the target tooltip trigger.
	/// To be connected in the inspector.
	/// </summary>
	public PowerTooltipTriggerEvent OnGetTooltipData = new PowerTooltipTriggerEvent();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Set the data to initialize the tooltip. The trigger will use it to decide which tooltip layout to use.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	/// <param name="_sourceDef">Source of the power: Skin, pet, special dragon, etc.</param>
	/// <param name="_powerMode">Info mode for this power.</param>
	/// <param name="_displayMode">The display mode for this power.</param>
	public void SetTooltipData(DefinitionNode _powerDef, DefinitionNode _sourceDef, PowerIcon.Mode _powerMode, PowerIcon.DisplayMode _displayMode) {
		// Just store data
		m_powerDef = _powerDef;
		m_sourceDef = _sourceDef;
		m_powerMode = _powerMode;
		m_displayMode = _displayMode;
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Instantiate or choose the right instance of the tooltip to be displayed.
	/// Will override current m_tooltip.
	/// </summary>
	/// <param name="_spawnTransform">The root where to instantiate the new tooltip.</param>
	/// <returns>The new tooltip instance. <c>null</c> if tooltip instance couldn't be created.</returns>
	protected override UITooltip InitTooltipInstance(RectTransform _spawnTransform) {
		// Obtain the data to initialize the tooltip
		if(OnGetTooltipData == null) return null;
		OnGetTooltipData.Invoke(this);

		// Choose target tooltip based on mode
		IPowerTooltip targetTooltip = null;
		switch(m_powerMode) {
			// Pet: choose between generic tooltip or baby pet tooltip
			case PowerIcon.Mode.PET: {
				// Is it a baby?
				if(m_sourceDef != null && m_sourceDef.GetAsString("category") == "baby") {
					// Yes! Use baby pet power tooltip
					targetTooltip = GetOrCreateTooltipInstance_BabyPet(_spawnTransform);
				} else {
					// No! Use generic power tooltip
					targetTooltip = GetOrCreateTooltipInstance_Generic(_spawnTransform);
				}
			} break;

			// Rest of cases: use generic power tooltip
			default: {
				targetTooltip = GetOrCreateTooltipInstance_Generic(_spawnTransform);
			} break;
		}

		// Initialize tooltip with given data
		targetTooltip.InitTooltip(m_powerDef, m_sourceDef, m_powerMode, m_displayMode);

		// Done!
		return targetTooltip;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the Generic tooltip instance. If not defined, try to instantiate a new one using the right prefab path.
	/// </summary>
	/// <param name="_spawnTransform">The root where to instantiate the new tooltip.</param>
	/// <returns>The tooltip instance. <c>null</c> if tooltip instance couldn't be created.</returns>
	private IPowerTooltip GetOrCreateTooltipInstance_Generic(RectTransform _spawnTransform) {
		// Is tooltip instance already created?
		if(m_genericTooltipInstance == null) {
			// No! Instantiate now
			m_genericTooltipInstance = InstantiateTooltipPrefab(m_genericPrefabPath, _spawnTransform) as IPowerTooltip;
		}
		return m_genericTooltipInstance;
	}

	/// <summary>
	/// Get the Baby Pet tooltip instance. If not defined, try to instantiate a new one using the right prefab path.
	/// </summary>
	/// <param name="_spawnTransform">The root where to instantiate the new tooltip.</param>
	/// <returns>The tooltip instance. <c>null</c> if tooltip instance couldn't be created.</returns>
	private IPowerTooltip GetOrCreateTooltipInstance_BabyPet(RectTransform _spawnTransform) {
		// Is tooltip instance already created?
		if(m_babyPetTooltipInstance == null) {
			// No! Instantiate now
			m_babyPetTooltipInstance = InstantiateTooltipPrefab(m_babyPetPrefabPath, _spawnTransform) as IPowerTooltip;
		}
		return m_babyPetTooltipInstance;
	}
}