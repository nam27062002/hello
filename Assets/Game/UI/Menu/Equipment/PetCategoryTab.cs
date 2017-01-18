// PetCategoryTab.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/01/2017.
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
/// Specialization of a tab for the pets screen.
/// </summary>
public class PetCategoryTab : Tab {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Header("PetCategoryTab")]
	[SerializeField] private ScrollRect m_scrollList = null;
	public ScrollRect scrollList {
		get { return m_scrollList; }
	}

	[SerializeField] private GameObject m_pillPrefab = null;

	// Internal
	private List<PetPill> m_pills = new List<PetPill>();
	public List<PetPill> pills {
		get { return m_pills; }
	}

	private string m_category = "";
	public string category {
		get { return m_category; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Clear all content of the scroll list (used to do the layout in the editor)
		int numChildren = m_scrollList.content.childCount;
		for(int i = numChildren - 1; i >= 0; i--) {	// Reverse loop since we're erasing
			GameObject.Destroy(m_scrollList.content.transform.GetChild(i).gameObject);
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the list with all the pets with the given category.
	/// All existing pills will be cleared.
	/// </summary>
	/// <param name="_category">Category.</param>
	public void InitFromDef(string _category) {
		// Store target category
		m_category = _category;

		// Get all the pets matching the target category
		// Sort them!
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "category", _category);
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);

		// Initialize one pill for each pet
		for(int i = 0; i < defs.Count; i++) {
			// If we don't have enough pills, instantiate new ones
			if(i >= m_pills.Count) {
				GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
				m_pills.Add(newPillObj.GetComponent<PetPill>());
			}

			// Initialize pill
			m_pills[i].InitFromDef(defs[i]);
			m_pills[i].gameObject.SetActive(true);
		}

		// Hide any non-used pills
		for(int i = defs.Count; i < m_pills.Count; i++) {
			m_pills[i].gameObject.SetActive(false);
		}

		// Scroll back to the beginning of the list
		m_scrollList.normalizedPosition = Vector2.zero;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}