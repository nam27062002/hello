// ScoreFeedback.cs
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
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specific controller for a score feedback.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
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
	[Comment("Thresholds should be scaled for the first dragon.\nFurther dragons will apply the \"scoreTextThresholdMultiplier\" scale factor from the DragonDefinitions content table.")]
	[SerializeField] private List<ScoreThreshold> m_thresholds = new List<ScoreThreshold>();
	public List<ScoreThreshold> thresholds {
		get { return m_thresholds; }
	}

	// References
	private TextMeshProUGUI m_text = null;

	// Internal
	private float m_baseFontSize = 0f;
	private StringBuilder m_sbuilder = new StringBuilder();
	private float m_dragonScaleFactor = 1f;	// Cached value of the "scoreTextThresholdMultiplier" property from the current dragon definition
	private bool m_dragonScaleCached = false;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get references
		m_text = GetComponent<TextMeshProUGUI>();

		// Init internal vars
		m_baseFontSize = m_text.fontSize;

		// Make sure score feedbacks are properly sorted
		m_thresholds.Sort((x, y) => {
			return x.maxScore.CompareTo(y.maxScore);
		});

		m_dragonScaleFactor = 1f;
		m_dragonScaleCached = false;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Set the score text and format the feedback accordingly
	/// </summary>
	/// <param name="_score">Score amount to be displayed.</param>
	public void SetScore(int _score) {
		if (!m_dragonScaleCached) {
			if (DragonManager.CurrentDragon != null) {
				// Cache scale factor for current dragon to avoid a constant acess to the dragon definition and parsing of the value
				m_dragonScaleFactor = DragonManager.CurrentDragon.def.GetAsFloat("scoreTextThresholdMultiplier", 1f);
				m_dragonScaleCached = true;
			}
		}

		// Using a string builder for optimal performance/memory usage
		// Add the '+' symbol when positive, negative values already are formatted with the '-' symbol, 0 has no symbol whatsoever
		m_sbuilder.Length = 0;	// [AOC] No Clear() in .Net 3.5 yet -_-
		if(_score > 0) {
			m_sbuilder.Append("+");
		}
		m_sbuilder.Append(StringUtils.FormatNumber(_score));

		// Apply to text
		m_text.text = m_sbuilder.ToString();

		// Scale score value according to current dragon
		_score = (int)(_score/m_dragonScaleFactor);

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