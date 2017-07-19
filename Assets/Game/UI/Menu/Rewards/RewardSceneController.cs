// RewardSceneController.cs
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
public class RewardSceneController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Anchors")]
	[SerializeField] private Transform m_eggAnchor = null;
	[SerializeField] private Transform m_rewardAnchor = null;
	public Transform rewardAnchor {
		get { return m_rewardAnchor; }
	}

	[Separator("VFX")]
	[SerializeField] private ParticleSystem m_explosionFX = null;
	[SerializeField] private GodRaysFXFast m_godRaysFX = null;
	[SerializeField] private ParticleSystem m_goldenFragmentsSwapFX = null;
	[SerializeField] private Transform m_tapFXPool = null;
	[Tooltip("One per rarity, matching order")]
	[SerializeField] private ParticleSystem[] m_tapFX = new ParticleSystem[(int)EggReward.Rarity.COUNT];
	[Tooltip("One per rarity, matching order")]
	[SerializeField] private ParticleSystem[] m_openFX = new ParticleSystem[(int)EggReward.Rarity.COUNT];

	[Separator("Reward views")]
	[SerializeField] private GameObject m_petReward = null;
	[SerializeField] private GameObject m_hcReward = null;
	[SerializeField] private GameObject m_scReward = null;
	[Tooltip("One per rarity, matching order. None for \"special\".")]
	[SerializeField] private GameObject[] m_goldenFragmentsRewards = new GameObject[(int)EggReward.Rarity.COUNT - 1];

	[Separator("Others")]
	[Tooltip("Will replace the camera snap point for the photo screen when doing photos to the egg reward.")]
	[SerializeField] private CameraSnapPoint m_photoCameraSnapPoint = null;

	//------------------------------------------------------------------------------------------------------------//

	public UnityEvent OnAnimFinished = new UnityEvent();

	// Internal
	private GameObject m_rewardView = null;
	public GameObject rewardView {
		get { return m_rewardView; }
	}

	private EggView m_eggView = null;
	public EggView eggView {
		get { return m_eggView; }
	}

	public Egg eggData {
		get { return (m_eggView == null) ? null : m_eggView.eggData; }
	}

	// Other references that must be set from script
	private DragControlRotation m_dragController = null;
	private CameraSnapPoint m_originalPhotoCameraSnapPoint = null;

	private Metagame.Reward m_currentReward;

	//------------------------------------------------------------------------------------------------------------//


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Store original camera snap point for the photo screen
		m_originalPhotoCameraSnapPoint = InstanceManager.menuSceneController.screensController.cameraSnapPoints[(int)MenuScreens.PHOTO];

		// Don't show anything
		Clear();
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
		m_goldenFragmentsRewards.Resize((int)EggReward.Rarity.COUNT);
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

		// Unlink any transform from the drag controller
		SetDragTarget(null);

		// Destroy egg view
		if (m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;
		}

		HideAllRewards();
		m_rewardView = null;

		// Stop all FX
		if (m_godRaysFX != null) {
			m_godRaysFX.StopFX();
		}

		if (m_goldenFragmentsSwapFX != null) {
			m_goldenFragmentsSwapFX.Stop();
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

	private void HideAllRewards() {
		m_petReward.SetActive(false);
		m_hcReward.SetActive(false);
		m_scReward.SetActive(false);

		for (int i = 0; i < m_goldenFragmentsRewards.Length; ++i) {
			m_goldenFragmentsRewards[i].SetActive(false);
		}
	}

	/// <summary>
	/// Initialize some external references that can't be linked via inspector.
	/// Should be called asap before actually launching any animation.
	/// </summary>
	/// <param name="_dragController">Drag controller to be used for rewards.</param>
	public void InitReferences(DragControlRotation _dragController) {
		m_dragController = _dragController;
	}

	public void OpenReward() {
		m_currentReward = UsersManager.currentUser.rewardStack.Pop();

		if (m_currentReward is Metagame.RewardEgg) {
			OpenEggReward(m_currentReward as Metagame.RewardEgg);
		} else if (m_currentReward is Metagame.RewardPet) {
			m_currentReward.Collect();
			OpenPetReward(m_currentReward as Metagame.RewardPet);
		} else { // reward currency
			m_currentReward.Collect();
			OpenCurrencyReward(m_currentReward as Metagame.RewardCurrency);
		}
	}

	private void OpenEggReward(Metagame.RewardEgg _eggReward) {
		_eggReward.egg.ChangeState(Egg.State.OPENING);
		InitEggView(_eggReward);
		LaunchIntro();
	}

	private void OpenPetReward(Metagame.RewardPet _petReward) {
		InitPetView(_petReward);
	}

	private void OpenCurrencyReward(Metagame.RewardCurrency _currencyReward) {
		InitCurrencyView(_currencyReward);
	}


	/// <summary>
	/// Initialize the egg view with the given egg data. Optionally reuse an existing
	/// egg view.
	/// </summary>
	/// <param name="_egg">The egg to be opened.</param>
	private void InitEggView(Metagame.RewardEgg _eggReward) {
		// Clear any active stuff
		Clear();

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

	private void InitPetView(Metagame.RewardPet _petReward) {
		HideAllRewards();

		// Aux vars
		Sequence seq = DOTween.Sequence();
		Vector2 baseIdleVelocity = m_dragController.idleVelocity;

		// Use a PetLoader to simplify things
		m_rewardView = m_petReward;
		m_rewardView.SetActive(true);

		MenuPetLoader loader = m_rewardView.GetComponent<MenuPetLoader>();
		loader.Setup(MenuPetLoader.Mode.MANUAL, MenuPetPreview.Anim.IN, true);
		loader.Load(_petReward.value);

		// Animate it
		seq.AppendInterval(0.05f);	// Initial delay
		seq.Append(m_rewardView.transform.DOScale(0f, 0.5f).From().SetRecyclable(true).SetEase(Ease.OutBack));

		// Make it target of the drag controller
		seq.AppendCallback(() => { SetDragTarget(m_rewardView.transform); });

		// If the reward is a duplicate, check which alternate reward are we giving instead and switch the reward view by the replacement with a nice animation
		GameObject replacementRewardView = null;
		if (_petReward.WillBeReplaced()) {
			if (_petReward.ReplacementCurrency() == UserProfile.Currency.GOLDEN_FRAGMENTS) {
				replacementRewardView = m_goldenFragmentsRewards[(int)_petReward.rarity];
			} else {
				replacementRewardView = m_scReward;
			}
		}

		// Launch a nice animation
		if (replacementRewardView != null) {
			// 1. Restart infinite rotation tween
			replacementRewardView.transform.DORestart();

			// 2. Reward acceleration
			// Make it compatible with the drag controller!
			seq.Append(DOTween.To(
				() => { return baseIdleVelocity; },	// Getter
				(Vector2 _v) => { m_dragController.idleVelocity = _v; },	// Setter
				Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)),	// Final value
				1f)	// Duration
				.SetEase(Ease.InCubic)
			);

			// 3. Show VFX to cover the swap
			// We want it to launch a bit before doing the swap. To do so, use a combination of InserCallback() with the sequence's current duration.
			seq.InsertCallback(seq.Duration() - 0.15f, () => {
				if (m_goldenFragmentsSwapFX != null) {
					m_goldenFragmentsSwapFX.Clear();
					m_goldenFragmentsSwapFX.Play(true);
				}
			});

			// 4. Swap
			seq.AppendCallback(() => {
				// Swap reward view with replacement view
				m_rewardView.SetActive(false);
				replacementRewardView.SetActive(true);

				// Make it target of the drag controller
				SetDragTarget(replacementRewardView.transform);
			});

			// 5. Replacement reward initial inertia and scale up
			// Make it compatible with the drag controller!
			seq.Append(replacementRewardView.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack));
			seq.Join(DOTween.To(
				() => { return baseIdleVelocity; },	// Getter
				(Vector2 _v) => { m_dragController.idleVelocity = _v; },	// Setter
				Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)),	// Final value
				2f)	// Duration
				.From()
				.SetEase(Ease.OutCubic)
			);
		}

		// Show reward godrays
		// Except if duplicate! (for now)
		if (m_godRaysFX != null && !_petReward.WillBeReplaced()) {
			// Custom color based on reward's rarity
			m_godRaysFX.StartFX(m_currentReward.rarity);

			// Show with some delay to sync with pet's animation
			seq.Insert(0.15f, m_godRaysFX.transform.DOScale(0f, 0.05f).From().SetRecyclable(true));
		}

		seq.OnComplete(OnAnimationFinish);
	}

	private void InitCurrencyView(Metagame.RewardCurrency _reward) {
		HideAllRewards();

		// Aux vars
		Sequence seq = DOTween.Sequence();
		Vector2 baseIdleVelocity = m_dragController.idleVelocity;

		if (_reward.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
			m_rewardView = m_goldenFragmentsRewards[(int)_reward.rarity];
		} else if (_reward.currency == UserProfile.Currency.HARD) {
			m_rewardView = m_hcReward;
		} else {
			m_rewardView = m_scReward;
		}

		m_rewardView.SetActive(true);

		SetDragTarget(m_rewardView.transform);

		seq.Append(m_rewardView.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack));
		seq.Join(DOTween.To(
			() => { return baseIdleVelocity; },	// Getter
			(Vector2 _v) => { m_dragController.idleVelocity = _v; },	// Setter
			Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)),	// Final value
			2f)	// Duration
			.From()
			.SetEase(Ease.OutCubic)
		).OnComplete(OnAnimationFinish);
	}

	//------------------------------------------------------------------//
	// ANIMATIONS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start the intro animation for the egg!
	/// </summary>
	private void LaunchIntro() {
		// Ignore if we don't have a valid egg view
		if(m_eggView == null) return;

		// Assume we can do it (no checks)
		// Activate egg
		m_eggView.gameObject.SetActive(true);

		// [AOC] TODO!! Some awesome FX!!
		m_eggView.transform.DOScale(0f, 0.5f).From().SetEase(Ease.OutElastic);//.OnComplete(OnOpenEggReady);
	}

	/// <summary>
	/// Launch the egg open animation.
	/// </summary>
	public void LaunchOpenEggAnim() {
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

		// Program reward animation
		UbiBCN.CoroutineManager.DelayedCall(OnEggOpenFinishedCallback, 0.35f, false);
	}


	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Define a transform as target of the drag controller.
	/// </summary>
	/// <param name="_target">The new target.</param>
	private void SetDragTarget(Transform _target) {
		// Drag controller must be valid
		if(m_dragController == null) return;

		// Just do it!
		m_dragController.target = _target;

		// Enable/Disable based on new target
		m_dragController.gameObject.SetActive(_target != null);
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

	private void OnAnimationFinish() {
		m_currentReward = null;
		// Notify external script
		OnAnimFinished.Invoke();
	}

	private void OnEggOpenFinishedCallback() {
		// Next we have to open the reward inside the egg
		m_currentReward = null;
		OpenReward();
	}
}