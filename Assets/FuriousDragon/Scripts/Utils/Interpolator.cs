// Interpolator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Simple class to interpolate between two values using different easing functions.
/// <list type="bullet">
/// 	<listheader>TODO:</listheader>
/// 	<item>Pause/resume</item>
/// 	<item>Delay</item>
/// 	<item>Auto-destruction - requires Update()</item>
/// 	<item>Dispatch event when finished - requires Update()</item>
/// 	<item>Interpolate any type (use templates?)</item>
/// </list>
/// </summary>
public class Interpolator {
	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private float mStartTime;
	private float mFrom;
	private float mTo;
	private float mDuration;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization.
	/// </summary>
	public Interpolator() {
		mStartTime = -1;
		mFrom = 0;
		mTo = 0;
		mDuration = 0;
	}

	/// <summary>
	/// Reset the interpolator to use the given data.
	/// </summary>
	/// <param name="_fFrom">Start value.</param>
	/// <param name="_fTo">Target value.</param>
	/// <param name="_fDuration">Duration of the interpolation.</param>
	public void Start(float _fFrom, float _fTo, float _fDuration) {
		// Store initial time
		mStartTime = Time.time;

		// Store params
		mFrom = _fFrom;
		mTo = _fTo;
		mDuration = _fDuration;
	}

	/// <summary>
	/// Compute the linear delta (no easing) of the interpolation.
	/// </summary>
	/// <returns>The progress [0..1] of the interpolation. If Start() was never called, 1 will be returned (as if the interpolation was finished).</returns>
	public float GetDelta() {
		// If Start() was never called or duration is 0, mark it as completed
		if(mStartTime < 0f || mDuration <= 0f) return 1f;

		// Compute and return delta
		float fDelta = (Time.time - mStartTime) / mDuration;
		return Mathf.Clamp01(fDelta);
	}

	/// <summary>
	/// Whether the interpolator has finished or not.
	/// </summary>
	/// <returns><c>true</c> if the time elapsed since the Start() has reached the given duration; otherwise, <c>false</c>.</returns>
	public bool IsFinished() {
		return GetDelta() >= 1f;
	}
	#endregion

	#region VALUE GETTERS ----------------------------------------------------------------------------------------------
	/// <summary>
	/// Get the current value of the interpolation using linear easing.
	/// </summary>
	/// <returns>The value of the interpolation at the current time.</returns>
	public float GetLinear() {
		return Mathf.Lerp(mFrom, mTo, GetDelta());
	}

	/// <summary>
	/// Get the current value of the interpolation using exponential easing.
	/// </summary>
	/// <returns>The value of the interpolation at the current time.</returns>
	public float GetExponential() {
		return Mathf.Lerp(mFrom, mTo, Mathf.Pow(GetDelta(), 0.5f));
	}
	#endregion
}
#endregion