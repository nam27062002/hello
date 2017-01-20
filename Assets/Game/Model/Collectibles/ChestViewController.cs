// ChestViewController.cs
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
	[SerializeField] private ParticleSystem m_glowFX = null;
	[SerializeField] private ParticleSystem m_openFX = null;
	[SerializeField] private ParticleSystem m_dustFX = null;

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
		ToggleFX(m_openFX, false);
		ToggleFX(m_glowFX, false);
		ToggleFX(m_dustFX, false);

		// Get references (from FBX names)
		// Respect enum name
		m_rewardViews = new GameObject[] {
			this.FindObjectRecursive("Gold"),
			this.FindObjectRecursive("Gems")
		};
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
	public void Open(Chest.RewardType _reward) {
		// Launch animation - particle effect will be launched with the animation event
		m_animator.SetTrigger("open");

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
		// Stop particles
		ToggleFX(m_openFX, false);
		ToggleFX(m_dustFX, false);

		// Launch close animation
		m_animator.SetTrigger("close");
	}

	/// <summary>
	/// Triggers the results animation.
	/// </summary>
	public void ResultsAnim() {
		// Stop all particles
		ToggleFX(m_openFX, false);
		ToggleFX(m_glowFX, false);
		ToggleFX(m_dustFX, false);

		// Launch animation
		m_animator.SetTrigger("results_in");
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Aux method to quickly toggle a particle system on/off.
	/// </summary>
	/// <param name="_fx">The system to be toggled.</param>
	/// <param name="_active">Whether to turn it on or off.</param>
	private void ToggleFX(ParticleSystem _fx, bool _active) {
		// Ignore if given FX is not valid
		if(_fx == null) return;

		// Activate?
		if(_active) {
			_fx.gameObject.SetActive(true);
			_fx.Play();
		} else {
			_fx.Stop();
			_fx.gameObject.SetActive(false);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Lid open animation event.
	/// </summary>
	public void OnLidOpen() {
		// Launch particle system
		ToggleFX(m_openFX, true);
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
		ToggleFX(m_dustFX, true);

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