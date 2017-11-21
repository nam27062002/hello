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
using System;

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
	[Serializable]
	public class RewardSetup {
		public GameObject view = null;
		public GameObject godrays = null;
		public string sfx = "";

		public void Clear() {
			if(view != null) {
				view.SetActive(false);
			}
			if(godrays != null) {
				godrays.SetActive(false);
			}
		}
	}

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
	[Separator("References")]
	[SerializeField] private Transform m_eggAnchor = null;
	[SerializeField] private Transform m_rewardAnchor = null;
	public Transform rewardAnchor {
		get { return m_rewardAnchor; }
	}

	[SerializeField] private RaritySetup[] m_rarityFXSetup = new RaritySetup[(int)EggReward.Rarity.COUNT];
	[SerializeField] private RewardSetup m_petRewardSetup = new RewardSetup();
	[SerializeField] private RewardSetup m_hcRewardSetup = new RewardSetup();
	[SerializeField] private RewardSetup m_scRewardSetup = new RewardSetup();
	[SerializeField] private RewardSetup[] m_goldenFragmentsRewardsSetup = new RewardSetup[(int)EggReward.Rarity.COUNT - 1];

	[Separator("Other VFX")]
	[SerializeField] private ParticleSystem m_goldenFragmentsSwapFX = null;
	[SerializeField] private Transform m_tapFXPool = null;

	[Separator("Other SFX")]
	[SerializeField] private string m_eggTapSFX = "";
	[SerializeField] private string m_eggExplosionSFX = "";
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
	private Metagame.Reward m_currentReward;
	public Metagame.Reward currentReward {
		get { return m_currentReward; }
	}

	private RewardSetup m_currentRewardSetup = null;
	public RewardSetup currentRewardSetup {
		get { return m_currentRewardSetup; }
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

		// Unlink any transform from the drag controller
		SetDragTarget(null);

		// Destroy egg view
		if(m_eggView != null) {
			GameObject.Destroy(m_eggView.gameObject);
			m_eggView = null;

			// Unsubscribe from external events.
			Messenger.RemoveListener<Egg>(GameEvents.EGG_OPENED, OnEggCollected);
		}

		// Hide any reward
		HideAllRewards();
		m_currentRewardSetup = null;

		// Stop all other FX
		if (m_goldenFragmentsSwapFX != null) {
			m_goldenFragmentsSwapFX.Stop();
		}
	}

	/// <summary>
	/// Hide all reward views.
	/// </summary>
	private void HideAllRewards() {
		m_petRewardSetup.Clear();
		m_hcRewardSetup.Clear();
		m_scRewardSetup.Clear();
		for(int i = 0; i < m_goldenFragmentsRewardsSetup.Length; ++i) {
			m_goldenFragmentsRewardsSetup[i].Clear();
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
		// Don't pop immediately, the Reward.Collect() will do it
		m_currentReward = UsersManager.currentUser.rewardStack.Peek();

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
		seq.Append(m_currentRewardSetup.view.transform.DOScale(0f, 0.5f).From().SetRecyclable(true).SetEase(Ease.OutBack));

		// Trigger UI animation
		seq.InsertCallback(seq.Duration() - 0.15f, () => { m_rewardInfoUI.InitAndAnimate(_petReward); });

		// Make it target of the drag controller
		seq.AppendCallback(() => { SetDragTarget(m_currentRewardSetup.view.transform); });

		// If the reward is a duplicate, check which alternate reward are we giving instead and switch the reward view by the replacement with a nice animation
		RewardSetup replacementSetup = null;
		if(_petReward.WillBeReplaced()) {
			if (_petReward.replacement.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
				replacementSetup = m_goldenFragmentsRewardsSetup[(int)_petReward.rarity];
			} else {
				replacementSetup = m_scRewardSetup;
			}
		}

		// Launch a nice animation
		if(replacementSetup != null) {
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
				m_currentRewardSetup.view.SetActive(false);
				replacementSetup.view.SetActive(true);

				// Make it target of the drag controller
				SetDragTarget(replacementSetup.view.transform);
			});

			// Show replacement UI info
			seq.AppendCallback(() => {
				// Depending on replacement type, show different texts
				string replacementInfoText = "";
				if(_petReward.replacement.currency == UserProfile.Currency.GOLDEN_FRAGMENTS) {
					replacementInfoText = LocalizationManager.SharedInstance.Localize(
						"TID_EGG_REWARD_DUPLICATED_1", 
						_petReward.def.GetLocalized("tidName"), 
						StringUtils.FormatNumber(_petReward.replacement.amount)
					);
				} else if(_petReward.replacement.currency == UserProfile.Currency.SOFT) {
					replacementInfoText = LocalizationManager.SharedInstance.Localize(
						"TID_EGG_REWARD_DUPLICATED_2", 
						_petReward.def.GetLocalized("tidName"), 
						StringUtils.FormatNumber(_petReward.replacement.amount)
					);
				}
				m_rewardInfoUI.InitAndAnimate(_petReward.replacement, replacementInfoText);

				// Play specific sound as well!
				AudioController.Play(replacementSetup.sfx);
			});

			// Replacement reward initial inertia and scale up
			// Make it compatible with the drag controller!
			seq.Append(replacementSetup.view.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack));
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
		if(m_petRewardSetup.godrays != null && !_petReward.WillBeReplaced()) {
			// Custom color based on reward's rarity
			GodRaysFXFast godraysFX = m_petRewardSetup.godrays.GetComponent<GodRaysFXFast>();
			if(godraysFX != null) {
				godraysFX.StartFX(m_currentReward.rarity);
			}

			// Show with some delay to sync with pet's animation
			seq.Insert(0.15f, m_petRewardSetup.godrays.transform.DOScale(0f, 0.05f).From().SetRecyclable(true));
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

			// Trigger SFX, depends on reward tyoe
			AudioController.Play(m_currentRewardSetup.sfx);
		});

		seq.Append(m_currentRewardSetup.view.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack));
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
		ParticleSystem tapFX = m_rarityFXSetup[(int)_eggReward.rarity].tapFX;
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

		m_currentRewardSetup = m_petRewardSetup;
		m_currentRewardSetup.view.SetActive(true);

		// Use a PetLoader to simplify things
		MenuPetLoader loader = m_currentRewardSetup.view.GetComponent<MenuPetLoader>();
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
			m_currentRewardSetup = m_goldenFragmentsRewardsSetup[(int)_reward.rarity];
		} else if (_reward.currency == UserProfile.Currency.HARD) {
			m_currentRewardSetup = m_hcRewardSetup;
		} else {
			m_currentRewardSetup = m_scRewardSetup;
		}

		m_currentRewardSetup.view.SetActive(true);

		SetDragTarget(m_currentRewardSetup.view.transform);
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
		ParticleSystem openFX = m_rarityFXSetup[(int)m_currentReward.rarity].openFX;
		if(openFX != null) {
			openFX.gameObject.SetActive(true);
			openFX.Clear();
			openFX.Play(true);
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
			ParticleSystem tapFX = m_rarityFXSetup[(int)m_currentReward.rarity].tapFX;
			if(tapFX != null) {
				tapFX.gameObject.SetActive(true);
				tapFX.Stop(true);
				tapFX.Clear();
				tapFX.Play(true);
			}

			tapFX = m_rarityFXSetup[(int)m_currentReward.rarity].tapFXStatic;
			if(tapFX != null) {
				tapFX.gameObject.SetActive(true);
				tapFX.Stop(true);
				tapFX.Clear();
				tapFX.Play(true);
			}

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

				// Nullify reward reference
				m_currentReward = null;

				// Restore default camera snap point for the photo screen
				InstanceManager.menuSceneController.screensController.cameraSnapPoints[(int)MenuScreens.PHOTO] = m_originalPhotoCameraSnapPoint;
			}
		}
	}
}