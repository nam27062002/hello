// CoinsFeedbackController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/04/2015.
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
public class CoinsFXController : MonoBehaviour {
	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	[SerializeField] private Range particleRange;	// Amount of particles based on reward amount
	[SerializeField] private Range rewardRange;		// Reward limits to compute the intensity of the effect
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private ParticleSystem mPS;
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Awake() {
		// Get required components
		mPS = GetComponent<ParticleSystem>();

		DebugUtils.Assert(mPS != null, "Required component!");
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// Use late update since we want the animation to change the relative position 
	/// during the Update() call and then apply the offset.
	/// </summary>
	void Update() {
		// Wait for particle system to end
		if(mPS.isStopped) {
			// De-activate game object
			gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	void OnDestroy() {
		// Nothing to do for now
	}
	#endregion

	#region PUBLIC UTILS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Launches the feedback animation.
	/// </summary>
	/// <param name="_startPos">World position to spawn the feedback.</param>
	/// <param name="_iAmount">The amount of coins rewarded. Will be used to compute effect's intensity.</param>
	public void Launch(Vector3 _startPos, long _iAmount) {
		// Make sure we're active
		gameObject.SetActive(true);

		// Put ourselves in position
		// [AOC] Move slightly above ([AOC] TODO!! Find a way to define this offset from the editor)
		transform.position = _startPos;
		transform.SetPosY(transform.position.y + 100);

		// Edit particle system to adjust to the given amount
		float fDelta = Mathf.InverseLerp((float)rewardRange.min, (float)rewardRange.max, (float)_iAmount);
		mPS.maxParticles = (int)Mathf.Lerp((float)particleRange.min, (float)particleRange.max, fDelta);
		mPS.emissionRate = mPS.maxParticles * 4f;	// [AOC] Magic numbers, the ratio maxParticles/emissionRate could be stored in the Awake() call :P

		// Restart particle system
		mPS.Play();
	}
	#endregion
}
#endregion