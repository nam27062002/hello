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
public class PetsScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private ScrollRect m_scrollList = null;
	public ScrollRect scrollList {
		get { return m_scrollList; }
	}
	[SerializeField] private TextMeshProUGUI m_counterText = null;

	[Space]
	[SerializeField] private float m_pillCreationDelay = 0.05f;

	[Space]
	[SerializeField] private List<PowerIcon> m_powerIcons = new List<PowerIcon>();

	// Collections
	private List<PetPill> m_pills = new List<PetPill>();
	public List<PetPill> pills {
		get { return m_pills; }
	}

	private List<DefinitionNode> m_defs = new List<DefinitionNode>();
	public List<DefinitionNode> defs {
		get { return m_defs; }
	}

	private PetSlotInfo[] m_slotInfos = null;
	public PetSlotInfo[] slotInfos {
		get { return m_slotInfos; }
	}

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

	private PetsSceneController m_petsScene = null;
	private PetsSceneController petsScene {
		get {
			if(m_petsScene == null) {
				m_petsScene = InstanceManager.menuSceneController.GetScreenScene(MenuScreens.PETS).GetComponent<PetsSceneController>();
			}
			return m_petsScene;
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

		// Clear all placeholder content from the scroll list
		m_scrollList.content.DestroyAllChildren(false);
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
		animator.OnHidePreAnimation.RemoveListener(OnHidePreAnimation);
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
		this.gameObject.SetActive(true);

		// If not done yet, load the pet definitions!
		if(m_defs.Count == 0) {
			// Get all pet definitions, no filter
			m_defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		}

		// Sort them!
		// Put owned pets at the beginning of the list, then sort by order
		m_defs.Sort((DefinitionNode _def1, DefinitionNode _def2) => {
			bool unlocked1 = UsersManager.currentUser.petCollection.IsPetUnlocked(_def1.sku);
			bool unlocked2 = UsersManager.currentUser.petCollection.IsPetUnlocked(_def2.sku);
			if(unlocked1 && !unlocked2) {
				return -1;
			} else if(unlocked2 && !unlocked1) {
				return 1;
			} else {
				return _def1.GetAsInt("order").CompareTo(_def2.GetAsInt("order"));
			}
		});

		// Store reference to target dragon data for faster access
		MenuSceneController menuController = InstanceManager.menuSceneController;
		m_dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Slots
		// Make sure the list is initialized
		if(m_slotInfos == null) {
			m_slotInfos = this.GetComponentsInChildren<PetSlotInfo>();
		}
		for(int i = 0; i < m_slotInfos.Length; i++) {
			m_slotInfos[i].Init(i);
		}

		// Do a first refresh - without animation
		Refresh(false);

		// Load/show the pills one by one to prevent a massive lag spike (and for a beautiful chain effect ^^)
		StartCoroutine(ShowPillsAsync());
	}

	/// <summary>
	/// Update visuals with current data.
	/// </summary>
	/// <param name="_animate">Whether to show animations or not.</param>
	public void Refresh(bool _animate) {
		// We must have the data!
		if(m_dragonData == null) return;

		// Init pets collection counter
		m_counterText.text = LocalizationManager.SharedInstance.Localize(
			"TID_FRACTION",
			StringUtils.FormatNumber(UsersManager.currentUser.petCollection.unlockedPetsCount),
			StringUtils.FormatNumber(m_defs.Count)
		);

		// Powers
		for(int i = 0; i < m_powerIcons.Count; i++) {
			// 3 possibilities: equipped, not-equipped or invisible
			if(i < m_dragonData.pets.Count) {
				// Show
				m_powerIcons[i].gameObject.SetActive(true);
				m_powerIcons[i].anim.ForceShow(false);

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
		// Find pet's pill
		for(int i = 0; i < m_pills.Count; i++) {
			if(m_pills[i].def.sku == _petSku) {
				PetPill pill = m_pills[i];

				// Prepare unlock anim
				pill.PrepareUnlockAnim();

				// Kill any existing anim on the scrolllist
				m_scrollList.DOKill();

				// Scroll to pill!
				float pillDeltaX = Mathf.InverseLerp(0, m_pills.Count/2f, Mathf.Floor(i/2f));	// scroll list has 2 rows! super-dirty trick
				m_scrollList.DOHorizontalNormalizedPos(pillDeltaX, 0.15f)
					.SetDelay(_additionalDelay)
					.SetEase(Ease.OutQuad)
					.OnComplete(() => {
						// Show unlock anim!
						pill.LaunchUnlockAnim();
					});

				// We're done! break loop
				break;
			}
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Show/load all the pills one by one to prevent a massive Awake lag spike
	/// (and for a beautiful chain effect ^_^).
	/// </summary>
	/// <returns>The coroutine.</returns>
	private IEnumerator ShowPillsAsync() {
		// Scroll to the start of the list
		m_scrollList.normalizedPosition = Vector2.one;

		// Hide all pills
		for(int i = 0; i < m_pills.Count; i++) {
			m_pills[i].animator.ForceHide(false);	// Force to interrupt any running hide animation (if the popup was closed and reopened very fast) 
		}

		// Initialize one pill for each pet
		for(int i = 0; i < m_defs.Count; i++) {
			// Interrupt if leaving the screen!
			if(!isActiveAndEnabled) {
				yield break;
			}

			// If we don't have enough pills, instantiate new ones
			if(i >= m_pills.Count) {
				// Instantiate pill
				GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
				m_pills.Add(newPillObj.GetComponent<PetPill>());
				m_pills[i].animator.ForceHide(false);	// Start hidden

				// React if the pill is tapped!
				m_pills[i].OnPillTapped.AddListener(OnPillTapped);
			}

			// Initialize pill
			m_pills[i].Init(m_defs[i], m_dragonData);

			// Show with a nice animation
			m_pills[i].animator.Show();

			// Wait a bit before next pill
			yield return new WaitForSecondsRealtime(m_pillCreationDelay);
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
		// Propagate to scene
		petsScene.OnShowPreAnimation();

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
	/// Screen is about to be closed.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Propagate to scene
		petsScene.OnHidePreAnimation();
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

	/// <summary>
	/// A pet pill has been tapped.
	/// </summary>
	/// <param name="_pill">The target pill.</param>
	private void OnPillTapped(PetPill _pill) {
		// Nothing to do if pet is locked
		if(_pill.locked) {
			// Different feedback if pet is unlocked with golden egg fragments
			if(_pill.special) {
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PET_UNLOCK_INFO_SPECIAL"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
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
}