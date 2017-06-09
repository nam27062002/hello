// EggView.cs
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
public class EggView : MonoBehaviour {
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

	// Data - can be null
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

	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create an egg view by its sku.
	/// If the application is running, a new Egg data will be created as well and initialized to SHOWROOM state.
	/// </summary>
	/// <returns>The new egg view. <c>null</c> if the egg view couldn't be created.</returns>
	/// <param name="_eggSku">The sku of the egg in the EGGS definitions category.</param>
	public static EggView CreateFromSku(string _eggSku) {
		// Egg can't be created if definitions are not loaded
		Debug.Assert(ContentManager.ready, "Definitions not yet loaded!");

		// Find and validate definition
		DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _eggSku);

		// Use internal method
		EggView newEggView = CreateFromDef(eggDef);

		// If the application is running, create a new egg data and assign it to the new view
		if(newEggView != null && Application.isPlaying) {
			Egg newEgg = Egg.CreateFromDef(eggDef);
			newEgg.ChangeState(Egg.State.SHOWROOM);
			newEggView.eggData = newEgg;
		}

		// Return the newly created instance
		return newEggView;
	}

	/// <summary>
	/// Create an egg view for the given egg data.
	/// </summary>
	/// <returns>The new egg view. <c>null</c> if the egg view couldn't be created.</returns>
	/// <param name="_eggData">The egg data to be used to initialize this egg.</param>
	public static EggView CreateFromData(Egg _eggData) {
		// Ignore if data not valid
		if(_eggData == null) return null;

		// Use internal method
		EggView newEggView = CreateFromDef(_eggData.def);

		// Assign the given egg data to the newly created egg view
		if(newEggView != null) {
			newEggView.eggData = _eggData;
		}

		// Return the newly created instance
		return newEggView;
	}

	/// <summary>
	/// Create an egg view given an egg definition. Internal usage only.
	/// </summary>
	/// <returns>The new egg view. <c>null</c> if the egg view couldn't be created.</returns>
	/// <param name="_def">The egg definition.</param>
	private static EggView CreateFromDef(DefinitionNode _def) {
		// Def must be valid!
		if(_def == null) return null;

		// Create new egg view from the definition
		// Load the prefab for this egg as defined in the definition
		GameObject prefabObj = Resources.Load<GameObject>(Egg.PREFAB_PATH + _def.GetAsString("prefabPath"));
		Debug.Assert(prefabObj != null, "The prefab defined to egg " + _def.sku + " couldn't be found");

		// Create a new instance and obtain the egg view component
		GameObject newInstance = GameObject.Instantiate<GameObject>(prefabObj);
		EggView newEggView = newInstance.GetComponent<EggView>();

		// Return the newly created instance
		return newEggView;
	}

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
		// If we don't have valid data, simulate SHOWROOM state
		Egg.State state = Egg.State.SHOWROOM;
		if(m_eggData != null) {
			state = m_eggData.state;
		}

		// Enable/disable behaviours based on current egg's state
		m_openBehaviour.enabled = (state == Egg.State.OPENING);
		m_readyBehaviour.enabled = (state == Egg.State.READY);

		// Set animator's parameters
		m_animator.SetInteger("egg_state", (int)state);

		// Collect steps
		int step = Mathf.Clamp(m_openBehaviour.tapCount, 0, OpenEggBehaviour.TAPS_TO_OPEN);
		m_animator.SetInteger("collect_step", step);

		// Rarity
		if(m_eggData != null) {
			m_animator.SetInteger("rarity", (int)m_eggData.rewardData.rarity);
		} else {
			m_animator.SetInteger("rarity", (int)EggReward.Rarity.COMMON);
		}

		// Idle FX - disabled after tapping the egg
		if(m_idleFX != null) {
			/*
			bool hide = (state == Egg.State.OPENING && step > 0);
			hide |= state == Egg.State.COLLECTED;
			m_idleFX.SetActive(!hide);
			*/
			// This works for both eggs:
			bool show = false;
			switch(state) {
				case Egg.State.READY: {
					show = true;	// Show always
				} break;

				case Egg.State.OPENING: {
					show = (step <= 0);	// Show only while no tap has been done
				} break;

				case Egg.State.SHOWROOM: {
					// Only for premium eggs
					if(m_eggData != null && m_eggData.def.sku == Egg.SKU_PREMIUM_EGG) {
						show = true;
					}
				} break;

				default: {
					show = false;	// Hide for the rest of cases
				} break;
			}

			m_idleFX.SetActive(show);
		}

		// Animation intensity - reset to default if state is different than collected
		if(state != Egg.State.COLLECTED) {
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

