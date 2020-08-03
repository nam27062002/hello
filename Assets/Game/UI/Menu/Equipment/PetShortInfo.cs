// PetShortInfo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to display a pet's basic info.
/// </summary>
public class PetShortInfo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private TextMeshProUGUI m_powerDescText = null;
	[SerializeField] private UISpriteAddressablesLoader m_iconLoader = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	public void Awake() {
		// If icon loader was not initialized, try to retrieve it automatically
		if(m_iconLoader == null) {
			m_iconLoader = GetComponentInChildren<UISpriteAddressablesLoader>();
		}
	}

	/// <summary>
	/// Initialize with a pet definition.
	/// </summary>
	/// <param name="_petDef">Pet definition.</param>
	public void InitWithPet(DefinitionNode _petDef) {
		// Make sure data is valid
		if(_petDef == null) return;

        // The icon is now loaded asynchronously from the catalog
		if(m_iconLoader != null) {
			m_iconLoader.LoadAsync(_petDef.Get("icon"));
		}

		// Name
		if(m_nameText != null) {
			m_nameText.Localize(_petDef.GetAsString("tidName"));
		}

		// Power short description
		if(m_powerDescText != null) {
			DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, _petDef.Get("powerup"));
			if(powerDef != null) {
				m_powerDescText.text = DragonPowerUp.GetDescription(powerDef, true, true);   // Custom formatting depending on powerup type, already localized
			}
		}
	}
}