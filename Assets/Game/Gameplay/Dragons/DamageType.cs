// AmbientHazard.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
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
	DRAIN,
	BIG_DAMAGE,
	LIGHTNING,
    CURSE
}

public class DamageTypeComparer : IEqualityComparer<DamageType>
{
	public bool Equals(DamageType b1, DamageType b2)
    {
        return b1 == b2;
    }

	public int GetHashCode(DamageType bx)
    {
        return (int)bx;
    }
}