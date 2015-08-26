// NumberTextAnimator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.UI;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Animate a textfield by interpolating between two numbers.
/// </summary>
[RequireComponent (typeof(Text))]
public class NumberTextAnimator : MonoBehaviour {
	#region CONSTANTS --------------------------------------------------------------------------------------------------

	#endregion

	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	public float m_duration = 0.5f;
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private Text m_targetTxt;
	private int m_initialValue = 0;
	private int m_finalValue = 0;
	private int m_currentValue = 0;
	private float m_startTime = 0;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		// Get target text component
		m_targetTxt = gameObject.GetComponent<Text>();	// Should have one, since we're using the [RequireComponent] tag
	}
	
	/// <summary>
	/// Called every frame
	/// </summary>
	void Update() {
		// Skip if we've reached the final value
		if(m_currentValue != m_finalValue) {
			// Update current value
			int iNewValue = (int)Mathf.SmoothStep(m_initialValue, m_finalValue, (Time.time - m_startTime)/m_duration);

			// If value has changed, update textfield
			if(iNewValue != m_currentValue) {
				ApplyValue(iNewValue);
				m_currentValue = iNewValue;
			}
		}
	}

	/// <summary>
	/// Animate the textfield to interpolate using these parameters.
	/// </summary>
	/// <param name="_iInitialValue">Initial value of the textfield.</param>
	/// <param name="_iFinalValue">Final value of the textfield.</param>
	public void SetValue(int _iInitialValue, int _iFinalValue) {
		// Store parameters
		m_initialValue = _iInitialValue;
		m_finalValue = _iFinalValue;

		// Reset timestamp and current value
		m_currentValue = m_initialValue;
		m_startTime = Time.time;

		// Initialize textfield
		ApplyValue(m_initialValue);
	}

	/// <summary>
	/// Alternative version, current value will be used as starting value.
	/// </summary>
	/// <param name="_iFinalValue">Final value of the textfield.</param>
	public void SetValue(int _iFinalValue) {
		// Call the other version
		SetValue(m_currentValue, _iFinalValue);
	}
	#endregion

	#region INTERNAL UTILS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Apply the given value to the textfield. That way we make sure formatting is always respected.
	/// </summary>
	/// <param name="_iValue">The value to be applied.</param>
	private void ApplyValue(int _iValue) {
		// Just do it
		m_targetTxt.text = _iValue.ToString("N0", CultureInfo.CurrentCulture);
	}
	#endregion
}
#endregion
