// AmbientHazard.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Enum for the different types of damage.
/// </summary>
public enum DamageType {
	// New values always at the end! Otherwise serialized properties will break!
	NONE = -1,

	NORMAL,
	POISON,
	LATCH,
	MINE,
	EXPLOSION,
	ARROW,
	FLOUR,
	DRAIN
}