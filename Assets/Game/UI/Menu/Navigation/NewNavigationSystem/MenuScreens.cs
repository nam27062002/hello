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

	LAB_DRAGON_SELECTION,
	LAB_MISSIONS,
	LAB_PETS,
	LEAGUES,
	LEAGUES_REWARD,

	COUNT
}