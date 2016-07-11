// DisguisesScreenController.cs
// Hungry Dragon
// 
// Created by Marc Saña Forrellach on DD/MM/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the disguises screen.
/// </summary>
public class DisguisesScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[Separator("References")]
	//[SerializeField] private DisguiseRarityTitle m_disguiseTitle;
	[SerializeField] private DisguisePowerIcon[] m_powers;
	[SerializeField] private RectTransform m_layout;

	// Buttons
	[Separator("Buttons")]
	[SerializeField] private GameObject m_buyButton;

	// Preview
	private Transform m_previewAnchor;
	private Transform m_dragonRotationArrowsPos;

	// Set it before opening this screen to start with a specific disguise selected
	private string m_previewDisguise = "";
	public string previewDisguise {
		get { return m_previewDisguise; }
		set { m_previewDisguise = value; }
	}

	// Pills management
	private DisguisePill[] m_pills;
	private DisguisePill m_equippedPill;	// Pill corresponding to the equipped disguise 
	private DisguisePill m_selectedPill;	// Pill corresponding to the selected disguise

	// Powers
	private ShowHideAnimator[] m_powerAnims = new ShowHideAnimator[3];

	// Buy button
	private DefinitionNode m_eggDef = null;	// Egg matching dragon sku
	private EggUIScene3D m_eggPreviewScene = null;	// Container holding the preview scene (camera, egg, decos, etc.)

	// Other data
	private string m_dragonSku;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Instantiate pills
		m_pills = new DisguisePill[9];
		GameObject prefab = Resources.Load<GameObject>("UI/Popups/Disguises/PF_DisguisesPill");
		for (int i = 0; i < 9; i++) {
			GameObject pill = GameObject.Instantiate<GameObject>(prefab);
			pill.transform.parent = m_layout;
			pill.transform.localScale = Vector3.one;

			m_pills[i] = pill.GetComponent<DisguisePill>();
			m_pills[i].OnPillClicked.AddListener(OnPillClicked);
		}

		m_dragonSku = "";
		m_eggDef = null;

		// Store some references
		for(int i = 0; i < m_powers.Length; i++) {
			m_powerAnims[i] = m_powers[i].GetComponent<ShowHideAnimator>();
			Debug.Log("power anim for " + i + ": " + (m_powerAnims[i] == null ? "NULL" : m_powerAnims[i].ToString()));
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Use internal initializer
		//Initialize();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		/*Canvas canvas = GetComponentInParent<Canvas>();
		Vector3 viewportPos = canvas.worldCamera.WorldToViewportPoint(m_dragonUIPos.position);

		Camera camera = InstanceManager.GetSceneController<MenuSceneController>().screensController.camera;
		viewportPos.z = m_depth;
		m_previewAnchor.position = camera.ViewportToWorldPoint(viewportPos);
		m_dragonRotationArrowsPos.position = camera.ViewportToWorldPoint(viewportPos) + Vector3.down;*/
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Use internal finalizer
		Finalize();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Destroy Egg 3D scene
		if(m_eggPreviewScene != null) {
			UIScene3DManager.Remove(m_eggPreviewScene);
			m_eggPreviewScene = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Trigger all animators needed to display the pets screen.
	/// </summary>
	public void Show() {
		// The list
		this.GetComponent<ShowHideAnimator>().Show();

		// Make sure the screen is properly initialized
		Initialize();
	}

	/// <summary>
	/// Trigger all animators needed to hide the pets screen.
	/// </summary>
	public void Hide() {
		// The list
		this.GetComponent<ShowHideAnimator>().Hide();

		// Make sure the screen is properly finalized
		Finalize();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup the screen with the data of the currently selected dragon.
	/// </summary>
	private void Initialize() {
		// Get target dragon
		m_dragonSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;

		// Find the 3D dragon preview
		MenuScreenScene scene = InstanceManager.GetSceneController<MenuSceneController>().screensController.GetScene((int)MenuScreens.DISGUISES);
		if(scene != null) {
			MenuDragonPreview preview = scene.GetComponent<MenuDragonScroller3D>().GetDragonPreview(m_dragonSku);
			if(preview != null) m_previewAnchor = preview.transform;
			//m_dragonRotationArrowsPos = scene.transform.FindChild("Arrows");
		}

		// Get egg corresponding to target dragon
		m_eggDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.EGGS, "dragonSku", m_dragonSku);

		// get disguises levels of the current dragon
		List<DefinitionNode> defList = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", m_dragonSku);
		DefinitionsManager.SharedInstance.SortByProperty(ref defList, "shopOrder", DefinitionsManager.SortType.NUMERIC);

		// Load disguise icons for this dragon
		Sprite[] icons = Resources.LoadAll<Sprite>("UI/Popups/Disguises/" + m_dragonSku);

		// Hide all the info
		//m_disguiseTitle.GetComponent<ShowHideAnimator>().ForceHide(false);
		for(int i = 0; i < m_powerAnims.Length; i++) {
			m_powerAnims[i].ForceHide(false);
		}

		// Find out initial disguise
		// Dragon's current disguise by default, but can be overriden by setting the previewDisguise property before opening the screen
		string currentDisguise = Wardrobe.GetEquipedDisguise(m_dragonSku);
		if (m_previewDisguise != "") {
			currentDisguise = m_previewDisguise;
			m_previewDisguise = "";
		}

		// Initialize pills
		m_equippedPill = null;
		m_selectedPill = null;
		DisguisePill initialPill = m_pills[0];	// There will always be at least the default pill
		for (int i = 0; i < m_pills.Length; i++) {
			if (i <= defList.Count) {
				if (i == 0) {
					// First pill is the default one
					m_pills[i].LoadAsDefault(GetFromCollection(ref icons, "icon_default"));
				} else {
					// Standard pill
					DefinitionNode def = defList[i - 1];

					Sprite spr = GetFromCollection(ref icons, def.GetAsString("icon"));
					int level = Wardrobe.GetDisguiseLevel(def.sku);
					m_pills[i].Load(def, level, spr);

					// Is it the initial pill?
					if(def.sku == currentDisguise) {
						initialPill = m_pills[i];
					}
				}
				m_pills[i].Use(false);
				m_pills[i].Select(false);
				m_pills[i].gameObject.SetActive(true);
			} else {
				// Unused pill
				m_pills[i].gameObject.SetActive(false);
			}
		}

		// Force a first refresh
		// This will initialize both the equipped and selected pills as well
		OnPillClicked(initialPill);

		// Initialize buy button
		if(m_eggDef != null) {
			// Price
			m_buyButton.FindComponentRecursive<Text>("TextPrice").text = StringUtils.FormatNumber(m_eggDef.GetAsInt("pricePC"));

			// If the 3D preview scene was not created, do it now and link it to the raw image
			if(m_eggPreviewScene == null) {
				RawImage eggPreviewArea = m_buyButton.GetComponentInChildren<RawImage>();
				m_eggPreviewScene = EggUIScene3D.CreateEmpty();
				m_eggPreviewScene.InitRawImage(ref eggPreviewArea);
			}

			// Initialize with the target egg
			// The scene will take care of everything
			m_eggPreviewScene.SetEgg(Egg.CreateFromDef(m_eggDef));
		}
	}

	/// <summary>
	/// Perform all required actions when leaving the screen.
	/// </summary>
	private void Finalize() {
		// Restore equipped disguise
		if(m_equippedPill != null) {
			Wardrobe.Equip(m_dragonSku, m_equippedPill.sku);
		} else {
			Wardrobe.Equip(m_dragonSku, "default");
		}
		PersistenceManager.Save();

		// Hide all powerups
		for(int i = 0; i < m_powerAnims.Length; i++) {
			m_powerAnims[i].Hide();
		}
	}

	/// <summary>
	/// Given an array of sprites, get the first one with a target name.
	/// </summary>
	/// <returns>The first sprite in the <paramref name="_array"/> with name <paramref name="_name"/>.</returns>
	/// <param name="_array">The array to be looked.</param>
	/// <param name="_name">The name we're looking for.</param>
	private Sprite GetFromCollection(ref Sprite[] _array, string _name) {
		for (int i = 0; i < _array.Length; i++) {
			if (_array[i].name == _name) {
				return _array[i];
			}
		}

		return null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A disguise pill has been clicked.
	/// </summary>
	/// <param name="_pill">The pill that has been clicked.</param>
	void OnPillClicked(DisguisePill _pill) {
		// Skip if pill is already the selected one
		if(m_selectedPill == _pill) return;

		// SFX - not during the intialization
		if(m_selectedPill != null) AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Update and Show/Hide title
		/*ShowHideAnimator titleAnimator = m_disguiseTitle.GetComponent<ShowHideAnimator>();
		titleAnimator.Hide(false);
		titleAnimator.Set(_pill != null);
		m_disguiseTitle.InitFromDefinition(_pill.def);*/

		// Remove highlight from previously selected pill
		if(m_selectedPill != null) m_selectedPill.Select(false);

		// Refresh power icons
		// Except default disguise, which has no powers whatsoever
		if(_pill.isDefault) {
			// Hide all power icons
			for(int i = 0; i < m_powerAnims.Length; i++) {
				m_powerAnims[i].Hide();
			}
		} else {
			// Init powers
			DefinitionNode powerSetDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES_POWERUPS, _pill.powerUpSet);
			for(int i = 0; i < 3; i++) {
				// Get def
				string powerUpSku = powerSetDef.GetAsString("powerup"+(i+1).ToString());
				DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);

				// Refresh data
				bool locked = (i >= _pill.level);
				m_powers[i].InitFromDefinition(powerDef, locked);

				// Show
				// Force an instant hide first to force the animation to be launched
				m_powerAnims[i].Hide(false);
				m_powerAnims[i].Show();
			}
		}

		// Store as selected pill and show highlight
		m_selectedPill = _pill;
		m_selectedPill.Select(true);

		// Apply selected disguise to dragon preview and animate
		string oldDisguiseSku = Wardrobe.GetEquipedDisguise(m_dragonSku);
		Wardrobe.Equip(m_dragonSku, m_selectedPill.sku);
		if(oldDisguiseSku != m_selectedPill.sku) {
			// Only animate if disguise has actually changed
			DOTween.Kill("DisguiseEquippedAnim", true);	// Kill any existing tween
			if(m_previewAnchor != null) m_previewAnchor.DOScale(Vector3.zero, 1f).From().SetId("DisguiseEquippedAnim").SetEase(Ease.OutElastic);
		}

		// If selected disguise is equippable, do it
		if(m_selectedPill != m_equippedPill && m_selectedPill.level > 0) {
			// Refresh previous equipped pill
			if(m_equippedPill != null) {
				m_equippedPill.Use(false);
			} 

			// Refresh and store new equipped pill
			m_selectedPill.Use(true);
			m_equippedPill = m_selectedPill;
			PersistenceManager.Save();
		}
	}

	/// <summary>
	/// Buy button has been pressed.
	/// </summary>
	public void OnBuy() {
		// SFX
		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Get price and start purchase flow
		long pricePC = m_eggDef.GetAsLong("pricePC");
		if(UserProfile.pc >= pricePC) {
			// Perform transaction
			UserProfile.AddPC(-pricePC);
			PersistenceManager.Save();

			// Create a new egg instance
			Egg purchasedEgg = Egg.CreateFromDef(m_eggDef);
			purchasedEgg.ChangeState(Egg.State.READY);	// Already ready for collection!

			// Go to OPEN_EGG screen and start flow
			MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
			OpenEggScreenController openEggScreen = screensController.GetScreen((int)MenuScreens.OPEN_EGG).GetComponent<OpenEggScreenController>();
			screensController.GoToScreen((int)MenuScreens.OPEN_EGG);
			openEggScreen.StartFlow(purchasedEgg);
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
            UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}
