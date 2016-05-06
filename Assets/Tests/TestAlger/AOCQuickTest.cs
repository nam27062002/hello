// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	public int initialLevel = 0;
	public int finalLevel = 3;

	[Space]
	[Range(0f, 1f)] public float initialProgress = 0f;
	[Range(0f, 1f)] public float finalProgress = 0.75f;

	[Space]
	public int numLevels = 10;
	public int maxLevel { get { return numLevels - 1; }}

	[Space]
	public float speed = 1f;
	public Ease ease = Ease.OutExpo;

	[Space]
	public Slider m_levelBar = null;
	public Localizer m_levelText = null;

	// Internal
	private int m_levelAnimCount = 0;
	private Tweener m_xpBarTween;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		// Check values!
		finalLevel = Mathf.Min(finalLevel, maxLevel);
		initialLevel = Mathf.Min(initialLevel, finalLevel);
		if(finalLevel == initialLevel) initialProgress = Mathf.Min(initialProgress, finalProgress);

		// Do it
		m_levelBar.minValue = 0f;
		m_levelBar.maxValue = 1f;
		m_levelBar.value = initialProgress;
		m_levelAnimCount = initialLevel;
		m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_levelAnimCount + 1));
		LaunchXPBarAnim();
	}

	/// <summary>
	/// Launches the XP anim.
	/// </summary>
	private void LaunchXPBarAnim() {
		// Compute target level and delta
		bool isTargetLevel = (m_levelAnimCount == finalLevel);
		float targetDelta = isTargetLevel ? finalProgress : 1f;	// Full bar if not target level

		// Create animation
		m_xpBarTween = DOTween.To(
			// Getter function
			() => { 
				return m_levelBar.value; 
			}, 

			// Setter function
			(_newValue) => {
				m_levelBar.value = _newValue;
			},

			// Value and duration
			targetDelta, speed
		)

			// Other setup parameters
			.SetSpeedBased(true)
			.SetEase(ease)

			// What to do once the anim has finished?
			.OnComplete(
				() => {
					// Was it the target level? We're done!
					if(isTargetLevel) return;

					// Not the target level, increase level counter and restart animation!
					m_levelAnimCount++;

					// Set text and animate
					m_levelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_levelAnimCount + 1));
					m_levelText.transform.DOScale(1.5f, 0.15f).SetLoops(2, LoopType.Yoyo);

					// Put bar to the start
					m_levelBar.value = 0f;

					// Lose tween reference and create a new one
					m_xpBarTween = null;
					LaunchXPBarAnim();
				}
			);
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		
	}
}