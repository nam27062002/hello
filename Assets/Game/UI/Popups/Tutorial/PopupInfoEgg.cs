// PopupInfoEgg.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Eggs info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoEgg : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoEgg";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Reusing Drop Info Popup logic
	[SerializeField] private PopupInfoDropChance.RarityInfo[] m_rarityInfos = new PopupInfoDropChance.RarityInfo[3];

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Reusing Drop Info Popup logic
		PopupInfoDropChance.InitInfos(m_rarityInfos);
	}
}
