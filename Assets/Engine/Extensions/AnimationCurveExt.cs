// AnimationCurveExt.cs
// 
// Created by Alger Ortín Castellví on 27/06/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom extensions to the AnimationCurve class.
/// </summary>
public static class AnimationCurveExt {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// EXTENSION METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Makes all the keyframes in the curve linear.
	/// </summary>
	/// <param name="_curve">The curve to be modified.</param>
	public static void MakeLinear(this AnimationCurve _curve) {
		// Grab all the keyframes and modify their tangent properties
		Keyframe k;
		for(int i = 0; i < _curve.length; i++) {
			// Grab the target keyframe
			k = _curve[i];

			// Tell editor this keygrame is using linear tangents
			k.tangentMode = 21;	// Undocumented Unity feature, 21 is the mask for "Linear" and "Break" tangent setup

			// Compute linear tangent values for this keyframe
			k.inTangent = _curve.CalculateLinearTangent(i, true);
			k.outTangent = _curve.CalculateLinearTangent(i, false);

			// Apply changes
			_curve.MoveKey(i, k);
		}
	}

	/// <summary>
	/// Compute either the in or out tangent of a point in the curve.
	/// Doesn't modify the point.
	/// See http://wiki.unity3d.com/index.php/EditorAnimationCurveExtension
	/// </summary>
	/// <returns>The requested tangent value.</returns>
	/// <param name="_curve">The target curve.</param>
	/// <param name="_index">Index of the keyframe whose tangent we want to compute.</param>
	/// <param name="_inTangent">Whether to compute the in (<c>true</c>) or the out (<c>false</c>) tangent.</param>
	public static float CalculateLinearTangent(this AnimationCurve _curve, int _index, bool _inTangent) {
		// Check that index is valid
		if(_inTangent && _index <= 0) return 0f;
		if(!_inTangent && _index >= (_curve.length - 1)) return 0f;

		// Compute index to compute the tangent with
		int toIndex = _inTangent ? _index - 1 : _index + 1;
		return (_curve[_index].value - _curve[toIndex].value) / (_curve[_index].time - _curve[toIndex].time);
	}
}
