﻿// ScoreFeedback.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/06/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specific controller for a score feedback.
/// </summary>
[RequireComponent(typeof(Text))]
public class ScoreFeedback : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	public class ScoreThreshold {
		public int maxScore;
		public Gradient colorRange;
		public Range fontScaleRange;
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private List<ScoreThreshold> m_thresholds = new List<ScoreThreshold>();
	public List<ScoreThreshold> thresholds {
		get { return m_thresholds; }
	}

	// References
	private Text m_text = null;

	// Internal
	private float m_baseFontSize = 0f;
	private StringBuilder m_sbuilder = new StringBuilder();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get references
		m_text = GetComponent<Text>();

		// Init internal vars
		m_baseFontSize = m_text.fontSize;

		// Make sure score feedbacks are properly sorted
		m_thresholds.Sort((x, y) => {
			return x.maxScore.CompareTo(y.maxScore);
		});
	}

	/// <summary>
	/// A change has been made on the inspector.
	/// </summary>
	private void OnValidate() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Set the score text and format the feedback accordingly
	/// </summary>
	/// <param name="_score">Score amount to be displayed.</param>
	public void SetScore(int _score) {
		// Using a string builder for optimal performance/memory usage
		// Add the '+' symbol when positive, negative values already are formatted with the '-' symbol, 0 has no symbol whatsoever
		m_sbuilder.Length = 0;	// [AOC] No Clear() in .Net 3.5 yet -_-
		if(_score > 0) {
			m_sbuilder.Append("+");
		}
		m_sbuilder.Append(StringUtils.FormatNumber(_score));

		// Apply to text
		m_text.text = m_sbuilder.ToString();

		// Find out target threshold to apply proper formatting
		if(m_thresholds.Count > 0) {
			int selectedIdx = m_thresholds.Count - 1;
			int baseScore = 0;
			for(int i = m_thresholds.Count - 1; i >= 0; i--) {
				// Do we fit into this threshold?
				if(_score < m_thresholds[i].maxScore) {
					// Yes! Keep looping
					selectedIdx = i;
				} else {
					// No! This is the threshold previous to our selected one, store base score
					baseScore = m_thresholds[i].maxScore;
					break;
				}
			}

			// Compute score delta within the selected threshold
			// If we've reached the high end of the last threshold, delta will be just 1
			float delta = 1f;
			if(m_thresholds[selectedIdx].maxScore > _score) {
				delta = Mathf.InverseLerp(baseScore, m_thresholds[selectedIdx].maxScore, _score);
			}

			// Apply text color
			m_text.color = m_thresholds[selectedIdx].colorRange.Evaluate(delta);

			// Apply text font size 
			m_text.fontSize = (int)(m_baseFontSize * m_thresholds[selectedIdx].fontScaleRange.Lerp(delta));
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}