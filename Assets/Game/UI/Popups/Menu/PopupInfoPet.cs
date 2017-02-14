// PopupInfoPet.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 23/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
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
	public const string PATH = "UI/Popups/PF_PopupInfoPet";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private UIScene3DLoader m_preview = null;
	[SerializeField] private Localizer m_nameText = null;
	[SerializeField] private Localizer m_infoText = null;
	[SerializeField] private PowerTooltip m_powerInfo = null;
	[Space]
	[SerializeField] private GameObject m_lockedInfo = null;
	[SerializeField] private GameObject m_ownedInfo = null;
	[Space]
	[SerializeField] private GameObject m_panel = null;
	[SerializeField] private GameObject m_arrows = null;

	// Internal
	private DefinitionNode m_def = null;
	private MenuPetLoader m_petLoader = null;

	// Scroll logic
	private List<DefinitionNode> m_scrollDefs = null;
	private int m_scrollIdx = -1;
	private Sequence m_scrollSequence = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
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

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

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

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with the given pet info.
	/// </summary>
	/// <param name="_petDef">Pet definition used for the initialization.</param>
	/// <param name="_scrollDefs">List of pet definitions to scroll around. Initial def should be included. Arrows won't be displayed if null or 0-lengthed</param>
	public void Refresh(DefinitionNode _petDef, List<DefinitionNode> _scrollDefs) {
		// Skip if definition is not valid
		if(_petDef == null) return;

		// Store definition
		m_def = _petDef;

		// Init list of pets to scroll around and set arrows visibility
		m_scrollDefs = _scrollDefs;
		if(m_scrollDefs == null) {
			m_scrollIdx = -1;
		} else {
			m_scrollIdx = m_scrollDefs.IndexOf(m_def);
		}
		m_arrows.SetActive(m_scrollDefs != null && m_scrollDefs.Count > 0);

		// Initialize with target pet def
		Refresh();
	}

	/// <summary>
	/// Initialize the popup with the current pet info.
	/// </summary>
	private void Refresh() {
		// Only if current def is valid
		if(m_def == null) return;

		// Load 3D preview
		if(m_petLoader == null) {
			// Find and start infinite rotation
			m_petLoader = m_preview.scene.FindComponentRecursive<MenuPetLoader>();
			m_petLoader.gameObject.transform.DOLocalRotate(m_petLoader.gameObject.transform.localRotation.eulerAngles + Vector3.up * 360f, 10f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Restart).SetRecyclable(true);
		}
		if(m_petLoader != null) {
			// Load target pet!
			m_petLoader.Load(m_def.sku);
			//m_petLoader.petInstance.SetAnim(MenuPetPreview.Anim.IDLE);	// [AOC] TODO!! Pose the pet
		}

		// Initialize name and description texts
		if(m_nameText != null) m_nameText.Localize(m_def.Get("tidName"));
		if(m_infoText != null) m_infoText.Localize(m_def.Get("tidDesc"));

		// Initialize power info
		if(m_powerInfo != null) {
			DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_def.Get("powerup"));
			m_powerInfo.InitFromDefinition(powerDef);
		}

		// Initialize lock state
		bool owned = UsersManager.currentUser.petCollection.IsPetUnlocked(m_def.sku);
		if(m_ownedInfo != null) m_ownedInfo.SetActive(owned);
		if(m_lockedInfo != null) m_lockedInfo.SetActive(!owned);
	}

	//------------------------------------------------------------------------//
	// INTERNAL																  //
	//------------------------------------------------------------------------//
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
	/// Scroll to next pet!
	/// </summary>
	public void OnNextPet() {
		// Ignore if animating
		if(m_scrollSequence != null && m_scrollSequence.IsPlaying()) return;

		// Ignore if definitions list is not valid
		if(m_scrollDefs == null || m_scrollDefs.Count == 0) return;

		// Next index - clamp to definitions list
		m_scrollIdx++;
		if(m_scrollIdx >= m_scrollDefs.Count) {
			m_scrollIdx = 0;
		}

		// Store new selection
		m_def = m_scrollDefs[m_scrollIdx];

		// Launch animation forward
		LaunchAnim(false);
	}

	/// <summary>
	/// Scroll to previous pet!
	/// </summary>
	public void OnPreviousPet() {
		// Ignore if animating
		if(m_scrollSequence != null && m_scrollSequence.IsPlaying()) return;

		// Ignore if definitions list is not valid
		if(m_scrollDefs == null || m_scrollDefs.Count == 0) return;

		// Next index - clamp to definitions list
		m_scrollIdx--;
		if(m_scrollIdx < 0) {
			m_scrollIdx = m_scrollDefs.Count - 1;
		}

		// Store new selection
		m_def = m_scrollDefs[m_scrollIdx];

		// Launch animation backwards
		LaunchAnim(true);
	}
}