// CoinsFeedbackController.cs
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

	// Internal
	private ParticleSystem m_ps;
	
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
	}
	
	/// <summary>
	/// Update is called once per frame.
	/// Use late update since we want the animation to change the relative position 
	/// during the Update() call and then apply the offset.
	/// </summary>
	private void Update() {
		// Wait for particle system to end
		if(m_ps.isStopped) {
			// De-activate game object
			gameObject.SetActive(false);
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
		m_ps.emissionRate = m_ps.maxParticles * 4f;	// [AOC] Magic numbers, the ratio maxParticles/emissionRate could be stored in the Awake() call :P

		// Start particle system
		m_ps.Play();
	}
}
