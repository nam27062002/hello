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
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize with a pet definition.
	/// </summary>
	/// <param name="_petDef">Pet definition.</param>
	public void InitWithPet(DefinitionNode _petDef) {
		// Make sure data is valid
		if(_petDef == null) return;

        // The icon is now loaded asynchronously from the catalog
        UISpriteAddressablesLoader iconLoader = GetComponent<UISpriteAddressablesLoader>();
		if(iconLoader != null) {
            iconLoader.LoadAsync(_petDef.Get("icon"));
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