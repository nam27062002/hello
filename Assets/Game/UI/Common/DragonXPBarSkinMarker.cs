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

    public DefinitionNode Definition
    {
        get { return m_definition; }
        set { m_definition = value; }
    }

    private DefinitionNode m_definition;

    // Cached reference
    //private UITooltipTrigger m_trigger;
    //private MenuDragonLevelTooltip m_tooltip;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//

    public void Awake()
    {
        
    }

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

    public void OnTooltipOpen()
    {
        UITooltipTrigger m_trigger = GetComponent<UITooltipTrigger>();
        MenuDragonLevelTooltip m_tooltip = (MenuDragonLevelTooltip) m_trigger.tooltip;

        // Show the tooltip to the left or to the right based on its position on 
        // screen, trying to avoid the player's fingers covering it.

        // Find out best direction (Multidirectional tooltip makes it easy for us)
        UITooltipMultidirectional.ShowDirection bestDir = m_tooltip.CalculateBestDirection(
            m_trigger.anchor.position,
            UITooltipMultidirectional.BestDirectionOptions.HORIZONTAL_ONLY
        );

        // Adjust offset based on best direction
        Vector2 offset = m_trigger.offset;
        if (bestDir == UITooltipMultidirectional.ShowDirection.LEFT)
        {
            offset.x = -Mathf.Abs(offset.x);
        }
        else if (bestDir == UITooltipMultidirectional.ShowDirection.RIGHT)
        {
            offset.x = Mathf.Abs(offset.x);
        }

        // Apply new offset and direction
        m_trigger.offset = offset;
        m_tooltip.SetupDirection(bestDir);


        // Set the content of the tooltip
        m_tooltip.Init(m_definition);

    }
}
