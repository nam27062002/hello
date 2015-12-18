// DamageDealer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Behaviour for any entity able to do damage to the dragon who should display some kind of feedback.
/// </summary>
public class DamageDealer_OLD : MonoBehaviour {
	#region EXPOSED MEMBERS ---------------------------------------------------------------------------------------------
	[Range(0, 1)] public float feedbackProbability = 0.5f;
	public List<UIFeedbackMessage> damageFeedbacks = new List<UIFeedbackMessage>();
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------

	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization.
	/// </summary>
	void Start() {
		// Nothing for now
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// </summary>
	void Update() {
		// Nothing for now
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Nothing for now
	}
	#endregion
}
#endregion