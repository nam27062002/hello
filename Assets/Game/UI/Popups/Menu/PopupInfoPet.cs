// PopupInfoPet.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Popup to show extra info of a pet in the pets screen.
/// </summary>
public class PopupInfoPet : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoPet";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private UIScene3DLoader m_preview = null;
	[SerializeField] private DragControlRotation m_rotationController = null;
	[Space]
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Localizer m_infoText = null;
	[SerializeField] private PowerTooltip m_powerInfo = null;
	[Space]
	[SerializeField] private Localizer m_rarityText = null;
	[SerializeField] private Image m_rarityIcon = null;
	[Space]
	[SerializeField] private GameObject m_ownedInfo = null;
	[SerializeField] private GameObject m_lockedInfo = null;
	[Space]
	[SerializeField] private GameObject m_basicLock = null;
	[SerializeField] private GameObject m_specialLock = null;
	[SerializeField] private Localizer m_unlockInfoText = null;
	[Space]
	[SerializeField] private GameObject m_panel = null;
	[SerializeField] private ShowHideAnimator m_arrowAnimPrev = null;
	[SerializeField] private ShowHideAnimator m_arrowanimNext = null;
	[SerializeField] private PopupInfoPetScroller m_scroller = null;

	// Internal
	private MenuPetLoader m_petLoader = null;
	private Sequence m_scrollSequence = null;
	private bool m_hasScrolled = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		m_scroller.OnSelectionIndexChanged.AddListener(OnPetSelected);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		m_scroller.OnSelectionIndexChanged.RemoveListener(OnPetSelected);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given pet info.
	/// </summary>
	/// <param name="_petDef">Pet definition used for the initialization.</param>
	/// <param name="_scrollDefs">List of pet definitions to scroll around. Initial def should be included. Arrows won't be displayed if null or 0-lengthed</param>
	public void Init(DefinitionNode _petDef, List<DefinitionNode> _scrollDefs) {
		// Skip if definition is not valid
		if(_petDef == null) return;

		// Disable selection change events
		m_scroller.enableEvents = false;

		// Init list of pets to scroll around
		m_scroller.Init(_scrollDefs);

		// Select target def
		m_scroller.SelectItem(_petDef);

		// Initialize with currently selected pet
		Refresh();

		// Restore selection change events
		m_scroller.enableEvents = true;
	}

	/// <summary>
	/// Initialize the popup with the current pet info.
	/// </summary>
	private void Refresh() {
		// Only if current def is valid
		DefinitionNode petDef = m_scroller.selectedItem;
		if(petDef == null) return;

		// Load 3D preview
		if(m_petLoader == null) {
			// Find it 
			m_petLoader = m_preview.scene.FindComponentRecursive<MenuPetLoader>();
		}
		if(m_petLoader != null) {
			// Assign it as target of the rotation drag controller
			if(m_rotationController != null) {
				// Reset value
				m_rotationController.target = m_petLoader.transform;
				m_rotationController.RestoreOriginalValue();
			}

			// Load target pet!
			m_petLoader.Load(petDef.sku);
			//m_petLoader.petInstance.SetAnim(MenuPetPreview.Anim.IDLE);	// [AOC] TODO!! Pose the pet
		}

		// Initialize name and description texts
		if(m_nameText != null) m_nameText.Localize(petDef.Get("tidName"));
		if(m_infoText != null) m_infoText.Localize(petDef.Get("tidDesc"));

		// Initialize rarity info
		// Don't show if common
		EggReward.Rarity rarity = EggReward.SkuToRarity(petDef.Get("rarity"));
		if(m_rarityIcon != null) {
			m_rarityIcon.gameObject.SetActive(rarity != EggReward.Rarity.COMMON);
			m_rarityIcon.sprite = UIConstants.RARITY_ICONS[(int)rarity];
		}
		if(m_rarityText != null) {
			DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, petDef.Get("rarity"));
			m_rarityText.gameObject.SetActive(rarity != EggReward.Rarity.COMMON);
			m_rarityText.Localize(rarityDef.Get("tidName"));
			m_rarityText.text.color = UIConstants.RARITY_COLORS[(int)rarity];
		}

		// Initialize power info
		if(m_powerInfo != null) {
			DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, petDef.Get("powerup"));
			m_powerInfo.InitFromDefinition(powerDef);
		}

		// Initialize lock state
		bool owned = UsersManager.currentUser.petCollection.IsPetUnlocked(petDef.sku);
		if(m_ownedInfo != null) m_ownedInfo.SetActive(owned);
		if(m_lockedInfo != null) m_lockedInfo.SetActive(!owned);

		// Initialize lock info
		if(!owned) {
			// Special pets are unlocked with golden fragments!
			bool isSpecial = (rarity == EggReward.Rarity.SPECIAL);
			m_basicLock.SetActive(!isSpecial);
			m_specialLock.SetActive(isSpecial);
			if(isSpecial) {
				m_unlockInfoText.Localize("TID_PET_UNLOCK_INFO_SPECIAL");
			} else {
				m_unlockInfoText.Localize("TID_PET_UNLOCK_INFO");
			}
		}

		// Update arrows visibility
		m_arrowAnimPrev.Set(m_scroller.items.Count > 1 && m_scroller.selectedItem != m_scroller.items.First());	// At least 2 pets and selected pet is not the first one
		m_arrowanimNext.Set(m_scroller.items.Count > 1 && m_scroller.selectedItem != m_scroller.items.Last());	// At least 2 pets and selected pet is not the last one
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launch the scroll animation in the target direction.
	/// The popup's info will be refreshed with currently selected item on the "invisible" frame.
	/// </summary>
	/// <param name="_backwards">Left or right?</param>
	private void LaunchAnim(bool _backwards) {
		// If not already programmed, do it now
		if(m_scrollSequence == null) {
			float offset = 500f;
			float duration = 0.25f;
			CanvasGroup canvasGroup = m_panel.ForceGetComponent<CanvasGroup>();
			m_scrollSequence = DOTween.Sequence()
				.Append(m_panel.transform.DOLocalMoveX(-offset, duration).SetEase(Ease.InCubic))
				.Join(canvasGroup.DOFade(0f, duration * 0.5f).SetDelay(duration * 0.5f))

				.AppendCallback(Refresh)

				.Append(m_panel.transform.DOLocalMoveX(offset, 0.01f))	// [AOC] Super-dirty: super-fast teleport to new position, no other way than via tween
				.Append(m_panel.transform.DOLocalMoveX(0f, duration).SetEase(Ease.OutCubic))
				.Join(canvasGroup.DOFade(1f, duration * 0.5f))

				.SetAutoKill(false)
				.Pause();
		}

		// Launch the animation in the proper direction
		if(_backwards) {
			m_scrollSequence.Goto(m_scrollSequence.Duration());
			m_scrollSequence.PlayBackwards();
		} else {
			m_scrollSequence.Goto(0f);
			m_scrollSequence.PlayForward();
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// New pet selected!
	/// </summary>
	/// <param name="_oldIdx">Previous selected pet index.</param>
	/// <param name="_newIdx">New selected pet index.</param>
	/// <param name="_looped">Have we looped to do the new selection?
	public void OnPetSelected(int _oldIdx, int _newIdx, bool _looped) {
		// Ignore if animating
		//if(m_scrollSequence != null && m_scrollSequence.IsPlaying()) return;

		// Record selection change
		m_hasScrolled = true;

		// Figure out animation direction and launch it!
		bool backwards = _oldIdx > _newIdx;
		if(_looped) backwards = !backwards;	// Reverse animation direction if a loop was completed
		LaunchAnim(backwards);
	}

	/// <summary>
	/// Scroll to next pet!
	/// </summary>
	public void OnNextPet() {
		// UISelector will do it for us
		m_scroller.SelectNextItem();
	}

	/// <summary>
	/// Scroll to previous pet!
	/// </summary>
	public void OnPreviousPet() {
		// UISelector will do it for us
		m_scroller.SelectPreviousItem();
	}

	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Reset scrolled flag
		m_hasScrolled = false;
	}

	/// <summary>
	/// The popup has just closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// If we have changed selected pet, scroll to it!
		if(m_hasScrolled || true) {	// Do it always for now
			// If active, scroll pets screen to the current pet!
			if(InstanceManager.menuSceneController == null) return;
			if(InstanceManager.menuSceneController.screensController.currentScreenIdx != (int)MenuScreens.PETS) return;	// Only if we are in the pets screen!

			// Get pets screen ref
			PetsScreenController petsScreen = InstanceManager.menuSceneController.GetScreen(MenuScreens.PETS).GetComponent<PetsScreenController>();
			if(petsScreen == null) return;

			// Tell it to scroll to the target pet!
			petsScreen.ScrollToPet(m_scroller.selectedItem.sku, false, 0.15f);
		}
	}
}