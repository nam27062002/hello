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
	[SerializeField] private GameObject m_goldRewardView = null;
	[SerializeField] private GameObject m_gemsRewardView = null;
	[Space]
	[SerializeField] private ViewParticleSpawner m_glowFX = null;
	[SerializeField] private ViewParticleSpawner m_goldOpenFX = null;
	[SerializeField] private ViewParticleSpawner m_gemsOpenFX = null;

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
	private Chest.RewardType m_rewardType = Chest.RewardType.SC;

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
		ToggleFX(m_goldOpenFX, false);
		ToggleFX(m_gemsOpenFX, false);

		// Get references (from FBX names)
		// Respect enum name
		m_rewardViews = new GameObject[] {
			m_goldRewardView,
			m_gemsRewardView
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

		// Stop reward FX - target reward FX will be triggered synched with the animation (OnLidOpen event)
		ToggleFX(m_goldOpenFX, false);
		ToggleFX(m_gemsOpenFX, false);

		// Store reward type to spawn the right FX once the lid is open
		m_rewardType = _reward;
	}

	/// <summary>
	/// Launch the close animation.
	/// </summary>
	public void Close() {
		// Launch close animation
		m_animator.SetTrigger( GameConstants.Animator.CLOSE );

		// Stop reward FX
		ToggleFX(m_goldOpenFX, false);
		ToggleFX(m_gemsOpenFX, false);
	}

	/// <summary>
	/// Triggers the results animation.
	/// </summary>
	public void ResultsAnim() {
		// Stop all particles
		ToggleFX(m_glowFX, false);
		ToggleFX(m_goldOpenFX, false);
		ToggleFX(m_gemsOpenFX, false);

		// Launch animation
		m_animator.SetTrigger( GameConstants.Animator.RESULTS_IN );
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Enable/Disable FX, making sure it's a valid reference.
	/// </summary>
	/// <param name="_fx">The FX to be toggled.</param>
	/// <param name="_toggle">Whether to enable or disable it.</param>
	private void ToggleFX(ViewParticleSpawner _fx, bool _toggle) {
		if(_fx == null) return;
		_fx.gameObject.SetActive(_toggle);
	}

	/// <summary>
	/// Get the target Open FX Data based on reward type.
	/// </summary>
	/// <returns>The target open FX. <c>null</c> if none assigned to the given reward type.</returns>
	/// <param name="_rewardType">Reward type whose Open FX we want.</param>
	private ViewParticleSpawner GetOpenFX(Chest.RewardType _rewardType) {
		switch(_rewardType) {
			case Chest.RewardType.SC: return m_goldOpenFX;
			case Chest.RewardType.PC: return m_gemsOpenFX;
		}
		return null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Lid open animation event.
	/// </summary>
	public void OnLidOpen() {
		// Launch particle system matching the current reward type
		ViewParticleSpawner targetFX = GetOpenFX(m_rewardType);
		if(targetFX != null) {
			ToggleFX(targetFX, true);
			Debug.Log("<color=lime>Spawning " + targetFX.name + "</color>");
			/*if(m_openFXInstance != null) {
				m_openFXInstance.SetLayerRecursively(this.gameObject.layer);
				m_openFXInstance.transform.rotation = transform.rotation;
			}*/
		}

		// Turn off glow FX
		ToggleFX(m_glowFX, false);

		// Notify delegates
		OnChestOpenEvent.Invoke();
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnChestLanded() {
		// Notify delegates
		OnChestAnimLandedEvent.Invoke();
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnCameraShake() {
		Messenger.Broadcast<float, float>(MessengerEvents.CAMERA_SHAKE, 0.1f, 0.5f);
	}
}