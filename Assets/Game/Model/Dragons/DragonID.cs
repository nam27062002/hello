﻿// DragonID.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Single enumerator of the types of dragons available in the game.
/// Every dragon must be listed here, and every dragon must have its own
/// prefab under Resources/Dragons/ with a DragonPlayer on it and its own DragonData
/// in the DragonManager prefab.
/// </summary>
public enum DragonID {
	NONE = -1,

	SMALL,
	MEDIUM,
	BIG,

	COUNT
}