// NumberTextAnimator.cs
// 
// Created by Alger Ortín Castellví on 31/03/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Animate a textfield by interpolating between two numbers.
/// </summary>
[RequireComponent(typeof(Text))]
public class NumberTextAnimator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_duration = 0.5f;
	public float duration {
		get { return m_duration; }
		set { m_duration = value; }
	}

	// References
	private Text m_targetTxt;
	public Text text {
		get { return m_targetTxt; }
	}

	// Value tracking
	private int m_initialValue = 0;
	public int initialValue {
		get { return m_initialValue; }
	}

	private int m_finalValue = 0;
	public int finalValue {
		get { return m_finalValue; }
	}

	private int m_currentValue = 0;
	public int currentValue {
		get { return m_currentValue; }
	}

	// Internal logic
	private float m_startTime = 0;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
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

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply the given value to the textfield. That way we make sure formatting is always respected.
	/// </summary>
	/// <param name="_iValue">The value to be applied.</param>
	private void ApplyValue(int _iValue) {
		// Just do it
		m_targetTxt.text = StringUtils.FormatNumber(_iValue);
	}
}
