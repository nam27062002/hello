// PopupClassicDragonInfo.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup to display information of a special dragon.
/// </summary>
public class PopupClassicDragonInfo : PopupDragonInfo {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	new public const string PATH = "UI/Popups/Menu/PF_PopupClassicDragonInfo";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Classic Dragon Stuff")]
	[SerializeField] protected Localizer m_tierText = null;
	[SerializeField] protected TintByTierColor m_tierColorTint = null;

	[SerializeField] protected Localizer m_dragonLevelText = null;

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the popup with the info from the currently selected dragon (in the scroller).
	/// </summary>
	protected override void Refresh() {
		// Call parent
		base.Refresh();

		// Aux vars
		DragonDataClassic dragonData = m_dragonData as DragonDataClassic;
		Debug.Assert(dragonData != null, "ONLY FOR CLASSIC DRAGONS!");

		// Dragon level
		if(m_dragonLevelText != null) {
			MenuDragonInfo.FormatLevel(m_dragonData, m_dragonLevelText);
		}

        // Dragon Icon
        if (m_tierText != null)
        {
            // Replace with xxL, xS, L, etc.
			m_tierText.Localize("TID_DRAGON_TIER_" + (int) dragonData.tier + "_NAME");
        }

		if (m_tierColorTint != null)
		{
			// Set the proper icon color
			m_tierColorTint.Apply(dragonData.tier, TintByTierColor.ColorSet.ICON_TIER_COLOR);
		}

	}
}