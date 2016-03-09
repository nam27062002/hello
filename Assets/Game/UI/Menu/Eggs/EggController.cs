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

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main control of a single egg prefab in the menu.
/// </summary>
[RequireComponent(typeof(IncubatorEggBehaviour))]
[RequireComponent(typeof(OpenEggBehaviour))]
[RequireComponent(typeof(ReadyEggBehaviour))]
public class EggController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Data
	private Egg m_eggData = null;
	public Egg eggData {
		get { return m_eggData; }
		set { m_eggData = value; }
	}

	// Egg behaviours
	private IncubatorEggBehaviour m_incubatorBehaviour = null;
	public IncubatorEggBehaviour incubatorBehaviour {
		get { return m_incubatorBehaviour; }
	}

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
		m_incubatorBehaviour = GetComponent<IncubatorEggBehaviour>();
		m_openBehaviour = GetComponent<OpenEggBehaviour>();
		m_readyBehaviour = GetComponent<ReadyEggBehaviour>();
		m_animator = GetComponentInChildren<Animator>();
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Subscribe to external events
		Messenger.AddListener<Egg, Egg.State, Egg.State>(GameEvents.EGG_STATE_CHANGED, OnEggStateChanged);

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
		m_incubatorBehaviour.enabled = (m_eggData.state == Egg.State.STORED);
		m_openBehaviour.enabled = (m_eggData.state == Egg.State.OPENING);
		m_readyBehaviour.enabled = (m_eggData.state == Egg.State.READY);

		// Update animator! - Luckily animator is self-managed
		m_animator.SetInteger("egg_state", (int)m_eggData.state);
		m_animator.SetTrigger("egg_state_changed");
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
			Refresh();
		}
	}
}

