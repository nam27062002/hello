// OpenEggScreenController.cs
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
/// Global controller for the Open Egg screen in the main menu.
/// </summary>
public class OpenEggScreenController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum State {
		IDLE,
		INTRO_DELAY,
		INTRO,
		OPENING
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed References
	[Separator("Info")]
	[SerializeField] private Localizer m_tapInfoText = null;

	[Separator("Buttons")]
	[SerializeField] private ShowHideAnimator m_actionButtonsAnimator = null;
	[SerializeField] private Button m_instantOpenButton = null;
	[SerializeField] private Button m_callToActionButton = null;
	[SerializeField] private Button m_buyButton = null;
	[SerializeField] private Button m_backButton = null;

	[Separator("Rewards")]
	[SerializeField] private GameObject m_rewardInfo = null;
	[SerializeField] private DisguiseRarityTitle m_rewardRarity = null;
	[SerializeField] private Localizer m_rewardDescText = null;
	[SerializeField] private GameObject m_rewardPowers = null;

	// References
	private EggController m_egg = null;
	public EggController egg {
		get { return m_egg; }
	}

	private MenuScreenScene m_scene = null;		// Reference to the 3d scene
	private Transform m_eggAnchor = null;
	private Transform m_rewardAnchor = null;
	private GameObject m_rewardView = null;
	private EggUIScene3D m_buyButtonEggScene = null;

	// Internal
	private State m_state = State.IDLE;

	// FX
	private GameObject m_flashFX = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Prepare the flash FX image
		m_flashFX = new GameObject("FlashFX");
		if(m_flashFX != null) {
			// Transform - full screen rect transform
			RectTransform rectTransform = m_flashFX.AddComponent<RectTransform>();
			rectTransform.SetParent(this.transform, false);
			rectTransform.anchorMin = Vector2.zero;
			rectTransform.anchorMax = Vector2.one;
			rectTransform.offsetMin = Vector2.zero;
			rectTransform.offsetMax = Vector2.zero;

			// Image
			Image image = m_flashFX.AddComponent<Image>();
			image.color = Colors.white;

			// Start hidden
			m_flashFX.SetLayerRecursively("UI");
			m_flashFX.SetActive(false);
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events.
		Messenger.AddListener<Egg>(GameEvents.EGG_COLLECTED, OnEggCollected);
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// State-dependent
		if(m_state == State.INTRO_DELAY) {
			// Has camera finished moving?
			if(!InstanceManager.GetSceneController<MenuSceneController>().screensController.tweening) {
				// Yes!! Launch intro
				LaunchIntroNewEgg();
			}
		}
	}

