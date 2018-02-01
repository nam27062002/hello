// LockViewController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to control lock icon animations and state.
/// </summary>
public class LockViewController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PREFAB_PATH = "UI/Metagame/PF_Lock";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Animator m_animator = null;
	[Space]
	[SerializeField] private string m_bounceSFX = "hd_padlock";
	[SerializeField] private ViewParticleSpawner m_bounceVFX = null;
	[Space]
	[SerializeField] private string m_unlockSFX = "";
	[SerializeField] private ViewParticleSpawner m_unlockVFX = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Trigger the bounce animation.
	/// </summary>
	public void LaunchBounceAnim() {
		// Trigger the animation
		m_animator.SetTrigger(GameConstants.Animator.BOUNCE);

		// Trigger SFX
		if(!string.IsNullOrEmpty(m_bounceSFX)) {
			AudioController.Play(m_bounceSFX);
		}

		// Trigger VFX
		if(m_bounceVFX != null) {
			m_bounceVFX.Spawn();
		}
	}

	/// <summary>
	/// Trigger the unlock animation.
	/// </summary>
	public void LaunchUnlockAnim() {
		// Trigger the animation
		m_animator.SetTrigger(GameConstants.Animator.UNLOCK);

		// Trigger SFX
		if(!string.IsNullOrEmpty(m_unlockSFX)) {
			AudioController.Play(m_unlockSFX);
		}

		// Trigger VFX
		if(m_unlockVFX != null) {
			m_unlockVFX.Spawn();
		}
	}

	/// <summary>
	/// Stops any active anim.
	/// </summary>
	public void StopAllAnims() {
		// Trigger the animation
		m_animator.SetTrigger(GameConstants.Animator.IDLE);

		// Stop any active VFX
		if(m_bounceVFX != null) m_bounceVFX.Stop();
		if(m_unlockVFX != null) m_unlockVFX.Stop();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}