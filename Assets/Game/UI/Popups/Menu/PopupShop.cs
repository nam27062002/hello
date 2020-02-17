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
using System.IO;
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
[RequireComponent(typeof(ShopController))]
public class PopupShop : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShop";

	public enum Mode {
		DEFAULT,
		SC_ONLY,
		PC_ONLY,
		OFFERS_FIRST,
		PC_FIRST
	};

	// Hide popup's content when any of these popups are open
	protected static readonly HashSet<string> POPUPS_TO_HIDE_CONTENT = new HashSet<string>() {
		Path.GetFileNameWithoutExtension(PopupShopOfferPack.PATH)
		//Path.GetFileNameWithoutExtension(PopupShopOfferPackSkins.PATH)
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed Setup
    [SerializeField] private ShopController m_shopController = null;
	[SerializeField] private GameObject m_contentRoot = null;

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

    protected string m_openOrigin = "";
    protected bool m_trackScreenChange = false;
    protected int m_lastTrackedScreen = -1;

    //Internal
    private float m_timer = 0; // Refresh timer
    private HappyHour m_happyHour = null;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization
    /// </summary>
    private void Awake() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.POPUP_OPENED, this);
		Broadcaster.AddListener(BroadcastEventType.POPUP_CLOSED, this);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Subscribe to external events
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_OPENED, this);
		Broadcaster.RemoveListener(BroadcastEventType.POPUP_CLOSED, this);
	}

	/// <summary>
	/// Initialize the popup with the requested mode. Should be called before opening the popup.
	/// </summary>
	/// <param name="_mode">Target mode.</param>
	public void Init(Mode _mode, string _origin ) {
        m_shopController.Init(_mode, _origin);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
    public void Update()
    {
            // Refresh offers periodically for better performance
            if (m_timer <= 0)
            {
                m_timer = 1f; // Refresh every second
                Refresh();
            }
            m_timer -= Time.deltaTime;
    }

	/// <summary>
	/// Periodic refresh.
	/// </summary>
    public void Refresh ()
    {

    }

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A tab's pills list has changed.
	/// </summary>
	/// <param name="_tab">The tab that triggered the event.</param>
	
	/// <summary>
	/// The popup is about to be been opened.
	/// </summary>
	public void OnOpenPreAnimation() {

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
		
	}

	/// <summary>
	/// IBroadcastListener implementation.
	/// </summary>
	/// <param name="eventType"></param>
	/// <param name="broadcastEventInfo"></param>
	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
		// Ignore if not active
		if(!this.isActiveAndEnabled) return;

		// Which event?
		switch(eventType) {
			case BroadcastEventType.POPUP_OPENED: {
				// Only if content is valid
				if(m_contentRoot != null) {
					string popupName = (broadcastEventInfo as PopupManagementInfo).popupController.name;
					if(POPUPS_TO_HIDE_CONTENT.Contains(popupName)) {
						// Hide content!
						m_contentRoot.SetActive(false);
					}
				}
			} break;

			case BroadcastEventType.POPUP_CLOSED: {
				// Only if content is valid
				if(m_contentRoot != null) {
					string popupName = (broadcastEventInfo as PopupManagementInfo).popupController.name;
					if(POPUPS_TO_HIDE_CONTENT.Contains(popupName)) {
						// Restore content!
						m_contentRoot.SetActive(true);
					}
				}
			} break;
		}
	}
}
