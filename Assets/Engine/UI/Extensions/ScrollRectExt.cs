// ScrollRectExt.cs
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

using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Extension methods to Unity's UI ScrollRect class.
/// </summary>
public static class ScrollRectExt {
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
		_target.velocity = Vector2.zero;
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
		_target.velocity = Vector2.zero;
	}

	/// <summary>
	/// Obtain the normalized position to be applied to the ScrollRect so that
	/// the given item (assuming item belongs to the content) is centered in the
	/// ScrollRect's viewport.
	/// </summary>
	/// <param name="_scroll">Target ScrollRect.</param>
	/// <param name="_item">The item to be centered.</param>
	/// <param name="_clampDirection">If set to true, only the directions enabled in the ScrollRect will be taken in consideration.</param>
	/// <returns>The normalized position of the ScrollRect to be applied in order to center _item in the viewport.</returns>
	public static Vector2 GetNormalizedPositionForItem(this ScrollRect _scroll, Transform _item, bool _clampDirection = true) {
		// Aux vars for shorter notation
		Rect contentRect = _scroll.content.rect;
		Rect viewportRect = _scroll.viewport.rect;

		// Get item's position in scroll's viewport local space
		Vector3 itemPos = _scroll.viewport.InverseTransformPoint(_item.position);

		// Get viewport's center in viewport's local space
		Vector3 viewportCenter = viewportRect.center;

		// Compute how much we need to move the content so target item gets centered
		Vector2 offset = new Vector2(
			itemPos.x - viewportCenter.x,
			itemPos.y - viewportCenter.y
		);

		// Use the normalized property of the ScrollRect rather than directly setting the content's position
		// This way it's easier to handle conflicts with the Elastic ScrollRects
		Vector2 hiddenSize = new Vector2(   // This works assuming there is no scale change between viewport and content
			contentRect.width - viewportRect.width,
			contentRect.height - viewportRect.height
		);

		// Check div/0!
		Vector2 normalizedOffset = new Vector2(
			Mathf.Abs(hiddenSize.x) > 0f ? offset.x / hiddenSize.x : 0f,
			Mathf.Abs(hiddenSize.y) > 0f ? offset.y / hiddenSize.y : 0f
		);

		// Clamp direction if needed
		if(_clampDirection) {
			if(!_scroll.horizontal)	normalizedOffset.x = 0f;
			if(!_scroll.vertical)	normalizedOffset.y = 0f;
		}

		// Compute final position
		Vector2 targetNormalizedPos = _scroll.normalizedPosition + normalizedOffset;

		// Clamp? Only in certain scroll modes
		switch(_scroll.movementType) {
			case ScrollRect.MovementType.Clamped:
			case ScrollRect.MovementType.Elastic: {
				targetNormalizedPos.Set(
					Mathf.Clamp01(targetNormalizedPos.x),
					Mathf.Clamp01(targetNormalizedPos.y)
				);
			} break;
		}

		// Done!
		return targetNormalizedPos;
	}

	/// <summary>
	/// Tweens a ScrollRect's horizontal/verticalNormalizedPosition to the given item.
	/// Also stores the ScrollRect as the tween's target so it can be used for filtered operations.
	/// </summary>
	/// <param name="_item">The item to reach.</param>
	/// <param name="_duration">The duration of the tween.</param>
	/// <param name="_snapping">If <c>true</c> the tween will smoothly snap all values to integers.</param>
	public static Tweener DOGoToItem(this ScrollRect _scroll, Transform _item, float _duration, bool _snapping = false) {
		// Get target normalized position
		Vector2 targetPos = _scroll.GetNormalizedPositionForItem(_item);

		// Different tween setup depending on allowed scroll directions
		Tweener tween = null;
		if(_scroll.horizontal && !_scroll.vertical) {
			tween = _scroll.DOHorizontalNormalizedPos(targetPos.x, _duration);
		} else if(!_scroll.horizontal && _scroll.vertical) {
			tween = _scroll.DOVerticalNormalizedPos(targetPos.y, _duration);
		} else {
			tween = _scroll.DONormalizedPos(targetPos, _duration);
		}

		// Done!
		return tween;
	}
}