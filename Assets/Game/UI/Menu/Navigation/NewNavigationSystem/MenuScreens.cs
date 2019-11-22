// MenuScreens.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Just the list of screens :)
/// </summary>
public enum MenuScreen {
	// Don't change order! Insert new screens at the end.
	NONE = -1,

	PLAY,
	DRAGON_SELECTION,
	MISSIONS,
	CHESTS,
	GLOBAL_EVENTS,
	PETS,
	SKINS,
	OPEN_EGG,
	PHOTO,
	DRAGON_UNLOCK,
	EVENT_REWARD,
	PENDING_REWARD,

	TOURNAMENT_INFO,
	TOURNAMENT_DRAGON_SELECTION,
	TOURNAMENT_DRAGON_SETUP,
	TOURNAMENT_REWARD,

	ANIMOJI,

	EMPTY_0, // We cannot remove elements, so we just leave them empty
    EMPTY_1,
    EMPTY_2,
	LEAGUES,
	LEAGUES_REWARD,

	COUNT
}