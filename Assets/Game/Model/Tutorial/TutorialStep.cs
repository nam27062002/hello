// TutorialSteps.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Masked enum with the main tutorial steps.
/// Using a mask since we don't care in which order they are executed (moreover 
/// we don't want to restrict the order so we can be more flexible when adding/removing
/// steps).
/// </summary>
// Using flags attribute for easier mask setup 
// see http://stackoverflow.com/questions/3261451/using-a-bitmask-in-c-sharp
// https://msdn.microsoft.com/en-us/library/system.flagsattribute(v=vs.110).aspx
[Flags]
public enum TutorialStep {
	// [AOC] Max 32 values (try inheriting from long if more are needed)
	// [AOC] DON'T CHANGE THE ORDER!!
	INIT						= 1 << 0,

    /*
    // The steps below have been commented out because we need to use FIRST_RUN step for all of them instead since we want the user to have exactly the same
    // experience until she completes a whole run, which finishes when she sees the results screen.
    //
    // IMPORTANT: The values of the constants have to stay the same in order to maintain compatibility with users who started playing before this change was applied
	FIRST_PLAY_SCREEN			= 1 << 1,
	CONTROLS_POPUP				= 1 << 2,
    */
	BOOST						= 1 << 3,    
	FIRST_RUN					= 1 << 4,

	DRAGON_SELECTION			= 1 << 5,
	MISSIONS_INFO				= 1 << 6,
	SECOND_RUN					= 1 << 7,

	EGG_INCUBATOR				= 1 << 8,
	EGG_REWARD					= 1 << 9,
	PETS_INFO					= 1 << 10,
	PETS_EQUIP					= 1 << 11,
	SKINS_INFO					= 1 << 12,
	CHESTS_INFO					= 1 << 13,

	FIRST_MISSIONS_GENERATED	= 1 << 14,

	EGG_INFO					= 1 << 15,
	GOLDEN_FRAGMENTS_INFO		= 1 << 16,

	FIRST_EGG_COLLECTED			= 1 << 17,

	BOOST_SPACE					= 1 << 18,

	TOURNAMENT_INFO				= 1 << 19,
	TOURNAMENT_BUILD			= 1 << 20,
	QUEST_INFO					= 1 << 21,

	PRE_REG_REWARDS				= 1 << 22,

	SPECIAL_DRAGONS_UNLOCKED	= 1 << 23,
	LEAGUES_INFO				= 1 << 24,

	// [AOC] Max 32 values (try inheriting from long if more are needed)
	// [AOC] DON'T CHANGE THE ORDER!!
	ALL							= ~(0)		// http://stackoverflow.com/questions/7467722/how-to-set-all-bits-of-enum-flag
}