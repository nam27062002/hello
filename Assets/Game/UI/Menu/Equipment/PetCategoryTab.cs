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
using DG.Tweening;

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

	private List<DefinitionNode> m_defs = new List<DefinitionNode>();
	public List<DefinitionNode> defs {
		get { return m_defs; }
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
	/// <param name="_dragonData">The dragon we're tuning.</param> 
	public void Init(string _category, IDragonData _dragonData) {
		// Store target category
		m_category = _category;

		// If not yet done, get all the pets matching the target category
		if(m_defs.Count == 0) {
			m_defs = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.PETS, "category", _category);
		}

		// Sort them!
		// Put owned pets at the beginning of the list, then sort by order
		m_defs.Sort((DefinitionNode _def1, DefinitionNode _def2) => {
			bool unlocked1 = UsersManager.currentUser.petCollection.IsPetUnlocked(_def1.sku);
			bool unlocked2 = UsersManager.currentUser.petCollection.IsPetUnlocked(_def2.sku);
			if(unlocked1 && !unlocked2) {
				return -1;
			} else if(unlocked2 && !unlocked1) {
				return 1;
			} else {
				return _def1.GetAsInt("order").CompareTo(_def2.GetAsInt("order"));
			}
		});

		// Initialize one pill for each pet
		for(int i = 0; i < m_defs.Count; i++) {
			// If we don't have enough pills, instantiate new ones
			if(i >= m_pills.Count) {
				GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
				m_pills.Add(newPillObj.GetComponent<PetPill>());
			}

			// Initialize pill
			m_pills[i].Init(m_defs[i], _dragonData);
			m_pills[i].gameObject.SetActive(true);
		}

		// Hide any non-used pills
		for(int i = m_defs.Count; i < m_pills.Count; i++) {
			m_pills[i].gameObject.SetActive(false);
		}

		// Scroll back to the beginning of the list
		m_scrollList.normalizedPosition = Vector2.zero;
	}

	/// <summary>
	/// Get the pill corresponding to a given pet.
	/// </summary>
	/// <returns>The pill, if found. <c>null</c> otherwise.</returns>
	/// <param name="_petSku">The pet we're looking for.</param>
	public PetPill GetPill(string _petSku) {
		// Check pills one by one
		// Not optimal, but we'll have few pills so we can afford it
		for(int i = 0; i < m_pills.Count; i++) {
			if(m_pills[i].def.sku == _petSku) {
				// Found! Return target pill
				return m_pills[i];
			}
		}

		// Pill not found
		return null;
	}

	/// <summary>
	/// Scroll to a specific pet in the list.
	/// Will be ignored if the given pet doesn't belong to this tab.
	/// </summary>
	/// <param name="_petSku">Pet to scroll to.</param>
	/// <param name="_delay">Optional delay before launching the animation.</param>
	public void ScrollToPet(string _petSku, float _delay = 0f) {
		ScrollToPill(GetPill(_petSku), _delay);
	}

	/// <summary>
	/// Scroll to a specific pill in the list.
	/// Will be ignored if the given pill doesn't belong to this tab.
	/// </summary>
	/// <param name="_pill">Pill to scroll to.</param>
	/// <param name="_delay">Optional delay before launching the animation.</param>
	public void ScrollToPill(PetPill _pill, float _delay = 0f) {
		// Make sure target pill belongs to this tab
		int pillIdx = m_pills.IndexOf(_pill);
		if(pillIdx < 0) return;

		// Prepare unlock anim
		_pill.PrepareUnlockAnim();

		// Kill any existing anim on the scrolllist
		m_scrollList.DOKill();

		// Scroll content to pill!
		//float pillDeltaX = Mathf.InverseLerp(m_scrollList.content.rect.xMin, m_scrollList.content.rect.xMax, _pill.transform.position.x);
		float pillDeltaX = Mathf.InverseLerp(0, m_pills.Count, pillIdx);
		m_scrollList.DOHorizontalNormalizedPos(pillDeltaX, 0.15f)
			.SetDelay(_delay)
			.SetEase(Ease.OutQuad)
			.OnComplete(() => {
				// Show unlock anim!
				_pill.LaunchUnlockAnim();
			});
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}