// FeedbackData.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/10/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Behaviour for any entity interacting with the dragon who should display some kind of feedback.
/// </summary>
public class FeedbackData_OLD : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Type {
		DAMAGE,
		EAT,
		BURN,
		DESTROY,

		COUNT
	};

	// Aux class since we are not able to do a custom editor with an array of arrays -_-
	// http://answers.unity3d.com/questions/411696/how-to-initialize-array-via-serializedproperty.html
	[Serializable]
	private class FeedbackList {
		public string[] data;
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private float[] m_probabilities = new float[(int)Type.COUNT];
	[SerializeField] private FeedbackList[] m_feedbacks = new FeedbackList[(int)Type.COUNT];

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call
	/// </summary>
	private void Start() {

	}

	//------------------------------------------------------------------//
	// GETTERS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Returns a randomly selected feedback of the requested type, taking probability into account.
	/// </summary>
	/// <returns>The feedback string to be displayed, empty string if none should be displayed.</returns>
	/// <param name="_type">The type of feedback to be obtained.</param>
	public string GetFeedback(Type _type) {
		int typeIdx = (int)_type;
		if(m_feedbacks[typeIdx].data.Length > 0) {
			// Probability of spawning a feedback message
			if(UnityEngine.Random.Range(0f, 1f) < m_probabilities[typeIdx]) {
				// Pick a random one and return it
				return m_feedbacks[typeIdx].data[UnityEngine.Random.Range(0, m_feedbacks[typeIdx].data.Length)];
			}
		}
		return "";
	}
}