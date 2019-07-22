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
	TIER_5, 	// XXL

    COUNT
}

class DragonTierGlobals
{
	public static DragonTier LAST_TIER = DragonTier.COUNT - 1;

	public static DragonTier GetFromInt(int _tier) {
		if (_tier == -1) {
			return DragonTier.COUNT;
		}

		return (DragonTier) _tier;
	}
}