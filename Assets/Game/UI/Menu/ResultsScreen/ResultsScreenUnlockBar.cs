// ResultsScreenUnlockBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controls the summary screen unlock bar.
/// Animation is controlled by the Carousel's progression pill.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class ResultsScreenUnlockBar : DragonXPBar {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Auxiliar class to display disguises unlocking
	private class DisguiseInfo {
		public DefinitionNode def = null;
		public float delta = 0f;
		public DragonXPBarSeparator barMarker = null;
		public bool unlocked = false;
		public ResultsScreenDisguiseFlag flag = null;
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Separator("Custom Results Screen Stuff")]
	[SerializeField] private Localizer m_infoText = null;
	[SerializeField] private Slider m_secondaryXPBar = null;

	[Separator("FX")]
	[SerializeField] private ParticleSystem m_receiveFX = null;
	[SerializeField] private ParticleSystem m_levelUpFX = null;
	[SerializeField] private GameObject m_dragonUnlockBG = null;
	[SerializeField] private GameObject m_dragonUnlockFX = null;
	[SerializeField] private UIScene3DLoader m_nextDragonScene3DLoader = null;

	[Separator("Disguises")]
	[SerializeField] private GameObject m_disguisesContainer = null;
	[SerializeField] private Button m_disguisesFoldToggle = null;
	[SerializeField] protected GameObject m_disguiseMarkerPrefab = null;
	[SerializeField] private GameObject m_disguiseUnlockPrefab = null;

	// Anim speeds
	[Separator("To easily tune animations")]
	[SerializeField] [Range(0.1f, 2f)] private float m_dragonUnlockSpeedMultiplier = 1f;

	// Disguises unlock
	private List<DisguiseInfo> m_disguises = new List<DisguiseInfo>();
	private List<ResultsScreenDisguiseFlag> m_flags = new List<ResultsScreenDisguiseFlag>();

	// Internal
	private DragonData m_nextDragonData = null;
	private Tween m_xpBarTween = null;
	private float m_initialDelta = 0f;
	private float m_targetDelta = 1f;
	private float m_deltaPerLevel = 0f;
	private bool m_flagsFolded = true;	// By default flags are folded when animation has finished
	private bool m_nextDragonLocked = true;	// Is the next dragon locked or has it been already unlocked using PC?

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// If we got a tween running, kill it immediately
		if(m_xpBarTween != null) {
			m_xpBarTween.Kill(false);
			m_xpBarTween = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Manual initialization.
	/// Animation is triggered by the Carousel's progression pill.
	/// </summary>
	/// <param name="_progressionPill">Carousel's progression pill to attach events</param>
	public void Init(ResultsScreenProgressionPill _progressionPill) {
		// Initialize with current dragon's data
		Refresh(DragonManager.currentDragon);
		m_deltaPerLevel = 1f/(m_dragonData.progression.maxLevel);

		// Change separators to work with the aux bar rather than the main bar
		for(int i = 0; i < m_barSeparators.Count; i++) {
			m_barSeparators[i].slider = m_auxBar;
		}

		// Find out next dragon to unlock
		// New design states that it's just the dragon following the one we played with
		// None if we played with the last dragon
		m_nextDragonData = null;
		int order = m_dragonData.def.GetAsInt("order");
		if(order < DragonManager.dragonsByOrder.Count - 1) {	// Exclude if playing with last dragon
			m_nextDragonData = DragonManager.dragonsByOrder[order + 1];
		}

		// Load and pose next dragon's preview
		if(m_nextDragonData != null) {
			MenuDragonLoader dragonLoader = m_nextDragonScene3DLoader.scene.FindComponentRecursive<MenuDragonLoader>();
			if(dragonLoader != null) {
				dragonLoader.LoadDragon(m_nextDragonData.def.sku);
				dragonLoader.dragonInstance.SetAnim(MenuDragonPreview.Anim.IDLE);
				m_dragonUnlockBG.SetActive(true);
			}
		} else {
			m_dragonUnlockBG.SetActive(false);
			m_nextDragonScene3DLoader.gameObject.SetActive(false);
		}

		// Compute initial and target deltas
		if(CPResultsScreenTest.testEnabled) {
			// For testing purposes
			m_initialDelta = CPResultsScreenTest.xpInitialDelta;
			m_targetDelta = CPResultsScreenTest.xpFinalDelta;
		} else {
			m_initialDelta = (RewardManager.dragonInitialLevel * m_deltaPerLevel) + Mathf.Lerp(0f, m_deltaPerLevel, m_dragonData.progression.progressCurrentLevel);	// [AOC] This should do it!
			m_targetDelta = (m_dragonData.progression.level * m_deltaPerLevel) + Mathf.Lerp(0f, m_deltaPerLevel, m_dragonData.progression.progressCurrentLevel);	// [AOC] This should do it!
		}

		// Setup bar
		if(m_xpBar != null) {
			// Use XP progress before the game as bar value (this bar won't be animated)
			m_xpBar.minValue = 0f;
			m_xpBar.maxValue = 1f;
			m_xpBar.value = m_initialDelta;
		}

		// Setup aux bar
		if(m_auxBar != null) {
			// Use XP progress before the game as bar value (we will animate with the XP earned during the game)
			m_auxBar.minValue = 0f;
			m_auxBar.maxValue = 1f;
			m_auxBar.value = m_initialDelta;
		}

		// Setup secondary XP bar
		if(m_secondaryXPBar != null) {
			// Use XP progress before the game as bar value (we will animate with the XP earned during the game)
			m_secondaryXPBar.minValue = 0f;
			m_secondaryXPBar.maxValue = 1f;
			m_secondaryXPBar.value = m_initialDelta;
		}

		// FX
		if(m_receiveFX != null) {
			m_receiveFX.Stop();
		}

		if(m_levelUpFX != null) {
			m_levelUpFX.Stop();
		}

		// Dragon unlock stuff
		if(CPResultsScreenTest.testEnabled) {
			m_nextDragonLocked = m_nextDragonData != null ? CPResultsScreenTest.nextDragonLocked : false;
		} else {
			m_nextDragonLocked = m_nextDragonData != null ? m_nextDragonData.isLocked : false;
		}
		m_dragonUnlockFX.SetActive(false);

		// Initialize info text
		// Special case for last dragon
		if(m_nextDragonData == null) {
			m_infoText.Localize("All dragons unlocked!");	// "All dragons unlocked!"
		} else if(m_nextDragonLocked) {
			m_infoText.Localize("TID_RESULTS_TO_UNLOCK_NEXT_DRAGON", m_nextDragonData.def.GetLocalized("tidName"));	// "To unlock Brute:"
		} else {
			m_infoText.Localize("TID_RESULTS_DRAGON_ALREADY_UNLOCKED", m_nextDragonData.def.GetLocalized("tidName"));	// "Brute already unlocked!"
		}

		// Initialize disguises unlock info
		// Get all disguises for this dragon and sort them by unlockLevel property
		Transform markersParent = m_barSeparatorsParent == null ? m_xpBar.transform : m_barSeparatorsParent;
		List<DefinitionNode> defList = DefinitionsManager.SharedInstance.GetDefinitionsByVariable(DefinitionsCategory.DISGUISES, "dragonSku", m_dragonData.def.sku);
		DefinitionsManager.SharedInstance.SortByProperty(ref defList, "unlockLevel", DefinitionsManager.SortType.NUMERIC);
		for(int i = 0; i < defList.Count; i++) {
			// Skip if unlockLevel is 0 (default skin)
			int unlockLevel = defList[i].GetAsInt("unlockLevel");
			if(unlockLevel <= 0) continue;

			// Create info for this disguise
			DisguiseInfo info = new DisguiseInfo();
			info.def = defList[i];

			// Compute delta corresponding to this disguise unlock level
			info.delta = Mathf.InverseLerp(0, m_dragonData.progression.maxLevel, unlockLevel);
			info.unlocked = (info.delta <= m_auxBar.normalizedValue);	// Use aux var to quicly determine initial state

			// Create and initialize bar marker
			GameObject markerObj = (GameObject)GameObject.Instantiate(m_disguiseMarkerPrefab, markersParent, false);;
			info.barMarker = markerObj.GetComponent<DragonXPBarSeparator>();
			info.barMarker.AttachToSlider(m_auxBar, info.delta);

			// If the disguise is going to be unlocked, crate a flag for it!
			if(info.delta <= m_targetDelta) {
				// Instantiate and initialize flag
				GameObject flagObj = GameObject.Instantiate(m_disguiseUnlockPrefab, m_disguisesContainer.transform, false) as GameObject;
				info.flag = flagObj.GetComponent<ResultsScreenDisguiseFlag>();
				info.flag.InitFromDef(info.def);
				m_flags.Add(info.flag);

				// Start hidden
				info.flag.gameObject.SetActive(false);
			}

			// Add to disguises list
			m_disguises.Add(info);
		}

		// Initialize disguises fold toggle
		// Move to top to make sure nothing blocks the input
		m_disguisesFoldToggle.transform.SetAsLastSibling();
		m_disguisesFoldToggle.interactable = false;
		m_disguisesFoldToggle.onClick.AddListener(ToggleFlags);

		// Attach to carousel's progression pill animation events
		_progressionPill.OnAnimStart.AddListener(OnXPAnimStart);
		_progressionPill.OnAnimUpdate.AddListener(OnXPAnimUpdate);
		_progressionPill.OnAnimLevelChanged.AddListener(OnXPAnimLevelChanged);
		_progressionPill.OnAnimEnd.AddListener(OnXPAnimEnd);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launches the dragon unlock animation.
	/// </summary>
	private void LaunchDragonUnlockAnimation() {
		// Prepare for animation
		m_dragonUnlockFX.SetActive(true);
		m_dragonUnlockFX.transform.localScale = Vector3.zero;

		// Animation
		DOTween.Sequence()
			// Initial Pause
			.SetId(this)
			.AppendInterval(0.25f * m_dragonUnlockSpeedMultiplier)

			// Scale up
			.Append(m_dragonUnlockFX.transform.DOScale(1f, 0.25f * m_dragonUnlockSpeedMultiplier).SetEase(Ease.OutBack))

			// Change bar text as well
			.AppendCallback(() => {
				if(m_infoText != null) {
					m_infoText.Localize("TID_RESULTS_DRAGON_UNLOCKED", m_nextDragonData.def.GetLocalized("tidName"));
				}
			})

			// Pause
			.AppendInterval(1f * m_dragonUnlockSpeedMultiplier)

			// Scale out
			.Append(m_dragonUnlockFX.transform.DOScale(2f, 0.5f * m_dragonUnlockSpeedMultiplier).SetEase(Ease.OutCubic))

			// Fade out
			.Join(m_dragonUnlockFX.GetComponent<CanvasGroup>().DOFade(0f, 0.5f * m_dragonUnlockSpeedMultiplier).SetEase(Ease.OutCubic))

			// Disable object once the sequence is completed
			.OnComplete(() => {
				m_dragonUnlockFX.SetActive(false);
			})

			// Go!!!
			.Play();

		// Update text with the name of the unlocked dragon
		Localizer unlockText = m_dragonUnlockFX.FindComponentRecursive<Localizer>("UnlockText");
		if(unlockText != null) unlockText.Localize("TID_RESULTS_DRAGON_UNLOCKED", m_nextDragonData.def.GetLocalized("tidName"));

		// Play cool dragon animation!
		/*if(m_nextDragonScene3D != null) {
			Animator anim = m_nextDragonScene3D.FindComponentRecursive<Animator>();
			if(anim != null) {
				anim.SetTrigger("unlocked");
			}
		}*/
	}

	/// <summary>
	/// Launches the disguise unlock animation.
	/// </summary>
	/// <param name="_disguiseDef">Idnex of the unlocked disguise within the m_disguises list.</param>
	private void LaunchDisguiseUnlockAnimation(int _disguiseIdx) {
		// Get target disguise info
		DisguiseInfo info = m_disguises[_disguiseIdx];

		// It should have a flag instanced, activate it and launch animation
		if(info.flag == null) return;
		info.flag.gameObject.SetActive(true);
		info.flag.LaunchAnim();
		info.flag.ToggleHighlight(true);

		// If there are other flags, move them out of the way
		// Reverse-loop so the first flags are the farther away (disguises are sorted, so no problem)
		Vector3 offset = new Vector3(-50f, 0f, -10f);	// Let's try playing with Z (shouldn't do anything since canvas' camera is orto
		Vector3 totalOffset = offset;
		for(int i = _disguiseIdx - 1; i >= 0; i--) {
			// Move out!
			m_flags[i].DOKill(false);
			m_flags[i].transform.DOLocalMove(totalOffset, 1f)	// Assuming flags are placed at 0 by default
				.SetEase(Ease.OutCubic)
				.SetAutoKill(true)
				.Play();

			// Remove highlight
			m_flags[i].ToggleHighlight(false);

			// Increase offset for next flag
			totalOffset += offset;
		}
	}

	/// <summary>
	/// Utility function to convert a delta within a level to a global delta.
	/// </summary>
	/// <returns>The global delta.</returns>
	/// <param name="_level">The target level.</param>
	/// <param name="_levelDelta">The local delta within the level.</param>
	private float LocalToGlobalDelta(int _level, float _levelDelta) {
		// Dragon data must be initialized
		if(m_dragonData == null) return 0f;

		// [AOC] This should do it!
		return (_level * m_deltaPerLevel) + Mathf.Lerp(0f, m_deltaPerLevel, _levelDelta);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Fold/Unfold flags. Connect to the button.
	/// </summary>
	public void ToggleFlags() {
		// Update control var
		m_flagsFolded = !m_flagsFolded;

		// Do it
		for(int i = 0; i < m_flags.Count; i++) {
			m_flags[i].ToggleFold(m_flagsFolded);
		}
	}

	/// <summary>
	/// Progression bar animation event.
	/// </summary>
	/// <param name="_currentLevel">Animation's current level.</param>
	/// <param name="_levelDelta">Animation's delta within the current level.</param>
	public void OnXPAnimStart(int _currentLevel, float _levelDelta) {
		// Don't allow to fold/unfold disguises during animation
		m_disguisesFoldToggle.interactable = false;
		m_flagsFolded = false;	// Flags should be unfolded by the end of the animation

		// Show FX!
		if(m_receiveFX != null) m_receiveFX.Play(true);
	}

	/// <summary>
	/// Progression bar animation event.
	/// </summary>
	/// <param name="_currentLevel">Animation's current level.</param>
	/// <param name="_levelDelta">Animation's delta within the current level.</param>
	public void OnXPAnimUpdate(int _currentLevel, float _levelDelta) {
		// We're actually animating the secondary bar
		m_auxBar.value = LocalToGlobalDelta(_currentLevel, _levelDelta);
	}

	/// <summary>
	/// Progression bar animation event.
	/// </summary>
	/// <param name="_currentLevel">Animation's current level.</param>
	/// <param name="_levelDelta">Animation's delta within the current level.</param>
	public void OnXPAnimLevelChanged(int _currentLevel, float _levelDelta) {
		// Move secondary xp bar forward and show some FX
		if(m_secondaryXPBar != null) {
			m_secondaryXPBar.value = m_auxBar.value;
		}

		if(m_levelUpFX != null) {
			m_levelUpFX.Stop();
			m_levelUpFX.Play();
		}

		// [AOC] TODO!! Some SFX?

		// Check if a disguise has been unlocked!
		for(int i = 0; i < m_disguises.Count; i++) {
			// Skip if already unlocked
			if(m_disguises[i].unlocked) continue;
			if(m_disguises[i].def.GetAsInt("unlockLevel") == _currentLevel) {
				LaunchDisguiseUnlockAnimation(i);
				m_disguises[i].unlocked = true;	// Mark as unlocked
			}
		}
	}

	/// <summary>
	/// Progression bar animation event.
	/// </summary>
	/// <param name="_currentLevel">Animation's current level.</param>
	/// <param name="_levelDelta">Animation's delta within the current level.</param>
	public void OnXPAnimEnd(int _currentLevel, float _levelDelta) {
		// If we reached max delta, a dragon has been unlocked!
		if(m_targetDelta >= 1f && m_nextDragonLocked) {
			// Only if next dragon was locked, obviously!
			LaunchDragonUnlockAnimation();
		}

		// Stop FX!
		if(m_receiveFX != null) m_receiveFX.Stop(true);

		// Allow to fold/unfold disguises
		m_disguisesFoldToggle.interactable = true;

		// Quickly advance secondary bar to final value
		if(m_secondaryXPBar != null) {
			m_secondaryXPBar.DOValue(m_auxBar.value, 0.15f);
		}
	}
}
