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
	[SerializeField] private ShowHideAnimator m_disguiseTitle;
	[SerializeField] private Localizer m_name;
	[SerializeField] private ShowHideAnimator[] m_powers;
	[SerializeField] private RectTransform m_layout;

	// Buttons
	[Separator("Buttons")]
	[SerializeField] private GameObject m_buyButton;

	// Preview
	[Separator("Preview")]
	[SerializeField] private RectTransform m_dragonUIPos;
	[SerializeField] private float m_depth = 25f;
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
	private DefinitionNode[] m_powerDefs = new DefinitionNode[3];
	private Sprite[] m_powerIcons = new Sprite[3];
	private Sprite[] m_allPowerIcons = null;

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

		// Preload the powerup icon spritesheet
		m_allPowerIcons = Resources.LoadAll<Sprite>("UI/Popups/Disguises/powers/icons_powers"); 

		m_dragonSku = "";
		m_eggDef = null;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// find the 3D dragon position
		GameObject disguiseScene = GameObject.Find("PF_MenuDisguisesScene");
		if (disguiseScene != null) {
			m_previewAnchor = disguiseScene.transform.FindChild("CurrentDragon");
			m_dragonRotationArrowsPos = disguiseScene.transform.FindChild("Arrows");
		}

		// Get target dragon
		m_dragonSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;

		// Get egg corresponding to target dragon
		m_eggDef = DefinitionsManager.GetDefinitionByVariable(DefinitionsCategory.EGGS, "dragonSku", m_dragonSku);

		// get disguises levels of the current dragon
		List<DefinitionNode> defList = DefinitionsManager.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", m_dragonSku);
		DefinitionsManager.SortByProperty(ref defList, "shopOrder", DefinitionsManager.SortType.NUMERIC);

		// Load disguise icons for this dragon
		Sprite[] icons = Resources.LoadAll<Sprite>("UI/Popups/Disguises/" + m_dragonSku);

		// Hide all the info
		m_disguiseTitle.ForceHide(false);
		for(int i = 0; i < m_powers.Length; i++) {
			m_powers[i].ForceHide(false);
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

			// Create the 3D preview scene and initialize the raw image
			RawImage eggPreviewArea = m_buyButton.GetComponentInChildren<RawImage>();
			m_eggPreviewScene = EggUIScene3D.CreateEmpty();
			m_eggPreviewScene.InitRawImage(ref eggPreviewArea);

			// The scene will take care of everything
			m_eggPreviewScene.SetEgg(Egg.CreateFromDef(m_eggDef));
		}
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		Canvas canvas = GetComponentInParent<Canvas>();
		Vector3 viewportPos = canvas.worldCamera.WorldToViewportPoint(m_dragonUIPos.position);

		Camera camera = InstanceManager.GetSceneController<MenuSceneController>().screensController.camera;
		viewportPos.z = m_depth;
		m_previewAnchor.position = camera.ViewportToWorldPoint(viewportPos);
		m_dragonRotationArrowsPos.position = camera.ViewportToWorldPoint(viewportPos) + Vector3.down;
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Hide preview
		if (m_previewAnchor != null) {
			m_previewAnchor.gameObject.SetActive(false);
		}

		// Destroy Egg 3D scene
		if(m_eggPreviewScene != null) {
			UIScene3DManager.Remove(m_eggPreviewScene);
			m_eggPreviewScene = null;
		}

		// Restore equipped disguise
		if(m_equippedPill != null) {
			Wardrobe.Equip(m_dragonSku, m_equippedPill.sku);
		} else {
			Wardrobe.Equip(m_dragonSku, "default");
		}
		PersistenceManager.Save();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
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

		// Show/Hide title
		m_disguiseTitle.Hide(false);
		m_disguiseTitle.Set(_pill != null);

		// Remove highlight from previously selected pill
		if(m_selectedPill != null) m_selectedPill.Select(false);

		// Update name
		m_name.Localize(_pill.tidName);

		// Refresh power icons
		// Except default disguise, which has no powers whatsoever
		if(_pill.isDefault) {
			for(int i = 0; i < m_powers.Length; i++) {
				m_powers[i].Hide();
			}
		} else {
			// Get defs
			DefinitionNode powerSetDef = DefinitionsManager.GetDefinition(DefinitionsCategory.DISGUISES_POWERUPS, _pill.powerUpSet);
			for (int i = 0; i < 3; i++) {
				string powerUpSku = powerSetDef.GetAsString("powerup"+(i+1).ToString());
				m_powerDefs[i] = DefinitionsManager.GetDefinition(DefinitionsCategory.POWERUPS, powerUpSku);
			}

			// Update icons
			for (int i = 0; i < m_powers.Length; i++) {
				// Show
				// Force an instant hide first to force the animation to be launched
				m_powers[i].Hide();
				m_powers[i].Show();

				// Lock
				m_powers[i].transform.FindChild("IconLock").gameObject.SetActive(i >= _pill.level);

				// Icons
				if(m_powerDefs[i] != null) {
					// Search icon within the spritesheet
					string iconName = m_powerDefs[i].GetAsString("icon");
					Image img = m_powers[i].FindComponentRecursive<Image>("PowerIcon");
					img.sprite = GetFromCollection(ref m_allPowerIcons, iconName);
					img.SetNativeSize();

					// Gray out if power is locked
					if(i < _pill.level) {
						img.color = Color.white;
					} else {
						img.color = Color.gray;
					}

					// Store for further use
					m_powerIcons[i] = img.sprite;
				}
			}
		}

		// Store as selected pill and show highlight
		m_selectedPill = _pill;
		m_selectedPill.Select(true);

		// Apply selected disguise to dragon preview and animate
		Wardrobe.Equip(m_dragonSku, m_selectedPill.sku);
		m_previewAnchor.GetComponent<ShowHideAnimator>().ForceHide(false);
		m_previewAnchor.GetComponent<ShowHideAnimator>().Show();

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
	/// A tooltip is about to be opened.
	/// In this context, it means that a power button has been pressed.
	/// </summary>
	/// <param name="_tooltip">The tooltip about to be opened.</param>
	/// <param name="_trigger">The button which triggered the event.</param>
	public void OnTooltipOpen(UITooltip _tooltip, UITooltipTrigger _trigger) {
		// Find out which power has been tapped (buttons have the trigger component)
		for(int i = 0; i < m_powers.Length; i++) {
			if(m_powers[i].gameObject == _trigger.gameObject) {
				// Found! Initialized tooltip with data from this power
				DefinitionNode def = m_powerDefs[i];

				// Name
				_tooltip.FindComponentRecursive<Localizer>("PowerupNameText").Localize(def.Get("tidName"));

				// Desc
				_tooltip.FindComponentRecursive<Text>("PowerupDescText").text = DragonPowerUp.GetDescription(def.sku);	// Custom formatting depending on powerup type, already localized

				// Icon
				Image img = _tooltip.FindComponentRecursive<Image>("Icon");
				img.sprite = m_powerIcons[i];
				img.SetNativeSize();	// Icons already have the desired size

				// Move arrow based on wich powerup has been tapped
				// [AOC] Quick'n'dirty: hardcoded values
				RectTransform arrowTransform = _tooltip.FindComponentRecursive<RectTransform>("Arrow");
				if(arrowTransform != null) {
					switch(i) {
						case 0: {
							arrowTransform.anchorMin = new Vector2(0f, 0f);
							arrowTransform.anchorMax = new Vector2(0f, 0f);
							arrowTransform.anchoredPosition = new Vector2(48f, 0f);
						} break;

						case 1: {
							arrowTransform.anchorMin = new Vector2(0.5f, 0f);
							arrowTransform.anchorMax = new Vector2(0.5f, 0f);
							arrowTransform.anchoredPosition = Vector2.zero;
						} break;

						case 2: {
							arrowTransform.anchorMin = new Vector2(1f, 0f);
							arrowTransform.anchorMax = new Vector2(1f, 0f);
							arrowTransform.anchoredPosition = new Vector2(-48f, 0f);
						} break;
					}
				}

				// We're done!
				break;
			}
		}
	}

	/// <summary>
	/// Buy button has been pressed.
	/// </summary>
	public void OnBuy() {
		/*PopupController popup = PopupManager.OpenPopupInstant(PopupEggShop.PATH);
		popup.GetComponent<PopupEggShop>().SetInitialEgg(m_dragonSku);
		popup.GetComponent<PopupEggShop>().SetVisibleEggs(new string[] { m_dragonSku });*/

		// Can the egg be purchased? It should be, we can't open the disguises screen for dragons we don't yet own! Check it just in case
		// Dragon must be owned
		DragonData requiredDragon = DragonManager.GetDragonData(m_dragonSku);
		if(!requiredDragon.isOwned) {
			// Show feedback and return
			string text = Localization.Localize("TID_EGG_SHOP_DRAGON_REQUIRED", requiredDragon.def.GetLocalized("tidName"));
			UIFeedbackText textObj = UIFeedbackText.CreateAndLaunch(text, new Vector2(0.5f, 0.5f), (RectTransform)this.transform);
			textObj.GetComponent<Text>().color = Colors.red;
			return;
		}

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
			PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);
		}
	}
}
