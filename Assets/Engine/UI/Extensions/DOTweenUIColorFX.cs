// DOTweenUIColorFX.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
namespace DG.Tweening {
	/// <summary>
	/// Methods that extend UIColorFX objects and allow to directly create and control tweens from their instances.
	/// </summary>
	public static class DOTweenUIColorFX {
		//------------------------------------------------------------------------//
		// EXTENSION METHODS													  //
		//------------------------------------------------------------------------//
		/// <summary>
		/// Tweens a UIColorFX brightness parameter.
		/// </summary>
		/// <returns>The newly created tween.</returns>
		/// <param name="_target">The target UIColorFX.</param>
		/// <param name="_endValue">Final brightness value.</param>
		/// <param name="_duration">Tween duration.</param>
		public static Tweener DOBrightness(this UIColorFX _target, float _endValue, float _duration) {
			return DOTween.To(
				() => _target.brightness,
				_x => _target.brightness = _x,
				_endValue,
				_duration
			).SetTarget(_target);
		}

		/// <summary>
		/// Tweens a UIColorFX saturation parameter.
		/// </summary>
		/// <returns>The newly created tween.</returns>
		/// <param name="_target">The target UIColorFX.</param>
		/// <param name="_endValue">Final saturation value.</param>
		/// <param name="_duration">Tween duration.</param>
		public static Tweener DOSaturation(this UIColorFX _target, float _endValue, float _duration) {
			return DOTween.To(
				() => _target.saturation,
				_x => _target.saturation = _x,
				_endValue,
				_duration
			).SetTarget(_target);
		}

		/// <summary>
		/// Tweens a UIColorFX contrast parameter.
		/// </summary>
		/// <returns>The newly created tween.</returns>
		/// <param name="_target">The target UIColorFX.</param>
		/// <param name="_endValue">Final contrast value.</param>
		/// <param name="_duration">Tween duration.</param>
		public static Tweener DOContrast(this UIColorFX _target, float _endValue, float _duration) {
			return DOTween.To(
				() => _target.contrast,
				_x => _target.contrast = _x,
				_endValue,
				_duration
			).SetTarget(_target);
		}

		/// <summary>
		/// Tweens a UIColorFX alpha parameter.
		/// </summary>
		/// <returns>The newly created tween.</returns>
		/// <param name="_target">The target UIColorFX.</param>
		/// <param name="_endValue">Final alpha value.</param>
		/// <param name="_duration">Tween duration.</param>
		public static Tweener DOFade(this UIColorFX _target, float _endValue, float _duration) {
			return DOTween.To(
				() => _target.alpha,
				_x => _target.alpha = _x,
				_endValue,
				_duration
			).SetTarget(_target);
		}

		/// <summary>
		/// Tweens a UIColorFX multiply color parameter.
		/// </summary>
		/// <returns>The newly created tween.</returns>
		/// <param name="_target">The target UIColorFX.</param>
		/// <param name="_endValue">Final multiply color value.</param>
		/// <param name="_duration">Tween duration.</param>
		public static Tweener DOColorMultiply(this UIColorFX _target, Color _endValue, float _duration) {
			return DOTween.To(
				() => _target.colorMultiply,
				_x => _target.colorMultiply = _x,
				_endValue,
				_duration
			).SetTarget(_target);
		}

		/// <summary>
		/// Tweens a UIColorFX add color parameter.
		/// </summary>
		/// <returns>The newly created tween.</returns>
		/// <param name="_target">The target UIColorFX.</param>
		/// <param name="_endValue">Final add color value.</param>
		/// <param name="_duration">Tween duration.</param>
		public static Tweener DOColorAdd(this UIColorFX _target, Color _endValue, float _duration) {
			return DOTween.To(
				() => _target.colorAdd,
				_x => _target.colorAdd = _x,
				_endValue,
				_duration
			).SetTarget(_target);
		}
	}
}