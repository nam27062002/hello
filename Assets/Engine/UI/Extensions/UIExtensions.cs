// UIExtensions.cs
// 
// Created by Alger Ortín Castellví on 24/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension methods to UI related classes.
/// </summary>
public static class UIExtensions {
	//------------------------------------------------------------------------//
	// SCROLL RECT															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Scroll the list to a target position after some frames.
	/// </summary>
	/// <returns>The coroutine.</returns>
	/// <param name="_target">The target scroll list.</param>
	/// <param name="_targetNormalizedPosition">Target normalized position.</param>
	/// <param name="_frames">Frames to wait.</param>
	public static IEnumerator ScrollToPositionDelayedFrames(this ScrollRect _target, Vector2 _targetNormalizedPosition, uint _frames) {
		// Wait the required amount of frames
		while(_frames > 0) {
			_frames--;
			yield return new WaitForEndOfFrame();
		}

		// Do it!
		_target.normalizedPosition = _targetNormalizedPosition;
	}

	/// <summary>
	/// Scroll the list to a target position after some time.
	/// </summary>
	/// <returns>The coroutine.</returns>
	/// <param name="_target">The target scroll list.</param>
	/// <param name="_targetNormalizedPosition">Target normalized position.</param>
	/// <param name="_seconds">Seconds to wait.</param>
	public static IEnumerator ScrollToPositionDelayed(this ScrollRect _target, Vector2 _targetNormalizedPosition, float _seconds) {
		// Wait the required amount of time
		yield return new WaitForSecondsRealtime(_seconds);

		// Do it!
		_target.normalizedPosition = _targetNormalizedPosition;
	}
}