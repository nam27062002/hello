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
	[SerializeField] private GameObject m_disguiseUnlockPrefab = null;

	// Disguises unlock
	private List<ResultsScreenDisguiseFlag> m_flags = new List<ResultsScreenDisguiseFlag>();

	// Internal
	private Tween m_xpBarTween = null;

	private int m_initialLevel = 0;
	private int m_currentLevel = 0;	// Updated during animation
	private int m_targetLevel = 0;

	private float m_initialDelta = 0f;	// Bar position at the start of the animation
	private float m_targetDelta = 1f;	// Bar position at the end of the animation

	private float m_deltaPerLevel = 0f;

	private bool m_flagsFolded = true;	// By default flags are folded when animation has finished
	private bool m_nextDragonLocked = true;	// Is the next dragon locked or has it been already unlocked using PC?
	private DragonData m_nextDragonData = null;

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
	public void Init() {
		// [AOC] As usual, animating the XP bar is not obvious (dragon may have 
		//		 leveled up several times during a single game, disguises unlocked, etc.)

		// Initialize bar with current dragon's data
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

		// Compute initial and target deltas and levels
		// Are we testing?
		DragonProgression progression = m_dragonData.progression;
		if(CPResultsScreenTest.testEnabled) {
			// Initial level and delta
			m_initialDelta = CPResultsScreenTest.xpInitialDelta;
			float initialLevelRaw = Mathf.Lerp(0, progression.maxLevel, m_initialDelta);
			m_initialLevel = Mathf.FloorToInt(initialLevelRaw);

			// Special case for last level (should only happen with delta >= 1f)
			if(m_initialLevel >= progression.maxLevel) {
				m_initialLevel = progression.maxLevel;
				m_initialDelta = 1f;
			}

			// Do the same with the target level and delta
			m_targetDelta = CPResultsScreenTest.xpFinalDelta;
			float targetLevelRaw = Mathf.Lerp(0, progression.maxLevel, m_targetDelta);
			m_targetLevel = Mathf.FloorToInt(targetLevelRaw);

			// Special case for last level (should only happen with delta >= 1f)
			if(m_targetLevel >= progression.maxLevel) {
				m_targetLevel = progression.maxLevel;
				m_targetDelta = 1f;
			}
		} else {
			// Just get levels from the reward manager
			m_initialLevel = RewardManager.dragonInitialLevel;
			m_targetLevel = progression.level;

			// Deltas are stored as relative to the level. Must be scaled to the global scale.
			m_initialDelta = (m_initialLevel * m_deltaPerLevel) + Mathf.Lerp(0f, m_deltaPerLevel, RewardManager.dragonInitialLevelProgress);
			m_targetDelta = (m_targetLevel * m_deltaPerLevel) + Mathf.Lerp(0f, m_deltaPerLevel, progression.progressCurrentLevel);
		}

		// Start at the beginning!
		m_currentLevel = m_initialLevel;

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

		// Custom treatment to disguises markes in this screen
		for(int i = 0; i < m_disguises.Count; i++) {
			// Re-attach to use aux slider instead of main one
			m_disguises[i].barMarker.AttachToSlider(m_auxBar, m_disguises[i].delta);
			m_disguises[i].unlocked = (m_disguises[i].delta <= m_auxBar.normalizedValue);	// Use current var value to quicly determine initial state

			// If the disguise is going to be unlocked, crate a flag for it!
			if(m_disguises[i].delta <= m_targetDelta) {
				// Instantiate and initialize flag
				GameObject flagObj = GameObject.Instantiate(m_disguiseUnlockPrefab, m_disguisesContainer.transform, false) as GameObject;
				m_disguises[i].flag = flagObj.GetComponent<ResultsScreenDisguiseFlag>();
				m_disguises[i].flag.InitFromDef(m_disguises[i].def);
				m_flags.Add(m_disguises[i].flag);

				// Start hidden
				m_disguises[i].flag.gameObject.SetActive(false);
			}
		}

		// Initialize disguises fold toggle
		// Move to top to make sure nothing blocks the input
		m_disguisesFoldToggle.transform.SetAsLastSibling();
		m_disguisesFoldToggle.interactable = false;
		m_disguisesFoldToggle.onClick.AddListener(ToggleFlags);
	}

	/// <summary>
	/// Launch the animation from the XP at the start of the game to the XP at the end.
	/// Init() method must have been called first.
	/// </summary>
	/// <returns>The estimated total duration of the animation, in seconds.</returns>
	public float LaunchAnimation() {
		// Ignore if already animating
		if(m_xpBarTween != null) return 0f;

		// Don't allow to fold/unfold disguises during animation
		m_disguisesFoldToggle.interactable = false;
		m_flagsFolded = false;	// Flags should be unfolded by the end of the animation

		// Single super-tween to do so
		m_xpBarTween = m_auxBar
			.DOValue(m_targetDelta, UIConstants.resultsXPBarSpeed)	// Speed based, duration representes units/sec
			.SetSpeedBased(true)
			.SetDelay(0.25f)	// Add some delay to give time to appear (external animator)
			.SetEase(Ease.Linear)
			.OnUpdate(OnXPAnimUpdate)
			.OnComplete(OnXPAnimEnd)
			.SetAutoKill(true);

		// Show FX!
		if(m_receiveFX != null) m_receiveFX.Play(true);

		// Compute and return total animation duration
		// [AOC] We can't use the tween.Duration property because it's a speed base tween, so manually do the maths
		float duration = (m_targetDelta - m_initialDelta)/UIConstants.resultsXPBarSpeed;
		return m_xpBarTween.Delay() + duration;
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
			.AppendInterval(0.25f * UIConstants.resultsDragonUnlockSpeedMultiplier)

			// Scale up
			.Append(m_dragonUnlockFX.transform.DOScale(1f, 0.25f * UIConstants.resultsDragonUnlockSpeedMultiplier).SetEase(Ease.OutBack))

			// Change bar text as well
			.AppendCallback(() => {
				if(m_infoText != null) {
					m_infoText.Localize("TID_RESULTS_DRAGON_UNLOCKED", m_nextDragonData.def.GetLocalized("tidName"));
				}
			})

			// Pause
			.AppendInterval(1f * UIConstants.resultsDragonUnlockSpeedMultiplier)

			// Scale out
			.Append(m_dragonUnlockFX.transform.DOScale(2f, 0.5f * UIConstants.resultsDragonUnlockSpeedMultiplier).SetEase(Ease.OutCubic))

			// Fade out
			.Join(m_dragonUnlockFX.GetComponent<CanvasGroup>().DOFade(0f, 0.5f * UIConstants.resultsDragonUnlockSpeedMultiplier).SetEase(Ease.OutCubic))

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
		/*MenuDragonLoader dragonLoader = m_nextDragonScene3DLoader.scene.FindComponentRecursive<MenuDragonLoader>();
		if(dragonLoader != null) {
			dragonLoader.LoadDragon(m_nextDragonData.def.sku);
			dragonLoader.dragonInstance.SetAnim(MenuDragonPreview.Anim.UNLOCKED);
		}*/
	}

	/// <summary>
	/// Launches the disguise unlock animation.
	/// </summary>
	/// <param name="_disguiseDef">Idnex of the unlocked disguise within the m_disguises list.</param>
	private void LaunchDisguiseUnlockAnimation(int _disguiseIdx) {
		// Get target disguise info
		DisguiseInfo info = m_disguises[_disguiseIdx];
		Debug.Log("Launching anim for flag " + _disguiseIdx);

		// It should have a flag instanced, activate it and launch animation
		if(info.flag == null) return;
		info.flag.gameObject.SetActive(true);
		info.flag.LaunchAnim();
		info.flag.ToggleHighlight(true);
		Debug.Log("DONE!");

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
	public void OnXPAnimUpdate() {
		// Check for level ups!
		int newLevel = Mathf.FloorToInt(Mathf.Lerp(0, m_dragonData.progression.maxLevel, m_auxBar.value));
		if(newLevel != m_currentLevel) {
			// Update text!
			RefreshLevelText(newLevel, m_dragonData.progression.maxLevel, true);

			// Instantly move secondary xp bar forward
			if(m_secondaryXPBar != null) {
				m_secondaryXPBar.value = m_auxBar.value;
			}

			// Show some FX
			if(m_levelUpFX != null) {
				m_levelUpFX.Stop();
				m_levelUpFX.Play();
			}

			// [AOC] TODO!! Some SFX?

			// Check if a disguise has been unlocked!
			for(int i = 0; i < m_disguises.Count; i++) {
				// Skip if already unlocked
				Debug.Log("Disguise " + i + ":\nunlocked? " + m_disguises[i].unlocked + "\nlevel: " + m_disguises[i].def.GetAsInt("unlockLevel") + "\ncurrentLevel: " + newLevel);
				if(m_disguises[i].unlocked) continue;
				if(m_disguises[i].def.GetAsInt("unlockLevel") == newLevel) {
					LaunchDisguiseUnlockAnimation(i);
					m_disguises[i].unlocked = true;	// Mark as unlocked
				}
			}

			// Store new value
			m_currentLevel = newLevel;
		}
	}

	/// <summary>
	/// Progression bar animation event.
	/// </summary>
	public void OnXPAnimEnd() {
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

		// Lose tween reference
		if(m_xpBarTween != null) {
			m_xpBarTween = null;
		}
	}
}
