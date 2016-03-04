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
	[SerializeField] private ShowHideAnimator m_actionButtonsAnimator = null;
	[SerializeField] private Button m_instantOpenButton = null;
	[SerializeField] private Button m_callToActionButton = null;
	[SerializeField] private Button m_shopButton = null;
	[SerializeField] private Button m_backButton = null;

	// References
	private EggController m_egg = null;
	public EggController egg {
		get { return m_egg; }
	}

	private MenuScreenScene m_scene = null;		// Reference to the 3d scene
	private Transform m_eggAnchor = null;

	// Internal
	private State m_state = State.IDLE;

	// FX
	private GameObject m_flashFX = null;

	// Temp!!
	[Comment("TEMP PLACEHOLDERS!!", 10f)]
	[SerializeField] private Text m_rewardText = null;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_actionButtonsAnimator != null, "Required field!");
		Debug.Assert(m_instantOpenButton != null, "Required field!");
		Debug.Assert(m_callToActionButton != null, "Required field!");
		Debug.Assert(m_shopButton != null, "Required field!");
		Debug.Assert(m_backButton != null, "Required field!");
		Debug.Assert(m_rewardText != null, "Required field!");

		// Prepare the flash FX image
		m_flashFX = new GameObject("FlashFX");
		if(m_flashFX) {
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

		// Restore HUD
		InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().Show();

		// Unsubscribe to external events.
		Messenger.RemoveListener<Egg>(GameEvents.EGG_COLLECTED, OnEggCollected);
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

		// Hide HUD and buttons
		InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().Hide();
		m_actionButtonsAnimator.Hide();
		m_instantOpenButton.GetComponent<ShowHideAnimator>().Hide();
		m_backButton.GetComponent<ShowHideAnimator>().Hide(false);
		m_backButton.gameObject.SetActive(false);	// This will instantly disable the back button so the NavigationShowHideAnimator doesn't trigger when opening the screen

		// Hide Flash FX and temp reward text
		m_flashFX.SetActive(false);
		m_rewardText.gameObject.SetActive(false);

		// Reuse an existing egg view or create a new one?
		if(_eggView == null) {
			// Create a new instance of the egg prefab
			m_egg = _egg.CreateInstance();

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
		//m_flashFX.SetActive(true);
		//m_flashFX.GetComponent<Image>().color = Colors.white;
		//m_flashFX.GetComponent<Image>().DOFade(0f, 1f).SetEase(Ease.OutExpo).SetRecyclable(true).OnComplete(() => { m_flashFX.SetActive(false); });

		// [AOC] TEMP!! Some dummy effect on the egg xD
		m_egg.GetComponent<DOTweenAnimation>().DOKill();
		m_egg.transform.DOScale(new Vector3(15f, 5f, 15f), 1.0f).SetDelay(0.10f).SetEase(Ease.OutElastic);

		// [AOC] TODO!! Replace egg view by the reward prefab
		m_rewardText.gameObject.SetActive(true);
		//m_rewardText.transform.DOScale(0f, 0.5f).SetDelay(0.5f).From().SetEase(Ease.OutElastic).SetRecyclable(true);
		m_rewardText.transform.DOBlendableLocalMoveBy(Vector3.up * 500f, 0.30f).From().SetEase(Ease.OutBounce).SetRecyclable(true);
		m_rewardText.DOFade(0f, 0.15f).From().SetEase(Ease.Linear).SetRecyclable(true);
		m_rewardText.text = "You've got " + m_egg.eggData.rewardDef.sku + " for dragon " + m_egg.eggData.def.GetAsString("dragonSku") + "!";

		// Show/Hide buttons and HUD
		//InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().Show();	// Keep HUD hidden
		m_actionButtonsAnimator.Show();
		m_instantOpenButton.GetComponent<ShowHideAnimator>().Hide();
		m_backButton.gameObject.SetActive(true);	// This will instantly disable the back button so the NavigationShowHideAnimator doesn't trigger when opening the screen
		m_backButton.GetComponent<ShowHideAnimator>().Show();

		// Change logic state
		m_state = State.IDLE;
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

		// Show instant open button
		m_instantOpenButton.GetComponent<ShowHideAnimator>().Show();

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

		// Depending on opened egg's reward, perform different actions
		switch(m_egg.eggData.rewardDef.GetAsString("type")) {
			case "suit": {
				// [AOC] TODO!! Go to suits screen
			} break;

			case "pet": {
				// [AOC] TODO!! Go to pets screen
			} break;

			case "dragon": {
				// [AOC] TODO!!	Go to special dragons screen
			} break;
		}
	}

	/// <summary>
	/// Show the eggs shop popup.
	/// </summary>
	public void OnShopButton() {
		// This option should only be available on the IDLE state
		if(m_state != State.IDLE) return;

		// [AOC] TODO!! Show shop
		// Simulate a new egg's instant purchase
		Egg purchasedEgg = Egg.CreateBySku(Definitions.GetDefinitions(Definitions.Category.EGGS).GetRandomValue().sku);	// Pick a random egg from the definitions set
		purchasedEgg.ChangeState(Egg.State.READY);
		StartFlow(purchasedEgg);	// Restart flow!!
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
}