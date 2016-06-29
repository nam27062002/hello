// MathUtils.cs
// 
// Created by Alger Ortín Castellví on 27/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

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
		if(_factor == 0f) return _value;
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
		if(_factor == 0f) return _value;
		return Mathf.Floor(_value/_factor) * _factor;
	}

	/// <summary>
	/// Find the multiple of a given number nearest than the given value.
	/// </summary>
	/// <returns>The multiple.</returns>
	/// <param name="_value">Base value.</param>
	/// <param name="_factor">Multiple of this.</param>
	public static float Snap(float _value, float _factor) {
		if(_factor == 0f) return _value;
		return Mathf.Round(_value/_factor) * _factor;
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

	/// <summary>
	/// Check whether a given value is between two limits, <b>both included</b>.
	/// </summary>
	/// <returns><c>true</c> if is <paramref name="_value"/> is between <paramref name="_limit1"/> and <paramref name="_limit2"/>, regardless of their order; <c>false</c> otherwise.</returns>
	/// <param name="_value">The value to be checked.</param>
	/// <param name="_limit1">Min or max limit.</param>
	/// <param name="_limit2">The other limit.</param>
	public static bool IsBetween(IComparable _value, IComparable _limit1, IComparable _limit2) {
		// CompareTo:
		// - Less than zero: The current instance precedes the object specified by the CompareTo method in the sort order.
		// - Zero: This current instance occurs in the same position in the sort order as the object specified by the CompareTo method.
		// - Greater than zero: This current instance follows the object specified by the CompareTo method in the sort order.

		// a) _limit1 smaller than _limit2
		if(_value.CompareTo(_limit1) >= 0 && _value.CompareTo(_limit2) <= 0) {
			return true;
		}

		// b) _limit2 smaller than _limit1
		else if(_value.CompareTo(_limit2) >= 0 && _value.CompareTo(_limit1) <= 0) {
			return true;
		}

		// c) outside limits
		return false;
	}

	/// <summary>
	/// Check whether a given value is between two limits, <b>both excluded</b>.
	/// </summary>
	/// <returns><c>true</c> if is <paramref name="_value"/> is within <paramref name="_limit1"/> and <paramref name="_limit2"/>, regardless of their order; <c>false</c> otherwise.</returns>
	/// <param name="_value">The value to be checked.</param>
	/// <param name="_limit1">Min or max limit.</param>
	/// <param name="_limit2">The other limit.</param>
	public static bool IsWithin(IComparable _value, IComparable _limit1, IComparable _limit2) {
		// CompareTo:
		// - Less than zero: The current instance precedes the object specified by the CompareTo method in the sort order.
		// - Zero: This current instance occurs in the same position in the sort order as the object specified by the CompareTo method.
		// - Greater than zero: This current instance follows the object specified by the CompareTo method in the sort order.

		// a) _limit1 smaller than _limit2
		if(_value.CompareTo(_limit1) > 0 && _value.CompareTo(_limit2) < 0) {
			return true;
		}

		// b) _limit2 smaller than _limit1
		else if(_value.CompareTo(_limit2) > 0 && _value.CompareTo(_limit1) < 0) {
			return true;
		}

		// c) outside limits
		return false;
	}



	/**
	 * Tests intersection between a circle and a segment of another circle, given arc length.
	 * This is only approximate.
	 * Note: Ensure the arcCentreLine is a normalised vector
	 */
	public static bool TestCircleVsArc(Vector3 arcCentre, float arcAngle, float arcRadius, Vector3 arcCentreLine, Vector3 circleCentre, float circleRadius)
	{
		Vector3 disp = circleCentre - arcCentre;
		if(disp.sqrMagnitude <= (arcRadius + circleRadius) * (arcRadius + circleRadius))
		{
			Vector3 perp = disp.normalized * circleRadius;
			perp = new Vector3(-perp.y, perp.x, 0);

			// get points at the sides of the circle
			Vector3 p1 = circleCentre + perp;
			Vector3 p2 = circleCentre - perp;

			float cosa = Mathf.Cos(arcAngle/2.0f * Mathf.Deg2Rad);
			return Vector3.Dot(arcCentreLine, (p1 - arcCentre).normalized) >= cosa || Vector3.Dot(arcCentreLine, (p2 - arcCentre).normalized) >= cosa;
		}
		return false;
	}
}
