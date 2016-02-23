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
using UnityEngine.UI;

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
	public static readonly string PATH = "UI/Popups/Shop/PF_PopupCurrencyShop";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Setup
	[Separator]
	[SerializeField] private int m_coinsAmount = 1000;
	[SerializeField] private int m_pcAmount = 1000;

	// References
	[Separator]
	[SerializeField] private Text m_coinsAmountText = null;
	[SerializeField] private Text m_pcAmountText = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		// Check required fields
		DebugUtils.Assert(m_coinsAmountText != null, "Missing required reference!");
		DebugUtils.Assert(m_pcAmountText != null, "Missing required reference!");
	}
	
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	void OnEnable() {
		// Update textfields
		m_coinsAmountText.text = StringUtils.FormatNumber(m_coinsAmount);
		m_pcAmountText.text = StringUtils.FormatNumber(m_pcAmount);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary
	/// Add coins button.
	/// </summary>
	public void OnAddCoins() {
		// Just do it
		UserProfile.AddCoins(m_coinsAmount);
		PersistenceManager.Save();
	}

	/// <summary
	/// Add PC button.
	/// </summary>
	public void OnAddPC() {
		// Just do it
		UserProfile.AddPC(m_pcAmount);
		PersistenceManager.Save();
	}
}
