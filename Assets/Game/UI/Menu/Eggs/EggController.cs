// MenuEgg.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main control of a single egg prefab in the menu.
/// </summary>
public class EggController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Transform m_anchorFX = null;
	public Transform anchorFX {
		get { return m_anchorFX; }
	}

	[SerializeField] private GameObject m_idleFX = null;

	// Data
	private Egg m_eggData = null;
	public Egg eggData {
		get { return m_eggData; }
		set { m_eggData = value; }
	}

	// Egg behaviours
	private OpenEggBehaviour m_openBehaviour = null;
	public OpenEggBehaviour openBehaviour {
		get { return m_openBehaviour; }
	}

	private ReadyEggBehaviour m_readyBehaviour = null;
	public ReadyEggBehaviour readyBehaviour {
		get { return m_readyBehaviour; }
	}

	// Internal references
	private Animator m_animator = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_openBehaviour = GetComponent<OpenEggBehaviour>();
		m_readyBehaviour = GetComponent<ReadyEggBehaviour>();
		m_animator = GetComponentInChildren<Animator>();

		// Subscribe to external events
		Messenger.AddListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Make sure we're updated
		Refresh();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure we're updated
		Refresh();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe to external events
		Messenger.RemoveListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Refresh this object based on egg's current state.
	/// </summary>
	private void Refresh() {
		// Valid data required!
		if(m_eggData == null) return;

		// Enable/disable behaviours based on current egg's state
		m_openBehaviour.enabled = (m_eggData.state == Egg.State.OPENING);
		m_readyBehaviour.enabled = (m_eggData.state == Egg.State.READY);

		// Set animator's parameters
		m_animator.SetInteger("egg_state", (int)m_eggData.state);

		// Collect steps
		int step = Mathf.Clamp(m_openBehaviour.tapCount, 0, OpenEggBehaviour.TAPS_TO_OPEN);
		m_animator.SetInteger("collect_step", step);

		// Rarity
		m_animator.SetInteger("rarity", (int)m_eggData.rewardData.rarity);

		// Idle FX - disabled after tapping the egg
		if(m_idleFX != null) {
			bool hide = (m_eggData.state == Egg.State.OPENING && step > 0);
			hide |= m_eggData.state == Egg.State.COLLECTED;
			m_idleFX.SetActive(!hide);
		}

		// Animation intensity - reset to default if state is different than collected
		if(m_eggData.state != Egg.State.COLLECTED) {
			m_animator.SetFloat("intensity", 1f);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// An egg's state has changed.
	/// </summary>
	/// <param name="_egg">The egg whose state has changed.</param>
	/// <param name="_from">Previous state.</param>
	/// <param name="_to">New state.</param>
	private void OnEggStateChanged(Egg _egg, Egg.State _from, Egg.State _to) {
		// If it's this egg, refresh
		if(_egg == m_eggData) {
			// Update animator parameters
			Refresh();

			// If going to "collected", launch extra FX
			if(_to == Egg.State.COLLECTED) {
				// Increase intensity over time
				// Super-easy to do with DOTween library!
				DOVirtual.Float(1f, UIConstants.openEggSpinIntensity, 1.75f, 
					(float _value) => { m_animator.SetFloat("intensity", _value); }
				)
				.SetEase(UIConstants.openEggSpinEase)
				.SetDelay(0f)
				.OnComplete(
					() => { m_animator.SetFloat("intensity", 1f); }
				);
			}
		}
	}

	/// <summary>
	/// The egg has been tapped.
	/// To be called by child behaviours.
	/// </summary>
	/// <param name="_tapCount">Total tap count (including this one).</param>
	public void OnTap(int _tapCount) {
		// Update animator parameters
		Refresh();
	}
}

