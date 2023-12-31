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
[Serializable]
public class FeedbackData {
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
		public List<string> data = new List<string>();
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
	/// Initialize extracting the data from the given Entity definition.
	/// </summary>
	/// <param name="_def">The definition to be used.</param>
	public void InitFromDef(DefinitionNode _def) {
		// Clear previous data
		for(int i = 0; i < m_feedbacks.Length; i++) {
			if(m_feedbacks[i] == null) {
				m_feedbacks[i] = new FeedbackList();
			} else {
				m_feedbacks[i].data.Clear();
			}
		}

		// Probabilities
		m_probabilities[(int)Type.DAMAGE] = _def.GetAsFloat("damageFeedbackChance");
		m_probabilities[(int)Type.EAT] = _def.GetAsFloat("eatFeedbackChance");
		m_probabilities[(int)Type.BURN] = _def.GetAsFloat("burnFeedbackChance");
		m_probabilities[(int)Type.DESTROY] = _def.GetAsFloat("destroyFeedbackChance");

		// Feedback strings
		m_feedbacks[(int)Type.DAMAGE].data.AddRange(_def.GetAsArray<string>("tidDamageFeedback"));
		m_feedbacks[(int)Type.EAT].data.AddRange(_def.GetAsArray<string>("tidEatFeedback"));
		m_feedbacks[(int)Type.BURN].data.AddRange(_def.GetAsArray<string>("tidBurnFeedback"));
		m_feedbacks[(int)Type.DESTROY].data.AddRange(_def.GetAsArray<string>("tidDestroyFeedback"));
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
		if(m_feedbacks[typeIdx].data.Count > 0) {
			// Probability of spawning a feedback message
			if(UnityEngine.Random.Range(0f, 1f) < m_probabilities[typeIdx]) {
				// Pick a random one, localize and return it
				string feedbackTid = m_feedbacks[typeIdx].data[UnityEngine.Random.Range(0, m_feedbacks[typeIdx].data.Count)];
                return LocalizationManager.SharedInstance.Localize(feedbackTid);
			}
		}
		return "";
	}
}