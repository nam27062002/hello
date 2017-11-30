﻿// ChestViewController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Class dedicated exclusively to control the visuals of a 3D chest, regardless of its functionality.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ChestViewController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PREFAB_PATH = "UI/Metagame/Chests/PF_ChestView";

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed references
	[SerializeField] private GameObject m_goldRewardView = null;
	[SerializeField] private GameObject m_gemsRewardView = null;
	[Space]
    [SerializeField] private GameObject m_glowFX = null;
	[SerializeField] private ParticleData m_openParticle = null;
	// [SerializeField] private ParticleData m_dustParticle = null;

	// Exposed setup
	[Space]
	[SerializeField] private UnityEvent OnChestOpenEvent = new UnityEvent();
	public UnityEvent OnChestOpen {
		get { return OnChestOpenEvent; }
	}

	[SerializeField] private UnityEvent OnChestAnimLandedEvent = new UnityEvent();
	public UnityEvent OnChestAnimLanded {
		get { return OnChestAnimLandedEvent; }
	}

	// Internal
	private Animator m_animator = null;
	private GameObject[] m_rewardViews = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get animator ref
		m_animator = GetComponent<Animator>();

		// Start with all particles stopped
		ToggleFX(m_glowFX, false);

		// Get references (from FBX names)
		// Respect enum name
		m_rewardViews = new GameObject[] {
			m_goldRewardView,
			m_gemsRewardView
		};

		m_openParticle.CreatePool();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Turns the glow effect on/off.
	/// </summary>
	/// <param name="_show">Whether to show it or not.</param>
	public void ShowGlowFX(bool _show) {
		ToggleFX(m_glowFX, _show);
	}

	/// <summary>
	/// Launch the open animation.
	/// </summary>
	/// <param name="_reward">What reward to show after the open animation.</param>
	/// <param name="_instant">Instantly pose the chest to the open position.</param>
	public void Open(Chest.RewardType _reward, bool _instant) {
		// Launch animation
		if(_instant) {
			m_animator.SetTrigger( GameConstants.Animator.OPEN_POSE );
		} else {
			// Particle effect will be launched with the animation event
			m_animator.SetTrigger( GameConstants.Animator.OPEN);
		}

		// Show the right reward
		if(m_rewardViews != null) {
			for(int i = 0; i < m_rewardViews.Length; i++) {
				m_rewardViews[i].SetActive(i == (int)_reward);
			}
		}
	}

	/// <summary>
	/// Launch the close animation.
	/// </summary>
	public void Close() {
		// Launch close animation
		m_animator.SetTrigger( GameConstants.Animator.CLOSE );
	}

	/// <summary>
	/// Triggers the results animation.
	/// </summary>
	public void ResultsAnim() {
		// Stop all particles
		ToggleFX(m_glowFX, false);

		// Launch animation
		m_animator.SetTrigger( GameConstants.Animator.RESULTS_IN );
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Aux method to quickly toggle a particle system on/off.
	/// </summary>
	/// <param name="_fx">The system to be toggled.</param>
	/// <param name="_active">Whether to turn it on or off.</param>
	private void ToggleFX(GameObject _fx, bool _active) {
		// Ignore if given FX is not valid
		if(_fx == null) return;
		_fx.SetActive( _active );

		/*
		// Activate?
		if(_active) {
			_fx.gameObject.SetActive(true);
			_fx.Play();

            // If it has a CustomParticlesCulling assigned then it checks if it's invisible, if so then it has to pause the effect
            if (CustomParticlesCulling != null && !CustomParticlesCulling.IsVisible())
            {
                _fx.Pause();
            }
		} else {
			_fx.Stop();
			_fx.gameObject.SetActive(false);
		}
		*/
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Lid open animation event.
	/// </summary>
	public void OnLidOpen() {
		// Launch particle system
		// ToggleFX(m_openFX, true);
		GameObject go = m_openParticle.Spawn(this.transform, m_openParticle.offset);
		if (go != null) {
			go.SetLayerRecursively(this.gameObject.layer);
			go.transform.rotation = transform.rotation;
		}
		
		ToggleFX(m_glowFX, false);

		// Notify delegates
		OnChestOpenEvent.Invoke();
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnChestLanded() {
		// [AOC] TODO!! Play some SFX

		// Play some VFX
		/*
		if ( m_dustParticle.IsValid() )
		{
			GameObject go = ParticleManager.Spawn(m_dustParticle, transform.position + m_dustParticle.offset );
			go.transform.rotation = transform.rotation;
		}
		*/
		// Notify delegates
		OnChestAnimLandedEvent.Invoke();
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnCameraShake() {
		Messenger.Broadcast<float, float>(GameEvents.CAMERA_SHAKE, 0.1f, 0.5f);
	}
}