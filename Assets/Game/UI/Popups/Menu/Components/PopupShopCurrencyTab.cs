// PopupShopTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/08/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tab in the currency shop!
/// </summary>
public class PopupShopCurrencyTab : IPopupShopTab {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[List("sc", "hc")]
	[SerializeField] private string m_type = "sc";

	//------------------------------------------------------------------------//
	// IPopupShopTab IMPLEMENTATION											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this tab and instantiate required pills.
	/// </summary>
	override public void Init() {
		// Clear current pills
		Clear();

		// Gather definitions based on type
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.SHOP_PACKS, "type", m_type);

		// Create pills
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < defs.Count; i++) {
			// Create new instance and initialize it
			GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
			PopupShopCurrencyPill newPill = newPillObj.GetComponent<PopupShopCurrencyPill>();
			newPill.InitFromDef(defs[i]);

			// Store to local collection for further use
			m_pills.Add(newPill);
		}

		// Reset scroll list position
		m_scrollList.horizontalNormalizedPosition = 0f;

		// Notify listeners
		OnPillListChanged.Invoke(this);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}