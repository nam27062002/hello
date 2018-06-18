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
using UnityEngine.Events;

using TMPro;
using DG.Tweening;

using System.Linq;
using System.Collections.Generic;

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
	private const string TUTORIAL_HIGHLIGHT_PREFAB_PATH = "UI/Metagame/Pets/PF_PetPillTutorialFX";
	private const string UNLOCK_EFFECT_PREFAB_PATH = "UI/Metagame/Pets/PF_PetPill_FX";

	public class PetPillEvent : UnityEvent<PetPill> { }

	[System.Serializable]
	private class PetShadowEffect {
		public float brightness = -0.8f;
		public float saturation = -0.7f;
		public float contrast   = -0.6f; 
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Image m_preview = null;
	[Tooltip("Optional")] [SerializeField] private Image m_powerIcon = null;
	[Tooltip("Optional")] [SerializeField] private TextMeshProUGUI m_shortDescriptionText = null;
	[Space]
	[SerializeField] private GameObject m_equippedFrame = null;
	[SerializeField] private GameObject m_equippedPowerFrame = null;
	[Space]
	[SerializeField] private Image m_seasonalIcon = null;
	[SerializeField] private GameObject m_seasonalIconRoot = null;
	[Space]
	[SerializeField] private GameObject[] m_rarityDecorations = new GameObject[(int)Metagame.Reward.Rarity.COUNT];
	[SerializeField] private Gradient4[] m_rarityFrameColors = new Gradient4[(int)Metagame.Reward.Rarity.COUNT];
	[SerializeField] private UIGradient m_rarityFrameGradient = null;
	[Space]
	[SerializeField] private UIColorFX m_frameColorFX = null;
	public UIColorFX frameColorFX {
		get { return m_frameColorFX; }
	}

	[SerializeField] private PetShadowEffect m_shadowEffect;
	[SerializeField] private UIColorFX m_colorFX;

	// Internal
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get { return m_def; }
	}

	private GameObject m_tutorialHighlightFX = null;

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

	private ShowHideAnimator m_animator = null;
	public ShowHideAnimator animator {
		get { 
			if(m_animator == null) {
				m_animator = GetComponent<ShowHideAnimator>();
			}
			return m_animator; 
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
	public bool special {
		get { return m_special; }
	}

	private DefinitionNode m_seasonDef = null;	// If it's a seasonal pet, store its target season
	public DefinitionNode seasonDef {
		get { return m_seasonDef; }
	}

	public bool m_isNotInGatcha = false;

	private DragonData m_dragonData = null;

	// Events
	public PetPillEvent OnPillTapped = new PetPillEvent();

	// Internal logic
	private bool m_tapAllowed = true;

	ResourceRequest m_previewRequest = null;
	ResourceRequest m_powerIconRequest = null;

	private static bool m_useAsycLoading = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Reset internal logic
		m_tapAllowed = true;

		// Subscribe to external events
		Messenger.AddListener<string, int, string>(MessengerEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);

		// Make sure pill is updated
		Refresh();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string, int, string>(MessengerEvents.MENU_DRAGON_PET_CHANGE, OnPetChanged);
	}

	/// <summary>
	/// A change has been done in the inspector.
	/// </summary>
	private void OnValidate() {
		// Make sure the rarity array has exactly the same length as rarities in the game.
		m_rarityDecorations.Resize((int)Metagame.Reward.Rarity.COUNT);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//º	
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
		Metagame.Reward.Rarity rarity = Metagame.Reward.SkuToRarity(_petDef.Get("rarity"));
		m_special = (rarity == Metagame.Reward.Rarity.SPECIAL);

		// Load preview
		if(m_preview != null) {
			if (m_useAsycLoading)
			{
				m_preview.sprite = null;
				m_preview.enabled = false;
				m_previewRequest = Resources.LoadAsync<Sprite>(UIConstants.PET_ICONS_PATH + m_def.Get("icon"));	
			}
			else
			{
				m_preview.sprite = Resources.Load<Sprite>(UIConstants.PET_ICONS_PATH + m_def.Get("icon"));	
			}

		}

		// Power data
		DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
		if(powerDef != null) {
			// Power icon
			if(m_powerIcon != null) {
				if ( m_useAsycLoading )
				{
					m_powerIcon.sprite = null;	// If null it will look ugly, that way we know we have a miniIcon missing
					m_powerIcon.enabled = false;
					m_powerIconRequest = Resources.LoadAsync<Sprite>(UIConstants.POWER_MINI_ICONS_PATH + powerDef.Get("miniIcon"));
				}
				else
				{
					m_powerIcon.sprite = Resources.Load<Sprite>(UIConstants.POWER_MINI_ICONS_PATH + powerDef.Get("miniIcon"));
					m_powerIcon.enabled = true;
				}
			}

			// Power short description
			if(m_shortDescriptionText != null) {
				m_shortDescriptionText.text = DragonPowerUp.GetDescription(powerDef, true, true);	// Custom formatting depending on powerup type, already localized
			}
		} else {
			if(m_powerIcon != null) {
				m_powerIcon.sprite = null;	// If null it will look ugly, that way we know we have a miniIcon missing
				m_powerIcon.enabled = false;
			}
		}

		// Rarity effects
		int rarityInt = (int)rarity;

		// Rarity icon
		for(int i = 0; i < m_rarityDecorations.Length; i++) {
			if(m_rarityDecorations[i] != null) {
				m_rarityDecorations[i].SetActive(i == rarityInt);
			}
		}

		// Rarity gradient
		if(m_rarityFrameGradient != null) {
			m_rarityFrameGradient.gradient = m_rarityFrameColors[rarityInt];

			// UIGradient inherits from Unity's BaseMeshEffect, who doesn't refresh the visuals when the object is off-viewport
			// Force a refresh by doing this
			m_rarityFrameGradient.enabled = false;
			m_rarityFrameGradient.enabled = true;
		}

		// Seasonal icon
		string targetSeason = _petDef.GetAsString("associatedSeason", SeasonManager.NO_SEASON_SKU);
		if(targetSeason == SeasonManager.NO_SEASON_SKU) {
			// Default season: Don't show icon
			m_seasonDef = null;
			m_seasonalIconRoot.SetActive(false);
		} else {
			// Custome season: Show icon
			m_seasonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SEASONS, targetSeason);
			m_seasonalIconRoot.SetActive(true);
			m_seasonalIcon.sprite = Resources.Load<Sprite>(UIConstants.SEASON_ICONS_PATH + m_seasonDef.Get("icon"));
		}
		m_isNotInGatcha = _petDef.GetAsBool("notInGatcha", false);

		// Refresh contextual elements
		Refresh();
	}


	void Update(){
		if (m_useAsycLoading)
		{
			if ( m_previewRequest != null ){
				if ( m_previewRequest.isDone ){
					m_preview.sprite = m_previewRequest.asset as Sprite;
					m_preview.enabled = true;
					m_previewRequest = null;
				}
			}

			if ( m_powerIconRequest != null ){
				if ( m_powerIconRequest.isDone ){
					m_powerIcon.sprite = m_powerIconRequest.asset as Sprite;
					m_powerIcon.enabled = true;
					m_powerIconRequest = null;
				}
			}
		}
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

		// Hide power icon for locked special pets
		if(m_powerIcon != null) {
			bool isSpecialAndLocked = m_special && m_locked;
			m_powerIcon.transform.parent.gameObject.SetActive(!isSpecialAndLocked);
		}

		// Color highlight when equipped
		if(m_frameColorFX != null) {
			m_frameColorFX.brightness = m_locked ? -0.25f : 0f;
			m_frameColorFX.saturation = m_locked ? -0.25f : 0f;
		}
		if(m_equippedFrame != null) m_equippedFrame.SetActive(equipped);
		if(m_equippedPowerFrame != null) m_equippedPowerFrame.SetActive(equipped);

		// Tone down pet preview when locked for better contrast with the lock icon
		m_preview.color = Color.white;

		if (m_locked) {
			m_colorFX.brightness = m_shadowEffect.brightness;
			m_colorFX.saturation = m_shadowEffect.saturation;
			m_colorFX.contrast   = m_shadowEffect.contrast;
		} else {
			m_colorFX.brightness = 0f;
			m_colorFX.saturation = 0f; 
			m_colorFX.contrast   = 0f;
		}
	}

	/// <summary>
	/// Prepare the unlock animation.
	/// Refresh() shouldn't be called between this call and LaunchUnlockAnim();
	/// </summary>
	public void PrepareUnlockAnim() {
		//PF_PetPill_FX
		GameObject prefab = Resources.Load<GameObject>(PetPill.UNLOCK_EFFECT_PREFAB_PATH);
		GameObject.Instantiate<GameObject>(prefab, this.transform, false);
	}

	/// <summary>
	/// Show the unlock animation.
	/// </summary>
	public void LaunchUnlockAnim() {		
		// If equip tutorial is not yet completed, show highlight around the pill!
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.PETS_EQUIP)) {
			// Give enough time for the unlock animation to finish
			UbiBCN.CoroutineManager.DelayedCall(
				() => {
					// Instantiate highlight prefab
					GameObject prefab = Resources.Load<GameObject>(PetPill.TUTORIAL_HIGHLIGHT_PREFAB_PATH);
					m_tutorialHighlightFX = GameObject.Instantiate<GameObject>(prefab, this.transform, false);
				},
				1f 	// Sync with animation!
			);
		}

		m_colorFX.brightness = 0f;
		m_colorFX.saturation = 0f; 
		m_colorFX.contrast   = 0f;
	}

	/// <summary>
	/// Do a short bounce animation on the pill.
	/// </summary>
	public void LaunchBounceAnim() {
		//this.transform.DOJump(this.transform.position, 0.15f, 1, 0.15f);
		this.transform.DOKill(true);
		this.transform.DOScale(1.25f, 0.25f).SetEase(Ease.OutCubic).SetLoops(2, LoopType.Yoyo);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The pill has been tapped.
	/// </summary>
	public void OnTap() {
		// Ignore if tap is not allowed
		if(!m_tapAllowed) return;

		// Propagate the event
		OnPillTapped.Invoke(this);

		// Ignore tap for a while to prevent spamming
		m_tapAllowed = false;
		UbiBCN.CoroutineManager.DelayedCall(() => m_tapAllowed = true, 0.25f);
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
			// Open popup with the filtered list!
			petPopup.Init(m_def, parentScreen.petFilters.filteredDefs);
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

			// If we were showing a highlight around this pill, remove it
			if(m_tutorialHighlightFX != null) {
				// Destroy and lose reference
				GameObject.Destroy(m_tutorialHighlightFX);
				m_tutorialHighlightFX = null;

				// Mark pet equip tutorial as completed
				UsersManager.currentUser.SetTutorialStepCompleted(TutorialStep.PETS_EQUIP, true);
			}
		}
	}
}