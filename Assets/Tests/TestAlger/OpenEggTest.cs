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

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[List("egg_standard", "egg_premium", "egg_golden")]
	[SerializeField] private string m_testEggSku = "egg_standard";
	[Space]
	[SerializeField] private Transform m_eggAnchor = null;
	[SerializeField] private ParticleSystem m_explosionFX = null;
	[SerializeField] private GodRaysFXFast m_godRaysFX = null;
	[SerializeField] private Transform m_tapFXPool = null;
	[Tooltip("One per rarity, matching order")]
	[SerializeField] private ParticleSystem[] m_tapFX = new ParticleSystem[(int)EggReward.Rarity.COUNT];
	[Tooltip("One per rarity, matching order")]
	[SerializeField] private ParticleSystem[] m_openFX = new ParticleSystem[(int)EggReward.Rarity.COUNT];

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
		ContentManager.InitContent(true);

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
		Messenger.AddListener<EggView, int>(GameEvents.EGG_TAP, OnEggTap);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Clean up
		Clear();

		// Unsubscribe from external events
		Messenger.RemoveListener<EggView, int>(GameEvents.EGG_TAP, OnEggTap);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Clean up
		Clear();
	}

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	private void OnValidate() {
		// Make sure the rarity array has exactly the same length as rarities in the game.
		m_openFX.Resize((int)EggReward.Rarity.COUNT);
		m_tapFX.Resize((int)EggReward.Rarity.COUNT);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Clear the whole 3D scene.
	/// </summary>
	public void Clear() {
		// Return tap FX to the pool
		for (int i = 0; i < m_tapFX.Length; i++) {
			if (m_tapFX[i] != null) {
				m_tapFX[i].transform.SetParent(m_tapFXPool);
				m_tapFX[i].Stop(true);
				m_tapFX[i].gameObject.SetActive(false);
			}
		}

		// Destroy egg view
		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;

			// Unsubscribe from external events.
			Messenger.RemoveListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
		}

		// Stop all FX
		if (m_godRaysFX != null) {
			m_godRaysFX.StopFX();
		}

		for (int i = 0; i < m_openFX.Length; i++) {
			if (m_openFX[i] != null) {
				m_openFX[i].Stop(true);
			}
		}

		if (m_explosionFX != null) {
			m_explosionFX.Stop(true);
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

			// [AOC] TODO!! Some awesome FX!!
			m_eggView.transform.DOScale(0f, 0.5f).From().SetEase(Ease.OutElastic);
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
		Messenger.AddListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);

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
		ParticleSystem tapFX = m_tapFX[(int)_eggReward.rarity];
		if(tapFX != null) {
			tapFX.transform.SetParentAndReset(m_eggView.anchorFX);
			tapFX.gameObject.SetActive(false);
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
		ParticleSystem openFX = m_openFX[(int)m_currentReward.rarity];
		if(openFX != null) {
			openFX.Clear();
			openFX.Play(true);
		}

		// Explosion FX
		// Match material with the egg shell!
		// [AOC] No longer needed!
		/*
		if(m_explosionFX != null) {
			// Find egg shell material
			Renderer[] renderers = m_eggView.GetComponentsInChildren<Renderer>();
			for(int i = 0; i < renderers.Length; i++) {
				for(int j = 0; j < renderers[i].materials.Length; j++) {
					if(renderers[i].materials[j].name.Contains("Scales")) {
						// Use this one!
						m_explosionFX.GetComponent<ParticleSystemRenderer>().material = renderers[i].materials[j];
						break;
					}
				}
			}

			m_explosionFX.Play();
		}
		*/

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
			// Activate FX
			ParticleSystem tapFX = m_tapFX[(int)m_currentReward.rarity];
			tapFX.gameObject.SetActive(true);
			tapFX.Stop(true);
			tapFX.Clear();
			tapFX.Play(true);
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
		OpenReward();
	}
}