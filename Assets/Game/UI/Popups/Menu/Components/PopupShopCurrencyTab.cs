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

using DG.Tweening;

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
	private const float REFRESH_FREQUENCY = 1f;	// Seconds

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[List("sc", "hc")]
	[SerializeField] private string m_type = "sc";

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		InvokeRepeating("PeriodicRefresh", 0f, REFRESH_FREQUENCY);
	}

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	private void PeriodicRefresh() {
		// Nothing if not enabled
		if(!this.isActiveAndEnabled) return;

		// Propagate to pills
		for(int i = 0; i < m_pills.Count; ++i) {
			(m_pills[i] as PopupShopCurrencyPill).PeriodicRefresh();
		}
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		CancelInvoke("PeriodicRefresh");
	}

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
			// Skip those definitions that are not enabled
			if(!defs[i].GetAsBool("enabled", false)) continue;

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
	/// <summary>
	/// The tab is about to be displayed.
	/// </summary>
	public void OnTabShow() {
		// Find first pill with Happy Hour active
		PopupShopCurrencyPill pill = null;
		PopupShopCurrencyPill targetPill = null;
		for(int i = 0; i < m_pills.Count; ++i) {
			pill = m_pills[i] as PopupShopCurrencyPill;
			if(pill.happyHourActive) {
				targetPill = pill;
				break;	// Found it!
			}
		}

		// Scroll to it!
		if(targetPill != null) {
			m_scrollList.DOGoToItem(targetPill.transform, 1f)
				.SetDelay(0.5f)
				.SetEase(Ease.OutQuad);
		}
	}
}