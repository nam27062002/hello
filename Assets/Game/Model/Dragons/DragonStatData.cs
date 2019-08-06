// DragonStat.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliary class to DragonDataSpecial storing data on a single stat.
/// </summary>
[Serializable]
public class DragonStatData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Stat definition, containing info like icon, tid, etc.
	/// </summary>
	public DefinitionNode def = null;

	/// <summary>
	/// Dragon whom this stat belongs to
	/// </summary>
	public DragonDataSpecial parentDragon = null;

	/// <summary>
	/// Level [0..N-1]
	/// </summary>
	public int level = 0;

	/// <summary>
	/// Max level that can be achieved on this stat
	/// </summary>
	public int maxLevel = 9;

	/// <summary>
	/// Bonus range
	/// Percentage bonus, i.e. 0.25 -> +25%
	/// </summary>
	public Range valueRange = new Range();

	/// <summary>
	/// Stat value at current level
	/// Percentage bonus, i.e. 0.25 -> +25% 
	/// </summary>
	public float value {
		get { return GetValueForLevel(level); }
	}

	/// <summary>
	/// Current level delta [0..1]
	/// </summary>
	public float progress {
		get { return level / (float)maxLevel; }
	}

	/// <summary>
	/// Amount increased for every level
	/// Percentage bonus, i.e. 0.05 -> +5%
	/// </summary>
	public float valueStep {
		get { return valueRange.distance / (float)maxLevel;  }
	}

	//------------------------------------------------------------------------//
	// METHODS														 		  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Compute the value corresponding to a given level.
	/// </summary>
	/// <returns>The value of the skill for the requested level.</returns>
	/// <param name="_level">Level.</param>
	public float GetValueForLevel(int _level) {
		return valueRange.Lerp(progress);
	}

    /// <summary>
    /// Is the stat at its maximum value?
    /// </summary>
    public bool IsMaxed ()
    {
        return (level  >= maxLevel);
    }



    //------------------------------------------------------------------------//
    // PERSISTENCE															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Reset persistence values. Make sure any new value is added to this method as well.
    /// </summary>
    public void ResetLoadedData() {
		// Level
		level = 0;
	}

	/// <summary>
	/// Load state from a persistence object.
	/// </summary>
	/// <param name="_data">The data object loaded from persistence.</param>
	public void Load(SimpleJSON.JSONNode _data) {
		// Level
		if(_data.ContainsKey("level")) {
			level = _data["level"].AsInt;
		} else {
			level = 0;
		}
	}

	/// <summary>
	/// Create and return a persistence save data object initialized with the data.
	/// </summary>
	/// <returns>A new data object to be stored to persistence by the PersistenceManager.</returns>
	public SimpleJSON.JSONNode Save() {
		// Create new object
		SimpleJSON.JSONClass data = new SimpleJSON.JSONClass();

		// Level
		data.Add("level", level);

		// Done!
		return data;
	}
}