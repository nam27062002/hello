﻿// HUDMessagesTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to test the HUD messaging system.
/// </summary>
public class HUDMessagesTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public float m_hp = 100f;
	public Text m_hpText = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Simulate HP increase/decrease to test starvation messages.
	/// </summary>
	/// <param name="_hpDiff">Health points to be added/removed.</param>
	public void SimulateStarvation(float _hpDiff) {
		// Store previous state
		bool wasStarving = m_hp < 50f;
		bool wasCritical = m_hp < 25f;

		// Apply differential
		m_hp += _hpDiff;
		if(m_hpText != null) m_hpText.text = m_hp.ToString();

		// Get new state
		bool isStarving = m_hp < 50f;
		bool isCritical = m_hp < 25f;

		// This behaves identically as the DragonPlayer class to detect when the starving/critical stages are reached
		if(isStarving != wasStarving) Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, isStarving);
		if(isCritical != wasCritical) {
			Messenger.Broadcast<bool>(GameEvents.PLAYER_CRITICAL_TOGGLED, isCritical);
			if(!isCritical && isStarving) {
				Messenger.Broadcast<bool>(GameEvents.PLAYER_STARVING_TOGGLED, isStarving);
			}
		}
	}

	/// <summary>
	/// Simulate a game message.
	/// </summary>
	/// <param name="_type">Type. Doesn't match HUDMessage.Type, careful!</param>
	public void SimulateMessage(int _type) {
		switch(_type) {
			case 0:		Messenger.Broadcast<DragonData>(GameEvents.DRAGON_LEVEL_UP, null);		break;
			case 1:		Messenger.Broadcast(GameEvents.SURVIVAL_BONUS_ACHIEVED);				break;
			case 2:		Messenger.Broadcast(GameEvents.PLAYER_CURSED);							break;
			case 3:		Messenger.Broadcast<DragonTier>(GameEvents.BIGGER_DRAGON_NEEDED, (DragonTier)UnityEngine.Random.Range((int)DragonTier.TIER_1, (int)DragonTier.COUNT));	break;

			case 4:	{
				Mission m = new Mission();
				m.InitFromDefinition(DefinitionsManager.GetDefinitions(DefinitionsCategory.MISSIONS).GetRandomValue());
				Messenger.Broadcast<Mission>(GameEvents.MISSION_COMPLETED, m);
			} break;

			case 5:		Messenger.Broadcast<Chest>(GameEvents.CHEST_COLLECTED, null);			break;
			default:	break;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}