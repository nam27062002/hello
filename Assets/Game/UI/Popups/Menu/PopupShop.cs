// PopupShop.cs
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
public class PopupShop : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShop";

	public enum Mode {
		DEFAULT,
		SC_ONLY,
		PC_ONLY,
		OFFERS_FIRST
	};

	public enum Tabs {
		PC,
		SC,
		OFFERS,
		COUNT
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed Setup
	[SerializeField] private TabSystem m_tabs = null;
	public TabSystem tabs {
		get { return m_tabs; }
	}

	[Space]
	[SerializeField] private float m_pillAnimDelay = 0.075f;
	[SerializeField] private float[] m_pillRotationSequence = new float[0];
	[SerializeField] private GameObject m_tabButtonsContainer = null;

	// Other setup parameters
	private bool m_closeAfterPurchase = false;
	public bool closeAfterPurchase {
		get { return m_closeAfterPurchase; }
		set { m_closeAfterPurchase = value; }
	}

	// Data
	private List<DefinitionNode> m_packsPurchased = new List<DefinitionNode>();
	public List<DefinitionNode> packsPurchased {
		get { return m_packsPurchased; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		// Check required fields
		Debug.Assert(m_tabs != null, "Missing required reference!");

		// Create pills for each tab
		for(int i = 0; i < (int)Tabs.COUNT; ++i) {
			// Get target tab
			IPopupShopTab tab = m_tabs[i] as IPopupShopTab;
			Debug.Assert(tab != null, "Unknown tab type!");
			tab.Init();

			// Do some extra initialization on the pills
			float totalDelay = 0f;
			for(int j = 0; j < tab.pills.Count; ++j) {
				// Get pill
				IPopupShopPill pill = tab.pills[j];

				// Subscribe to purchase events
				pill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);

				// Apply "random" rotation to the pill
				if(m_pillRotationSequence.Length > 0) {
					pill.transform.localRotation = Quaternion.Euler(0f, 0f, m_pillRotationSequence[j % m_pillRotationSequence.Length]);
				}

				// Animation!
				ShowHideAnimator anim = pill.GetComponent<ShowHideAnimator>();
				if(anim != null) {
					anim.tweenDelay += totalDelay;	// Keep original anim delay
					totalDelay += m_pillAnimDelay;
				}
			}
		}
	}

	/// <summary>
	/// Initialize the popup with the requested mode. Should be called before opening the popup.
	/// </summary>
	/// <param name="_mode">Target mode.</param>
	public void Init(Mode _mode) {
		// Refresh pills?

		// Reset scroll lists and hide all tabs
		for(int i = 0; i < (int)Tabs.COUNT; ++i) {
			(m_tabs[i] as IPopupShopTab).scrollList.horizontalNormalizedPosition = 0f;
			m_tabs[i].Hide(NavigationScreen.AnimType.NONE);
		}

		// If required, hide tab buttons
		m_tabButtonsContainer.SetActive(
			_mode == Mode.DEFAULT
		 || _mode == Mode.OFFERS_FIRST
		);

		// Select initial tab and scroll list
		int initialTab = m_tabs.GetScreenIndex(m_tabs.initialScreen);
		switch(_mode) {
			case Mode.SC_ONLY: {
				initialTab = (int)Tabs.SC; 
			} break;

			case Mode.PC_ONLY: 
			case Mode.DEFAULT: {
				initialTab = (int)Tabs.PC;
			} break;

			case Mode.OFFERS_FIRST: {
				initialTab = (int)Tabs.OFFERS;
			} break;
		}

		// Go to initial tab
		m_tabs.GoToScreen(initialTab);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary
	/// Successful purchase.
	/// </summary>
	/// <param name="_pill">The pill that triggered the event</param>
	private void OnPurchaseSuccessful(IPopupShopPill _pill) {
		// Add to purchased packs list
		m_packsPurchased.Add(_pill.def);

		// Close popup?
		if(m_closeAfterPurchase) GetComponent<PopupController>().Close(false);
	}

	/// <summary>
	/// The popup is about to be been opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Hide other currency counters to prevent conflicts
		Messenger.Broadcast<bool>(MessengerEvents.UI_TOGGLE_CURRENCY_COUNTERS, false);

        HDTrackingManager.Instance.Notify_StoreVisited();

        // Reset packs purchased list
        m_packsPurchased.Clear();
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
		Messenger.Broadcast<bool>(MessengerEvents.UI_TOGGLE_CURRENCY_COUNTERS, true);
	}
}