	/// <summary>
	/// Raises the disable event.
	/// </summary>
	private void OnDisable() {
		// Reset state
		m_state = State.IDLE;
		if(m_egg != null) {
			GameObject.Destroy(m_egg.gameObject);
			m_egg = null;
		}

		if(m_rewardView != null) {
			GameObject.Destroy(m_rewardView);
			m_rewardView = null;
		}

		// Unsubscribe to external events.
		Messenger.RemoveListener<Egg>(GameEvents.EGG_COLLECTED, OnEggCollected);
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEvent>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	private void OnDestroy() {
		// Destroy Egg 3D scene
		if(m_buyButtonEggScene != null) {
			UIScene3DManager.Remove(m_buyButtonEggScene);
			m_buyButtonEggScene = null;
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Launch an open flow with a given Egg instance.
	/// </summary>
	/// <param name="_egg">The egg to be opened.</param>
	/// <param name="_eggView">Optionally reuse an existing egg on the scene. If <c>null</c>, a new instance of the egg prefab will be used.</param>
	public void StartFlow(Egg _egg, EggController _eggView = null) {
		// Check params
		if(_egg == null) return;
		if(_egg.state != Egg.State.READY) return;
		if(m_state != State.IDLE) return;

		// Make sure all required references are set
		ValidateReferences();

		// If we already have an egg on screen, clear it
		if(m_egg != null) {
			GameObject.Destroy(m_egg.gameObject);
			m_egg = null;
		}

		// Same with reward view
		if(m_rewardView != null) {
			GameObject.Destroy(m_rewardView);
			m_rewardView = null;
		}

		// Hide HUD and buttons
		bool animate = this.gameObject.activeInHierarchy;	// If the screen is not visible, don't animate
		InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().ForceHide(animate);
		m_actionButtonsAnimator.GetComponent<ShowHideAnimator>().ForceHide(animate);
		m_instantOpenButton.GetComponent<ShowHideAnimator>().ForceHide(animate);
		m_tapInfoText.GetComponent<ShowHideAnimator>().ForceHide(animate);
		m_backButton.GetComponent<ShowHideAnimator>().ForceHide(animate);

		// Hide Flash FX
		if(m_flashFX != null) m_flashFX.SetActive(false);

		// Hide reward elements
		m_rewardInfo.GetComponent<ShowHideAnimator>().Hide(false);
		m_rewardPowers.GetComponent<ShowHideAnimator>().Hide(false);

		// Reuse an existing egg view or create a new one?
		if(_eggView == null) {
			// Create a new instance of the egg prefab
			m_egg = _egg.CreateView();

			// Attach it to the 3d scene's anchor point
			m_egg.transform.SetParent(m_eggAnchor, false);
			m_egg.transform.position = m_eggAnchor.position;

			// Launch intro as soon as possible (wait for the camera to stop moving)
			m_egg.gameObject.SetActive(false);
			m_state = State.INTRO_DELAY;
		} else {
			// Change hierarchy on the 3d scene - keep current position
			m_egg = _eggView;
			m_egg.transform.SetParent(m_eggAnchor, true);

			// Immediately launch intro animation (skip INTRO_DELAY) state
			LaunchIntroExistingEgg();
		}

		// Initialize buy button
		if(m_egg != null) {
			// Price
			m_buyButton.FindComponentRecursive<Text>("TextPrice").text = StringUtils.FormatNumber(m_egg.eggData.def.GetAsInt("pricePC"));

			// Initialize the scene with the egg
			// The scene will take care of everything
			m_buyButtonEggScene.SetEgg(Egg.CreateFromDef(m_egg.eggData.def));
		}
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Make sure all required references are initialized.
	/// </summary>
	private void ValidateReferences() {
		// 3d scene for this screen
		if(m_scene == null) {
			MenuSceneController sceneController = InstanceManager.GetSceneController<MenuSceneController>();
			Debug.Assert(sceneController != null, "This component must be only used in the menu scene!");
			m_scene = sceneController.screensController.GetScene((int)MenuScreens.OPEN_EGG);
		}

		// Egg view anchor in the 3d scene
		if(m_eggAnchor == null) {
			if(m_scene != null) {
				m_eggAnchor = m_scene.FindTransformRecursive("OpenEggAnchor");
				Debug.Assert(m_eggAnchor != null, "Required \"OpenEggAnchor\" transform not found!");
			}
		}

		// Reward anchor in the 3d scene
		if(m_rewardAnchor == null) {
			if(m_scene != null) {
				m_rewardAnchor = m_scene.FindTransformRecursive("RewardAnchor");
				Debug.Assert(m_rewardAnchor != null, "Required \"RewardAnchor\" transform not found!");
			}
		}

		// Egg scene for the buy button
		if(m_buyButtonEggScene == null) {
			// Create a new scene and link it to the buy button raw image
			m_buyButtonEggScene = EggUIScene3D.CreateEmpty();
			RawImage eggPreviewArea = m_buyButton.GetComponentInChildren<RawImage>();
			m_buyButtonEggScene.InitRawImage(ref eggPreviewArea);
		}
	}

	//------------------------------------------------------------------//
	// ANIMATIONS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Start the intro animation for a new egg!
	/// </summary>
	private void LaunchIntroNewEgg() {
		// Assume we can do it (no checks)
		// Activate egg
		m_egg.gameObject.SetActive(true);

		// [AOC] TODO!! Some awesome FX!!
		m_egg.transform.DOScale(0f, 0.5f).From().SetEase(Ease.OutElastic).OnComplete(OnIntroFinished);

		// Change logic state
		m_state = State.INTRO;
	}

	/// <summary>
	/// Start the intro animation for an existing egg!
	/// </summary>
	private void LaunchIntroExistingEgg() {
		// Assume we can do it (no checks)
		// Make sure egg is active
		m_egg.gameObject.SetActive(true);

		// [AOC] TODO!! Some awesome FX!!
		m_egg.transform.DOMove(m_eggAnchor.position, 0.5f).SetEase(Ease.InOutCirc).OnComplete(OnIntroFinished);	// Try to sync with camera transition (values copied from MenuScreensController)

		// Change logic state
		m_state = State.INTRO;
	}

	/// <summary>
	/// Launches the open egg animation!
	/// </summary>
	private void LaunchOpenAnimation() {
		// This option should only be available on the OPENING state and with a valid egg
		if(m_state != State.OPENING) return;
		if(m_egg == null) return;

		// [AOC] TODO!! Nice FX!
		// Do a full-screen flash FX
		if(m_flashFX != null) {
			m_flashFX.SetActive(true);
			m_flashFX.GetComponent<Image>().color = Colors.white;
			m_flashFX.GetComponent<Image>().DOFade(0f, 1f).SetEase(Ease.OutExpo).SetRecyclable(true).OnComplete(() => { m_flashFX.SetActive(false); });
		}

		// [AOC] TEMP!! Some dummy effect on the egg xD
		//m_egg.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).OnComplete(() => { m_egg.gameObject.SetActive(false); });
		m_egg.gameObject.SetActive(false);

		// Show reward info
		LaunchRewardAnimation();

		// Show/Hide buttons and HUD
		InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().Show();
		//m_callToActionButton.GetComponent<ShowHideAnimator>().Show(false);	// Always visible for now
		m_actionButtonsAnimator.GetComponent<ShowHideAnimator>().Show();
		m_instantOpenButton.GetComponent<ShowHideAnimator>().Hide();
		m_tapInfoText.GetComponent<ShowHideAnimator>().Hide();
		m_backButton.GetComponent<ShowHideAnimator>().Show();

		// Change logic state
		m_state = State.IDLE;
	}

	/// <summary>
	/// Launches the animation of the reward components.
	/// </summary>
	private void LaunchRewardAnimation() {
		// Aux vars
		Egg.EggReward rewardData = m_egg.eggData.rewardData;
		DefinitionNode rewardDef = m_egg.eggData.rewardDef;
		string rewardType = m_egg.eggData.rewardData.type;

		// Activate info container
		m_rewardInfo.GetComponent<ShowHideAnimator>().Show(false);

		// Different initializations based on reward type
		switch(rewardType) {
			case "suit": {
				// Get disguise def
				DefinitionNode disguiseDef = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES, rewardData.value);

				// Disguise rarity
				m_rewardRarity.InitFromRarity(disguiseDef.GetAsString("rarity"), rewardDef.GetLocalized("tidName"));

				// Different texts if the disguise was just unlocked, it was upgraded or it was already maxed
				int disguiseLevel = Wardrobe.GetDisguiseLevel(disguiseDef.sku);
				if(rewardData.coins > 0) {
					m_rewardDescText.Localize("TID_EGG_REWARD_DISGUISE_MAXED", disguiseDef.GetLocalized("tidName"), StringUtils.FormatNumber(rewardData.coins));
				} else if(disguiseLevel == 1) {
					m_rewardDescText.Localize("TID_EGG_REWARD_DISGUISE_UNLOCKED", disguiseDef.GetLocalized("tidName"));
				} else {
					m_rewardDescText.Localize("TID_EGG_REWARD_DISGUISE_UPGRADED", disguiseDef.GetLocalized("tidName"));
				}
			} break;

			case "pet": {
				// Pet rarity
				string rarity = rewardDef.sku.Replace("pet_", "");
				m_rewardRarity.InitFromRarity(rarity, rewardDef.GetLocalized("tidName"));

				// [AOC] TODO!!
				//m_rewardDescText.Localize("TID_EGG_REWARD_PET", rewardDef.sku);
				m_rewardDescText.Localize("TID_GEN_COMING_SOON");
			} break;

			case "dragon": {
				// Rarity
				m_rewardRarity.InitFromRarity("epic", rewardDef.GetLocalized("tidName"));

				// [AOC] TODO!!
				//m_rewardDescText.Localize("TID_EGG_REWARD_DRAGON", rewardDef.sku);
				m_rewardDescText.Localize("TID_GEN_COMING_SOON");
			} break;
		}

		// Rarity title animation
		CanvasGroup rarityCanvasGroup = m_rewardRarity.ForceGetComponent<CanvasGroup>();
		rarityCanvasGroup.alpha = 1f;
		rarityCanvasGroup.DOFade(0f, 0.15f).From().SetEase(Ease.Linear).SetRecyclable(true);
		m_rewardRarity.transform.localScale = Vector3.one;
		m_rewardRarity.transform.DOScale(3f, 0.30f).From().SetEase(Ease.OutCubic).SetRecyclable(true);

		// Description text animation
		m_rewardDescText.text.color = Colors.WithAlpha(m_rewardDescText.text.color, 1f);
		m_rewardDescText.transform.DOBlendableLocalMoveBy(Vector3.right * 500f, 0.30f).From().SetDelay(0.20f).SetEase(Ease.OutCubic).SetRecyclable(true);
		m_rewardDescText.text.DOFade(0f, 0.15f).From().SetDelay(0.20f).SetEase(Ease.Linear).SetRecyclable(true);

		// Create a fake reward view
		switch(rewardType) {
			case "dragon":
			case "pet":
			case "suit": {
				// Initialize reward view
				// Attach it to the anchor and reset transformation
				m_rewardView = new GameObject("RewardView");
				m_rewardView.transform.SetParentAndReset(m_rewardAnchor);

				// Instantiate dragon view
				DefinitionNode dragonDef = DefinitionsManager.GetDefinition(DefinitionsCategory.DRAGONS, m_egg.eggData.def.GetAsString("dragonSku"));
				if(dragonDef != null) {
					// Create instance
					GameObject prefab = Resources.Load<GameObject>(dragonDef.GetAsString("menuPrefab"));
					GameObject dragonObj = GameObject.Instantiate<GameObject>(prefab);

					// Attach it to the reward view and reset transformation
					// The anchor is setup to display all dragons at scale 1
					// [AOC] TODO!! Add some scale factor more or less proportional to content scale factor
					dragonObj.transform.SetParentAndReset(m_rewardView.transform);

					// Launch fly animation
					dragonObj.GetComponentInChildren<Animator>().SetTrigger("fly_idle");

					// If it's a suit, apply it
					if(rewardType == "suit") {
						dragonObj.GetComponent<DragonEquip>().PreviewDisguise(rewardData.value);
					}

					// Animate it
					dragonObj.transform.DOScale(0f, 0.75f).SetDelay(0f).From().SetRecyclable(true).SetEase(Ease.OutElastic);
					dragonObj.transform.DOLocalRotate(dragonObj.transform.localRotation.eulerAngles + Vector3.up * 360f, 10f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetDelay(0.5f).SetRecyclable(true);
				}

				// If the reward is being replaced by coins, show it
				if(rewardData.coins > 0) {
					// Create instance
					GameObject prefab = Resources.Load<GameObject>("UI/Metagame/Rewards/PF_CoinsReward");
					GameObject coinsObj = GameObject.Instantiate<GameObject>(prefab);

					// Attach it to the reward view and reset transformation
					coinsObj.transform.SetParent(m_rewardView.transform, false);
				}
			} break;
		}

		// If reward is a disguise, initialize and show powers
		if(rewardType == "suit") {
			// Show
			m_rewardPowers.GetComponent<ShowHideAnimator>().Show(false);

			// Aux vars
			DefinitionNode disguiseDef = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES, rewardData.value);
			int disguiseLevel = Wardrobe.GetDisguiseLevel(disguiseDef.sku);

			// Initialize with actual powers data
			DefinitionNode powerSetDef = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES_POWERUPS, disguiseDef.GetAsString("powerupSet"));
			for(int i = 0; i < 3; i++) {
				// Get def
				string powerUpSku = powerSetDef.GetAsString("powerup"+(i+1).ToString());
				DefinitionNode powerDef = DefinitionsManager.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);

				// Get and initialize power button
				DisguisePowerIcon powerIcon = m_rewardPowers.FindComponentRecursive<DisguisePowerIcon>("Power" + (i+1).ToString());
				powerIcon.InitFromDefinition(powerDef, i >= disguiseLevel);

				// Intro animation
				float delay = 0.65f + i * 0.15f;
				powerIcon.transform.DOScale(2f, 0.25f).From().SetDelay(delay).SetEase(Ease.OutCubic).SetRecyclable(true);
				powerIcon.GetComponent<CanvasGroup>().DOFade(0f, 0.15f).From().SetDelay(delay).SetRecyclable(true);

				// If just unlocked this power, add a nice unlock effect
				// The power will just be unlocked if the power index matches the level of the disguise and there is no coins reward (meaning disguise is not already maxed)
				GameObject godraysObj = powerIcon.FindObjectRecursive("godrayFX");	// Find previously instantiated effects first
				if(i == (disguiseLevel - 1) && rewardData.coins <= 0) {
					// Anim params
					float speedMult = 1f;	// To easily adjust timings

					// Show lock and animate it out
					Image lockIcon = powerIcon.FindComponentRecursive<Image>("IconLock");
					lockIcon.gameObject.SetActive(true);

					// Lock scale up
					delay += 0.1f;	// Sync with icon anim
					lockIcon.transform.DOScale(4f, 1.0f * speedMult).SetDelay(delay).SetEase(Ease.OutExpo);

					// Lock fade out (halfway through the scale up anim)
					delay += 0.75f * speedMult;
					lockIcon.DOFade(0f, 0.25f * speedMult).SetDelay(delay)
						.OnComplete(() => {
							// Reset icon for next time
							lockIcon.color = Color.white; 
							lockIcon.transform.localScale = Vector3.one;
							lockIcon.gameObject.SetActive(false);
						});

					// If not already done, instantiate God Rays effect
					if(godraysObj == null) {
						GameObject prefab = Resources.Load<GameObject>("UI/Common/Effects/PF_GodRayFX");
						godraysObj = GameObject.Instantiate<GameObject>(prefab);
						godraysObj.name = "godrayFX";
						godraysObj.transform.SetParentAndReset(powerIcon.transform);
						(godraysObj.transform as RectTransform).anchoredPosition = Vector2.zero;
						godraysObj.transform.SetSiblingIndex(0);	// At the bottom of the group
					}
					godraysObj.SetActive(true);

					// Animate godrays effect
					// Scale in
					delay += -0.10f * speedMult;	// Around the same time as the lock starts fading out
					godraysObj.transform.localScale = Vector3.zero;
					godraysObj.transform.DOScale(3f, 0.15f * speedMult).SetDelay(delay);

					// Scale back to default
					delay += 0.15f * speedMult;	// Right after the scale in
					godraysObj.transform.DOScale(1f, 0.25f * speedMult).SetDelay(delay);
				} else {
					// This power was not unlocked
					// Disable godray effect, if any
					if(godraysObj != null) godraysObj.SetActive(false);
				}
			}
		} else {
			m_rewardPowers.GetComponent<ShowHideAnimator>().Hide(false);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The intro has finished!
	/// </summary>
	private void OnIntroFinished() {
		// Change egg state
		m_egg.eggData.ChangeState(Egg.State.OPENING);

		// Show instant open button and info text
		m_instantOpenButton.GetComponent<ShowHideAnimator>().Show();
		m_tapInfoText.GetComponent<ShowHideAnimator>().Show();

		// Change logic state
		m_state = State.OPENING;
	}

	/// <summary>
	/// Skip the egg tapping process.
	/// </summary>
	public void OnInstantOpenButton() {
		// Open the egg!
		// This option should only be available on the OPENING state and with a valid egg
		if(m_state != State.OPENING) return;
		if(m_egg == null) return;
		if(m_egg.eggData.state != Egg.State.OPENING) return;

		// Collect the egg! - this automatically empties the incubator
		m_egg.eggData.Collect();
		PersistenceManager.Save();

		// Animation will be triggered by the EGG_COLLECTED event
	}

	/// <summary>
	/// Depending on opened egg's reward, perform different actions.
	/// </summary>
	public void OnCallToActionButton() {
		// This option should only be available on the IDLE state and with a valid egg
		if(m_state != State.IDLE) return;
		if(m_egg == null) return;
		if(m_egg.eggData.state != Egg.State.COLLECTED) return;

		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");
		// Depending on opened egg's reward, perform different actions
		switch(m_egg.eggData.rewardData.type) {
			case "suit": {
				NavigationScreen disguiseScreen = screensController.GetScreen((int)MenuScreens.DISGUISES);				
				disguiseScreen.FindComponentRecursive<DisguisesScreenController>().previewDisguise = m_egg.eggData.rewardData.value;
				screensController.GoToScreen((int)MenuScreens.DISGUISES);
			} break;

			case "pet":
			case "dragon": {
				// [AOC] TODO!!	Go to pets/special dragons screen
				UIFeedbackText.CreateAndLaunch(Localization.Localize("TID_GEN_COMING_SOON"), m_callToActionButton.transform as RectTransform, Vector2.zero, this.transform as RectTransform);
			} break;
		}
	}

	/// <summary>
	/// Buy another egg.
	/// </summary>
	public void OnBuyButton() {
		// This option should only be available on the IDLE state
		if(m_state != State.IDLE) return;

		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Get price and start purchase flow
		long pricePC = m_egg.eggData.def.GetAsLong("pricePC");
		if(UserProfile.pc >= pricePC) {
			// Perform transaction
			UserProfile.AddPC(-pricePC);
			PersistenceManager.Save();

			// Create a new egg instance
			Egg purchasedEgg = Egg.CreateFromDef(m_egg.eggData.def);
			purchasedEgg.ChangeState(Egg.State.READY);	// Already ready for collection!

			// Restart flow!
			StartFlow(purchasedEgg);
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(Localization.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// An egg has been opened and its reward collected.
	/// </summary>
	/// <param name="_egg">The egg.</param>
	private void OnEggCollected(Egg _egg) {
		// Must have a valid egg
		if(m_egg == null) return;

		// If it matches our curent egg, launch its animation!
		if(_egg == m_egg.eggData) {
			// Launch animation!
			LaunchOpenAnimation();
		}
	}

	/// <summary>
	/// Navigation screen has changed (animation starts now).
	/// </summary>
	/// <param name="_event">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEvent _event) {
		// Only if it comes from the main screen navigation system
		if(_event.dispatcher != InstanceManager.GetSceneController<MenuSceneController>().screensController) return;

		// If leaving this screen, launch all the hide animations that are not automated
		if(_event.fromScreenIdx == (int)MenuScreens.OPEN_EGG) {
			// Hide reward elements
			m_rewardInfo.GetComponent<ShowHideAnimator>().Hide();
			m_rewardPowers.GetComponent<ShowHideAnimator>().Hide();

			// Destroy reward view
			if(m_rewardView != null) {
				GameObject.Destroy(m_rewardView);
				m_rewardView = null;
			}

			// Restore HUD
			InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().Show();
		}

		// If entering this screen, force some show/hide animations that conflict with automated ones
		if(_event.fromScreenIdx == (int)MenuScreens.OPEN_EGG) {
			// At this point automated ones have already been launched, so we override them
			m_backButton.GetComponent<ShowHideAnimator>().Hide(false);
			m_tapInfoText.GetComponent<ShowHideAnimator>().Hide(false);
		}
	}
}