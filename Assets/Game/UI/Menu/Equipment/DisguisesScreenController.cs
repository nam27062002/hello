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
	private const string PILL_PREFAB_NAME = "PF_DisguisesPill";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[Separator("Scene References")]
	[SerializeField] private DisguisesScreenTitle m_title = null;
	[SerializeField] private PowerIcon m_powerIcon;
	[SerializeField] private ShowHideAnimator m_powerSlotAnim = null;
	[SerializeField] private SnappingScrollRect m_scrollList = null;

	[Space]
	[SerializeField] private CurrencyButton m_SCButton = null;
	[SerializeField] private CurrencyButton m_PCButton = null;
	[SerializeField] private AnimatedButton m_offerButton = null;
	[SerializeField] private AnimatedButton m_equipButton = null;

	[Space]
	[SerializeField] private Localizer m_lockText = null;

	[Space]
	[SerializeField] private PopupShopOffersPill m_offerPill = null;

	// Setup
	private string m_initialSkin = string.Empty;	// String to be selected upon entering the screen. Will be resetted every time the screen is reloaded.
	public string initialSkin {
		get { return m_initialSkin; }
		set { m_initialSkin = value; }
	}

	// Preview
	private Transform m_previewAnchor;
	private Transform m_dragonRotationArrowsPos;

	// Pills management
	private DisguisePill[] m_pills;
	private DisguisePill m_equippedPill;	// Pill corresponding to the equipped disguise 
	private DisguisePill m_selectedPill;	// Pill corresponding to the selected disguise

	// Other data
	private IDragonData m_dragonData = null;
	private Wardrobe m_wardrobe = null;
	private OfferPack m_linkedOffer = null;

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

	private Button m_photoButton = null;
	private Button photoButton {
		get {
			if(m_photoButton == null) {
				m_photoButton = InstanceManager.menuSceneController.hud.photoButton.GetComponentInChildren<Button>();
			}
			return m_photoButton;
		}
	}

	// Internal logic
	private bool m_waitingForDragonPreviewToLoad = false;

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
		GameObject prefab = Resources.Load<GameObject>(UIConstants.DISGUISE_ICONS_PATH + PILL_PREFAB_NAME);
		for (int i = 0; i < MAX_PILLS; i++) {
			GameObject pill = (GameObject)GameObject.Instantiate(prefab, m_scrollList.content.transform, false);
			pill.transform.localScale = Vector3.one;
			m_pills[i] = pill.GetComponent<DisguisePill>();
			//m_pills[i].OnPillClicked.AddListener(OnPillClicked);		// [AOC] Will be handled by the snap scroll list
		}
		prefab = null;

		// Store some references
		m_dragonData = null;
		m_wardrobe = UsersManager.currentUser.wardrobe;

		// Subscribe to animator's events
		animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
        animator.OnHidePreAnimation.AddListener(OnHidePreAnimation);

		// Subscribe to purchase buttons
		m_SCButton.button.onClick.AddListener(OnPurchaseDisguiseButton);
		m_PCButton.button.onClick.AddListener(OnPurchaseDisguiseButton);
		m_offerButton.button.onClick.AddListener(OnOfferButton);
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

		// Are we waiting for the dragon preview to be ready?
		if(m_waitingForDragonPreviewToLoad) {
			// Is it ready?
			if(InstanceManager.menuSceneController.selectedDragonPreview != null) {
				// Hide pets
				DragonEquip equip = InstanceManager.menuSceneController.selectedDragonPreview.GetComponent<DragonEquip>();
				if(equip != null) {
					equip.TogglePets(false, true);
				}

				// Toggle flag
				m_waitingForDragonPreviewToLoad = false;
			}
		}
	}

	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	private void PeriodicRefresh() {
		// Nothing if not enabled
		if(!this.isActiveAndEnabled) return;

		// Refresh offer pill
		m_offerPill.RefreshTimer();

		// If invalid pack or pack has expired, refresh buttons
		if(m_linkedOffer == null || !m_linkedOffer.isActive) {
			Refresh();
		}
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
		animator.OnHidePreAnimation.RemoveListener(OnHidePreAnimation);

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
	private void Initialize() {
		// Aux vars
		MenuSceneController menuController = InstanceManager.menuSceneController;

		// Reset internal vars
		m_linkedOffer = null;

		// Store current dragon
		m_dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Find out initial disguise - dragon's current disguise
		string currentDisguise = UsersManager.currentUser.GetEquipedDisguise(m_dragonData.def.sku);

		// Find the 3D dragon preview
		MenuScreenScene scene3D = menuController.GetScreenData(MenuScreen.SKINS).scene3d;
		if(scene3D != null) {
			MenuDragonPreview preview = scene3D.GetComponent<MenuDragonScroller>().GetDragonPreview(m_dragonData.def.sku);
			if(preview != null) m_previewAnchor = preview.transform;
			//m_dragonRotationArrowsPos = scene.transform.FindChild("Arrows");
		}

		// Get all the disguises of the current dragon
		List<DefinitionNode> defList = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", m_dragonData.def.sku);
		DefinitionsManager.SharedInstance.SortByProperty(ref defList, "shopOrder", DefinitionsManager.SortType.NUMERIC);

		// Hide all the contextual info
		if(m_powerSlotAnim != null) m_powerSlotAnim.ForceHide(false);
		if(m_title != null) m_title.showHideAnimator.ForceHide(false);
		if(m_lockText != null) m_lockText.GetComponent<ShowHideAnimator>().ForceHide(false);
		if(m_SCButton != null) m_SCButton.animator.ForceHide(false);
		if(m_PCButton != null) m_PCButton.animator.ForceHide(false);
		if(m_offerButton != null) m_offerButton.animator.ForceHide(false);
		if(m_equipButton != null) m_equipButton.animator.ForceHide(false);

		// Initialize pills
		// Find initial pill
		m_equippedPill = null;
		m_selectedPill = null;
		DisguisePill initialPill = null;
		for (int i = 0; i < m_pills.Length; i++) {
			if (i < defList.Count) {
				// Init pill
				DefinitionNode def = defList[i];
				m_pills[i].Load(def, m_wardrobe.GetSkinState(def.sku));
				m_pills[i].name = def.sku;	// [AOC] For debug purposes

				// Is it the forced initial skin?
				if(def.sku == m_initialSkin) {
					initialPill = m_pills[i];
				}

				// Is it the currently equipped disguise?
				if(def.sku == currentDisguise) {
					// Mark it as the initial pill
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

		// If no initial pill was forced, select current equipped skin
		if(initialPill == null) {
			if(m_equippedPill != null) {
				initialPill = m_equippedPill;
			} else {
				initialPill = m_pills[0];	// There will always be at least the default pill
			}
		}
		m_initialSkin = string.Empty;	// Reset for next time

		// Force a first refresh
		// This will initialize both the equipped and selected pills as well
		m_scrollList.SelectPoint(initialPill.snapPoint);
		OnPillClicked(initialPill);	// [AOC] If selected point is the same that was already selected, the OnSelectionChanged callback won't be called. Make sure the pill is properly initialized by manually inboking OnPillClicked.
	}

	/// <summary>
	/// Leave the screen ready to be hidden.
	/// </summary>
	public void FinalizeScreen() {
		// Restore equiped disguise on target dragon
		bool newEquip = false;
		if(m_equippedPill != null) {
			newEquip = UsersManager.currentUser.EquipDisguise(m_dragonData.def.sku, m_equippedPill.def.sku, true);
		}

		// Broadcast message
		if(newEquip) {
			Messenger.Broadcast<string>(MessengerEvents.MENU_DRAGON_DISGUISE_CHANGE, m_dragonData.def.sku);
		}
        PersistenceFacade.instance.Save_Request();

        // Hide all powerups
		if(m_powerSlotAnim != null) m_powerSlotAnim.Hide();

		// Hide header
		m_title.GetComponent<ShowHideAnimator>().Hide();

		// Stop repeating refresh
		CancelInvoke("PeriodicRefresh");
	}

	/// <summary>
	/// Performs all the logic to mark selected pill as equipped.
	/// </summary>
	/// <returns>Whether the pill cpuld be equipped or not (valid selected pill? owned skin?)</returns>
	private bool EquipSelectedPill() {
		// Is current pill valid and equippable?
		if(m_selectedPill == null) return false;
		if(!m_selectedPill.owned) return false;

		// Yeah! Equip
		// Refresh previous equipped pill
		if(m_equippedPill != null) {
			m_equippedPill.Equip(false);
		} 

		// Refresh and store new equipped pill
		m_selectedPill.Equip(true);
		m_equippedPill = m_selectedPill;

		// Everything ok!
		return true;
	}

	/// <summary>
	/// Refresh visuals keeping current selected pill.
	/// </summary>
	private void Refresh() {
		// Just re-select current skin
		DisguisePill pill = m_selectedPill;
		m_selectedPill = null;
		OnPillClicked(pill);
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

		// Hide dragon's pets whenever preview is ready
		m_waitingForDragonPreviewToLoad = true;
	}

	/// <summary>
	/// Screen has just closed
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Make sure screens are properly finalized
		FinalizeScreen();

		// Restore pets on the current dragon preview
		if (InstanceManager.menuSceneController.selectedDragonPreview)
		{
			DragonEquip equip = InstanceManager.menuSceneController.selectedDragonPreview.GetComponent<DragonEquip>();
			if(equip != null) {
				equip.TogglePets(true, true);
			}
		}

		// Restore photo button
		// We can only enter the disguises screen with owned dragons, so photo button should be enabled
		if(photoButton != null) {
			photoButton.interactable = true;
		}
	}

	/// <summary>
	/// A new pill has been selected on the snapping scroll list.
	/// </summary>
	/// <param name="_selectedPoint">Selected point.</param>
	public void OnSelectionChanged(ScrollRectSnapPoint _selectedPoint) {
		if(_selectedPoint == null) return;
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
		// Refresh data
		m_powerIcon.InitFromDefinition(powerDef, false);	// [AOC] Powers are not locked anymore

		// Show
		// Force the animation to be launched
		m_powerSlotAnim.RestartShow();

		// Refresh the lock info
		if(m_lockText != null) {
			ShowHideAnimator anim = m_lockText.GetComponent<ShowHideAnimator>();
			if(_pill.locked) {
				// Locked by season or by level?
				if(_pill.seasonDef != null) {
					m_lockText.Localize("TID_DISGUISES_UNLOCK_INFO_SEASON", _pill.seasonDef.GetLocalized("tidName"));    // Only available in %U0!
				} else {
					m_lockText.Localize("TID_DISGUISES_UNLOCK_INFO", StringUtils.FormatNumber(_pill.def.GetAsInt("unlockLevel") + 1), m_dragonData.def.GetLocalized("tidName"));    // Reach level %U0 on %U1 to unlock!
				}

				anim.RestartShow();	// Restart animation
			} else {
				anim.Hide();
			}
		}

		// Gather some data from the skin
		float priceSC = _pill.def.GetAsFloat("priceSC");
		float pricePC = _pill.def.GetAsFloat("priceHC");
		bool isPC = pricePC > priceSC;

		m_linkedOffer = null;
		foreach(OfferPack offer in OffersManager.activeOffers) {
			foreach(OfferPackItem item in offer.items) {
				if(item.sku == _pill.def.sku) {
					m_linkedOffer = offer;
					break;
				}
			}
		}

		// Refresh buttons
		// SC button
		if(m_SCButton != null) {
			// Show button?
			// Only if disguise is neither owned nor locked and is purchased with SC
			if(!isPC && !_pill.owned && !_pill.locked) {
				// Set price and restart animation
				m_SCButton.SetAmount(priceSC, UserProfile.Currency.SOFT);
				m_SCButton.animator.RestartShow();
			} else {
				m_SCButton.animator.Hide(false);
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
				m_PCButton.animator.Hide(false);
			}
		}

		// Offer button
		if(m_offerButton != null) {
			// Show button?
			// Only if skin is neither owned nor locked, and there is an active offer pack selling this skin
			if(m_linkedOffer != null && !_pill.owned && !_pill.locked) {
				// Init offer pill and program periodic update
				m_offerPill.InitFromOfferPack(m_linkedOffer);
				InvokeRepeating("PeriodicRefresh", 0f, 1f);
				m_offerButton.animator.RestartShow();
			} else {
				CancelInvoke("PeriodicRefresh");
				m_offerButton.animator.Hide(false);
			}
		}

		// Photo button
		if(photoButton != null) {
			// Only enabled for owned skins
			photoButton.interactable = _pill.owned;
		}

		// Store as selected pill
		m_selectedPill = _pill;

		// Equip button or auto-equip? Check settings
		bool persist = false;
		if(DebugSettings.menuDisguisesAutoEquip) {
			// If selected disguise is owned and not already equipped, equip it
			if(_pill.owned && _pill != m_equippedPill) {
				persist = EquipSelectedPill();
			}
		} else {
			// Show equip button?
			if(m_equipButton != null) {
				// Only if selected disguise is owned and not equipped
				if(_pill.owned && _pill != m_equippedPill) {
					m_equipButton.animator.RestartShow();
				} else {
					m_equipButton.animator.Hide(false);
				}
			}
		}

		// Remove "new" flag from the skin
		if(_pill.state == Wardrobe.SkinState.NEW) {
			m_wardrobe.SetSkinState(_pill.def.sku, Wardrobe.SkinState.AVAILABLE);
			_pill.SetState(Wardrobe.SkinState.AVAILABLE);
		}

		// Apply selected disguise to dragon preview
		if(UsersManager.currentUser.EquipDisguise(m_dragonData.def.sku, m_selectedPill.def.sku, persist)) {
			// Notify game
			Messenger.Broadcast<string>(MessengerEvents.MENU_DRAGON_DISGUISE_CHANGE, m_dragonData.def.sku);
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

				// Show some nice FX
				// Let's re-select the skin for now
				Refresh();

				// Immediately equip it if auto_equip is not enabled!
				if(!DebugSettings.menuDisguisesAutoEquip) {
					OnEquipButton();
				}

				// Save!
				PersistenceFacade.instance.Save_Request(true);

				// Throw out some fireworks!
				InstanceManager.menuSceneController.dragonScroller.LaunchDisguisePurchasedFX();
			}
		);
		if(isPC) {
			purchaseFlow.Begin(pricePC, UserProfile.Currency.HARD, HDTrackingManager.EEconomyGroup.ACQUIRE_DISGUISE, m_selectedPill.def);
		} else {
			purchaseFlow.Begin(priceSC, UserProfile.Currency.SOFT, HDTrackingManager.EEconomyGroup.ACQUIRE_DISGUISE, m_selectedPill.def);
		}
	}

	/// <summary>
	/// Offer button has been pressed.
	/// </summary>
	public void OnOfferButton() {
		// Open the featured offer popup with the skin's offer
		if(m_linkedOffer == null) return;
		PopupController popup = PopupManager.LoadPopup(PopupFeaturedOffer.PATH);
		popup.GetComponent<PopupFeaturedOffer>().InitFromOfferPack(m_linkedOffer);
		popup.Open();
	}

	/// <summary>
	/// Equip button has been pressed.
	/// </summary>
	public void OnEquipButton() {
		// Equip selected pill!
		if(!EquipSelectedPill()) return;

		// Save persistence
        PersistenceFacade.instance.Save_Request();

        // Hide button!
        m_equipButton.animator.Hide();
	}
}
