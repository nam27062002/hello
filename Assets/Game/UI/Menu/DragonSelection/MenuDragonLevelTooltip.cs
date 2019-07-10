// MenuDragonLevelTooltip.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 08/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuDragonLevelTooltip : UITooltipMultidirectional
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
    [Separator("DragonLevelBarTooltip")]
    [SerializeField] private TextMeshProUGUI m_unlockLevelText = null;
    [SerializeField] private UISpriteAddressablesLoader m_skinIcon = null;
    [SerializeField] private PowerIcon m_powerUpIcon = null;

    // Internal
    private string m_skinName;
    private string m_skinIconId;
    private string m_powerUpText;
    private DefinitionNode m_powerUpDef;
    private int m_unlockLevel;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//


    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initialize the tooltip with the given disguise.
    /// If the tooltip has no textfields or icon assigned, will be ignored.
    /// If a text or icon is left empty, its corresponding game object will be disabled.
    /// </summary>
    /// <param name="_skinDefinition">Node definition of the disguise</param>
    public void Init(DefinitionNode _skinDefinition)
    {

        // Skin info
        m_skinName = LocalizationManager.SharedInstance.Localize(_skinDefinition.Get("tidName"));
        m_skinIconId = _skinDefinition.Get("icon");
        m_unlockLevel = _skinDefinition.GetAsInt("level");

        // Power up info
        string powerUpSku = _skinDefinition.Get("powerup");
        m_powerUpDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.POWERUPS, "sku", powerUpSku);
        m_powerUpText = DragonPowerUp.GetDescription(m_powerUpDef, false, false);

        Refresh();
        
    }


    /// <summary>
    /// Refresh all the visual elements 
    /// </summary>
    public void Refresh ()
    {

        // Title
        if (m_titleText != null)
        {
            m_titleText.text = m_skinName;
            m_titleText.gameObject.SetActive(!string.IsNullOrEmpty(m_skinName));
        }

        // Message
        if (m_messageText != null)
        {
            m_messageText.text = m_powerUpText;
            m_messageText.gameObject.SetActive(!string.IsNullOrEmpty(m_powerUpText));
        }

        // Skin Icon
        if (m_skinIcon != null)
        {
            if (m_skinIconId != null)
            {
                m_skinIcon.LoadAsync(m_skinIconId);
            }
            m_skinIcon.gameObject.SetActive(m_skinIconId != null);
        }

        // Powerup Icon
        if (m_powerUpIcon != null)
        {
            if (m_powerUpDef != null)
            {
                m_powerUpIcon.InitFromDefinition(m_powerUpDef, false, false, 0);
            }
            m_powerUpIcon.gameObject.SetActive(m_powerUpDef != null);
        }

        // Unlock level
        if (m_unlockLevelText != null)
        {
            m_unlockLevelText.text = LocalizationManager.SharedInstance.Localize("TID_LAB_TOOLTIP_UNLOCK_LEVEL",
                                                                          m_unlockLevel.ToString()
                                                                          );

        }
    }

    
    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}