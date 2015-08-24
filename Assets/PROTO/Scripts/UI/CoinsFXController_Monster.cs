// CoinsFXController_Monster.cs
// Monster
// 
// Created by Alger Ortín Castellví on 02/06/2015.
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
public class CoinsFXController_Monster : MonoBehaviour {
	#region EXPOSED MEMBERS --------------------------------------------------------------------------------------------
	[SerializeField] private Range particleRange;	// Amount of particles based on reward amount
	[SerializeField] private Range rewardRange;		// Reward limits to compute the intensity of the effect
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------
	private ParticleSystem mPS;
	private float mDefaultEmissionRateMaxParticlesRatio;
	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Awake() {
		// Get required components
		mPS = GetComponent<ParticleSystem>();

		DebugUtils.Assert(mPS != null, "Required component!");

		// Store emission rate/max particles ratio to keep emission rate constant
		mDefaultEmissionRateMaxParticlesRatio = mPS.emissionRate/mPS.maxParticles;
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
	public void Launch(Vector3 _startPos, double _amount) {
		// Make sure we're active
		gameObject.SetActive(true);

		// Put ourselves in position
		transform.position = _startPos;

		// Edit particle system to adjust to the given amount
		float fDelta = Mathf.InverseLerp(rewardRange.min, rewardRange.max, (float)_amount);
		mPS.maxParticles = (int)Mathf.Lerp(particleRange.min, particleRange.max, fDelta);
		mPS.emissionRate = mPS.maxParticles * mDefaultEmissionRateMaxParticlesRatio;	// [AOC] Keep the emission rate proportional to the amount of particles

		// Restart particle system
		mPS.Play();
	}
	#endregion
}
#endregion