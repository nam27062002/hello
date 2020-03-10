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
public class PopupShop : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShop";



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
	/// Initialize the popup with the requested mode. Should be called before opening the popup.
	/// </summary>
	/// <param name="_mode">Target mode.</param>
	public void Init(ShopController.Mode _mode, string _origin) {
		m_openOrigin = _origin;
        m_shopController.Init(_mode, OnPurchaseSuccessful);
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
		// Propagate to shop controller
		m_shopController.OnShopEnter(m_openOrigin);

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
		
	}


    /// <summary
    /// Successful purchase.
    /// </summary>
    /// <param name="_pill">The pill that triggered the event</param>
    private void OnPurchaseSuccessful(IShopPill _pill)
    {
        // Add to purchased packs list
        m_packsPurchased.Add(_pill.def);

        // Close popup?
        if (m_closeAfterPurchase)
        {
            if (_pill is ShopCurrencyPill )
            {
                // For currency packs wait some time before closing so the coins/gems trail FX can finish
                UbiBCN.CoroutineManager.DelayedCall(() => GetComponent<PopupController>().Close(false) , 1.5f, false);
            }
            else
            {
                GetComponent<PopupController>().Close(false);
            }
            
        }
            
    }
   
}
