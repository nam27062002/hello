// MathUtils.cs
// 
// Created by Alger Ortín Castellví on 27/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Static utils related to custom mathematic tools.
/// </summary>
public class MathUtils {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//


	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Find the first multiple of a given number bigger than the given value.
	/// </summary>
	/// <returns>The multiple.</returns>
	/// <param name="_value">Base value.</param>
	/// <param name="_factor">Multiple of this.</param>
	public static float NextMultiple(float _value, float _factor) {
		if(_value == 0f) return _factor;	// BIG exception for 0
		return Mathf.Ceil(_value/_factor) * _factor;
	}

	/// <summary>
	/// Find the first multiple of a given number lower than the given value.
	/// </summary>
	/// <returns>The multiple.</returns>
	/// <param name="_value">Base value.</param>
	/// <param name="_factor">Multiple of this.</param>
	public static float PreviousMultiple(float _value, float _factor) {
		if(_value == 0f) return -_factor;	// BIG exception for 0
		return Mathf.Floor(_value/_factor) * _factor;
	}

	/// <summary>
	/// Find out the order of magnitude of the given value.
	/// </summary>
	/// <returns>1, 10, 100, 1000, 10000, etc.</returns>
	/// <param name="_value">The value to be checked.</param>
	public static float GetMagnitude(float _value) {
		float abs = Mathf.Abs(_value);
		float magnitude = 1f;
		while(abs >= magnitude) {	// Check 1, 10, 100, 1000, 10000, etc
			magnitude *= 10f;
		}
		return magnitude;
	}
}
