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
	// Internal
	private float m_floatMin = float.MinValue;
	private float m_floatMax = float.MaxValue;
	private int m_intMin = int.MinValue;
	private int m_intMax = int.MaxValue;

	// Float getters
	public float floatMin { 
		get { return m_floatMin; }
	}
	public float floatMax { 
		get { return m_floatMax; }
	}

	// Int getters
	// We must make sure that values don't 
	public int intMin { 
		get { return m_intMin; }
	}
	public int intMax { 
		get { return m_intMax; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_min">The minimum allowed value for this property.</param>
	/// <param name="_max">The maximum allowed value for this property.</param>
	public NumericRangeAttribute(float _min = float.MinValue, float _max = float.MaxValue) {
		// Store float values
		m_floatMin = _min;
		m_floatMax = _max;

		// Make sure we don't overreach int limits
		// We have to add some margin 
		m_intMin = (int)Mathf.Max((float)(int.MinValue + 100), _min);
		m_intMax = (int)Mathf.Min((float)(int.MaxValue - 100), _max);
	}
}

