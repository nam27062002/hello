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
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the 3D scene of the Open Egg screen.
/// </summary>
public class RewardSceneController : MenuScreenScene {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Serializable]
	public class RewardSetup {
		public GameObject view = null;
		public ShinyGodraysFX godrays = null;
		public string sfx = "";

		public virtual void Clear() {
			if(view != null) {
				view.transform.DOKill(false);
				view.SetActive(false);
			}
			if(godrays != null) {
				godrays.ResetColor();
				godrays.gameObject.SetActive(false);
			}
		}
	}

	[Serializable]
	public class RaritySetup {
		public ParticleSystem tapFX = null;
		public ParticleSystem tapFXStatic = null;
		public ParticleSystem openFX = null;
		public string sfx = "";

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

	[SerializeField] private RaritySetup[] m_rarityFXSetup = new RaritySetup[(int)Metagame.Reward.Rarity.COUNT];
	[SerializeField] private RewardSetup m_petRewardSetup = new RewardSetup();
	[SerializeField] private RewardSetup m_skinRewardSetup = new RewardSetup();
	[SerializeField] private RewardSetup m_hcRewardSetup = new RewardSetup();
	[SerializeField] private RewardSetup m_scRewardSetup = new RewardSetup();
	[SerializeField] private RewardSetup[] m_goldenFragmentsRewardsSetup = new RewardSetup[(int)Metagame.Reward.Rarity.COUNT];
	[SerializeField] private RewardSetup m_dragonRewardSetup = new RewardSetup();
    [SerializeField] private RewardSetup m_removeAdsRewardSetup = new RewardSetup();

    [Separator("Other VFX")]
	[SerializeField] private ParticleSystem m_goldenFragmentsSwapFX = null;
	[SerializeField] private Transform m_tapFXPool = null;
	[SerializeField] private Transform m_confettiAnchor = null;
	[SerializeField] private GameObject m_confettiPrefab = null;

	[Separator("Other SFX")]
	[SerializeField] private string m_eggTapSFX = "";
	[SerializeField] private string m_goldenEggCompletedSFX = "";
	[SerializeField] private string m_goldenEggIntroSFX = "";
	[SerializeField] private string m_halloweenPetSFX = "";

	[Separator("Others")]
	[Tooltip("Will replace the camera snap point for the photo screen when doing photos to the egg reward.")]
	[SerializeField] private CameraSnapPoint m_photoCameraSnapPoint = null;
	public CameraSnapPoint photoCameraSnapPoint {
		get { return m_photoCameraSnapPoint; }
	}

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

	//------------------------------------------------------------------------------------------------------------//


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Don't show anything
		Clear();

		// Subscribe to external events
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);
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
		// Unsubscribe from external events
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnMenuScreenTransitionStart);

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
			Messenger.RemoveListener<Egg>(MessengerEvents.EGG_OPENED, OnEggCollected);
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
		m_skinRewardSetup.Clear();
		m_skinRewardSetup.view.GetComponent<MenuDragonLoader>().UnloadDragon();

		m_petRewardSetup.Clear();
		m_petRewardSetup.view.GetComponent<MenuPetLoader>().Unload();

		m_hcRewardSetup.Clear();
		m_scRewardSetup.Clear();
		for(int i = 0; i < m_goldenFragmentsRewardsSetup.Length; ++i) {
			m_goldenFragmentsRewardsSetup[i].Clear();
		}

		m_dragonRewardSetup.Clear();
		m_dragonRewardSetup.view.GetComponent<MenuDragonLoader>().UnloadDragon();
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
		if(UsersManager.currentUser.rewardStack.Count > 0) {
			m_currentReward = UsersManager.currentUser.rewardStack.Peek();

			// Hide UI reward elements
			m_rewardInfoUI.SetRewardType(string.Empty);

			// Launch the animation based on reward type
			// Egg
			if(m_currentReward is Metagame.RewardEgg) {
				OpenEggReward(m_currentReward as Metagame.RewardEgg);
			}

			// Pet
			else if(m_currentReward is Metagame.RewardPet) {
				m_currentReward.Collect();
				OpenPetReward(m_currentReward as Metagame.RewardPet);
			}

			// Skin
			else if(m_currentReward is Metagame.RewardSkin) {
				m_currentReward.Collect();
				OpenSkinReward(m_currentReward as Metagame.RewardSkin);
			}

			// Skin
			else if(m_currentReward is Metagame.RewardDragon) {
				m_currentReward.Collect();
				OpenDragonReward(m_currentReward as Metagame.RewardDragon);
			}

			// Multi
			else if(m_currentReward is Metagame.RewardMulti) {
				m_currentReward.Collect();	// This will push all sub-rewards to the stack
				OpenReward();	// Call ourselves to open the newly pushed rewards
				return;	// Don't do anything else
			}
            
            // No ads offer
            else if (m_currentReward is Metagame.RewardRemoveAds)
            {
                m_currentReward.Collect();  
                OpenRemoveAdsReward(m_currentReward as Metagame.RewardRemoveAds); 
            }

            // Currency
            else {
				m_currentReward.Collect();
				OpenCurrencyReward(m_currentReward as Metagame.RewardCurrency);
			}

			// Notify listeners
			OnAnimStarted.Invoke();
		} else {
			// NO REWARD. This shouldnt happen. But just in case
			// Notify listeners
			OnAnimStarted.Invoke();

			Sequence seq = DOTween.Sequence();
			seq.AppendInterval(0.1f);
			seq.OnComplete(OnAnimationFinish);
		}
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
		}

		// Trigger UI animation
		float duration = 0.25f;
		UbiBCN.CoroutineManager.DelayedCall(() => { m_rewardInfoUI.InitAndAnimate(_eggReward); }, duration, false);
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

		// Initial delay
		seq.AppendInterval(0.05f);

		// Scale Up
		seq.Append(m_currentRewardSetup.view.transform.DOScale(0f, 0.5f).From().SetRecyclable(true).SetEase(Ease.OutBack));

		// Trigger UI animation
		seq.InsertCallback(seq.Duration() - 0.15f, () => { m_rewardInfoUI.InitAndAnimate(_petReward); });

		// Make it target of the drag controller
		seq.AppendCallback(() => { SetDragTarget(m_currentRewardSetup.view.transform); });

		// Show reward godrays
		// Except if duplicate! (for now)
		if(m_petRewardSetup.godrays != null && !_petReward.WillBeReplaced()) {
			// Custom color based on reward's rarity
			m_petRewardSetup.godrays.gameObject.SetActive(true);
			m_petRewardSetup.godrays.Tint(UIConstants.GetRarityColor(m_currentReward.rarity));

			// Show with some delay to sync with pet's animation
			seq.Insert(0.15f, m_petRewardSetup.godrays.transform.DOScale(0f, 0.05f).From().SetRecyclable(true));
		}

		// If the reward is a duplicate, check which alternate reward are we giving instead and switch the reward view by the replacement with a nice animation
		RewardSetup replacementSetup = null;
		string replacementInfoText = "";
		if(_petReward.WillBeReplaced()) {
			switch(_petReward.replacement.currency) {
				// Coins
				case UserProfile.Currency.SOFT:	{
					replacementSetup = m_scRewardSetup;
					replacementInfoText = LocalizationManager.SharedInstance.Localize(
						"TID_EGG_REWARD_DUPLICATED_2",
						_petReward.def.GetLocalized("tidName"),
						StringUtils.FormatNumber(_petReward.replacement.amount)
					);
				} break;

				// Gems
				case UserProfile.Currency.HARD: {
					replacementSetup = m_hcRewardSetup;
					replacementInfoText = LocalizationManager.SharedInstance.Localize(
						"TID_EGG_REWARD_DUPLICATED_3",
						_petReward.def.GetLocalized("tidName"),
						StringUtils.FormatNumber(_petReward.replacement.amount)
					);
				} break;

				// Golden egg fragments
				case UserProfile.Currency.GOLDEN_FRAGMENTS: {
					replacementSetup = m_goldenFragmentsRewardsSetup[(int)_petReward.rarity];
					replacementInfoText = LocalizationManager.SharedInstance.Localize(
						"TID_EGG_REWARD_DUPLICATED_1",
						_petReward.def.GetLocalized("tidName"),
						StringUtils.FormatNumber(_petReward.replacement.amount)
					);
				} break;
			}
		}

		// Do some extra stuff if the reward is going to be replaced
		if(replacementSetup != null) {
			AppendReplacementAnim(ref seq, replacementSetup, replacementInfoText);
		}

		seq.OnComplete(OnAnimationFinish);
	}

	/// <summary>
	/// Start the skin reward flow.
	/// </summary>
	/// <param name="_skinReward">Skin reward.</param>
	private void OpenSkinReward(Metagame.RewardSkin _skinReward) {
		// Initialize skin view
		InitSkinView(_skinReward);

		// Trigger confetti anim
		LaunchConfettiFX(true);

		// Animate it!
		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(0.05f);	// Initial delay
		seq.Append(m_currentRewardSetup.view.transform.DOScale(0f, 0.6f).From().SetRecyclable(true).SetEase(Ease.OutBack));

		// Trigger UI animation
		seq.InsertCallback(seq.Duration() - 0.15f, () => { m_rewardInfoUI.InitAndAnimate(_skinReward); });

		// Make it target of the drag controller
		seq.AppendCallback(() => { SetDragTarget(m_currentRewardSetup.view.transform); });

		// Show reward godrays
		if(m_currentRewardSetup.godrays != null && !_skinReward.WillBeReplaced()) {
			// Custom color based on reward's rarity
			m_currentRewardSetup.godrays.gameObject.SetActive(true);

			// Show with some delay to sync with reward's animation
			seq.Insert(0.15f, m_currentRewardSetup.godrays.transform.DOScale(0f, 0.05f).From().SetRecyclable(true));
		}

		// If the reward will be replaced, append the replace animation
		if(_skinReward.WillBeReplaced()) {
			AppendReplacementAnim(ref seq);
		}

		seq.OnComplete(OnAnimationFinish);
	}

	/// <summary>
	/// Start the dragon reward flow.
	/// </summary>
	/// <param name="_dragonReward">Dragon reward.</param>
	private void OpenDragonReward(Metagame.RewardDragon _dragonReward) {
		// If we're in the right mode, make it the selected dragon
		IDragonData dragonData = DragonManager.GetDragonData(_dragonReward.sku);
		if(dragonData != null) {
			// Tell the dragon selection screen for that dragon to make it the selected one next time we go there
			MenuDragonScreenController dragonSelectionScreen = InstanceManager.menuSceneController.GetScreenData(MenuScreen.DRAGON_SELECTION).ui.GetComponent<MenuDragonScreenController>();
			dragonSelectionScreen.pendingToSelectDragon = _dragonReward.sku;
		}

		// Initialize dragon view
		InitDragonView(_dragonReward);

		// Trigger confetti anim
		LaunchConfettiFX(true);

		// Animate it!
		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(0.05f);  // Initial delay
		float originalScale = m_currentRewardSetup.view.transform.localScale.x;
		m_currentRewardSetup.view.transform.SetLocalScale(0f);
		seq.Append(m_currentRewardSetup.view.transform.DOScale(originalScale, 0.6f).SetRecyclable(true).SetEase(Ease.OutBack));

		// Trigger UI animation - except if the reward is going to be replaced
		if(!_dragonReward.WillBeReplaced()) {
			seq.InsertCallback(seq.Duration() - 0.15f, () => { m_rewardInfoUI.InitAndAnimate(_dragonReward); });
		}

		// Make it target of the drag controller
		seq.AppendCallback(() => { SetDragTarget(m_currentRewardSetup.view.transform); });

		// Show reward godrays - unless replaced
		if(m_currentRewardSetup.godrays != null && !_dragonReward.WillBeReplaced()) {
			// Custom color based on reward's rarity
			m_currentRewardSetup.godrays.gameObject.SetActive(true);

			// Show with some delay to sync with reward's animation
			seq.Insert(0.15f, m_currentRewardSetup.godrays.transform.DOScale(0f, 0.05f).From().SetRecyclable(true));
		}

		// If the reward will be replaced, append the replace animation
		if(_dragonReward.WillBeReplaced()) {
			AppendReplacementAnim(ref seq);
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

			// Show godrays
			if(m_currentRewardSetup.godrays != null) {
				m_currentRewardSetup.godrays.gameObject.SetActive(true);
			}
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
    /// Start the remove ads reward flow.
    /// </summary>
    /// <param name="_removeAdsReward">Skin reward.</param>
    private void OpenRemoveAdsReward(Metagame.RewardRemoveAds _removeAdsReward)
    {

        HideAllRewards();

        m_currentRewardSetup = m_removeAdsRewardSetup;

        // Animate
        Vector2 baseIdleVelocity = m_dragController.idleVelocity;
        Sequence seq = DOTween.Sequence();

        seq.AppendCallback(() => {
            // Show UI
            m_rewardInfoUI.InitAndAnimate(_removeAdsReward);

            // Trigger SFX, depends on reward tyoe
            AudioController.Play(m_currentRewardSetup.sfx);

            // Show godrays
            if (m_currentRewardSetup.godrays != null)
            {
                m_currentRewardSetup.godrays.gameObject.SetActive(true);
            }
        });

        seq.Append(m_currentRewardSetup.view.transform.DOScale(0f, 1f).From().SetEase(Ease.OutBack));

        seq.Join(DOTween.To(
            () => { return baseIdleVelocity; }, // Getter
            (Vector2 _v) => { m_dragController.idleVelocity = _v; },    // Setter
            Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)), // Final value
            2f) // Duration
            .From()
            .SetEase(Ease.OutCubic)
        );

        seq.OnComplete(OnAnimationFinish);

    }

	/// <summary>
	/// Choose the rigth preview setup based on reward type, initializes it and
	/// makes it the current one (hiding all the oters).
	/// </summary>
	/// <param name="_reward">The reward to be used for initialization.</param>
	private void InitRewardView(Metagame.Reward _reward) {
		// Just switch reward type and choose the right view initializer
		switch(_reward.type) {
			// Items
			case Metagame.RewardEgg.TYPE_CODE: InitEggView(_reward as Metagame.RewardEgg);	break;
			case Metagame.RewardPet.TYPE_CODE: InitPetView(_reward as Metagame.RewardPet); break;
			case Metagame.RewardSkin.TYPE_CODE: InitSkinView(_reward as Metagame.RewardSkin); break;
			case Metagame.RewardDragon.TYPE_CODE: InitDragonView(_reward as Metagame.RewardDragon); break;

			// Currencies
			case Metagame.RewardHardCurrency.TYPE_CODE:
			case Metagame.RewardSoftCurrency.TYPE_CODE:
			case Metagame.RewardGoldenFragments.TYPE_CODE:
				InitCurrencyView(_reward as Metagame.RewardCurrency); break;
		}
	}

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
	/// Initialize the skin reward view with the given reward data.
	/// </summary>
	/// <param name="_removeAdsReward">Skin reward data.</param>
	private void InitSkinView(Metagame.RewardSkin _skinReward) {
		HideAllRewards();

		m_currentRewardSetup = m_skinRewardSetup;
		m_currentRewardSetup.view.SetActive(true);

		// Use a DragonLoader to simplify things
		MenuDragonLoader loader = m_currentRewardSetup.view.GetComponent<MenuDragonLoader>();
		loader.Setup(MenuDragonLoader.Mode.MANUAL, MenuDragonPreview.Anim.IDLE, true);
		loader.LoadDragon(_skinReward.def.GetAsString("dragonSku"), _skinReward.sku);
	}

	/// <summary>
	/// Initialize the dragon reward view with the given reward data.
	/// </summary>
	/// <param name="_dragonReward">Skin reward data.</param>
	private void InitDragonView(Metagame.RewardDragon _dragonReward) {
		HideAllRewards();

		m_currentRewardSetup = m_dragonRewardSetup;
		m_currentRewardSetup.view.SetActive(true);

		// Use a DragonLoader to simplify things
		MenuDragonLoader loader = m_currentRewardSetup.view.GetComponent<MenuDragonLoader>();
		loader.Setup(MenuDragonLoader.Mode.MANUAL, MenuDragonPreview.Anim.IDLE, true);
		loader.LoadDragon(_dragonReward.sku, IDragonData.GetDefaultDisguise(_dragonReward.sku).sku);
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
		TriggerFX(m_rarityFXSetup[(int)m_currentReward.rarity].openFX);

		// Trigger SFX
		// [AOC] Special SFX if it's a Halloween reward!
		Metagame.RewardEgg r = m_currentReward as Metagame.RewardEgg;
		if(r != null && r.reward.def.GetAsString("associatedSeason") == "halloween") {
			AudioController.Play(m_halloweenPetSFX);
		} else {
			AudioController.Play(m_rarityFXSetup[(int)m_currentReward.rarity].sfx);
		}

		// Program reward animation
		UbiBCN.CoroutineManager.DelayedCall(OnEggExplosionAnimFinished, 0.35f, false);
	}

	/// <summary>
	/// Append a replacement animation to the current reward animation.
	/// Replacement reward setup and text will be automatically chosen based on replacement reward.
	/// No checks performed!
	/// </summary>
	/// <param name="_seq">The sequence to be extended.</param>
	private void AppendReplacementAnim(ref Sequence _seq) {
		// Make sure current reward has a replacement
		if(m_currentReward.replacement == null) return;

		// Aux vars
		Metagame.Reward replacementReward = m_currentReward.replacement;

		// Replacement name: depends on type (consumables vs non-consumables)
		string replacementName = "";
		switch(replacementReward.type) {
			// Non-consumable rewards
			case Metagame.RewardDragon.TYPE_CODE:
			case Metagame.RewardSkin.TYPE_CODE:
			case Metagame.RewardPet.TYPE_CODE:
			case Metagame.RewardRemoveAds.TYPE_CODE: {
				// i.e. "Umbra", "Captain Geckles", "Burner King"...
				replacementName = LocalizationManager.SharedInstance.Localize(replacementReward.GetTID(false));
			} break;

			// Rest of rewards
			default: {
				// i.e. "300 Gems", "3 Rare Eggs", "1,200 Coins"...
				replacementName = LocalizationManager.SharedInstance.Localize(
					"TID_REWARD_AMOUNT",
					StringUtils.FormatNumber(replacementReward.amount, 0),
					LocalizationManager.SharedInstance.Localize(replacementReward.GetTID(replacementReward.amount > 1))
				);
			} break;
		}

		// Full replacement text
		// %U0 already owned!\nYou get %U1 instead!
		// Umbra already owned! You get Blaze instead! ----> Non-consumable as replacement: show name
		// Umbra already owned! You get 3,000 Coins instead! ----> Consumable as replacement: show amount and name
		// Coins already owned! You get 10 Gems instead! ----> Doesn't make any sense, but shouldn't actually happen because currencies and eggs are consumables and are never replaced.
		string replacementInfoText = LocalizationManager.SharedInstance.Localize(
			"TID_REWARD_DUPLICATED",
			LocalizationManager.SharedInstance.Localize(m_currentReward.GetTID(m_currentReward.amount > 1)),
			replacementName
		);

		// Choose target replacement setup
		RewardSetup replacementSetup = null;
		switch(m_currentReward.replacement.type) {
			case Metagame.RewardHardCurrency.TYPE_CODE: replacementSetup = m_hcRewardSetup;	break;
			case Metagame.RewardSoftCurrency.TYPE_CODE: replacementSetup = m_scRewardSetup; break;
			case Metagame.RewardGoldenFragments.TYPE_CODE: replacementSetup = m_goldenFragmentsRewardsSetup.Length > 0 ? m_goldenFragmentsRewardsSetup[0] : null; break;
			case Metagame.RewardDragon.TYPE_CODE: replacementSetup = m_dragonRewardSetup; break;
			case Metagame.RewardSkin.TYPE_CODE: replacementSetup = m_skinRewardSetup; break;
			case Metagame.RewardPet.TYPE_CODE: replacementSetup = m_petRewardSetup; break;
			case Metagame.RewardRemoveAds.TYPE_CODE: replacementSetup = m_removeAdsRewardSetup; break;
			//case Metagame.RewardEgg.TYPE_CODE: replacementSetup = m_hcRewardSetup; break;	// [AOC] TODO!!
		}

		// We have all we need! Call the parametrized version of this method
		AppendReplacementAnim(ref _seq, replacementSetup, replacementInfoText);
	}

	/// <summary>
	/// Append a replacement animation to the current reward animation.
	/// Version with more custom parameters.
	/// No checks performed!
	/// </summary>
	/// <param name="_seq">The sequence to be extended.</param>
	/// <param name="_replacementSetup">The setup to be displayed with the replacement.</param>
	/// <param name="_replacementInfoText">Extra text to be displayed with the replacement.</param>
	private void AppendReplacementAnim(ref Sequence _seq, RewardSetup _replacementSetup, string _replacementInfoText) {
		// Aux vars
		Metagame.Reward replacementReward = m_currentReward.replacement;

		// Reward acceleration
		// Make it compatible with the drag controller!
		Vector2 baseIdleVelocity = m_dragController.idleVelocity;
		_seq.Append(DOTween.To(
			() => { return baseIdleVelocity; }, // Getter
			(Vector2 _v) => { m_dragController.idleVelocity = _v; },    // Setter
			Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)), // Final value
			1.5f) // Duration
			.SetEase(Ease.InCubic)
		);

		// Hide UI Info
		_seq.InsertCallback(
			_seq.Duration() - 0.25f,
			() => { m_rewardInfoUI.showHideAnimator.Hide(); }
		);

		// Show VFX to cover the swap
		// We want it to launch a bit before doing the swap. To do so, use a combination of InserCallback() with the sequence's current duration.
		_seq.InsertCallback(_seq.Duration() - 0.15f, () => {
			TriggerFX(m_goldenFragmentsSwapFX);

			// Play SFX as well!
			AudioController.Play(_replacementSetup.sfx);
		});

		// Swap
		_seq.AppendCallback(() => {
			// Hide current godrays
			if(m_currentRewardSetup.godrays != null) {
				m_currentRewardSetup.godrays.gameObject.SetActive(false);
			}

			// Swap reward view with replacement view
			InitRewardView(replacementReward);

			// Make it target of the drag controller
			SetDragTarget(_replacementSetup.view.transform);
		});

		// Show replacement UI info
		_seq.AppendCallback(() => {
			// Reward info UI does all the hard work for us
			m_rewardInfoUI.InitAndAnimate(replacementReward, _replacementInfoText);

			// Show godrays
			if(_replacementSetup.godrays != null) {
				_replacementSetup.godrays.gameObject.SetActive(true);
			}
		});

		// Replacement reward initial inertia
		// Make it compatible with the drag controller!
		_seq.Join(DOTween.To(
			() => { return baseIdleVelocity; }, // Getter
			(Vector2 _v) => { m_dragController.idleVelocity = _v; },    // Setter
			Vector2.Scale(baseIdleVelocity, new Vector2(100f, 1f)),     // Final value
			2f) // Duration
			.From()
			.SetEase(Ease.OutCubic)
		);

		// Do some post-processing based on replacement reward
		// [AOC] Don't like doing this here, since it's a method that should be only
		//		 used for animation purposes, but don't have time for a better solution
		switch(replacementReward.type) {
			case Metagame.RewardDragon.TYPE_CODE: {
				// If we're in the right mode, make it the selected dragon
				IDragonData dragonData = DragonManager.GetDragonData(replacementReward.sku);
				if(dragonData != null) {
					// Tell the dragon selection screen for that dragon to make it the selected one next time we go there
					MenuDragonScreenController dragonSelectionScreen = InstanceManager.menuSceneController.GetScreenData(MenuScreen.DRAGON_SELECTION).ui.GetComponent<MenuDragonScreenController>();
					dragonSelectionScreen.pendingToSelectDragon = replacementReward.sku;
				}
			} break;
		}
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
			anim.SetTrigger( GameConstants.Animator.START );
		}
	}

	/// <summary>
	/// Instantiate and launch the confetti VFX.
	/// </summary>
	/// <param name="_triggerSound">Whether to also trigger confetti SFX.</param>
	private void LaunchConfettiFX(bool _triggerSound) {
		// Check required stuff
		if(m_confettiAnchor == null) return;
		if(m_confettiPrefab == null) return;

		// Create a new instance of the FX and put it on the dragon loader
		GameObject newObj = Instantiate<GameObject>(
			m_confettiPrefab,
			m_confettiAnchor,
			false
		);

		// Auto-destroy after the FX has finished
		// Sync with FX duration
		newObj.AddSelfDestroyComponent(9f, SelfDestroy.Mode.SECONDS);

		// Trigger SFX
		if(_triggerSound) AudioController.Play("hd_unlock_dragon");
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
			// Activate tap FX, both static and dynamic
			ParticleSystem tapFX = m_rarityFXSetup[(int)m_currentReward.rarity].tapFX;
			if(tapFX != null) {
				tapFX.transform.SetParentAndReset(m_eggView.anchorFX);
			}

			TriggerFX(m_rarityFXSetup[(int)m_currentReward.rarity].tapFX);
			TriggerFX(m_rarityFXSetup[(int)m_currentReward.rarity].tapFXStatic);

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
		// Notify external script
		OnAnimFinished.Invoke();
	}

	/// <summary>
	/// The menu screen change animation is about to start.
	/// </summary>
	/// <param name="_from">Screen we come from.</param>
	/// <param name="_to">Screen we're going to.</param>
	private void OnMenuScreenTransitionStart(MenuScreen _from, MenuScreen _to) {
		// Check params
		if(_from == MenuScreen.NONE || _to == MenuScreen.NONE) return;

		// Aux vars
		MenuScreenScene fromScene = InstanceManager.menuSceneController.GetScreenData(_from).scene3d;
		MenuScreenScene toScene = InstanceManager.menuSceneController.GetScreenData(_to).scene3d;

		// Leaving a screen using this scene
		if(fromScene != null && fromScene.gameObject == this.gameObject) {
			// Do some stuff if not going to take a picture of the reward
			if(_to != MenuScreen.PHOTO) {
				// Clear the scene
				Clear();

				// Nullify reward reference
				m_currentReward = null;

				// Remove screen from screen history
				// We want to prevent going back to this screen which has already been cleared
				// At this point (SCREEN_TRANSITION_START) the screen has already been added to the history
				// Resolves issue HDK-3436 among others
				List<MenuScreen> history = InstanceManager.menuSceneController.transitionManager.screenHistory;
				if(history.Last() == _from) {	// Make sure the screen we come from is actually in the history (i.e. screens that don't allow back to them are not added to the history)
					history.RemoveAt(history.Count - 1);
				}
			}
		}
	}
}
 