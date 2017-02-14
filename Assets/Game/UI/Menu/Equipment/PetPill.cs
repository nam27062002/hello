﻿
// PetPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single pill representing a pet.
/// </summary>
public class PetPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static readonly Color LOCKED_COLOR = new Color(0.5f, 0.5f, 0.5f);

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Image m_preview = null;
	[SerializeField] private GameObject m_lockIcon = null;
	[SerializeField] private GameObject m_lockIconSpecial = null;
	[SerializeField] private Image m_powerIcon = null;
	[Space]
	[SerializeField] private GameObject m_equippedFrame = null;
	[SerializeField] private GameObject m_equippedPowerFrame = null;

	// Internal
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private GameObject m_currentLockIcon = null;
	private Animator m_currentLockIconAnim = null;

	// Shortcuts
	private PetCollection petCollection {
		get { return UsersManager.currentUser.petCollection; }
	}

	private PetsScreenController m_parentScreen = null;
	private PetsScreenController parentScreen {
		get {
			if(m_parentScreen == null) {
				m_parentScreen = this.gameObject.FindComponentInParents<PetsScreenController>();	// Find rather than Get to include inactive objects (this method could be called with the screen disabled)
			}
			return m_parentScreen;
		}
	}

	// Cache some data for convenience
	private bool m_locked = true;
	public bool locked {
		get { return m_locked; }
	}

	private int m_slot = -1;	// [AOC] If the pet is equipped at the current dragon, index of the slot corresponding to this pet
	public int slot {
		get { return m_slot; }
	}

	public bool equipped {
		get { return m_slot >= 0; }
	}

	private bool m_special = false;
	private DragonData m_dragonData = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);

		// Make sure pill is updated
		Refresh();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string, int, string>(GameEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize from a given pet definition.
	/// </summary>
	/// <param name="_petDef">The definition used to initialize the pill.</param>
	/// <param name="_dragonData">The dragon we're tuning.</param> 
	public void Init(DefinitionNode _petDef, DragonData _dragonData) {
		// Store target dragon data
		m_dragonData = _dragonData;

		// Optimization: if target def is the same as current one, just do a refresh
		if(_petDef == m_def) {
			Refresh();
			return;
		}

		// Store definition and some data
		m_def = _petDef;
		m_special = (_petDef.Get("rarity") == "special");

		// Load preview
		if(m_preview != null) {
			m_preview.sprite = Resources.Load<Sprite>(UIConstants.PET_ICONS_PATH + m_def.Get("icon"));
		}

		// Power icon
		if(m_powerIcon != null) {
			DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
			Sprite powerIcon = null;
			if(powerDef != null) {
				parentScreen.powerMiniIcons.TryGetValue(powerDef.Get("miniIcon"), out powerIcon);
			}
			m_powerIcon.sprite = powerIcon;	// If null it will look ugly, that way we know we have a miniIcon missing
		}

		// Lock icons
		// Different lock icon for special pets
		if(m_special) {
			m_currentLockIcon = m_lockIconSpecial;
		} else {
			m_currentLockIcon = m_lockIcon;
		}
		m_lockIcon.SetActive(!m_special);
		m_lockIconSpecial.SetActive(m_special);
		m_currentLockIconAnim = m_currentLockIcon.GetComponent<Animator>();

		// Refresh contextual elements
		Refresh();
	}

	/// <summary>
	/// Refresh pill's contextual elements based on assigned pet's state.
	/// </summary>
	public void Refresh() {
		// Ignore if required data is not ready
		if(UsersManager.currentUser == null) return;
		if(petCollection == null) return;
		if(m_dragonData == null) return;
		if(m_def == null) return;

		// Status flags
		m_locked = !petCollection.IsPetUnlocked(m_def.sku);
		m_slot = UsersManager.currentUser.GetPetSlot(m_dragonData.def.sku, m_def.sku);

		// Lock icon
		m_currentLockIcon.SetActive(m_locked);

		// Hide power icon for locked special pets
		bool isSpecialAndLocked = m_special && m_locked;
		m_powerIcon.transform.parent.gameObject.SetActive(!isSpecialAndLocked);

		// Color highlight when equipped
		m_equippedFrame.SetActive(equipped);
		m_equippedPowerFrame.SetActive(equipped);

		// Tone down pet preview when locked for better contrast with the lock icon
		m_preview.color = m_locked ? LOCKED_COLOR : Color.white;
	}

	/// <summary>
	/// Prepare the unlock animation.
	/// Refresh() shouldn't be called between this call and LaunchUnlockAnim();
	/// </summary>
	public void PrepareUnlockAnim() {
		m_currentLockIcon.SetActive(true);
		m_currentLockIconAnim.SetTrigger("idle");
	}

	/// <summary>
	/// Show the unlock animation.
	/// </summary>
	public void LaunchUnlockAnim() {
		m_currentLockIconAnim.SetTrigger("unlock");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The pill has been tapped.
	/// </summary>
	public void OnTap() {
		// If locked, show some feedback
		if(locked) {
			// Different feedback if pet is unlocked with golden egg fragments
			if(m_special) {
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PET_UNLOCK_INFO_SPECIAL"), new Vector2(0.5f, 0.4f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			} else {
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PET_UNLOCK_INFO"), new Vector2(0.5f, 0.4f), this.GetComponentInParent<Canvas>().transform as RectTransform);
			}

			// Small animation on the lock icon
			m_currentLockIconAnim.SetTrigger("bounce");
		}

		// If equipped, try to unequip
		else if(equipped) {
			// Unequip
			UsersManager.currentUser.UnequipPet(m_dragonData.def.sku, m_def.sku);
		} 

		// Otherwise try to equip
		else {
			// Equip
			// Refresh will be automatically triggered by the OnPetChanged callback
			int newSlot = UsersManager.currentUser.EquipPet(m_dragonData.def.sku, m_def.sku);
			if(newSlot == -4) {
				// No available slots, show feedback
				UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PET_NO_SLOTS"), new Vector2(0.5f, 0.4f), this.GetComponentInParent<Canvas>().transform as RectTransform);	// There are no available slots!\nUnequip another pet before equipping this one.
			}
		}
	}

	/// <summary>
	/// Info button was pressed.
	/// </summary>
	public void OnInfoButton() {
		// Ignore if pill is not initialized
		if(m_def == null) return;

		// Open info popup for this pet
		PopupController popup = PopupManager.OpenPopupInstant(PopupInfoPet.PATH);
		PopupInfoPet petPopup = popup.GetComponent<PopupInfoPet>();
		if(petPopup != null) {
			petPopup.Refresh(m_def, parentScreen.currentTab.defs);
		}
	}

	/// <summary>
	/// The pets loadout has changed in the menu.
	/// </summary>
	/// <param name="_dragonSku">The dragon whose assigned pets have changed.</param>
	/// <param name="_slotIdx">Slot that has been changed.</param>
	/// <param name="_newPetSku">New pet assigned to the slot. Empty string for unequip.</param>
	public void OnPetChanged(string _dragonSku, int _slotIdx, string _newPetSku) {
		// Ignore if pill is not initialized
		if(m_def == null) return;

		// Check whether it affects this pill
		if(m_slot == _slotIdx || _newPetSku == m_def.sku) {
			Refresh();
		}
	}
}