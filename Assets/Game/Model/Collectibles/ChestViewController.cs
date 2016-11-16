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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ParticleSystem m_glowFX = null;
	[SerializeField] private ParticleSystem m_openFX = null;

	// Exposed setup
	[Space]
	[SerializeField] private UnityEvent OnChestOpenEvent = new UnityEvent();
	public UnityEvent OnChestOpen {
		get { return OnChestOpenEvent; }
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
		ShowGlowFX(false);
		if(m_openFX != null) {
			m_openFX.Stop();
			m_openFX.gameObject.SetActive(false);
		}

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
		if(m_glowFX != null) {
			if(_show) {
				m_glowFX.gameObject.SetActive(true);
				m_glowFX.Play();
			} else {
				m_glowFX.Stop();
				//m_glowFX.Clear();
				m_glowFX.gameObject.SetActive(false);
			}
		}
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
		if(m_openFX != null) {
			m_openFX.Stop();
			m_openFX.gameObject.SetActive(false);
		}

		// Launch close animation
		m_animator.SetTrigger("close");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Lid open animation event.
	/// </summary>
	public void OnLidOpen() {
		// Launch particle system
		if(m_openFX != null) {
			m_openFX.gameObject.SetActive(true);
			m_openFX.Play();
		}

		// Notify delegates
		OnChestOpenEvent.Invoke();
	}
}