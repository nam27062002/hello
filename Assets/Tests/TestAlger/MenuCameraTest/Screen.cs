// Screen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/02/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace MenuCameraTest {
	/// <summary>
	/// Just the list of screens :)
	/// </summary>
	public enum Screen {
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

		COUNT
	}
}