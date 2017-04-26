// PopupCurrencyShop.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Temp popup to "purchase" currencies.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupCurrencyShop : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string PATH = "UI/Popups/ResourcesFlow/PF_PopupCurrencyShop";

	public enum Mode {
		DEFAULT,
		SC_ONLY,
		PC_ONLY
	};

	public enum Tabs {
		PC,
		SC
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed Setup
	[SerializeField] private GameObject m_tabButtons = null;
	[SerializeField] private TabSystem m_tabs = null;
	public TabSystem tabs {
		get { return m_tabs; }
	}

	[Space]
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private ScrollRect m_scScrollList = null;
	[SerializeField] private ScrollRect m_pcScrollList = null;

	[Space]
	[SerializeField] private float[] m_pillRotationSequence = new float[0];

	// Other setup parameters
	private bool m_closeAfterPurchase = false;
	public bool closeAfterPurchase {
		get { return m_closeAfterPurchase; }
		set { m_closeAfterPurchase = value; }
	}

	// Internal
	private List<PopupCurrencyShopPill> m_scPills = new List<PopupCurrencyShopPill>();
	private List<PopupCurrencyShopPill> m_pcPills = new List<PopupCurrencyShopPill>();

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		// Check required fields
		Debug.Assert(m_tabs != null, "Missing required reference!");
		Debug.Assert(m_pillPrefab != null, "Missing required reference!");
		Debug.Assert(m_scScrollList != null, "Missing required reference!");
		Debug.Assert(m_pcScrollList != null, "Missing required reference!");

		// Clear containers
		m_scScrollList.content.DestroyAllChildren(false);
		m_pcScrollList.content.DestroyAllChildren(false);

		// Create pills
		List<DefinitionNode> defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.SHOP_PACKS);
		DefinitionsManager.SharedInstance.SortByProperty(ref defs, "order", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < defs.Count; i++) {
			// Create new instance and initialize it
			GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab);
			PopupCurrencyShopPill newPill = newPillObj.GetComponent<PopupCurrencyShopPill>();
			newPill.InitFromDef(defs[i]);

			// Based on pill's type, add it to corresponding list and container
			int pillIdx = -1;	// Will be used later on
			switch(newPill.type) {
				case UserProfile.Currency.SOFT: {
					// Store to list and add to parent
					newPill.transform.SetParentAndReset(m_scScrollList.content);
					m_scPills.Add(newPill);
					pillIdx = m_scPills.Count - 1;
				} break;

				case UserProfile.Currency.HARD: {
					// Store to list and add to parent
					newPill.transform.SetParentAndReset(m_pcScrollList.content);
					m_pcPills.Add(newPill);
					pillIdx = m_pcPills.Count - 1;
				} break;

				default: {
					// Unknown pill type, destroy it and move on
					GameObject.Destroy(newPillObj);
					continue;
				} break;
			}

			// Subscribe to purchase events
			newPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);

			// Apply "random" rotation to the pill
			if(m_pillRotationSequence.Length > 0 && pillIdx >= 0) {
				newPill.transform.localRotation = Quaternion.Euler(0f, 0f, m_pillRotationSequence[pillIdx % m_pillRotationSequence.Length]);
			}
		}

		// Reset scroll lists and program initial animation
		m_scScrollList.horizontalNormalizedPosition = 0f;
		m_pcScrollList.horizontalNormalizedPosition = 0f;
	}
	
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	void OnEnable() {
		
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Initialize the popup with the requested mode. Should be called before opening the popup.
	/// </summary>
	/// <param name="_mode">Target mode.</param>
	public void Init(Mode _mode) {
		// Refresh pills?

		// Reset scroll lists
		m_scScrollList.horizontalNormalizedPosition = 0f;
		m_pcScrollList.horizontalNormalizedPosition = 0f;

		// If required, hide tab buttons
		m_tabButtons.SetActive(_mode == Mode.DEFAULT);

		// Select initial tab and scroll list
		NavigationScreen initialTab = m_tabs.initialScreen;
		ScrollRect initialList = m_pcScrollList;
		switch(_mode) {
			case Mode.SC_ONLY: {
				initialTab = m_tabs.GetScreen((int)Tabs.SC);
				initialList = m_scScrollList;
			} break;

			case Mode.PC_ONLY:
			case Mode.DEFAULT: {	// By default show PC tab
				initialTab = m_tabs.GetScreen((int)Tabs.PC);
				initialList = m_pcScrollList;
			} break;
		}

		// Go to initial tab
		m_tabs.GoToScreen(initialTab, NavigationScreen.AnimType.NONE);

		// Program initial scroll list animation
		initialList.DOHorizontalNormalizedPos(-10f, 0.5f).From().SetEase(Ease.OutCubic).SetDelay(0.25f).SetUpdate(true);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary
	/// Successful purchase.
	/// </summary>
	/// <param name="_pill">The pill that triggered the event</param>
	public void OnPurchaseSuccessful(PopupCurrencyShopPill _pill) {
		// Close popup?
		if(m_closeAfterPurchase) GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// The popup is about to be been opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Hide other currency counters to prevent conflicts
		Messenger.Broadcast<bool>(GameEvents.UI_TOGGLE_CURRENCY_COUNTERS, false);
	}

	/// <summary>
	/// The popup has just been opened.
	/// </summary>
	public void OnOpenPostAnimation() {
		
	}

	/// <summary>
	/// Popup is about to be closed.
	/// </summary>
	public void OnClosePreAnimation() {
		// Restore currency counters
		Messenger.Broadcast<bool>(GameEvents.UI_TOGGLE_CURRENCY_COUNTERS, true);
	}
}
