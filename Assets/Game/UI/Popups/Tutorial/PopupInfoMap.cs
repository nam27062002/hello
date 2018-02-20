// PopupInfoMap.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoMap : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoMap";

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	[SerializeField] private Localizer m_messageTxt_2 = null;

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Popup is about to open
	/// </summary>
	public void OnOpenPreAnimation() {
		// Localize info message putting the actual map unlock duration
		DefinitionNode gameSettingsDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "gameSettings");
		double mapUnlockDuration = 0;	// Seconds
		if(gameSettingsDef != null) {
			mapUnlockDuration = gameSettingsDef.GetAsDouble("miniMapTimer") * 60;	// Content is stored in minutes
		} else {
			mapUnlockDuration = 24 * 3600;	// Default timer just in case (24h)
		}
		m_messageTxt_2.Localize(
			m_messageTxt_2.tid, 
			TimeUtils.FormatTime(
				mapUnlockDuration, 
				TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES, 
				3, 
				TimeUtils.EPrecision.DAYS
			)
		);
	}
}
