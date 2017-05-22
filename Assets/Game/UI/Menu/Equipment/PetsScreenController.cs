// PetsScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/07/2016.
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
/// Main controller for the pets menu screen.
/// </summary>
public class PetsScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private TabSystem m_categoryTabs = null;
	[SerializeField] private List<PowerIcon> m_powerIcons = new List<PowerIcon>();

	// Internal
	private Dictionary<string, PetCategoryTab> m_tabsByCategory = null;
	private PetSlotInfo[] m_slotInfos = null;

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

	// Cache some data for faster access
	private DragonData m_dragonData = null;
	private string m_initialPetSku = "";

	// Cache some assets for faster acess
	private Dictionary<string, Sprite> m_powerMiniIcons = null;
	public Dictionary<string, Sprite> powerMiniIcons {
		get {
			if(m_powerMiniIcons == null) {
				m_powerMiniIcons = ResourcesExt.LoadSpritesheet(UIConstants.POWER_MINI_ICONS_PATH);
			}
			return m_powerMiniIcons;
		}
	}

	// Some public getters
	public PetCategoryTab currentTab {
		get {
			return (PetCategoryTab)m_categoryTabs.currentScreen;
		}
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to animator's events
		animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
		animator.OnShowPostAnimation.AddListener(OnShowPostAnimation);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from animator's events
		animator.OnShowPreAnimation.RemoveListener(OnShowPreAnimation);
		animator.OnShowPostAnimation.RemoveListener(OnShowPostAnimation);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup the screen with a specific pet selected.
	/// </summary>
	/// <param name="_initialPetSku">The pet to focus. Leave empty to load current setup.</param>
	public void Initialize(string _initialPetSku) {
		// Store target pet
		m_initialPetSku = _initialPetSku;

		// Do the rest
		Initialize();
	}

	/// <summary>
	/// Setup the screen.
	/// </summary>
	public void Initialize() {
		// If not done yet, fill the tabs dictionary
		if(m_tabsByCategory == null) {
			m_tabsByCategory = new Dictionary<string, PetCategoryTab>();
			for(int i = 0; i < m_categoryTabs.screens.Count; i++) {
				PetCategoryTab tab = (PetCategoryTab)m_categoryTabs.screens[i];
				m_tabsByCategory.Add(tab.screenName, tab);	// [AOC] Screen name is set from the editor and it matches the category IDs
			}
		}

		// In order to properly initialize everything, object must be active
		this.gameObject.SetActive(true);

		// Store reference to target dragon data for faster access
		MenuSceneController menuController = InstanceManager.menuSceneController;
		m_dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Initialize all tabs one by one
		foreach(KeyValuePair<string, PetCategoryTab> kvp in m_tabsByCategory) {
			kvp.Value.Init(kvp.Key, m_dragonData);
		}

		// Slots
		// Make sure the list is initialized
		if(m_slotInfos == null) {
			m_slotInfos = this.GetComponentsInChildren<PetSlotInfo>();
		}
		MenuScreenScene scene3D = menuController.screensController.GetScene((int)MenuScreens.PETS);
		MenuDragonPreview dragonPreview = scene3D.GetComponent<MenuDragonScroller>().GetDragonPreview(m_dragonData.def.sku);
		for(int i = 0; i < m_slotInfos.Length; i++) {
			m_slotInfos[i].Init(i, dragonPreview);
		}

		// Do a first refresh - without animation
		Refresh(false);
	}

	/// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Whether to show animations or not.</param>
	public void Refresh(bool _animate) {
		// We must have the data!
		if(m_dragonData == null) return;

		// Powers
		for(int i = 0; i < m_powerIcons.Count; i++) {
			// 3 possibilities: equipped, not-equipped or invisible
			if(i < m_dragonData.pets.Count) {
				// Show
				m_powerIcons[i].gameObject.SetActive(true);

				// Equipped?
				DefinitionNode petDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PETS, m_dragonData.pets[i]);
				if(petDef == null) {
					m_powerIcons[i].InitFromDefinition(null, false, _animate);
				} else {
					// Get power definition
					DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup"));
					m_powerIcons[i].InitFromDefinition(powerDef, false, _animate);
				}
			} else {
				// Instant hide
				m_powerIcons[i].anim.ForceHide(false);
			}
		}

		// Slots
		for(int i = 0; i < m_slotInfos.Length; i++) {
			m_slotInfos[i].Refresh(m_dragonData, _animate);
		}
	}

	/// <summary>
	/// Scroll to a specific pet.
	/// </summary>
	/// <param name="_petSku">Pet sku.</param>
	/// <param name="_additionalDelay">Add extra delay, mostly to sync with other animations</param>
	public void ScrollToPet(string _petSku, float _additionalDelay = 0f) {
		// Only if already initialized
		if(m_tabsByCategory == null) return;

		// Find tab where the pet is
		for(int i = 0; i < m_categoryTabs.screens.Count; i++) {
			// Does this tab have the target pet?
			PetCategoryTab tab = (PetCategoryTab)m_categoryTabs.screens[i];
			PetPill pill = tab.GetPill(_petSku);
			if(pill != null) {
				// Found!! Make it the active tab
				PetCategoryTab previousTab = (PetCategoryTab)m_categoryTabs.currentScreen;
				m_categoryTabs.GoToScreen(tab);

				// Scroll to the target pill, adding a small delay if we're changing tabs
				float delay = _additionalDelay;
				if(previousTab != tab) {
					delay += 0.3f;
				}
				tab.ScrollToPill(pill, delay);

				// We're done! break loop
				break;
			}
		}
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
	}

	/// <summary>
	/// Screen has been opened.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPostAnimation(ShowHideAnimator _animator) {
		// If we want to scroll to a pet, do it now
		bool scrollToPet = !string.IsNullOrEmpty(m_initialPetSku);
		if(scrollToPet) {
			ScrollToPet(m_initialPetSku, 0.35f);	// Some extra delay
			m_initialPetSku = string.Empty;
		}

		// Should we show the pets info popup?
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.PETS_INFO)) {
			// If we're scrolling to a pet, give enough time to show the pet unlock animation
			float delay = 0f;
			if(scrollToPet) {
				delay += 2f;	// [AOC] Sync with animation
			}

			// Open popup with delay
			DOVirtual.DelayedCall(
				delay, 
				() => {
					PopupManager.OpenPopupInstant(PopupInfoPets.PATH);
				}
			);

			// Mark tutorial as completed
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.PETS_INFO, true);
		}
	}

	/// <summary>
	/// The pets loadout has changed in the menu.
	/// </summary>
	/// <param name="_dragonSku">The dragon whose assigned pets have changed.</param>
	/// <param name="_slotIdx">Slot that has been changed.</param>
	/// <param name="_newPetSku">New pet assigned to the slot. Empty string for unequip.</param>
	public void OnPetChanged(string _dragonSku, int _slotIdx, string _newPetSku) {
		// Update data!
		Refresh(true);

		// Save persistence - centralize all pets management persistence in here
		PersistenceManager.Save();
	}
}