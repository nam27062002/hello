// OpenEggTest.cs
// Hungry Dragon
// 
// Created by Marc Saña on 17/07/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the 3D scene of the Open Egg screen.
/// </summary>
public class OpenEggTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	public class RaritySetup {
		public ParticleSystem tapFX = null;
		public ParticleSystem tapFXStatic = null;
		public ParticleSystem openFX = null;

		public void Clear() {
			if(tapFX != null) {
				tapFX.Stop(true);
				tapFX.gameObject.SetActive(false);
			}
			if(tapFXStatic != null) {
				tapFXStatic.Stop(true);
				tapFXStatic.gameObject.SetActive(false);
			}
			if(openFX != null) {
				openFX.gameObject.SetActive(false);
			}
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[List("egg_standard", "egg_premium", "egg_golden")]
	[SerializeField] private string m_testEggSku = "egg_standard";
	[SerializeField] private Metagame.Reward.Rarity m_testRewardRarity = Metagame.Reward.Rarity.COMMON;
	[Space]
	[SerializeField] private Transform m_eggAnchor = null;
	[SerializeField] private Transform m_tapFXPool = null;
	[SerializeField] private RaritySetup[] m_rarityFXSetup = new RaritySetup[(int)Metagame.Reward.Rarity.COUNT];
	[Space]
	[SerializeField] private float m_newEggDelay = 1f;

	private EggView m_eggView = null;
	public EggView eggView {
		get { return m_eggView; }
	}

	public Egg eggData {
		get { return (m_eggView == null) ? null : m_eggView.eggData; }
	}

	private Metagame.RewardEgg m_currentReward;
	public Metagame.RewardEgg currentReward {
		get { return m_currentReward; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize some required managers
		ContentManager.InitContent(true, false);

		// Don't show anything
		Clear();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		OpenReward();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<EggView, int>(MessengerEvents.EGG_TAP, OnEggTap);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Clean up
		Clear();

		// Unsubscribe from external events
		Messenger.RemoveListener<EggView, int>(MessengerEvents.EGG_TAP, OnEggTap);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Clean up
		Clear();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear the whole 3D scene.
	/// </summary>
	public void Clear() {
		// Return tap FX to the pool (before deleting the egg view!!)
		for(int i = 0; i < m_rarityFXSetup.Length; ++i) {
			if(m_rarityFXSetup[i].tapFX != null) {
				m_rarityFXSetup[i].tapFX.transform.SetParent(m_tapFXPool);
			}
			m_rarityFXSetup[i].Clear();
		}

		// Destroy egg view
		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;

			// Unsubscribe from external events.
			Messenger.RemoveListener<Egg>(MessengerEvents.EGG_OPENED, OnEggCollected);
		}
	}

	/// <summary>
	/// Start the open reward flow with the top reward on the stack.
	/// </summary>
	public void OpenReward() {
		// Create a new egg reward
		m_currentReward = new Metagame.RewardEgg(m_testEggSku, "", false);

		// Set test mode
		m_currentReward.egg.testMode = true;
		m_currentReward.rarity = m_testRewardRarity;

		// Change egg state
		m_currentReward.egg.ChangeState(Egg.State.OPENING);

		// Initialize the view
		InitEggView(m_currentReward);

		// Launch intro!
		// Ignore if we don't have a valid egg view
		if(m_eggView != null) {
			// Assume we can do it (no checks)
			// Activate egg
			m_eggView.gameObject.SetActive(true);
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the egg view with the given egg reward data.
	/// </summary>
	/// <param name="_egg">The egg to be opened.</param>
	private void InitEggView(Metagame.RewardEgg _eggReward) {
		// Clear any active stuff
		Clear();

		// Be attentive to the egg collect event, which is managed by the egg view
		Messenger.AddListener<Egg>(MessengerEvents.EGG_OPENED, OnEggCollected);

		// Create a new instance of the egg prefab
		m_eggView = EggView.CreateFromData(_eggReward.egg);

		// Attach it to the 3d scene's anchor point
		// Make sure anchor is active!
		m_eggAnchor.gameObject.SetActive(true);
		m_eggView.transform.SetParent(m_eggAnchor, false);
		m_eggView.transform.position = m_eggAnchor.position;

		// Launch intro as soon as possible (wait for the camera to stop moving)
		m_eggView.gameObject.SetActive(false);

		// [AOC] Hacky!! Disable particle FX (Premium Egg Idle)
		ParticleSystem[] particleFX = m_eggView.GetComponentsInChildren<ParticleSystem>();
		for(int i = 0; i < particleFX.Length; i++) {
			particleFX[i].gameObject.SetActive(false);
		}

		// Attach tap FX to the egg's view (but don't activate it just yet)
		ParticleSystem tapFX = m_rarityFXSetup[(int)_eggReward.rarity].tapFX;
		if(tapFX != null) {
			tapFX.transform.SetParentAndReset(m_eggView.anchorFX);
			tapFX.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// Restarts the given FX tuning it with current reward rarity.
	/// </summary>
	/// <param name="_fx">FX to be triggered. Can be <c>null</c>.</param>
	private void TriggerFX(ParticleSystem _fx) {
		// Check validity
		if(_fx == null) return;

		// Make sure object is active
		_fx.gameObject.SetActive(true);

		// Reset particle system
		_fx.Stop(true);
		_fx.Clear();
		_fx.Play(true);

		// If the FX has an animator assigned, setup and trigger animation!
		Animator anim = _fx.GetComponent<Animator>();
		if(anim != null) {
			anim.SetInteger( GameConstants.Animator.RARITY , (int)m_currentReward.rarity);
			anim.SetTrigger( GameConstants.Animator.START);
		}
	}

	//------------------------------------------------------------------//
	// ANIMATIONS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launch the egg open animation.
	/// </summary>
	private void LaunchEggExplosionAnim() {
		// Ignore if we don't have a valid egg view
		if(m_eggView == null) return;

		// Hide egg
		m_eggView.gameObject.SetActive(false);

		// Trigger the proper FX based on reward rarity
		TriggerFX(m_rarityFXSetup[(int)m_currentReward.rarity].openFX);

		// Program reward animation
		UbiBCN.CoroutineManager.DelayedCall(OnEggExplosionAnimFinished, 0.35f, false);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// An opening egg has been tapped.
	/// </summary>
	/// <param name="_egg">The egg that has been tapped.</param>
	/// <param name="_tapCount">Tap count.</param>
	private void OnEggTap(EggView _egg, int _tapCount) {
		// Show the right particle effect based on rarity!
		if(_tapCount == 1 && _egg == m_eggView) {
			// Activate tap FX, both static and dynamic
			TriggerFX(m_rarityFXSetup[(int)m_currentReward.rarity].tapFX);
			TriggerFX(m_rarityFXSetup[(int)m_currentReward.rarity].tapFXStatic);
		}
	}

	/// <summary>
	/// An egg has been opened and its reward collected.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggCollected(Egg _egg) {
		// Must have a valid egg
		if(eggData == null) return;

		// If it matches our curent egg, launch its animation!
		if(_egg == eggData) {
			// Launch animation!
			// Delay to sync with the egg anim
			UbiBCN.CoroutineManager.DelayedCall(LaunchEggExplosionAnim, UIConstants.openEggExplosionDuration, false);
		}
	}

	/// <summary>
	/// The egg open animation has been finished.
	/// </summary>
	private void OnEggExplosionAnimFinished() {
		// Open the reward inside the egg, which has been pushed into the stack by the egg view
		UbiBCN.CoroutineManager.DelayedCall(OpenReward, m_newEggDelay);
	}
}