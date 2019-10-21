// LeaguePlayerInfoTooltip.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

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
/// Specialization of the generic UI Tooltip for the Leagues Leaderboard.
/// </summary>
public class LeaguesPlayerInfoTooltip : UITooltip {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Separator("LeaguePlayerInfoTooltip")]
    [SerializeField] private GameObject m_playerInfoLayout = null;
    [SerializeField] private GameObject m_updateNeededLayout = null;
    [Space]
    [SerializeField] private Text m_playerNameText = null;	// [AOC] Name text uses a dynamic font, so any special character should be properly displayed. On the other hand, instantiation time is increased for each pill containing non-cached characters.
	[SerializeField] private TextMeshProUGUI m_rankText = null;
	[Space]
	[SerializeField] private Localizer m_dragonNameText = null;
	[SerializeField] private TextMeshProUGUI m_dragonLevelText = null;
	[SerializeField] private UISpriteAddressablesLoader m_dragonIconLoader = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_healthText = null;
	[SerializeField] private TextMeshProUGUI m_energyText = null;
	[SerializeField] private TextMeshProUGUI m_speedText = null;
	[Space]
	[SerializeField] private PetShortInfo[] m_petSlots = new PetShortInfo[4];
	[SerializeField] private GameObject m_noPetsMessage = null;
	
	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
    /// Initialize the tooltip with the given data.
    /// </summary>
    /// <param name="_playerInfo">The data used to initialize the tooltip.</param>
	public void Init(HDLiveData.Leaderboard.Record _playerInfo) {

        // Aux vars
        DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, _playerInfo.build.dragon);

        // If the dragon doesnt exists in the content, ask the player to update the game        
        bool updateNeeded = (dragonDef == null);

        // Show a info panel asking him to download the latest version
        m_playerInfoLayout.SetActive(!updateNeeded);
        m_updateNeededLayout.SetActive(updateNeeded);

        if (updateNeeded)
            return;


        // Init visuals
        // Player Info
        if (m_playerNameText != null) {
			m_playerNameText.text = _playerInfo.name;
		}

		if(m_rankText != null) {
            if (LocalizationManager.SharedInstance.GetCurrentLanguageSKU() == "lang_chinese" ||
                LocalizationManager.SharedInstance.GetCurrentLanguageSKU() == "lang_chinese_trad")
            {
                // In chinese the ordinal symbol is at the same height than the number [HDK-4654]
                m_rankText.text = UIUtils.FormatOrdinalNumber(_playerInfo.rank + 1, UIUtils.OrdinalSuffixFormat.DEFAULT);
            } else
            {
                m_rankText.text = UIUtils.FormatOrdinalNumber(_playerInfo.rank + 1, UIUtils.OrdinalSuffixFormat.SUPERSCRIPT);
            }
		}

		// Dragon info
		if(m_dragonNameText != null) {
			m_dragonNameText.Localize(dragonDef.GetAsString("tidName"));
		}

		if(m_dragonLevelText != null) {
			m_dragonLevelText.text = StringUtils.FormatNumber(_playerInfo.build.level);
		}

		if(m_dragonIconLoader != null) {
            DefinitionNode skinDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _playerInfo.build.skin);
            if (skinDef != null)
            {
                string icon = skinDef.Get("icon");
                m_dragonIconLoader.LoadAsync(icon);

            }
            else
            {
                // Something failed, do not show any icon
                m_dragonIconLoader.IsVisible = false;
            }

		}

		// Stats
		// Different formats depending on whether the dragon is a classic one or a special one
		// We will use current player's dragon data to obtain some info. A bit dirty, 
		// but that way we don't need to send each player's dragon data to the server.
		IDragonData dragonData = DragonManager.GetDragonData(dragonDef.sku);	
		if(dragonData.type == IDragonData.Type.SPECIAL) {
			DragonDataSpecial specialDragonData = dragonData as DragonDataSpecial;

			if(m_healthText != null) {
				m_healthText.text = LocalizationManager.SharedInstance.Localize(
					"TID_FRACTION",
					StringUtils.FormatNumber(_playerInfo.build.health),
					StringUtils.FormatNumber(specialDragonData.GetStat(DragonDataSpecial.Stat.HEALTH).maxLevel)
				);
			}

			if(m_energyText != null) {
				m_energyText.text = LocalizationManager.SharedInstance.Localize(
					"TID_FRACTION",
					StringUtils.FormatNumber(_playerInfo.build.energy),
					StringUtils.FormatNumber(specialDragonData.GetStat(DragonDataSpecial.Stat.ENERGY).maxLevel)
				);
			} 

			if(m_speedText != null) {
				m_speedText.text = LocalizationManager.SharedInstance.Localize(
					"TID_FRACTION",
					StringUtils.FormatNumber(_playerInfo.build.speed),
					StringUtils.FormatNumber(specialDragonData.GetStat(DragonDataSpecial.Stat.SPEED).maxLevel)
				);
			} 
		} else {
			DragonDataClassic classicDragonData = dragonData as DragonDataClassic;
			int level = (int)_playerInfo.build.level;

			if(m_healthText != null) {
				m_healthText.text = StringUtils.FormatNumber(classicDragonData.GetMaxHealthAtLevel(level), 0);
			}

			if(m_energyText != null) {
				m_energyText.text = StringUtils.FormatNumber(classicDragonData.GetMaxEnergyBaseAtLevel(level), 0);
			}

			if(m_speedText != null) {
				m_speedText.text = StringUtils.FormatNumber(classicDragonData.GetMaxSpeedAtLevel(level) * 10f, 0);	// x10 to show nicer numbers
			}
		}

        // Pets
        int petsAmount = 0;
		for(int i = 0; i < m_petSlots.Length; ++i) {
			// Skip if invalid
			if(m_petSlots[i] == null) continue;

			// Show this pet?
			if(i >= _playerInfo.build.pets.Count) {
				m_petSlots[i].gameObject.SetActive(false);
				continue;
			}

			// Check pet definition
			DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, _playerInfo.build.pets[i]);
			if(petDef == null) {
				m_petSlots[i].gameObject.SetActive(false);
				continue;
			}

			// Initialize pet slot
			m_petSlots[i].InitWithPet(petDef);
			m_petSlots[i].gameObject.SetActive(true);
            petsAmount++;

        }

		// No pets error message
		if(m_noPetsMessage != null) {
			m_noPetsMessage.SetActive(petsAmount == 0);
		}
	}
}