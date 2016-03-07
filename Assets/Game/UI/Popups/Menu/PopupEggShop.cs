// PopupEggShop.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controller for the Eggs shop popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupEggShop : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Shop/PF_PopupEggShop";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private SnappingScrollRect m_scrollList = null;

	// Internal
	private List<PopupEggShopPill> m_pills = new List<PopupEggShopPill>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_pillPrefab != null, "Required field!");
		Debug.Assert(m_scrollList != null, "Required field!");

		// Subscribe to events
		GetComponent<PopupController>().OnOpenPreAnimation.AddListener(OnOpenPreAnimation);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Popup is going to be opened.
	/// </summary>
	private void OnOpenPreAnimation() {
		// Populate eggs list
		// [AOC] TODO!! Do it async? Not 100% required in this case, but may be necessary for longer lists
		List<DefinitionNode> eggDefs = Definitions.GetDefinitions(Definitions.Category.EGGS);
		Definitions.SortByProperty(ref eggDefs, "shopOrder", Definitions.SortType.NUMERIC);

		// Create a pill for each egg
		GameObject newPillObj = null;
		PopupEggShopPill newPill = null;
		for(int i = 0; i < eggDefs.Count; i++) {
			// Instantiate pill
			newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab);

			// Initialize with the given definition
			newPill = newPillObj.GetComponent<PopupEggShopPill>();
			newPill.InitFromDef(eggDefs[i]);

			// Add pill to scroll list
			newPillObj.transform.SetParent(m_scrollList.content.transform);

			// Add pill to local list for further access
			m_pills.Add(newPill);
		}

		// Force a scroll animation by setting the scroll instantly to last pill then scrolling to the first one
		m_scrollList.SelectPoint(m_pills.Last().snapPoint, false);
		m_scrollList.SelectPoint(m_pills.First().snapPoint, true);
	}
}
