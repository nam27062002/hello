// ResultsScreenSkinPower.cs
// Hungry Dragon
// 
// Created by  on 27/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

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
public class ResultsScreenSkinPower : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

    [SerializeField]
	private PowerIcon m_powerIcon;

    [SerializeField]
    private PowerTooltip_Generic m_powerTooltip;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//


    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    public void InitFromDefinition(DefinitionNode _powerDef, DefinitionNode _sourceDef, bool _locked)
    {
        if (m_powerIcon != null)
        {
            // Clickable power icon with tooltip 
            m_powerIcon.InitFromDefinition(_powerDef, _sourceDef, _locked);
        }

        if (m_powerTooltip != null)
        {
            // Just the tooltip, always visible and not clickable.
            m_powerTooltip.InitTooltip(_powerDef, _sourceDef, PowerIcon.Mode.SKIN, PowerIcon.DisplayMode.PREVIEW);

            // Display the  tooltip
            UITooltip.PlaceAndShowTooltip(m_powerTooltip, (RectTransform) m_powerTooltip.transform, Vector2.zero, true, true);
        }
    }

}