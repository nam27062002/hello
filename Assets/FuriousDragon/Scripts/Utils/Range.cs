// Range.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Simple struct to store a range between two values and give some utils.
/// </summary>
[Serializable]
public class Range {
	#region EXPOSED MEMBERS -------------------------------------------------------------------------------------------
	public float min = 0f;
	public float max = 1f;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Constructor with parameters.
	/// </summary>
	/// <param name="_fMin">Initial minimum value of the range.</param>
	/// <param name="_fMax">Initial maximum value of the range.</param>
	public Range(float _fMin, float _fMax) {
		min = _fMin;
		max = _fMax;
	}
	#endregion

	#region PUBLIC UTILS -----------------------------------------------------------------------------------------------
	/// <summary>
	/// Get a random value within the range.
	/// </summary>
	/// <returns>A randomly selected value between min and max.</returns>
	public float GetRandom() {
		return UnityEngine.Random.Range(min, max);
	}

	/// <summary>
	/// Get an interpolated value between min and max.
	/// </summary>
	/// <param name="_fDelta">The interpolation factor.</param>
	public float Lerp(float _fDelta) {
		return Mathf.Lerp(min, max, _fDelta);
	}
	#endregion
}
#endregion