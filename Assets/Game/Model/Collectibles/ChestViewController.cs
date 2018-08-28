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

	[System.Serializable]
	public class RewardSetup {
		[HideEnumValues(false, true)]
		public Chest.RewardType type = Chest.RewardType.SC;
		public GameObject view = null;
		public ViewParticleSpawner openFX = null;
		public ViewParticleSpawner openLoopFX = null;
	}

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // VFX
	[SerializeField] private ViewParticleSpawner m_glowFX = null;
	[SerializeField] private RewardSetup[] m_rewardSetups = new RewardSetup[(int)Chest.RewardType.COUNT];

	// SFX
	[Space]
	[SerializeField] private string m_openSFX = "";
	public string openSFX {
		get { return m_openSFX; }
		set { m_openSFX = value; }
	}

	[SerializeField] private string m_closeSFX = "";
	public string closeSFX {
		get { return m_closeSFX; }
		set { m_closeSFX = value; }
	}

	[SerializeField] private string m_resultsSFX = "";
	public string resultsSFX {
		get { return m_resultsSFX; }
		set { m_resultsSFX = value; }
	}

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
	private Chest.RewardType m_rewardType = Chest.RewardType.SC;

	private RewardSetup currentRewardSetup {
		get { return m_rewardSetups[(int)m_rewardType]; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Sort reward setup to match enum
		RewardSetup[] sortedSetups = new RewardSetup[(int)Chest.RewardType.COUNT];
		for(int i = 0; i < m_rewardSetups.Length; ++i) {
			sortedSetups[(int)m_rewardSetups[i].type] = m_rewardSetups[i];
		}
		m_rewardSetups = sortedSetups;

		// Get animator ref
		m_animator = GetComponent<Animator>();

		// Start with all particles stopped
		StopAllFX();
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

			// Play SFX
			AudioController.Play(m_openSFX);
		}

		// Show the right reward
		for(int i = 0; i < m_rewardSetups.Length; i++) {
			m_rewardSetups[i].view.SetActive(m_rewardSetups[i].type == _reward);
		}

		// Stop current reward FX - target reward FX will be triggered synched with the animation (OnLidOpen event)
		ToggleFX(currentRewardSetup.openFX, false);
		ToggleFX(currentRewardSetup.openLoopFX, false);

		// Store reward type to spawn the right FX once the lid is open
		m_rewardType = _reward;
	}

	/// <summary>
	/// Launch the close animation.
	/// </summary>
	public void Close() {
		// Launch close animation
		m_animator.SetTrigger( GameConstants.Animator.CLOSE );

		// Stop FX
		StopAllFX();

		// Play SFX
		if ( !string.IsNullOrEmpty(m_closeSFX) )
			AudioController.Play(m_closeSFX);
	}

	/// <summary>
	/// Triggers the results animation.
	/// </summary>
	public void ResultsAnim() {
		// Stop all particles
		StopAllFX();

		// Launch animation
		m_animator.SetTrigger(GameConstants.Animator.RESULTS_IN);

		// SFX will be played on the ChestLanded animation event so it's fully synced
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Stops all FX.
	/// </summary>
	private void StopAllFX() {
		ToggleFX(m_glowFX, false);
		for(int i = 0; i < m_rewardSetups.Length; ++i) {
			if(m_rewardSetups[i] == null) continue;
			ToggleFX(m_rewardSetups[i].openFX, false);
			ToggleFX(m_rewardSetups[i].openLoopFX, false);
		}
	}

	/// <summary>
	/// Enable/Disable FX, making sure it's a valid reference.
	/// </summary>
	/// <param name="_fx">The FX to be toggled.</param>
	/// <param name="_toggle">Whether to enable or disable it.</param>
	private void ToggleFX(ViewParticleSpawner _fx, bool _toggle) {
		if(_fx == null) return;
		_fx.gameObject.SetActive(_toggle);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Lid open animation event.
	/// </summary>
	public void OnLidOpen() {
		// Turn off glow FX
		ToggleFX(m_glowFX, false);

		// Trigger open and loop FXs corresponding to current reward type
		ToggleFX(currentRewardSetup.openFX, true);
		ToggleFX(currentRewardSetup.openLoopFX, true);

		// Disable openFX after some delay so it's not triggered again
		UbiBCN.CoroutineManager.DelayedCall(() => { ToggleFX(currentRewardSetup.openFX, false); }, 1.5f);

		// Notify delegates
		OnChestOpenEvent.Invoke();
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnChestLanded() {
		// Play SFX
		AudioController.Play(m_resultsSFX);

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