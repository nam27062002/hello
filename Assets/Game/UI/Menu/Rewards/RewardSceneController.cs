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

	[Separator("SFX")]
	[SerializeField] private string m_eggTapSFX = "";
	[SerializeField] private string m_eggExplosionSFX = "";
	[SerializeField] private string m_scSFX = "";
	[SerializeField] private string m_pcSFX = "";
	[SerializeField] private string m_goldenFragmentsSFX = "";
	[SerializeField] private string m_goldenEggCompletedSFX = "";

	[Separator("Others")]
	[Tooltip("Will replace the camera snap point for the photo screen when doing photos to the egg reward.")]
	[SerializeField] private CameraSnapPoint m_photoCameraSnapPoint = null;

	[Separator("Animation Setup")]
	[SerializeField] private float m_goldenEggDelay = 1f;

	[Separator("Events")]
	public UnityEvent OnAnimStarted = new UnityEvent();
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
	private RewardInfoUI m_rewardInfoUI = null;
	private CameraSnapPoint m_originalPhotoCameraSnapPoint = null;

	private Metagame.Reward m_currentReward;
	public Metagame.Reward currentReward {
		get { return m_currentReward; }
	}

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

		// Subscribe to external events
		Messenger.AddListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
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
		// Unsubscribe from external events
		Messenger.RemoveListener<MenuScreens, MenuScreens>(GameEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);

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
		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;

			// Unsubscribe from external events.
			Messenger.RemoveListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
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

	/// <summary>
	/// Hide all reward views.
	/// </summary>
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
	/// <param name="_rewardInfoUI">UI widget used to display the info on the reward.</param>
	public void InitReferences(DragControlRotation _dragController, RewardInfoUI _rewardInfoUI) {
		// Store and initialize drag controller
		m_dragController = _dragController;

		// Store and initialize UI
		m_rewardInfoUI = _rewardInfoUI;
		m_rewardInfoUI.goldenEggCompletedSFX = m_goldenEggCompletedSFX;
	}

	/// <summary>
	/// Start the open reward flow with the top reward on the stack.
	/// </summary>
	public void OpenReward() {
		// Get most recent reward in the stack
		m_currentReward = UsersManager.currentUser.rewardStack.Pop();

		// Hide UI reward elements
		m_rewardInfoUI.SetRewardType(string.Empty);

		// Launch the animation based on reward type
		if (m_currentReward is Metagame.RewardEgg) {
			OpenEggReward(m_currentReward as Metagame.RewardEgg);
		} else if (m_currentReward is Metagame.RewardPet) {
			m_currentReward.Collect();
			OpenPetReward(m_currentReward as Metagame.RewardPet);
		} else { // reward currency
			m_currentReward.Collect();
			OpenCurrencyReward(m_currentReward as Metagame.RewardCurrency);
		}

		// Notify listeners
		OnAnimStarted.Invoke();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Start the egg reward flow.
	/// </summary>
	/// <param name="_eggReward">Egg reward.</param>
	private void OpenEggReward(Metagame.RewardEgg _eggReward) {
		// Change egg state
		_eggReward.egg.ChangeState(Egg.State.OPENING);

		// Initialize the view
		InitEggView(_eggReward);

		// Launch intro!
		// Ignore if we don't have a valid egg view
		if(m_eggView != null) {
			// Assume we can do it (no checks)
			// Activate egg
			m_eggView.gameObject.SetActive(true);

			// [AOC] TODO!! Some awesome FX!!
			m_eggView.transform.DOScale(0f, 0.5f).From().SetEase(Ease.OutElastic);
		}

		// Trigger UI animation
		UbiBCN.CoroutineManager.DelayedCall(() => { m_rewardInfoUI.InitAndAnimate(_eggReward); }, 0.25f, false);
	}

	/// <summary>
	/// Start the pet reward flow.
	/// </summary>
	/// <param name="_petReward">Pet reward.</param>
	private void OpenPetReward(Metagame.RewardPet _petReward) {
		// Initialize pet view
		InitPetView(_petReward);

		// Animate it!
		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(0.05f);	// Initial delay
		seq.Append(m_rewardView.transform.DOScale(0f, 0.5f).From().SetRecyclable(true).SetEase(Ease.OutBack));

		// Trigger UI animation
		seq.InsertCallback(seq.Duration() - 0.15f, () => { m_rewardInfoUI.InitAndAnimate(_petReward); });

		// Make it target of the drag controller
		seq.AppendCallback(() => { SetDragTarget(m_rewardView.transform); });

		// If the reward is a duplicate, check which alternate reward are we giving instead and switch the reward view by the replacement with a nice animation
		GameObject replacementRewardView = null;
		if(_petReward.WillBeReplaced()) {
			if (_petReward.replacement.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
				replacementRewardView = m_goldenFragmentsRewards[(int)_petReward.rarity];
			} else {
				replacementRewardView = m_scReward;
			}
		}

		// Launch a nice animation
		if(replacementRewardView != null) {
			// Reward acceleration
			// Make it compatible with the drag controller!
			Vector2 baseIdleVelocity = m_dragController.idleVelocity;
			seq.Append(DOTween.To(
				() => { return baseIdleVelocity; },	// Getter
				(Vector2 _v) => { m_dragController.idleVelocity = _v; },	// Setter
				Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)),	// Final value
				1f)	// Duration
				.SetEase(Ease.InCubic)
			);

			// Hide UI Info
			seq.InsertCallback(
				seq.Duration() - 0.25f,
				() => { m_rewardInfoUI.showHideAnimator.Hide(); }
			);

			// Show VFX to cover the swap
			// We want it to launch a bit before doing the swap. To do so, use a combination of InserCallback() with the sequence's current duration.
			seq.InsertCallback(seq.Duration() - 0.15f, () => {
				if(m_goldenFragmentsSwapFX != null) {
					m_goldenFragmentsSwapFX.Clear();
					m_goldenFragmentsSwapFX.Play(true);
				}
			});

			// Swap
			seq.AppendCallback(() => {
				// Swap reward view with replacement view
				m_rewardView.SetActive(false);
				replacementRewardView.SetActive(true);

				// Make it target of the drag controller
				SetDragTarget(replacementRewardView.transform);
			});

			// Show replacement UI info
			seq.AppendCallback(() => {
				// Depending on replacement type, show different texts and play different sounds
				string replacementInfoText = "";
				string sfx = "";
				if(_petReward.replacement.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
					sfx = m_goldenFragmentsSFX;
					replacementInfoText = LocalizationManager.SharedInstance.Localize(
						"TID_EGG_REWARD_DUPLICATED_1", 
						_petReward.def.GetLocalized("tidName"), 
						StringUtils.FormatNumber(_petReward.replacement.amount)
					);
				} else if(_petReward.replacement.currency == UserProfile.Currency.SOFT) {
					sfx = m_scSFX;
					replacementInfoText = LocalizationManager.SharedInstance.Localize(
						"TID_EGG_REWARD_DUPLICATED_2", 
						_petReward.def.GetLocalized("tidName"), 
						StringUtils.FormatNumber(_petReward.replacement.amount)
					);
				}
				m_rewardInfoUI.InitAndAnimate(_petReward.replacement, replacementInfoText);
				AudioController.Play(sfx);
			});

			// Replacement reward initial inertia and scale up
			// Make it compatible with the drag controller!
			seq.Append(replacementRewardView.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack));
			seq.Join(DOTween.To(
				() => { return baseIdleVelocity; },	// Getter
				(Vector2 _v) => { m_dragController.idleVelocity = _v; },	// Setter
				Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)),		// Final value
				2f)	// Duration
				.From()
				.SetEase(Ease.OutCubic)
			);
		}

		// Show reward godrays
		// Except if duplicate! (for now)
		if(m_godRaysFX != null && !_petReward.WillBeReplaced()) {
			// Custom color based on reward's rarity
			m_godRaysFX.StartFX(m_currentReward.rarity);

			// Show with some delay to sync with pet's animation
			seq.Insert(0.15f, m_godRaysFX.transform.DOScale(0f, 0.05f).From().SetRecyclable(true));
		}

		seq.OnComplete(OnAnimationFinish);
	}

	/// <summary>
	/// Start the currency reward flow.
	/// </summary>
	/// <param name="_currencyReward">Currency reward.</param>
	private void OpenCurrencyReward(Metagame.RewardCurrency _currencyReward) {
		// Initialize view
		InitCurrencyView(_currencyReward);

		// Animate
		Vector2 baseIdleVelocity = m_dragController.idleVelocity;
		Sequence seq = DOTween.Sequence();

		seq.AppendCallback(() => {
			// Show UI
			m_rewardInfoUI.InitAndAnimate(_currencyReward);

			// Trigger SFX, depends on currency type
			string sfx = "";
			switch(_currencyReward.currency) {
				case UserProfile.Currency.SOFT: sfx = m_scSFX; break;
				case UserProfile.Currency.HARD: sfx = m_pcSFX; break;
				case UserProfile.Currency.GOLDEN_FRAGMENTS: sfx = m_goldenFragmentsSFX; break;
			}
			AudioController.Play(sfx);
		});

		seq.Append(m_rewardView.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack));
		seq.Join(DOTween.To(
			() => { return baseIdleVelocity; },	// Getter
			(Vector2 _v) => { m_dragController.idleVelocity = _v; },	// Setter
			Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)),	// Final value
			2f)	// Duration
			.From()
			.SetEase(Ease.OutCubic)
		);
			
		seq.OnComplete(OnAnimationFinish);
	}

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

	/// <summary>
	/// Initialize the pet reward view with the given reward data.
	/// </summary>
	/// <param name="_petReward">Pet reward data.</param>
	private void InitPetView(Metagame.RewardPet _petReward) {
		HideAllRewards();

		// Use a PetLoader to simplify things
		m_rewardView = m_petReward;
		m_rewardView.SetActive(true);

		MenuPetLoader loader = m_rewardView.GetComponent<MenuPetLoader>();
		loader.Setup(MenuPetLoader.Mode.MANUAL, MenuPetPreview.Anim.IN, true);
		loader.Load(_petReward.sku);
	}

	/// <summary>
	/// Initialize the currency reward view with the given reward data.
	/// </summary>
	/// <param name="_reward">The currency reward data.</param>
	private void InitCurrencyView(Metagame.RewardCurrency _reward) {
		HideAllRewards();

		if (_reward.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
			m_rewardView = m_goldenFragmentsRewards[(int)_reward.rarity];
		} else if (_reward.currency == UserProfile.Currency.HARD) {
			m_rewardView = m_hcReward;
		} else {
			m_rewardView = m_scReward;
		}

		m_rewardView.SetActive(true);

		SetDragTarget(m_rewardView.transform);
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

		// Trigger SFX
		AudioController.Play(m_eggExplosionSFX);

		// Program reward animation
		UbiBCN.CoroutineManager.DelayedCall(OnEggExplosionAnimFinished, 0.35f, false);
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

		// Reset current target to its original value
		m_dragController.RestoreOriginalValue(false);

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

			// Hide UI
			m_rewardInfoUI.SetRewardType(string.Empty);

			// Play SFX
			AudioController.Play(m_eggTapSFX);
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
			UbiBCN.CoroutineManager.DelayedCall(LaunchEggExplosionAnim, 1.75f, false);
		}
	}

	/// <summary>
	/// The egg open animation has been finished.
	/// </summary>
	private void OnEggExplosionAnimFinished() {
		// Open the reward inside the egg, which has been pushed into the stack by the egg view
		m_currentReward = null;
		OpenReward();
	}

	/// <summary>
	/// The whole open reward animation has finished.
	/// </summary>
	private void OnAnimationFinish() {
		// If next reward is a golden egg, instantly open it!
		if(UsersManager.currentUser.rewardStack.Count > 0) {
			Metagame.Reward nextReward = UsersManager.currentUser.rewardStack.Peek();
			if(nextReward != null && nextReward.sku == Egg.SKU_GOLDEN_EGG) {
				// Give it some delay!
				UbiBCN.CoroutineManager.DelayedCall(() => { OpenReward(); }, m_goldenEggDelay, false);
				return;
			}
		}

		// Nullify current reward
		m_currentReward = null;

		// Notify external script
		OnAnimFinished.Invoke();
	}

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionStart(MenuScreens _from, MenuScreens _to) {
		// Aux vars
		MenuScreenScene fromScene = InstanceManager.menuSceneController.GetScreenScene(_from);
		MenuScreenScene toScene = InstanceManager.menuSceneController.GetScreenScene(_to);

		// Entering a screen using this scene
		if(toScene.gameObject == this.gameObject) {
			// Override camera snap point for the photo screen so it looks to our reward
			InstanceManager.menuSceneController.screensController.cameraSnapPoints[(int)MenuScreens.PHOTO] = m_photoCameraSnapPoint;
		}

		// Leaving a screen using this scene
		else if(fromScene.gameObject == this.gameObject) {
			// Do some stuff if not going to take a picture of the reward
			if(_to != MenuScreens.PHOTO) {
				// Clear the scene
				Clear();

				// Restore default camera snap point for the photo screen
				InstanceManager.menuSceneController.screensController.cameraSnapPoints[(int)MenuScreens.PHOTO] = m_originalPhotoCameraSnapPoint;
			}
		}
	}
}