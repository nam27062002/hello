// DragonTier.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Group the dragons in tiers
/// </summary>
public enum DragonTier {
	TIER_0 = 0,	// XS
	TIER_1,		// S
	TIER_2,		// M
	TIER_3,		// L
	TIER_4,		// XL

	COUNT
}

class DragonTierGlobals
{
	public static DragonTier LAST_TIER = DragonTier.TIER_4;
}

