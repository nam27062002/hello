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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the pets menu screen.
/// </summary>
public class PetsScreenController : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private PetScrollRect m_petScrollRect = null;
	[SerializeField] private Localizer m_counterText = null;


	[Space]
	[SerializeField] private List<PetSlot> m_petSlots = new List<PetSlot>();
	public List<PetSlot> petSlots {
		get { return m_petSlots; }
	}

	// Animation setup
	[SerializeField] private float m_initialScrollAnimDelay = 0.5f;

    [Space]
    [SerializeField] private AssetsDownloadFlow m_assetsDownloadFlow;


    // Internal references
    private NavigationShowHideAnimator m_animator = null;
	public NavigationShowHideAnimator animator {
		get { 
			if(m_animator == null) {
				m_animator = GetComponent<NavigationShowHideAnimator>();
			}
			return m_animator;
		}
	}

	// Cache some data for faster access
	private IDragonData m_dragonData = null;
	private string m_initialPetSku = "";

	// Internal logic
	private bool m_waitingForDragonPreviewToLoad = false;

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
		animator.OnHidePreAnimation.AddListener(OnHidePreAnimation);

		// Subscribe to other events
		m_petScrollRect.OnFilterChanged.AddListener(OnFilterChanged);
		m_petScrollRect.OnPillTapped.AddListener(OnPillTapped);

		// Subscribe to external events
		Messenger.AddListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, OnCPBoolChanged);

        // OTA: Show the general download progress bar
        if (m_assetsDownloadFlow != null)
        {
            Downloadables.Handle allContentHandle = HDAddressablesManager.Instance.GetHandleForAllDownloadables();
            m_assetsDownloadFlow.InitWithHandle(allContentHandle);
        }

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
		Messenger.AddListener<string, int, string>(MessengerEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {		
		// Unsubscribe from external events
		Messenger.RemoveListener<string, int, string>(MessengerEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
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
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from animator's events
		animator.OnShowPreAnimation.RemoveListener(OnShowPreAnimation);
		animator.OnShowPostAnimation.RemoveListener(OnShowPostAnimation);
		animator.OnHidePreAnimation.RemoveListener(OnHidePreAnimation);

		// Unsubscribe from other events
		m_petScrollRect.OnFilterChanged.RemoveListener(OnFilterChanged);
		m_petScrollRect.OnPillTapped.RemoveListener(OnPillTapped);

		// Unsubscribe from external events
		Messenger.RemoveListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, OnCPBoolChanged);
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
		// In order to properly initialize everything, object must be active
		bool wasActive = this.gameObject.activeSelf;
		this.gameObject.SetActive(true);


		// Slots
		for(int i = 0; i < m_petSlots.Count; i++) {
			m_petSlots[i].Init(i);
		}

		// Store reference to target dragon data for faster access
		MenuSceneController menuController = InstanceManager.menuSceneController;
		m_dragonData = DragonManager.GetDragonData(menuController.selectedDragon);
		m_petScrollRect.Setup(m_dragonData);


		// Do a first refresh - without animation
		Refresh(false);

		// We're done! Restore original object state
		this.gameObject.SetActive(wasActive);
	}

	/// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Whether to show animations or not.</param>
	public void Refresh(bool _animate) {
		// We must have the data!
		if(m_dragonData == null) return;

		// Slots
		for(int i = 0; i < m_petSlots.Count; i++) {
			// Initialize slot
			m_petSlots[i].Refresh(m_dragonData, _animate);
		}
	}

	/// <summary>
	/// Scroll to a specific pet.
	/// </summary>
	/// <param name="_petSku">Pet sku.</param>
	/// <param name="_showUnlockAnim">Whether to launch the unlock animation or not.</param>
	/// <param name="_delay">Add delay, mostly to sync with other animations and to wait for the target pill to be instantiated.</param>
	public void ScrollToPet(string _petSku, bool _showUnlockAnim, float _delay = 0f) {
		m_petScrollRect.FocusOn(_petSku, _showUnlockAnim);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//


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
	/// Screen has been opened.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPostAnimation(ShowHideAnimator _animator) {
		// If we want to scroll to a pet, do it now
		bool scrollToPet = !string.IsNullOrEmpty(m_initialPetSku);
		if(scrollToPet) {
			ScrollToPet(m_initialPetSku, true, m_initialScrollAnimDelay);	// Some extra delay
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
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					// Tracking
					string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupInfoPets.PATH);
					HDTrackingManager.Instance.Notify_InfoPopup(popupName, "automatic");

					// Open popup
					PopupManager.OpenPopupInstant(PopupInfoPets.PATH);
				},
				delay
			);

			// Mark tutorial as completed
			UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.PETS_INFO, true);
		}
	}

	/// <summary>
	/// Screen is about to be closed.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Restore dragon's pets
		if (InstanceManager.menuSceneController.selectedDragonPreview)
			InstanceManager.menuSceneController.selectedDragonPreview.equip.TogglePets(true, true);
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
        PersistenceFacade.instance.Save_Request();
    }

	/// <summary>
	/// The filters have changed.
	/// </summary>
	/// <param name="_filters">The filters collection that triggered the event.</param>
	public void OnFilterChanged(PetScrollRect _filters) {
		// Find out unlocked pets in the current filtered list
		int unlockedCount = _filters.filteredDefs.Where(
			(_item) => UsersManager.currentUser.petCollection.IsPetUnlocked(_item.data.def.sku)
		).ToList().Count;

		// Get current filter name
		string categorySku = "";
		string categoryName = "all";
		if(_filters.currentFilter == PetScrollRect.ALL_FILTERS) {
			categoryName = LocalizationManager.SharedInstance.Localize("TID_PET_CATEGORY_ALL");
		} else {
			DefinitionNode categoryDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PET_CATEGORIES, _filters.currentFilter);
			if(categoryDef != null) {
				categorySku = categoryDef.sku;
				categoryName = categoryDef.GetLocalized("tidName");
			} else {
				categoryName = "UNKNOWN CATEGORY!";
			}
		}

		// Refresh counter text
		m_counterText.Localize(
			"TID_PET_COUNTER",
			StringUtils.FormatNumber(unlockedCount),
			StringUtils.FormatNumber(_filters.filteredDefs.Count),
			categoryName,
			UIConstants.GetPetCategoryColor(categorySku).ToHexString("#")
		);

		// Animate!
		m_counterText.GetComponent<ShowHideAnimator>().RestartShow();
	}

	/// <summary>
	/// A pet pill has been tapped.
	/// </summary>
	/// <param name="_pill">The target pill.</param>
	private void OnPillTapped(PetPill _pill) {
		// Nothing to do if pet is locked
		if(_pill.locked) {
			if(_pill.seasonDef != null) {
				// Also different feedback if it's a seasonal pet
				UIFeedbackText.CreateAndLaunch(
					LocalizationManager.SharedInstance.Localize("TID_PET_UNLOCK_INFO_SEASON", _pill.seasonDef.GetLocalized("tidName")), 
					new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform
				);
			}else if ( _pill.m_isNotInGatcha ){
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize(_pill.def.Get("tidUnlockCondition")), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			} else {
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PET_UNLOCK_INFO"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			}
		}

		// If equipped in the target slot, try to unequip
		else if(_pill.equipped) {
			// Unequip
			UsersManager.currentUser.UnequipPet(m_dragonData.def.sku, _pill.def.sku);
		} 

		// Otherwise try to equip to the target slot
		else {
			// Equip - find first available slot
			int newSlot = UsersManager.currentUser.EquipPet(m_dragonData.def.sku, _pill.def.sku);

			// Feedback
			if(newSlot == -4) {
				UIFeedbackText text = UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PET_NO_SLOTS"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);	// There are no available slots!\nUnequip another pet before equipping this one.
			} else if(newSlot < 0) {
				UIFeedbackText text = UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("Unknown error!"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);	// There are no available slots!\nUnequip another pet before equipping this one.
				text.text.color = Color.red;
			}
		}
	}

	/// <summary>
	/// A control panel boolean flag has been changed.
	/// </summary>
	/// <param name="_id">CP Property ID.</param>
	/// <param name="_newValue">New value.</param>
	private void OnCPBoolChanged(string _id, bool _newValue) {
		// Check id
		if(_id == DebugSettings.SHOW_HIDDEN_PETS) {
			// Force a reload of the pets list the next time we enter the screen
			Initialize();
		}
	}

	/// <summary>
	/// A global event has been sent.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Broadcast event info.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			// Language has changed
			case BroadcastEventType.LANGUAGE_CHANGED: {
				// Refresh visuals
				Refresh(false);
				OnFilterChanged(m_petScrollRect);
			} break;
		}
	}
}