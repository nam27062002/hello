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
	INIT						= 1 << 0,
	FIRST_PLAY_SCREEN			= 1 << 1,
	CONTROLS_POPUP				= 1 << 2,
	DRAGON_SELECTION			= 1 << 3,
	EGG_INCUBATOR				= 1 << 4,
	CHEST_REWARD				= 1 << 5,
	EGG_INCUBATOR_SKIP_TIMER 	= 1 << 6,
	FIRST_RUN					= 1 << 7,
	ALL							= ~(0)		// http://stackoverflow.com/questions/7467722/how-to-set-all-bits-of-enum-flag
}