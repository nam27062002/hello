// DragonXPBarSkinMarker.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 10/12/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Special marker on XP bar to show skin unlock level and state.
/// </summary>
public class DragonXPBarSkinMarker : DragonXPBarSeparator {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private string m_skinSku = "";
	public string skinSku {
		get { return m_skinSku; }
		set { m_skinSku = value; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show the proper clip based on slider's value and current delta.
	/// Put the separator in position.
	/// </summary>
	public override void Refresh() {
		// Call parent
		base.Refresh();

		// Skip if slider is not defined
		if(m_slider == null) {
			return;
		}

		// Add an extra condition to whether the active/inactive object should be displayed
		// Skin might have been acquired before reaching its unlock level (i.e. via an Offer Pack)
		bool active = m_activeObj.activeSelf || UsersManager.currentUser.wardrobe.GetSkinState(m_skinSku) == Wardrobe.SkinState.OWNED;
		m_activeObj.SetActive(active);
		m_inactiveObj.SetActive(!active);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}
