// ABTest.cs
// Hungry Dragon
// 
// Created by  on 28/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Use this class to centralize all the AB tests defined by the designers      
/// </summary>
[Serializable]
public class ABTest {

	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// The different AB tests configured
	public enum Test
	{
		MAP_AS_A_BUTTON,
		SHOW_NEXT_DRAGON_IN_XP_BAR,
		SHOW_UNLOCK_PROGRESSION_TEXT,
		UNLOCKED_SKIN_POWER_AS_TOOLTIP,
        UNLOCKED_SKIN_TAP_TO_CONTINUE_AS_BUTTON
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

    public static bool GetValue (Test _test)
    {

        switch (_test)
        {
			case Test.MAP_AS_A_BUTTON:
				return GameSettings.MAP_AS_BUTTON;

			case Test.SHOW_NEXT_DRAGON_IN_XP_BAR:
				return GameSettings.SHOW_NEXT_DRAGON_IN_XP_BAR;

			case Test.SHOW_UNLOCK_PROGRESSION_TEXT:
				return GameSettings.SHOWN_UNLOCK_PROGRESSION_TEXT;
				
			case Test.UNLOCKED_SKIN_POWER_AS_TOOLTIP:
				return GameSettings.UNLOCKED_SKIN_POWER_AS_TOOLTIP;
				
			case Test.UNLOCKED_SKIN_TAP_TO_CONTINUE_AS_BUTTON:
				return GameSettings.TAP_TO_CONTINUE_IN_UNLOCKED_SKIN_AS_BUTTON;

        }

		Debug.LogError("The test " + _test.ToString() + " is not defined");
		return false;
    }

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}