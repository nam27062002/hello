// HUDMessagesTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

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
	// Exposed
	public float m_hp = 100f;
	public TextMeshProUGUI m_hpText = null;
	public DragonHealthModifier[] m_healthModifiers = null;	// Sorted by threshold

	// Internal
	private float m_hpMax = 100f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// We need missions definitions for the test, as well as localization
		ContentManager.InitContent(true);
		LocalizationManager.SharedInstance.SetLanguage("lang_english");
		
		// Init health modifiers
		// Fake thresholds
		List<DefinitionNode> healthModifierDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.DRAGON_HEALTH_MODIFIERS);
		DefinitionsManager.SharedInstance.SortByProperty(ref healthModifierDefs, "threshold", DefinitionsManager.SortType.NUMERIC);		// Sort by threshold
		m_healthModifiers = new DragonHealthModifier[healthModifierDefs.Count];
		for(int i = 0; i < healthModifierDefs.Count; i++) {
			m_healthModifiers[i] = new DragonHealthModifier(healthModifierDefs[i]);
		}

		// Init HP text
		SimulateStarvation(0f);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Simulate HP increase/decrease to test starvation messages.
	/// </summary>
	/// <param name="_hpDiff">Health points to be added/removed.</param>
	public void SimulateStarvation(float _hpDiff) {
		// Store previous state
		DragonHealthModifier oldModifier = ComputeHealthModifier();

		// Apply differential
		m_hp += _hpDiff;
		if(m_hpText != null) m_hpText.text = m_hp + " HP";

		// Get new state
		bool isStarving = m_hp < 50f;
		bool isCritical = m_hp < 25f;

		// This behaves identically as the DragonPlayer class to detect when the health modifiers are reached
		DragonHealthModifier newModifier = ComputeHealthModifier();
		if(oldModifier != newModifier) {
			Messenger.Broadcast<DragonHealthModifier, DragonHealthModifier>(GameEvents.PLAYER_HEALTH_MODIFIER_CHANGED, oldModifier, newModifier);
		}
	}

	/// <summary>
	/// Simulate a game message.
	/// </summary>
	/// <param name="_type">Type. Doesn't match HUDMessage.Type, careful!</param>
	public void SimulateMessage(int _type) {
		string[] randomEntitySkus = {"entity_1", "entity_2", "entity_3"};
		switch(_type) {
			case 0:		Messenger.Broadcast<DragonData>(GameEvents.DRAGON_LEVEL_UP, null);		break;
			case 1:		Messenger.Broadcast(GameEvents.SURVIVAL_BONUS_ACHIEVED);				break;
			case 2:		Messenger.Broadcast<float, DamageType, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, 10f, DamageType.POISON, null);	break;
			case 3:		Messenger.Broadcast<DragonTier, string>(GameEvents.BIGGER_DRAGON_NEEDED, (DragonTier)UnityEngine.Random.Range((int)DragonTier.TIER_1, (int)DragonTier.COUNT), randomEntitySkus.GetRandomValue());	break;

			case 4:	{
				Mission m = new Mission();
				List<DefinitionNode> missionDefs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.MISSIONS);
				DefinitionNode missionDef = missionDefs.GetRandomValue();
				m.InitWithParams(
					missionDef,
					DefinitionsManager.SharedInstance.GetDefinition(missionDef.Get("type"), DefinitionsCategory.MISSION_TYPES),
					Random.Range(missionDef.GetAsInt("objectiveBaseQuantityMin"), missionDef.GetAsInt("objectiveBaseQuantityMax")),
					Random.value < 0.5f	// 50% chace
				);
				Messenger.Broadcast<Mission>(GameEvents.MISSION_COMPLETED, m);
			} break;

			case 5:		Messenger.Broadcast<CollectibleChest>(GameEvents.CHEST_COLLECTED, null);	break;
			case 6:		Messenger.Broadcast<CollectibleEgg>(GameEvents.EGG_COLLECTED, null);		break;
			default:	break;
		}
	}

	/// <summary>
	/// Compute the health modifier to be applied based on current health.
	/// </summary>
	/// <returns>The health modifier to be applied. <c>null</c> if none.</returns>
	private DragonHealthModifier ComputeHealthModifier() {
		// Modifiers are sorted, so this should work
		for(int i = 0; i < m_healthModifiers.Length; i++) {
			if(m_hp < m_hpMax * m_healthModifiers[i].threshold) {
				return m_healthModifiers[i];
			}
		}
		return null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}