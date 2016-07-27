// FloatRangeAttribute.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 25/07/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple custom attribute to limit the range of a numeric property.
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class NumericRangeAttribute : PropertyAttribute {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public float m_min = float.MinValue;
	public float m_max = float.MaxValue;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_min">The minimum allowed value for this property.</param>
	/// <param name="_max">The maximum allowed value for this property.</param>
	public NumericRangeAttribute(float _min = float.MinValue, float _max = float.MaxValue) {
		m_min = _min;
		m_max = _max;
	}
}

