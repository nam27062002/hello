// ShareScreenHighScore.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

#define RARITY_GRADIENT

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual layout for a pet share screen.
/// </summary>
public class ShareScreenHighScore : IShareScreen {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private TextMeshProUGUI m_scoreText = null;
	[SerializeField] private Image m_dragonImage = null;
	[SerializeField] private PetShortInfo[] m_petSlots = null;
	[SerializeField] private GameObject m_noPetsMessage = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this screen with current quest data (from HDQuestManager).
	/// </summary>
	/// <param name="_shareLocationSku">Location where this screen is triggered.</param>
	/// <param name="_refCamera">Reference camera. Its properties will be copied to the scene's camera.</param>
	/// <param name="_newScore">The new high score.</param>
	/// <param name="_dragonData">The setup used to achieve the new high score.</param>
	public void Init(string _shareLocationSku, Camera _refCamera, long _newScore, IDragonData _dragonData) {
		// Set location and camera
		SetLocation(_shareLocationSku);
		SetRefCamera(_refCamera);

		// Aux vars
		HDQuestManager quest = HDLiveDataManager.quest;

		// Initialize UI elements
		// Score text
		if(m_scoreText != null) {
			m_scoreText.text = StringUtils.FormatNumber(_newScore);
		}

		// Dragon portrait
		if(m_dragonImage != null) {
			DefinitionNode skinDef = skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _dragonData.disguise);
            m_dragonImage.sprite = HDAddressablesManager.Instance.LoadAsset<Sprite>(skinDef.Get("icon"));
		}

		// Pet Slots
		int equippedPets = 0;
		if(m_petSlots != null) {
			for(int i = 0; i < m_petSlots.Length; ++i) {
				if(m_petSlots[i] == null) continue;

				// Don't show if dragon doesn't have so many pet slots
				if(i >= _dragonData.pets.Count) {
					m_petSlots[i].gameObject.SetActive(false);
					continue;
				}

				// Get pet data
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _dragonData.pets[i]);

				// Show? Only if a pet is equipped in this slot
				m_petSlots[i].gameObject.SetActive(petDef != null);
				m_petSlots[i].InitWithPet(petDef);

				// Increase count
				if(petDef != null) equippedPets++;
			}
		}

		// Show message if no pets were equipped
		if(m_noPetsMessage != null) {
			m_noPetsMessage.SetActive(equippedPets == 0);
		}
	}
}