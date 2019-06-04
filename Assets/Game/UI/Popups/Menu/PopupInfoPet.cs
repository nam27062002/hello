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
	public const string PATH_SIMPLE = "UI/Popups/Tutorial/PF_PopupInfoPetSimple";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Separator("Pet Info")]
	[SerializeField] private MenuPetLoader m_preview = null;
	[SerializeField] private DragControlRotation m_rotationController = null;
	[Space]
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private PowerTooltip m_powerInfo = null;
	[Space]
	[SerializeField] private Localizer m_rarityText = null;
	[SerializeField] private Image m_rarityIcon = null;

	[Separator("Lock Info (Optional)")]
	[SerializeField] private GameObject m_ownedInfo = null;
	[SerializeField] private GameObject m_lockedInfo = null;
	[Space]
	[SerializeField] private GameObject m_basicLock = null;
	[SerializeField] private Localizer m_unlockInfoText = null;

	[Separator("Scrolling Between Pets (Optional)")]
	[SerializeField] private GameObject m_panel = null;
	[SerializeField] private ShowHideAnimator m_arrowAnimPrev = null;
	[SerializeField] private ShowHideAnimator m_arrowanimNext = null;
	[SerializeField] private PopupInfoPetScroller m_scroller = null;

	// Internal
	private DefinitionNode m_petDef = null;

	private Sequence m_scrollSequence = null;
	private bool m_hasScrolled = false;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		if(m_scroller != null) m_scroller.OnSelectionIndexChanged.AddListener(OnPetSelected);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
		if(m_scroller != null) m_scroller.OnSelectionIndexChanged.RemoveListener(OnPetSelected);
	}

    protected void OnDestroy() {
        m_preview.OnLoadingComplete.RemoveListener(OnLoadingComplete);
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize the popup with the given pet info.
    /// </summary>
    /// <param name="_petDef">Pet definition used for the initialization.</param>
    public virtual void Init(DefinitionNode _petDef) {
		List<DefinitionNode> defs = new List<DefinitionNode>();
		defs.Add(_petDef);
		Init(_petDef, defs);
	}

	/// <summary>
	/// Initialize the popup with the given pet info.
	/// </summary>
	/// <param name="_petDef">Pet definition used for the initialization.</param>
	/// <param name="_scrollDefs">List of pet definitions to scroll around. Initial def should be included. Arrows won't be displayed if null or 0-lengthed</param>
	public virtual void Init(DefinitionNode _petDef, List<DefinitionNode> _scrollDefs) {
		// Skip if definition is not valid
		if(_petDef == null) return;
		m_petDef = _petDef;

		// Init scroll logic
		if(m_scroller != null) {
			// Disable selection change events
			m_scroller.enableEvents = false;

			int defaultIndex = 0;
			List<PetScrollerItem> scrollItems = new List<PetScrollerItem>();
			foreach (DefinitionNode def in _scrollDefs) {
				PetScrollerItem item;
				item.def = def;
				scrollItems.Add(item);

				if (def == _petDef)
					defaultIndex = scrollItems.Count - 1;
			}
		
			// Init list of pets to scroll around
			m_scroller.Init(scrollItems);

			// Select target def
			m_scroller.SelectItem(scrollItems[defaultIndex]);
		}

		// Initialize with currently selected pet
		Refresh();

		// Restore selection change events
		if(m_scroller != null) m_scroller.enableEvents = true;
	}

	/// <summary>
	/// Initialize the popup with the current pet info.
	/// </summary>
	protected virtual void Refresh() {
		// Only if current def is valid
		if(m_petDef == null) return;

		// Load 3D preview
		if(m_preview != null) {
			// Assign it as target of the rotation drag controller
			if(m_rotationController != null) {
				// Reset value
				m_rotationController.target = m_preview.transform;
				m_rotationController.RestoreOriginalValue();
			}

			// Load target pet!
			m_preview.Load(m_petDef.sku);
            m_preview.OnLoadingComplete.AddListener(OnLoadingComplete);
		}

		// Initialize name and description texts
		if(m_nameText != null) m_nameText.Localize(m_petDef.Get("tidName"));

		// Initialize rarity info
		// Don't show if common
		string raritySku = m_petDef.Get("rarity");
		Metagame.Reward.Rarity rarity = Metagame.Reward.SkuToRarity(raritySku);
		if(m_rarityIcon != null) {
			m_rarityIcon.gameObject.SetActive(rarity != Metagame.Reward.Rarity.COMMON);
			m_rarityIcon.sprite = UIConstants.RARITY_ICONS[(int)rarity];
		}
		if(m_rarityText != null) {
			DefinitionNode rarityDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.RARITIES, raritySku);
			m_rarityText.gameObject.SetActive(rarity != Metagame.Reward.Rarity.COMMON);
			m_rarityText.Localize(rarityDef.Get("tidName"));

			// Text color based on rarity
			Gradient4 rarityGradient = UIConstants.GetRarityTextGradient(rarity);
			m_rarityText.text.color = Color.white;
			m_rarityText.text.enableVertexGradient = true;
			m_rarityText.text.colorGradient = new TMPro.VertexGradient(
				rarityGradient.topLeft,
				rarityGradient.topRight,
				rarityGradient.bottomLeft,
				rarityGradient.bottomRight
			);
		}

		// Initialize power info
		if(m_powerInfo != null) {
			DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_petDef.Get("powerup"));
			m_powerInfo.InitFromDefinition(powerDef, PowerIcon.Mode.PET);
		}

		// Initialize lock state
		bool owned = UsersManager.currentUser.petCollection.IsPetUnlocked(m_petDef.sku);
		if(m_ownedInfo != null) m_ownedInfo.SetActive(owned);
		if(m_lockedInfo != null) m_lockedInfo.SetActive(!owned);

		// Initialize lock info
		if(!owned) {
			if(m_basicLock != null) m_basicLock.SetActive(true);

			if(m_unlockInfoText != null) {				
				m_unlockInfoText.Localize("TID_PET_UNLOCK_INFO");				
			}
		}

		// Update arrows visibility
		if(m_scroller != null) {
			if(m_arrowAnimPrev != null) m_arrowAnimPrev.Set(m_scroller.items.Count > 1 && m_scroller.selectedItem.def != m_scroller.items.First().def);	// At least 2 pets and selected pet is not the first one
			if(m_arrowanimNext != null) m_arrowanimNext.Set(m_scroller.items.Count > 1 && m_scroller.selectedItem.def != m_scroller.items.Last().def);	// At least 2 pets and selected pet is not the last one
		}
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
		// Doesn't make sense if we have no scroller
		if(m_scroller == null) return;

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

				.OnStepComplete(() => {
					// Re-enable pet selector
					m_scroller.enabled = true;
				})

				.SetAutoKill(false)
				.Pause();
		}

		// Disable pet selector so we don't interrupt the animation
		// Will be automatically re-enabled upon finishing the animation
		m_scroller.enabled = false;

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
	public virtual void OnPetSelected(int _oldIdx, int _newIdx, bool _looped) {
		// Record selection change
		m_petDef = m_scroller.selectedItem.def;
		m_hasScrolled = true;

		// Figure out animation direction and launch it!
		bool backwards = _oldIdx > _newIdx;
		if(_looped) backwards = !backwards;	// Reverse animation direction if a loop was completed
		LaunchAnim(backwards);
	}

	/// <summary>
	/// Scroll to next pet!
	/// </summary>
	public virtual void OnNextPet() {
		// UISelector will do it for us
		if(m_scroller != null) m_scroller.SelectNextItem();
	}

	/// <summary>
	/// Scroll to previous pet!
	/// </summary>
	public virtual void OnPreviousPet() {
		// UISelector will do it for us
		if(m_scroller != null) m_scroller.SelectPreviousItem();
	}

	/// <summary>
	/// The popup is about to be opened.
	/// </summary>
	public virtual void OnOpenPreAnimation() {
		// Reset scrolled flag
		m_hasScrolled = false;
	}

	/// <summary>
	/// The popup has just closed.
	/// </summary>
	public virtual void OnClosePostAnimation() {
		// If we have changed selected pet, scroll to it!
		if(m_hasScrolled || true) {	// Do it always for now
			// If active, scroll pets screen to the current pet!
			if(InstanceManager.menuSceneController == null) return;
			MenuScreen currentScreen = InstanceManager.menuSceneController.currentScreen;
			if(currentScreen != MenuScreen.PETS && currentScreen != MenuScreen.LAB_PETS) return;	// Only if we are in the pets screen!

			// Get pets screen ref
			PetsScreenController petsScreen = InstanceManager.menuSceneController.GetScreenData(currentScreen).ui.GetComponent<PetsScreenController>();
			if(petsScreen == null) return;

			// Tell it to scroll to the target pet!
            if ( m_scroller != null )
			    petsScreen.ScrollToPet(m_scroller.selectedItem.def.sku, false, 0.15f);
		}
	}


    // Make sure billboards look at the right camera!
    private void OnLoadingComplete(MenuPetLoader _loader) {
        LookAtMainCamera[] billboards = m_preview.petInstance.GetComponentsInChildren<LookAtMainCamera>(true);
        for (int i = 0; i < billboards.Length; ++i) {
            billboards[i].overrideCamera = PopupManager.canvas.worldCamera;
        }
    }
}