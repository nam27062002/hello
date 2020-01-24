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
[RequireComponent(typeof(ShopController))]
public class PopupShop : MonoBehaviour {
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
    private HappyHourOffer m_happyHour;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization
    /// </summary>
    void Awake() {

	}

	/// <summary>
	/// Initialize the popup with the requested mode. Should be called before opening the popup.
	/// </summary>
	/// <param name="_mode">Target mode.</param>
	public void Init(Mode _mode, string _origin ) {

        m_shopController.Init(_mode, _origin);

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

}
