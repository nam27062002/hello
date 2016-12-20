// DisguisesScreenTitle.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controls the disguises screen title and xp bar.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class DisguisesScreenTitle : DragonXPBar {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Space]
	[SerializeField] private Localizer m_disguiseNameText;

	// Internal references
	private ShowHideAnimator m_showHideAnimator = null;
	public ShowHideAnimator showHideAnimator {
		get { return m_showHideAnimator; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Get refs
		m_showHideAnimator = GetComponent<ShowHideAnimator>();

		// Caall parent
		base.Awake();
	}

	/// <summary>
	/// Initialize using a disguise definition.
	/// </summary>
	/// <param name="_def">The disguise definition.</param>
	public void InitFromDef(DefinitionNode _def) {
		// Skip if definition is not good
		if(_def == null) return;

		// Update bar with the info of the dragon associated to this skin
		Refresh(_def.GetAsString("dragonSku"));

		// Set disguise description
		if(m_disguiseNameText != null) m_disguiseNameText.Localize(_def.GetAsString("tidName"));

		// Update auxiliar bar to show how much xp is missing to unlock this disguise
		if(m_dragonData != null && m_auxBar != null) {
			// Get target level
			int unlockLevel = _def.GetAsInt("unlockLevel", 0);

			// Setup bar
			m_auxBar.minValue = 0;
			m_auxBar.maxValue = 1;

			// Linear?
			if(m_linear) {
				m_auxBar.value = Mathf.InverseLerp(0, m_dragonData.progression.maxLevel, unlockLevel);
			} else {
				m_auxBar.value = m_dragonData.progression.GetXpRangeForLevel(unlockLevel).min;
			}
		}
	}
}
