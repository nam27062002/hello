// RangeInt.cs
// 
// Created by Alger Ortín Castellví on 05/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple struct to store a range between two values and give some utils.
/// </summary>
[Serializable]
public class RangeInt {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public int min = 0;
	public int max = 1;

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	public int distance {
		get { return Mathf.Abs(max - min); }
	}

	public int center {
		get { return Lerp(0.5f); }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public RangeInt() {
		// Nothing to do
	}

	/// <summary>
	/// Constructor with parameters.
	/// </summary>
	/// <param name="_min">Initial minimum value of the range.</param>
	/// <param name="_max">Initial maximum value of the range.</param>
	public RangeInt(int _min, int _max) {
		min = _min;
		max = _max;
	}

	/// <summary>
	/// Quick setter.
	/// </summary>
	/// <param name="_min">Initial minimum value of the range.</param>
	/// <param name="_max">Initial maximum value of the range.</param>
	public void Set(int _min, int _max) {
		min = _min;
		max = _max;
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Check whether this range contains the given value, including min and max.
	/// </summary>
	/// <param name="_value">The value to be checked.</param>
	/// <returns><c>true</c> if the given value is between min and max.</returns>
	public bool Contains(int _value) {
		return _value >= min && _value <= max;
	}

	/// <summary>
	/// Get a random value within the range.
	/// </summary>
	/// <returns>A randomly selected value between min and max.</returns>
	public int GetRandom() {
		return UnityEngine.Random.Range(min, max + 1);
	}

	/// <summary>
	/// Get an interpolated value between min and max.
	/// </summary>
	/// <param name="_delta">The interpolation factor.</param>
	/// <returns>The interpolated value between min and max at _delta position.</returns>
	public int Lerp(float _delta) {
		return (int)Mathf.Lerp(min, max, _delta);
	}

	/// <summary>
	/// Get the interpolation delta for a given value between min and max.
	/// </summary>
	/// <param name="_value">The value whose delta factor we want.</param>
	/// <returns>The delta corresponding to the interpolation of the given value between min and max.</returns>
	public int InverseLerp(float _value) {
		return (int)Mathf.InverseLerp(min, max, _value);
	}
}