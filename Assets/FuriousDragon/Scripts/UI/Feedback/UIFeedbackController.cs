// UIFeedbackController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Launches a feedback effect and auto-destroys itself on finish.
/// </summary>
public class UIFeedbackController : MonoBehaviour {
	#region PROPERTIES -------------------------------------------------------------------------------------------------
	private float mLaunchTime = 0;
	public float lifetime {
		get { return Time.time - mLaunchTime; }
	}
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private Animator mAnimator = null;
	private Text mTextfield = null;
	#endregion

	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Awake() {
		// Get required componentes
		mAnimator = GetComponent<Animator>();
		mTextfield = GetComponent<Text>();

		DebugUtils.Assert(mAnimator != null, "Required component!");
		DebugUtils.Assert(mTextfield != null, "Required component!");
	}
	#endregion

	#region PUBLIC UTILS -----------------------------------------------------------------------------------------------
	/// <summary>
	/// Start (or restart if already running) the controller with the data from the given message.
	/// </summary>
	/// <param name="_message">The feedback message to be displayed.</param>
	public void Launch(UIFeedbackMessage _message) {
		// Make sure we're active
		gameObject.SetActive(true);

		// Apply text
		mTextfield.text = _message.text;

		// Launch animation - will interrupt any active animation
		mAnimator.SetTrigger("start");

		// Internal vars
		mLaunchTime = Time.time;
	}

	/// <summary>
	/// Launch the end animation. Useful for looped animations.
	/// The object wont be deactivated, it will wait for the OnAnimationEnd() callback to be triggered by the animation.
	/// </summary>
	public void End() {
		// Just tell the animator
		mAnimator.SetTrigger("end");
	}
	#endregion

	#region CALLBACKS --------------------------------------------------------------------------------------------------
	/// <summary>
	/// To be called when the animation ends, via animation event system.
	/// </summary>
	public void OnAnimationEnd() {
		// Deactivate ourselves
		gameObject.SetActive(false);
	}
	#endregion
}
#endregion