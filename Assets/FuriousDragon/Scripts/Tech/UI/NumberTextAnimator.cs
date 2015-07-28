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
	public float mDuration = 0.5f;
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private Text mTargetTxt;
	private int mInitialValue = 0;
	private int mFinalValue = 0;
	private int mCurrentValue = 0;
	private float mStartTime = 0;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Initialization
	/// </summary>
	void Awake() {
		// Get target text component
		mTargetTxt = gameObject.GetComponent<Text>();	// Should have one, since we're using the [RequireComponent] tag
	}
	
	/// <summary>
	/// Called every frame
	/// </summary>
	void Update() {
		// Skip if we've reached the final value
		if(mCurrentValue != mFinalValue) {
			// Update current value
			int iNewValue = (int)Mathf.SmoothStep(mInitialValue, mFinalValue, (Time.time - mStartTime)/mDuration);

			// If value has changed, update textfield
			if(iNewValue != mCurrentValue) {
				ApplyValue(iNewValue);
				mCurrentValue = iNewValue;
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
		mInitialValue = _iInitialValue;
		mFinalValue = _iFinalValue;

		// Reset timestamp and current value
		mCurrentValue = mInitialValue;
		mStartTime = Time.time;

		// Initialize textfield
		ApplyValue(mInitialValue);
	}

	/// <summary>
	/// Alternative version, current value will be used as starting value.
	/// </summary>
	/// <param name="_iFinalValue">Final value of the textfield.</param>
	public void SetValue(int _iFinalValue) {
		// Call the other version
		SetValue(mCurrentValue, _iFinalValue);
	}
	#endregion

	#region INTERNAL UTILS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Apply the given value to the textfield. That way we make sure formatting is always respected.
	/// </summary>
	/// <param name="_iValue">The value to be applied.</param>
	private void ApplyValue(int _iValue) {
		// Just do it
		mTargetTxt.text = _iValue.ToString("N0", CultureInfo.CurrentCulture);
	}
	#endregion
}
#endregion
