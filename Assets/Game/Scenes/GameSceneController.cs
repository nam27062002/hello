// GameSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main controller for the game scene.
/// </summary>
public class GameSceneController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Game";

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	// [AOC] We want these to be consulted but never set from outside, so don't add a setter
	// Score
	private long m_score;
	public long score { 
		get { return m_score; }
	}
	
	// Score multiplier
	private int m_scoreMultiplierIdx;
	public float scoreMultiplier {
		get { return 1f; }	//[AOC] TODO!! Get score multipliers for the current dragon SCORE_MULTIPLIERS[mScoreMultiplierIdx].multiplier; }
	}
	
	// Fury/Gold Rush/Rage
	private float mFury;
	public float fury {
		get { return mFury; }
	}
	
	// Time
	private float mElapsedSeconds = 0;
	public float elapsedSeconds {
		get { return mElapsedSeconds; }
	}
	
	// Logic state
	/*private EStates mState = EStates.INIT;
	public EStates state {
		get { return mState; }
	}*/
	
	// Reference to player
	private DragonPlayer mPlayer = null;
	public DragonPlayer player {
		get { return mPlayer; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {

	}

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {

	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {

	}
}

