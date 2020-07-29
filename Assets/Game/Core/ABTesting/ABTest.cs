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
		UNLOCKED_SKIN_POWER_AS_INFO_BOX, // Show an info box instead of a power icon with tooltip
        UNLOCKED_SKIN_SHOW_CONTINUE_BUTTON // Show a continue button next to the purchase button
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
				
			case Test.UNLOCKED_SKIN_POWER_AS_INFO_BOX:
				return GameSettings.UNLOCKED_SKIN_POWER_AS_INFO_BOX;
				
			case Test.UNLOCKED_SKIN_SHOW_CONTINUE_BUTTON:
				return GameSettings.SHOW_CONTINUE_BUTTON_IN_UNLOCKED_SKIN;

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