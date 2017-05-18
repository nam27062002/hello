// HUDHealthBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 14/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple controller for a health bar in the debug hud.
/// </summary>
public class HUDMegaFireRushBar : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum State {
		Setup = 0,
		Filling_Up,
		Active
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private HUDMegaFireSlot[] m_slotList;

	private int m_megaFireTokens;
	private int m_megaFireProgress;

	private State m_state;


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {		
		m_state = State.Setup;        
    }

	IEnumerator Start() {
		while( !InstanceManager.gameSceneControllerBase.IsLevelLoaded()) {
			yield return null;
		}

		DefinitionNode settings = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SETTINGS, "dragonSettings");
		m_megaFireTokens = settings.GetAsInt("superfuryMax", 8);
		m_megaFireProgress = Mathf.FloorToInt(InstanceManager.player.superFuryProgression * m_megaFireTokens);

		for (int i = 0; i < m_slotList.Length; i++) {
			if (i < m_megaFireTokens) {
				if (i < m_megaFireProgress) {
					m_slotList[i].Full();
				} else {
					m_slotList[i].Empty();
				}
				m_slotList[i].CreatePools();
				m_slotList[i].gameObject.SetActive(true);
			} else {
				m_slotList[i].gameObject.SetActive(false);
			}
		}

		Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);

		m_state = State.Filling_Up;
	}

	void OnDestroy() {		
		Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
	}

	/// <summary>
	/// Keep values updated
	/// </summary>
	private void Update() {
		if (m_state == State.Filling_Up) {
			// Only if player is alive
			if (InstanceManager.player != null) {
				int currentMegaFireProgress = Mathf.FloorToInt(InstanceManager.player.superFuryProgression * m_megaFireTokens);
				if (m_megaFireProgress < currentMegaFireProgress) {
					m_slotList[m_megaFireProgress].Fill();
					m_megaFireProgress++;
				}
			}
		} else if (m_state == State.Active) {
			float currentMegaFireProgress = InstanceManager.player.superFuryProgression * m_megaFireTokens;
			if (currentMegaFireProgress <= 0f) {
				m_megaFireProgress = 0;
				m_slotList[0].Consume(0);
				m_state = State.Filling_Up;
			} else {				
				int currentSlot = Mathf.FloorToInt(currentMegaFireProgress);
				if (currentSlot < m_megaFireTokens) {
					if (currentSlot != m_megaFireProgress) {
						if (m_megaFireProgress < m_megaFireTokens) {
							m_slotList[m_megaFireProgress].Consume(0);
						}
						m_megaFireProgress = currentSlot;
					} else {
						m_slotList[m_megaFireProgress].Consume(currentMegaFireProgress - m_megaFireProgress);
					}
				}
			}
		}
	}

	void OnFuryToggled(bool _active, DragonBreathBehaviour.Type type) {
		if (type == DragonBreathBehaviour.Type.Mega) {			
			m_state = State.Active;
		}
	}
}
