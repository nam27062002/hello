// PetPowerTooltipTrigger.cs
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
/// Custom implementation of UITooltipTrigger adapted to a pet power.
/// Will choose the type of tooltip to display based on power type.
/// </summary>
public class PetPowerTooltipTrigger : UITooltipTrigger {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable] public class PetPowerTooltipTriggerEvent : UnityEvent<PetPowerTooltipTrigger> { }
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Tooltip prefab path
	[FileList("Resources/UI", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_defaultPrefabPath = "";

	[FileList("Resources/UI", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string m_babyPetPrefabPath = "";

	// Tooltip instances
	[SerializeField] private UITooltip m_defaultTooltipInstance = null;
	[SerializeField] private UITooltip m_babyPetTooltipInstance = null;

	// Other internal vars
	private DefinitionNode m_petDef = null;

	// Events
	/// <summary>
	/// Pet definition getter. Will be called before triggering the tooltip to 
	/// know which tooltip layout to use.
	/// The listeners should use the SetPetDef() method to initialize the target tooltip trigger.
	/// </summary>
	public PetPowerTooltipTriggerEvent OnGetPetDef = new PetPowerTooltipTriggerEvent();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Define the pet definition to be used to initialize the tooltip so the 
	/// trigger can choose which tooltip layout to use.
	/// </summary>
	/// <param name="_petDef">The pet definition to be used to initialize the tooltip.</param>
	public void SetPetDef(DefinitionNode _petDef) {
		// Just store it
		m_petDef = _petDef;
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
		// Obtain the target pet definition
		if(OnGetPetDef == null) return null;
		OnGetPetDef.Invoke(this);
		if(m_petDef == null) return null;

		// Choose target tooltip based on pet type
		UITooltip targetTooltip = null;
		if(m_petDef.GetAsString("category") == "baby") {
			// Is baby pet tooltip instance created?
			if(m_babyPetTooltipInstance == null) {
				// No! Instantiate it now
				m_babyPetTooltipInstance = InstantiateTooltipPrefab(m_babyPetPrefabPath, _spawnTransform);
			}
			targetTooltip = m_babyPetTooltipInstance;
		} else {
			// Is default tooltip instance created?
			if(m_defaultTooltipInstance == null) {
				// No! Instantiate it now
				m_defaultTooltipInstance = InstantiateTooltipPrefab(m_defaultPrefabPath, _spawnTransform);
			}
			targetTooltip = m_defaultTooltipInstance;
		}

		// Done!
		return targetTooltip;
	}
}