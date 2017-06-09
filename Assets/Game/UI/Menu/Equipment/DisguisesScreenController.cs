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
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the disguises screen.
/// </summary>
[RequireComponent(typeof(NavigationShowHideAnimator))]
public class DisguisesScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const int MAX_PILLS = 9;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Pill prefab
	[Separator("Project References")]
	[SerializeField] private GameObject m_pillPrefab = null;

	// References
	[Separator("Scene References")]
	[SerializeField] private DisguisesScreenTitle m_title = null;
	[SerializeField] private PowerIcon m_powerIcon;
	[SerializeField] private SnappingScrollRect m_scrollList = null;

	[Space]
	[SerializeField] private CurrencyButton m_SCButton = null;
	[SerializeField] private CurrencyButton m_PCButton = null;
	[SerializeField] private AnimatedButton m_equipButton = null;

	[Space]
	[SerializeField] private Localizer m_lockText = null;

	// Preview
	private Transform m_previewAnchor;
	private Transform m_dragonRotationArrowsPos;

	// Pills management
	private DisguisePill[] m_pills;
	private DisguisePill m_equippedPill;	// Pill corresponding to the equipped disguise 
	private DisguisePill m_selectedPill;	// Pill corresponding to the selected disguise

	// Powers
	private ShowHideAnimator m_powerAnim = null;

	// Other data
	private DragonData m_dragonData = null;
	private Wardrobe m_wardrobe = null;

	// Internal references
	private NavigationShowHideAnimator m_animator = null;
	private NavigationShowHideAnimator animator {
		get { 
			if(m_animator == null) {
				m_animator = GetComponent<NavigationShowHideAnimator>();
			}
			return m_animator;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Clear all content of the scroll list (used to do the layout)
		int numChildren = m_scrollList.content.childCount;
		for(int i = numChildren - 1; i >= 0; i--) {	// Reverse loop since we're erasing
			GameObject.Destroy(m_scrollList.content.transform.GetChild(i).gameObject);
		}

		// Instantiate pills - as many as needed!
		m_pills = new DisguisePill[MAX_PILLS];
		for (int i = 0; i < MAX_PILLS; i++) {
			GameObject pill = (GameObject)GameObject.Instantiate(m_pillPrefab, m_scrollList.content.transform, false);
			pill.transform.localScale = Vector3.one;
			m_pills[i] = pill.GetComponent<DisguisePill>();
			//m_pills[i].OnPillClicked.AddListener(OnPillClicked);		// [AOC] Will be handled by the snap scroll list
		}

		// Store some references
		m_powerAnim = m_powerIcon.GetComponent<ShowHideAnimator>();

		m_dragonData = null;
		m_wardrobe = UsersManager.currentUser.wardrobe;

		// Subscribe to animator's events
		animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
		animator.OnHidePostAnimation.AddListener(OnHidePostAnimation);

		// Subscribe to purchase buttons
		m_SCButton.button.onClick.AddListener(OnPurchaseDisguiseButton);
		m_PCButton.button.onClick.AddListener(OnPurchaseDisguiseButton);
		m_equipButton.button.onClick.AddListener(OnEquipButton);
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		/*Canvas canvas = GetComponentInParent<Canvas>();
		Vector3 viewportPos = canvas.worldCamera.WorldToViewportPoint(m_dragonUIPos.position);

		Camera camera = InstanceManager.menuSceneController.screensController.camera;
		viewportPos.z = m_depth;
		m_previewAnchor.position = camera.ViewportToWorldPoint(viewportPos);
		m_dragonRotationArrowsPos.position = camera.ViewportToWorldPoint(viewportPos) + Vector3.down;*/
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from animatior events
		animator.OnShowPreAnimation.RemoveListener(OnShowPreAnimation);
		animator.OnHidePostAnimation.RemoveListener(OnHidePostAnimation);

		// Unsubscribe from purchase buttons
		m_SCButton.button.onClick.RemoveListener(OnPurchaseDisguiseButton);
		m_PCButton.button.onClick.RemoveListener(OnPurchaseDisguiseButton);
		m_equipButton.button.onClick.RemoveListener(OnEquipButton);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup the screen with the data of the currently selected dragon.
	/// </summary>
	public void Initialize() {
		// Aux vars
		MenuSceneController menuController = InstanceManager.menuSceneController;

		// Store current dragon
		m_dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Find out initial disguise - dragon's current disguise
		string currentDisguise = UsersManager.currentUser.GetEquipedDisguise(m_dragonData.def.sku);

		// Find the 3D dragon preview
		MenuScreenScene scene3D = menuController.screensController.GetScene((int)MenuScreens.DISGUISES);
		if(scene3D != null) {
			MenuDragonPreview preview = scene3D.GetComponent<MenuDragonScroller>().GetDragonPreview(m_dragonData.def.sku);
			if(preview != null) m_previewAnchor = preview.transform;
			//m_dragonRotationArrowsPos = scene.transform.FindChild("Arrows");
		}

		// Get all the disguises of the current dragon
		List<DefinitionNode> defList = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", m_dragonData.def.sku);
		DefinitionsManager.SharedInstance.SortByProperty(ref defList, "shopOrder", DefinitionsManager.SortType.NUMERIC);

		// Hide all the contextual info
		if(m_powerAnim != null) m_powerAnim.ForceHide(false);
		if(m_title != null) m_title.showHideAnimator.ForceHide(false);
		if(m_lockText != null) m_lockText.GetComponent<ShowHideAnimator>().ForceHide(false);
		if(m_SCButton != null) m_SCButton.animator.ForceHide(false);
		if(m_PCButton != null) m_PCButton.animator.ForceHide(false);
		if(m_equipButton != null) m_equipButton.animator.ForceHide(false);

		// Initialize pills
		m_equippedPill = null;
		m_selectedPill = null;
		DisguisePill initialPill = m_pills[0];	// There will always be at least the default pill
		string disguisesIconPath = UIConstants.DISGUISE_ICONS_PATH + m_dragonData.def.sku + "/";
		for (int i = 0; i < m_pills.Length; i++) {
			if (i < defList.Count) {
				// Load icon sprite for this skin
				Sprite spr = Resources.Load<Sprite>(disguisesIconPath + defList[i].Get("icon"));

				// Init pill
				DefinitionNode def = defList[i];
				m_pills[i].Load(def, m_wardrobe.GetSkinState(def.sku), spr);

				// Is it the currently equipped disguise?
				if(def.sku == currentDisguise) {
					// Mark it as the initial pill
					initialPill = m_pills[i];
					m_pills[i].Equip(true, false);
					m_equippedPill = m_pills[i];
				} else {
					m_pills[i].Equip(false, false);
				}

				// Activate
				m_pills[i].gameObject.SetActive(true);
			} else {
				// Unused pill
				m_pills[i].gameObject.SetActive(false);
			}
		}

		// Force a first refresh
		// This will initialize both the equipped and selected pills as well
		m_scrollList.SelectPoint(initialPill.snapPoint);
		OnPillClicked(initialPill);	// [AOC] If selected point is the same that was already selected, the OnSelectionChanged callback won't be called. Make sure the pill is properly initialized by manually inboking OnPillClicked.
	}

	/// <summary>
	/// Setup the screen with the data of the currently selected dragon.
	/// </summary>
	/// <param name="_initialDisguiseSku">The disguise to focus. If it's from a different dragon than the current one, the target dragon will be selected. Leave empty to load current setup.</param>
	public void Finalize() {
		// Restore equiped disguise on target dragon
		bool newEquip = false;
		if(m_equippedPill != null) {
			newEquip = UsersManager.currentUser.EquipDisguise(m_dragonData.def.sku, m_equippedPill.def.sku);
		}

		// Broadcast message
		if(newEquip) {
			Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, m_dragonData.def.sku);
		}
		PersistenceManager.Save();

		// Hide all powerups
		if(m_powerAnim != null) m_powerAnim.Hide();

		// Hide header
		m_title.GetComponent<ShowHideAnimator>().Hide();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Screen is about to be open.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPreAnimation(ShowHideAnimator _animator) {
		// Refresh with initial data!
		Initialize();

		// Hide pets on the current dragon preview
		DragonEquip equip = InstanceManager.menuSceneController.selectedDragonPreview.GetComponent<DragonEquip>();
		if(equip != null) {
			equip.TogglePets(false, true);
		}
	}

	/// <summary>
	/// Screen has just closed
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePostAnimation(ShowHideAnimator _animator) {
		// Make sure screens are properly finalized
		Finalize();

		// Restore pets on the current dragon preview
		DragonEquip equip = InstanceManager.menuSceneController.selectedDragonPreview.GetComponent<DragonEquip>();
		if(equip != null) {
			equip.TogglePets(true, true);
		}
	}

	/// <summary>
	/// A new pill has been selected on the snapping scroll list.
	/// </summary>
	/// <param name="_selectedPoint">Selected point.</param>
	public void OnSelectionChanged(ScrollRectSnapPoint _selectedPoint) {
		OnPillClicked(_selectedPoint.GetComponent<DisguisePill>());
	}

	/// <summary>
	/// A disguise pill has been clicked.
	/// </summary>
	/// <param name="_pill">The pill that has been clicked.</param>
	void OnPillClicked(DisguisePill _pill) {
		// Skip if pill is already the selected one
		if(m_selectedPill == _pill) return;
		if(_pill.def == null) return;

		// SFX - not during the intialization
		// if(m_selectedPill != null) AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Update and Show/Hide title
		m_title.InitFromDef(_pill.def);
		m_title.showHideAnimator.Hide(false);
		m_title.showHideAnimator.Set(_pill != null);

		// Refresh power icon
		// Get def
		string powerSku = _pill.def.GetAsString("powerup");
		DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, powerSku);

		// If no power, hide the power icon
		if(powerDef == null) {
			m_powerAnim.Hide();
		} else {
			// Refresh data
			m_powerIcon.InitFromDefinition(powerDef, false);	// [AOC] Powers are not locked anymore

			// Show
			// Force an instant hide first to force the animation to be launched
			m_powerAnim.Hide(false);
			m_powerAnim.Show();
		}

		// Refresh the lock info
		if(m_lockText != null) {
			ShowHideAnimator anim = m_lockText.GetComponent<ShowHideAnimator>();
			if(_pill.locked) {
				m_lockText.Localize("TID_DISGUISES_UNLOCK_INFO", StringUtils.FormatNumber(_pill.def.GetAsInt("unlockLevel") + 1), m_dragonData.def.GetLocalized("tidName"));	// Reach level %U0 on %U1 to unlock!
				anim.RestartShow();	// Restart animation
			} else {
				anim.Hide();
			}
		}

		// Refresh visuals
		float priceSC = _pill.def.GetAsFloat("priceSC");
		float pricePC = _pill.def.GetAsFloat("priceHC");
		bool isPC = pricePC > priceSC;

		// SC button
		if(m_SCButton != null) {
			// Show button?
			// Only if disguise is neither owned nor locked and is purchased with SC
			if(!isPC && !_pill.owned && !_pill.locked) {
				// Set price and restart animation
				m_SCButton.SetAmount(priceSC, UserProfile.Currency.SOFT);
				m_SCButton.animator.RestartShow();
			} else {
				m_SCButton.animator.Hide();
			}
		}

		// PC button
		if(m_PCButton != null) {
			// Show button?
			// Only if disguise is neither owned nor locked and is purchased with PC
			if(isPC && !_pill.owned && !_pill.locked) {
				// Set price and restart animation
				m_PCButton.SetAmount(pricePC, UserProfile.Currency.HARD);
				m_PCButton.animator.RestartShow();
			} else {
				m_PCButton.animator.Hide();
			}
		}

		// Store as selected pill
		m_selectedPill = _pill;

		// Equip button or auto-equip? Check settings
		if(Prefs.GetBoolPlayer(DebugSettings.MENU_DISGUISES_AUTO_EQUIP)) {
			// If selected disguise is owned and not already equipped, equip it
			if(_pill.owned && _pill != m_equippedPill) {
				OnEquipButton();
			}
		} else {
			// Show equip button?
			if(m_equipButton != null) {
				// Only if selected disguise is owned and not equipped
				if(_pill.owned && _pill != m_equippedPill) {
					m_equipButton.animator.RestartShow();
				} else {
					m_equipButton.animator.Hide();
				}
			}
		}

		// Apply selected disguise to dragon preview
		if(UsersManager.currentUser.EquipDisguise(m_dragonData.def.sku, m_selectedPill.def.sku)) {
			// Notify game
			Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_DISGUISE_CHANGE, m_dragonData.def.sku);
		}

		// Remove "new" flag from the skin
		if(_pill.state == Wardrobe.SkinState.NEW) {
			m_wardrobe.SetSkinState(_pill.def.sku, Wardrobe.SkinState.AVAILABLE);
			_pill.SetState(Wardrobe.SkinState.AVAILABLE);
		}
	}

	/// <summary>
	/// Purchase button has been pressed (either SC or PC).
	/// </summary>
	public void OnPurchaseDisguiseButton() {
		// Buy currently selected skin
		// Check required data
		if(m_selectedPill == null) return;
		if(m_selectedPill.def == null) return;
		if(m_selectedPill.owned) return;	// Skin already owned! Buttons shouldn't be visible
		if(m_selectedPill.locked) return;	// Skin locked! Buttons shouldn't be visible

		// All checks passed, get price and currency
		long priceSC = m_selectedPill.def.GetAsLong("priceSC");
		long pricePC = m_selectedPill.def.GetAsLong("priceHC");
		bool isPC = pricePC > priceSC;

		// Perform transaction
		// Get price and start purchase flow
		ResourcesFlow purchaseFlow = new ResourcesFlow("ACQUIRE_DISGUISE");
		purchaseFlow.OnSuccess.AddListener(
			(ResourcesFlow _flow) => {
				// Acquire it!
				m_wardrobe.SetSkinState(_flow.itemDef.sku, Wardrobe.SkinState.OWNED);

				// Change selected pill state
				m_selectedPill.SetState(Wardrobe.SkinState.OWNED);

				// Save!
				PersistenceManager.Save(true);

				// Show some nice FX
				// Let's re-select the skin for now
				DisguisePill pill = m_selectedPill;
				m_selectedPill = null;
				OnPillClicked(pill);

				// Immediately equip it!
				OnEquipButton();

				// Throw out some fireworks!
				InstanceManager.menuSceneController.dragonScroller.LaunchDisguisePurchasedFX();
			}
		);
		if(isPC) {
			purchaseFlow.Begin(pricePC, UserProfile.Currency.HARD, m_selectedPill.def);
		} else {
			purchaseFlow.Begin(priceSC, UserProfile.Currency.SOFT, m_selectedPill.def);
		}
	}

	/// <summary>
	/// Equip button has been pressed.
	/// </summary>
	public void OnEquipButton() {
		// Is current pill valid and equippable?
		if(m_selectedPill == null) return;
		if(!m_selectedPill.owned) return;

		// Yeah! Equip
		// Refresh previous equipped pill
		if(m_equippedPill != null) {
			m_equippedPill.Equip(false);
		} 

		// Refresh and store new equipped pill
		m_selectedPill.Equip(true);
		m_equippedPill = m_selectedPill;
		PersistenceManager.Save();

		// Hide button!
		m_equipButton.animator.Hide();
	}
}
