﻿// CoinsFeedbackController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Launches a feedback effect and auto-destroys itself on finish.
/// </summary>
public class CoinsFeedbackController : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Range m_particleRange;		// Amount of particles based on reward amount
	[SerializeField] private Range m_rewardRange;		// Reward limits to compute the intensity of the effect
	[SerializeField] private string m_audio;		// Reward audio

	// Internal
	private ParticleSystem m_ps;
	private float m_maxParticlesToEmissionRateRatio;		// We will adjust the emission rate to the number of particles to be spawned so the total duration of the effect is the same regardless of how many particles we're spawning
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Use this for initialization
	/// </summary>
	private void Awake() {
		// Get required components
		m_ps = GetComponent<ParticleSystem>();
		DebugUtils.Assert(m_ps != null, "Required component!");

		// Store default max particles -> emission rate ratio
		// m_maxParticlesToEmissionRateRatio = m_ps.emission.rate/(float)m_ps.maxParticles;
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// Use late update since we want the animation to change the relative position 
	/// during the Update() call and then apply the offset.
	/// </summary>
	private void Update() {
		// Wait for particle system to end
		if(m_ps.isStopped && m_ps.particleCount <= 0) {
			// De-activate game object and return it to the pool
			gameObject.SetActive(false);
			PoolManager.ReturnInstance(this.gameObject);
		}
	}

	/// <summary>
	/// Called right before destroying the object.
	/// </summary>
	private void OnDestroy() {
		// Nothing to do for now
	}

	//------------------------------------------------------------------//
	// PUBLIC UTILS														//
	//------------------------------------------------------------------//
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
		//transform.SetPosY(transform.position.y + 100);

		// Edit particle system to adjust to the given amount
		float fDelta = Mathf.InverseLerp(m_rewardRange.min, m_rewardRange.max, (float)_iAmount);
		m_ps.maxParticles = (int)Mathf.Lerp((float)m_particleRange.min, (float)m_particleRange.max, fDelta);

		// var emision = m_ps.emission;
		// emision.rate = m_ps.maxParticles * m_maxParticlesToEmissionRateRatio;

		// Start particle system
		m_ps.Play();

        if (!string.IsNullOrEmpty(m_audio))
		    AudioController.Play(m_audio);
	}
}
