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
[RequireComponent(typeof(PetFilters))]
public class PetsScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private const int INSTANT_INITIAL_PILLS = 0;
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private ScrollRect m_scrollList = null;
	public ScrollRect scrollList {
		get { return m_scrollList; }
	}
	[SerializeField] private Localizer m_counterText = null;

	[Space]
	[SerializeField] private float m_pillCreationDelay = 0.025f;

	[Space]
	[SerializeField] private List<PetSlot> m_petSlots = new List<PetSlot>();
	public List<PetSlot> petSlots {
		get { return m_petSlots; }
	}

	// Animation setup
	[Space]
	[SerializeField] private float m_scrollAnimDuration = 0.5f;
	[SerializeField] private float m_initialScrollAnimDelay = 0.5f;

	// Collections
	private List<PetPill> m_pills = new List<PetPill>();
	public List<PetPill> pills {
		get { return m_pills; }
	}

	private List<DefinitionNode> m_defs = new List<DefinitionNode>();
	public List<DefinitionNode> defs {
		get { return m_defs; }
	}

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

	private PetsSceneController m_petsScene = null;
	public PetsSceneController petsScene {
		get {
			if(m_petsScene == null) {
				m_petsScene = InstanceManager.menuSceneController.GetScreenScene(MenuScreens.PETS).GetComponent<PetsSceneController>();
			}
			return m_petsScene;
		}
	}

	private PetFilters m_petFilters = null;
	public PetFilters petFilters {
		get {
			if(m_petFilters == null) {
				m_petFilters = this.GetComponent<PetFilters>();
			}
			return m_petFilters;
		}
	}

	// Cache some data for faster access
	private DragonData m_dragonData = null;
	private string m_initialPetSku = "";

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
		petFilters.OnFilterChanged.AddListener(OnFilterChanged);
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

		// Unsubscribe from other events
		petFilters.OnFilterChanged.RemoveListener(OnFilterChanged);
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
		// Cache some data
		Dictionary<string, int> filterOrder = new Dictionary<string, int>();
		for(int i = 0; i < petFilters.filterButtons.Length; i++) {
			filterOrder[petFilters.filterButtons[i].filterName] = i;
		}

		// Put owned pets at the beginning of the list, then sort by category (following filter buttons order), finally by content order
		m_defs.Sort((DefinitionNode _def1, DefinitionNode _def2) => {
			bool unlocked1 = UsersManager.currentUser.petCollection.IsPetUnlocked(_def1.sku);
			bool unlocked2 = UsersManager.currentUser.petCollection.IsPetUnlocked(_def2.sku);
			if(unlocked1 && !unlocked2) {
				return -1;
			} else if(unlocked2 && !unlocked1) {
				return 1;
			} else {
				// Both pets locked or unlocked: sort by category first (following filter buttons order), by content order afterwards
				int catOrder1 = int.MaxValue;
				int catOrder2 = int.MaxValue;
				filterOrder.TryGetValue(_def1.Get("category"), out catOrder1);
				filterOrder.TryGetValue(_def2.Get("category"), out catOrder2);
				if(catOrder1 < catOrder2) {
					return -1;
				} else if(catOrder2 < catOrder1) {
					return 1;
				} else {
					// Both pets of the same category: sort by order
					return _def1.GetAsInt("order").CompareTo(_def2.GetAsInt("order"));
				}
			}
		});

		// Store reference to target dragon data for faster access
		MenuSceneController menuController = InstanceManager.menuSceneController;
		m_dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Slots
		for(int i = 0; i < m_petSlots.Count; i++) {
			m_petSlots[i].Init(i);
		}

		// Reset filters
		petFilters.ResetFilters();

		// Do a first refresh - without animation
		Refresh(false);

		// Show the pills!
		ShowPills();
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
	/// <param name="_additionalDelay">Add extra delay, mostly to sync with other animations</param>
	public void ScrollToPet(string _petSku, bool _showUnlockAnim, float _additionalDelay = 0f) {
		// Aux vars
		int targetPillIdx = 0;
		int pillCount = 0;
		PetPill targetPill = null;
		PetPill pill = null;

		// Find pet's pill
		for(int i = 0; i < m_pills.Count; i++) {
			// If pill is not visible, ignore
			pill = m_pills[i];
			if(!pill.animator.visible) continue;

			// Increase active pill count
			pillCount++;

			// Is it the target pill?
			if(pill.def.sku == _petSku) {
				// Yes! Store reference
				targetPill = pill;
				targetPillIdx = pillCount;	// Relative index within all active pills
			}
		}

		// If a target pill was selected, scroll to it!
		if(targetPill != null) {
			// Prepare unlock anim
			if(_showUnlockAnim) {
				targetPill.PrepareUnlockAnim();
			}

			// Kill any existing anim on the scrolllist
			m_scrollList.DOKill();

			// BASED IN UNITY'S ScrollRect source code
			// https://bitbucket.org/Unity-Technologies/ui/src/0155c39e05ca5d7dcc97d9974256ef83bc122586/UnityEngine.UI/UI/Core/ScrollRect.cs?at=5.2&fileviewer=file-view-default
			// Get viewport bounds
			Bounds viewportBounds = new Bounds(m_scrollList.viewport.rect.center, m_scrollList.viewport.rect.size);

			// Get content bounds in viewport space:
			Vector3 vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			Matrix4x4 toLocal = m_scrollList.viewport.worldToLocalMatrix;
			Vector3[] corners = new Vector3[4];
			m_scrollList.content.GetWorldCorners(corners);
			for(int i = 0; i < 4; i++) {
				Vector3 v = toLocal.MultiplyPoint3x4(corners[i]);
				vMin = Vector3.Min(v, vMin);
				vMax = Vector3.Max(v, vMax);
			}
			Bounds contentBounds = new Bounds(vMin, Vector3.zero);
			contentBounds.Encapsulate(vMax);

			// Get pill bounds in viewport space:
			vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			(targetPill.transform as RectTransform).GetWorldCorners(corners);
			for(int i = 0; i < 4; i++) {
				Vector3 v = toLocal.MultiplyPoint3x4(corners[i]);
				vMin = Vector3.Min(v, vMin);
				vMax = Vector3.Max(v, vMax);
			}
			Bounds pillBounds = new Bounds(vMin, Vector3.zero);
			pillBounds.Encapsulate(vMax);

			// How much the content is larger than the view.
			float hiddenLength = contentBounds.size.x - viewportBounds.size.x;

			// Where the position of the lower left corner of the content bounds should be, in the space of the view.
			float targetDeltaX = (viewportBounds.min.x - contentBounds.min.x + pillBounds.center.x)/hiddenLength;
			targetDeltaX = Mathf.Clamp01(targetDeltaX);

			// Do it!
			m_scrollList.DOHorizontalNormalizedPos(targetDeltaX, m_scrollAnimDuration)
				.SetDelay(_additionalDelay)
				.SetEase(Ease.OutQuad)
				.OnComplete(() => {
					// Show some feedback!
					if(_showUnlockAnim) {
						targetPill.LaunchUnlockAnim();
					} else {
						targetPill.LaunchBounceAnim();
					}
				});
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Instantiates the required amount of pills in a background process.
	/// Do this to prevent massive Awake() lag spike.
	/// </summary>
	/// <returns>The coroutine.</returns>
	public IEnumerator InstantiatePillsAsync() {
		// If not done yet, load the pet definitions!
		if(m_defs.Count == 0) {
			// Get all pet definitions, no filter
			m_defs = DefinitionsManager.SharedInstance.GetDefinitionsList(DefinitionsCategory.PETS);
		}

		// Clear all placeholder content from the scroll list
		m_scrollList.content.DestroyAllChildren(false);
		m_pills.Clear();

		// Create a pill for every definition, one per frame
		// Do more than one per frame
		int createdThisFrame = 0;
		for(int i = 0; i < m_defs.Count; i++) {		// Only if we don't have enough of them!
			// Instantiate pill
			GameObject newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab, m_scrollList.content, false);
			m_pills.Add(newPillObj.GetComponent<PetPill>());
			m_pills[i].animator.ForceHide(false);	// Start hidden

			// React if the pill is tapped!
			m_pills[i].OnPillTapped.AddListener(OnPillTapped);

			// Wait a bit before next chunk of pills (to prevent massive lag spike)
			createdThisFrame++;
			if(createdThisFrame == 3) {
				createdThisFrame = 0;	// Reset counter
				yield return new WaitForSecondsRealtime(m_pillCreationDelay);
			}
		}
	}

	/// <summary>
	/// Show and initialize all the pills, and create new ones if needed.
	/// </summary>
	private void ShowPills() {
		// Hide all pills
		for(int i = 0; i < m_pills.Count; i++) {
			m_pills[i].animator.ForceHide(false);	// Force to interrupt any running hide animation (if the popup was closed and reopened very fast) 
		}

		// Initialize one pill for each pet
		for(int i = 0; i < m_defs.Count; i++) {
			// If we don't have enough pills, instantiate new ones
			// This should never happen since we've already instantiated all the necessary pills in the InstantiatePillsAsync method, but leave it just in case
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
			// Check filters to see whether this pet must be shown or not
			if(petFilters.CheckFilter(m_pills[i].def)) {
				// Restart animation when showing
				m_pills[i].animator.tweenDelay = 0f;
				m_pills[i].animator.RestartShow();
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
		// Propagate to scene
		petsScene.OnShowPreAnimation();

		// Refresh with initial data!
		Initialize();

		// Reset scroll list and program initial animation
		m_scrollList.horizontalNormalizedPosition = 0f;
		m_scrollList.DOHorizontalNormalizedPos(-10f, 0.5f).From().SetEase(Ease.OutCubic).SetDelay(0.25f).SetUpdate(true);
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
	/// The filters have changed.
	/// </summary>
	/// <param name="_filters">The filters collection that triggered the event.</param>
	public void OnFilterChanged(PetFilters _filters) {
		// Find out unlocked pets in the current filtered list
		int unlockedCount = _filters.filteredDefs.Where(
			(_def) => UsersManager.currentUser.petCollection.IsPetUnlocked(_def.sku)
		).ToList().Count;

		// Refresh counter text
		m_counterText.Localize(
			m_counterText.tid,
			StringUtils.FormatNumber(unlockedCount),
			StringUtils.FormatNumber(_filters.filteredDefs.Count)
		);
		m_counterText.GetComponent<ShowHideAnimator>().RestartShow();
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