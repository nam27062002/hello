// RecommendedDragonTooltip.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 15/11/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class RecommendedDragonTooltip : UITooltip {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//

    [Separator("Recommended Dragon")]
    [SerializeField] Localizer m_name;
    [SerializeField] UISpriteAddressablesLoader m_avatarIcon;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

    public void InitFromDragonDefinition (DefinitionNode _dragonDef)
    {
        // Message
        if (m_name != null)
        {
            m_name.Localize(_dragonDef.GetAsString("tidName"));
        }

        if (m_avatarIcon != null)
        {
            // The avatar of the dragon is the icon of the default skin
            string dragonIcon = IDragonData.GetDefaultDisguise(_dragonDef.sku).GetAsString("icon");
            m_avatarIcon.LoadAsync(dragonIcon);
        }
    }

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}