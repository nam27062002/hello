// PopupCurrencyShopTab.cs
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
public class PopupCurrencyShopTab : Tab {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private ScrollRect m_scrollList = null;
	public ScrollRect scrollList {
		get { return m_scrollList; }
	}

	// Internal
	private List<PopupCurrencyShopPill> m_pills = new List<PopupCurrencyShopPill>();
	public List<PopupCurrencyShopPill> pills {
		get { return m_pills; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Debug.Assert(m_pillPrefab != null, "Missing required reference!");
		Debug.Assert(m_scrollList != null, "Missing required reference!");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the tab with a list of shop pack definitions.
	/// </summary>
	/// <param name="_defs">Shop pack definitions to be displayed in this tab.</param>
	public void InitWithDefs(List<DefinitionNode> _defs) {
		// Clear current content
		m_scrollList.content.DestroyAllChildren(false);
		m_pills.Clear();

		// Create pills
		DefinitionsManager.SharedInstance.SortByProperty(ref _defs, "order", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < _defs.Count; i++) {
			// Create new instance and initialize it
			GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
			PopupCurrencyShopPill newPill = newPillObj.GetComponent<PopupCurrencyShopPill>();
			newPill.InitFromDef(_defs[i]);

			// Store to local collection for further use
			m_pills.Add(newPill);
		}

		// Reset scroll list position
		m_scrollList.horizontalNormalizedPosition = 0f;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}