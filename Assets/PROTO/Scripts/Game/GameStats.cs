// GameStats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Stats accumulated during a game. Use events to set them.
/// </summary>
public class GameStats : MonoBehaviour {
	#region PROPERTIES -------------------------------------------------------------------------------------------------
	// Eaten entities
	protected Dictionary<string, int> mEatenEntityCount;
	public Dictionary<string, int> eatenEntityCount { 
		get { return mEatenEntityCount; }
	}
	protected int mEatenEntityCountTotal;
	public int eatenEntityCountTotal { 
		get { return mEatenEntityCountTotal; }
	}

	// Burned entities
	protected Dictionary<string, int> mBurnedEntityCount;
	public Dictionary<string, int> burnedEntityCount { 
		get { return mBurnedEntityCount; }
	}
	protected int mBurnedEntityCountTotal;
	public int burnedEntityCountTotal { 
		get { return mBurnedEntityCountTotal; }
	}

	// Destroyed entities
	protected Dictionary<string, int> mDestroyedEntityCount;
	public Dictionary<string, int> destroyedEntityCount { 
		get { return mDestroyedEntityCount; }
	}
	protected int mDestroyedEntityCountTotal;
	public int destroyedEntityCountTotal { 
		get { return mDestroyedEntityCountTotal; }
	}

	// Collectibles
	protected Collectible[] mCollectibles;
	public Collectible[] collectibles {
		get { return mCollectibles; }
	}
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Pre-initialization.
	/// </summary>
	protected virtual void Awake() {
		// Create dictionaries and initialize default values
		mEatenEntityCount = new Dictionary<string, int>();
		mBurnedEntityCount = new Dictionary<string, int>();
		mDestroyedEntityCount = new Dictionary<string, int>();
		Reset();
	}
	#endregion

	#region PUBLIC UTILS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Reset stats to their default values.
	/// </summary>
	public virtual void Reset() {
		// Clear dictionaries
		mEatenEntityCount.Clear();
		mBurnedEntityCount.Clear();
		mDestroyedEntityCount.Clear();
		mCollectibles = new Collectible[0];

		// Reset the rest of the vars
		mEatenEntityCountTotal = 0;
		mBurnedEntityCountTotal = 0;
		mDestroyedEntityCountTotal = 0;
	}

	/// <summary>
	/// Incorporate a stats set to this one.
	/// </summary>
	/// <param name="name="_stats">The stats to be joined to these.</param>
	public virtual void JoinStats(GameStats _stats) {
		// [AOC] TODO!!
	}
	#endregion

	#region CALLBACKS -------------------------------------------------------------------------------------------------
	/// <summary>
	/// Actions to do when a game starts. Use only for in-game stats rather than global stats.
	/// </summary>
	public void OnGameStart() {
		// Reset stats
		Reset();

		// Initialize collectibles
		// http://docs.unity3d.com/ScriptReference/Object.FindObjectsOfType.html
		mCollectibles = FindObjectsOfType<Collectible>();
	}

	/// <summary>
	/// An entity has been eaten.
	/// </summary>
	/// <param name="_entity">The entity that was eaten.</param>
	public void OnEntityEaten(GameEntity _entity) {
		// Increase stat counter, both detailed and global
		if(mEatenEntityCount.ContainsKey(_entity.typeID)) {
			mEatenEntityCount[_entity.typeID]++;
		} else {
			mEatenEntityCount.Add(_entity.typeID, 1);
		}
		mEatenEntityCountTotal++;
	}

	/// <summary>
	/// An entity has been burned.
	/// </summary>
	/// <param name="_entity">The entity that was burned.</param>
	public void OnEntityBurned(GameEntity _entity) {
		// Increase stat counter, both detailed and global
		if(mBurnedEntityCount.ContainsKey(_entity.typeID)) {
			mBurnedEntityCount[_entity.typeID]++;
		} else {
			mBurnedEntityCount.Add(_entity.typeID, 1);
		}
		mBurnedEntityCountTotal++;
	}
	
	/// <summary>
	/// An entity has been destroyed.
	/// </summary>
	/// <param name="_entity">The entity that was destroyed.</param>
	public void OnEntityDestroyed(GameEntity _entity) {
		// Increase stat counter, both detailed and global
		if(mDestroyedEntityCount.ContainsKey(_entity.typeID)) {
			mDestroyedEntityCount[_entity.typeID]++;
		} else {
			mDestroyedEntityCount.Add(_entity.typeID, 1);
		}
		mDestroyedEntityCountTotal++;
	}
	#endregion
}
#endregion