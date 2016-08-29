// SlowMotionController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Generic behaviour to trigger slow motion effects.
/// </summary>
public class SlowMotionController : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	private static readonly float MIN_INTERVAL_BETWEEN_MOTIONS = 30f;	// Minimum seconds between effects
	#endregion

	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	[Range(0, 1)] public float timeScaleFactor = 0.25f;
	public float duration = 1.5f;
	public float zoomOffset = 750f;
	public float zoomInDuration = 0.5f;
	public float zoomOutDuration = 1f;
	public bool alwaysTrigger = false;	// If set to true, the effect will be triggered regardless of global interval timer
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private bool mIsActive = false;
	private float mTimer = -1f;
	private float mActiveFactor = 1f;
	private float mActiveDuration = 0f;
	#endregion

	#region INTERNAL STATIC MEMBERS ------------------------------------------------------------------------------------
	private static SlowMotionController mActiveController = null;
	private static float mLastMotionTimestamp = float.MinValue;
	#endregion

	#region REFERENCES -------------------------------------------------------------------------------------------------
	// private GameCameraController mMainCameraController = null;
	private MotionBlur mMotionBlur = null;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Start() {
		// mMainCameraController = Camera.main.GetComponent<GameCameraController>();
		mMotionBlur = Camera.main.GetComponent<MotionBlur>();
		mMotionBlur.enabled = false;
	}

	/// <summary>
	/// Logic update, called every frame.
	/// </summary>
	public void Update() {
		if ( Input.GetKeyDown(KeyCode.A) )
			StartSlowMotion();

		// Timer control - if required
		if(mIsActive) {
			// [AOC] We want the timer to update in normal time scale, so ignore current timescale
			mTimer -= Time.deltaTime / Time.timeScale;
			if(mTimer <= 0) {
				// Stop this slow motion controller
				StopSlowMotion();
			}

			// Keep global timer updated
			mLastMotionTimestamp = Time.time;
		}
	}

	/// <summary>
	/// Start slow motion effect with values defined in the inspector. 
	/// If already running, the timer and the factor will be reset.
	/// If another effect is already running, it will be cancelled and this one will take the lead.
	/// </summary>
	public void StartSlowMotion() {
		// Use the other call
		StartSlowMotion(duration, timeScaleFactor);
	}

	/// <summary>
	/// Start slow motion effect, provided the required minimum time has passed since the last slow motion was triggered.
	/// </summary>
	/// <param name="_fDuration">The duration of the effect. Will automatically stop once the given time has elapsed.</param>
	/// <param name="_fTimeScaleFactor">The intensity of the effect.</param>
	public void StartSlowMotion(float _fDuration, float _fTimeScaleFactor) {
		// Ignore if not enough time has passed since the last slow motion effect was triggered
		// Unless explicitely defined to ignore the interval
		if(alwaysTrigger) {
			// Stop current active controller
			if(mActiveController != null) {
				mActiveController.StopSlowMotion();
			}
		} else if(Time.time - mLastMotionTimestamp < MIN_INTERVAL_BETWEEN_MOTIONS) {
			// Not enough time has passed since last motion, return
			return;
		}

		// Store parameters
		mActiveFactor = _fTimeScaleFactor;
		mTimer = _fDuration;
		mActiveDuration = _fDuration;

		// Apply time scale
		Time.timeScale *= mActiveFactor;

		// Internal vars
		mIsActive = true;
		mLastMotionTimestamp = Time.time;
		mActiveController = this;

		// Motion blur!
		mMotionBlur.enabled = true;

		Messenger.Broadcast<bool>(GameEvents.SLOW_MOTION_TOGGLED, true);
	}

	/// <summary>
	/// Stop any active slow motion effect.
	/// </summary>
	public void StopSlowMotion() {
		// Nothing to do if stop motion is not active
		if(!mIsActive) return;

		// Revert time scale
		Time.timeScale /= mActiveFactor;

		// Internal vars
		mActiveFactor = 1f;
		mIsActive = false;
		mActiveController = null;	// There can only be one active controller and should be this one, so this is safe

		// Motion blur!
		mMotionBlur.enabled = false;

		Messenger.Broadcast<bool>(GameEvents.SLOW_MOTION_TOGGLED, false);
	}
	#endregion
}
#endregion