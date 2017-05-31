// MenuHUD.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controller for the HUD in the main menu.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class MenuHUD : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Shortcuts
	[SerializeField] private GameObject m_photoButton = null;
	public GameObject photoButton {
		get { return m_photoButton; }
	}

	// Internal
	private ShowHideAnimator m_animator = null;
	public ShowHideAnimator animator {
		get { 
			if(m_animator == null) {
				m_animator = GetComponent<ShowHideAnimator>();
			}
			return m_animator;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// HUD should always be on top, but for editing purposes, we keep it belows
		this.transform.SetAsLastSibling();
	}

	/// <summary>
	/// Start hidden.
	/// </summary>
	private void Start() {
		
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy() {
		
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Open the currency shop popup.
	/// </summary>
	public void OpenCurrencyShopPopup() {
		// Just do it
		PopupController popup = PopupManager.LoadPopup(PopupCurrencyShop.PATH);

		// In this particular case we want to allow several purchases in a row, so don't auto-close popup
		PopupCurrencyShop shopPopup = popup.GetComponent<PopupCurrencyShop>();
		shopPopup.closeAfterPurchase = false;
		shopPopup.Init(PopupCurrencyShop.Mode.DEFAULT);

		// Open popup!
		popup.Open();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}
