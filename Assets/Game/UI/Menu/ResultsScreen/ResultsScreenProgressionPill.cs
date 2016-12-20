// ResultsScreenProgressionPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Progression to unlock next dragon.
/// </summary>
public class ResultsScreenProgressionPill : ResultsScreenCarouselPill {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// To track bar's animation progress (int _currentLevel, float _levelDelta)
	public class BarAnimationEvent : UnityEvent<int, float> {}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private Localizer m_currentLevelText = null;
	[SerializeField] private Localizer m_nextLevelText = null;
	[SerializeField] private GameObject m_levelUpFX = null;
	[SerializeField] private ParticleSystem m_transferFX = null;

	// Exposed setup
	[Space]
	[SerializeField] [Range(0.1f, 2f)] private float m_animSpeedMultiplier = 1f;

	// Events
	[Space]
	[SerializeField] public BarAnimationEvent OnAnimStart = new BarAnimationEvent();
	[SerializeField] public BarAnimationEvent OnAnimUpdate = new BarAnimationEvent();
	[SerializeField] public BarAnimationEvent OnAnimLevelChanged = new BarAnimationEvent();
	[SerializeField] public BarAnimationEvent OnAnimEnd = new BarAnimationEvent();

	// Internal
	private Tween m_xpBarTween = null;

	private int m_initialLevel = 0;
	private int m_currentLevel = 0;	// Updated during animation
	private int m_targetLevel = 0;

	private float m_initialDelta = 0f;	// Bar position at the start of the animation
	private float m_finalDelta = 1f;	// Bar position at the end of the animation

	private bool m_isTargetMaxLevel = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_progressBar != null, "Required field not initialized!");
		Debug.Assert(m_currentLevelText != null, "Required field not initialized!");
		Debug.Assert(m_nextLevelText != null, "Required field not initialized!");

		// FX
		if(m_transferFX != null) m_transferFX.Stop(true);
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
	// ResultsScreenCarouselPill IMPLEMENTATION								  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this pill must be displayed on the carousel or not.
	/// </summary>
	/// <returns><c>true</c> if the pill must be displayed on the carousel, <c>false</c> otherwise.</returns>
	public override bool MustBeDisplayed() {
		// Always until further notice
		return true;
	}

	/// <summary>
	/// Initializes, shows and animates the pill.
	/// The <c>OnFinished</c> event will be invoked once the animation has finished.
	/// </summary>
	protected override void StartInternal() {
		// Show ourselves!
		gameObject.SetActive(true);
		animator.Show();

		// [AOC] As usual, animating the XP bar is not obvious (dragon may have leveled up several times during a single game)
		DragonData data = DragonManager.currentDragon;
		if(CPResultsScreenTest.testEnabled) {
			// Compute which level matches the cheats initial delta
			float initialLevelRaw = Mathf.Lerp(0, data.progression.maxLevel, CPResultsScreenTest.xpInitialDelta);
			m_initialLevel = Mathf.FloorToInt(initialLevelRaw);
			m_initialDelta = initialLevelRaw - m_initialLevel;	// The decimal part of the level ^^

			if(m_initialLevel >= data.progression.maxLevel) {
				m_initialLevel = data.progression.maxLevel;	// Special case for last level (should only happen with delta >= 1f)
				m_initialDelta = 1f;
			}

			// Do the same with the target level
			float targetLevelRaw = Mathf.Lerp(0, data.progression.maxLevel, CPResultsScreenTest.xpFinalDelta);
			m_targetLevel = Mathf.FloorToInt(targetLevelRaw);
			m_finalDelta = targetLevelRaw - m_targetLevel;	// The decimal part of the level ^^

			if(m_targetLevel >= data.progression.maxLevel) {
				m_targetLevel = data.progression.maxLevel;		// Special case for last level (should only happen with delta >= 1f)
				m_finalDelta = 1f;
			}

		} else {
			// Just get it from the reward manager
			m_initialLevel = RewardManager.dragonInitialLevel;
			m_targetLevel = data.progression.level;

			m_initialDelta = RewardManager.dragonInitialLevelProgress;
			m_finalDelta = data.progression.progressCurrentLevel;
		}
		m_currentLevel = m_initialLevel;

		m_isTargetMaxLevel = m_targetLevel == data.progression.maxLevel;

		// Initialize bar
		m_progressBar.minValue = 0;
		m_progressBar.maxValue = 1;
		m_progressBar.value = m_initialDelta;

		// All maths done! Launch anim!
		RefreshLevelTexts(false);
		LaunchXPBarAnim(0.5f);	// Give some time for the pill's show animation

		// Transfer FX
		if(m_transferFX != null) m_transferFX.Play(true);

		// Hide unlock group
		if(m_levelUpFX != null) {
			m_levelUpFX.SetActive(false);
		}
	}

	/// <summary>
	/// Show the pill once finished.
	/// </summary>
	public void ShowFinished() {
		// Show ourselves!
		gameObject.SetActive(true);
		animator.Show();

		// Turn off all FX
		if(m_transferFX != null) m_transferFX.Stop(true);
		if(m_levelUpFX != null) m_levelUpFX.SetActive(false);

		// Set bar into final position
		m_progressBar.value = m_finalDelta;

		// Set texts to final value
		m_currentLevel = m_targetLevel;
		RefreshLevelTexts(false);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launches the XP anim.
	/// </summary>
	/// <param name="_delay">Delay before actually starting the animation.</param> 
	private void LaunchXPBarAnim(float _delay) {
		// Aux vars
		DragonData data = DragonManager.currentDragon;
		bool isTargetLevel = false;
		float targetDelta = isTargetLevel ? m_finalDelta : 1f;	// Full bar if not target level

		if (m_isTargetMaxLevel) {
			isTargetLevel = (m_currentLevel == (m_targetLevel - 1));
		} else {
			isTargetLevel = (m_currentLevel == m_targetLevel);
		}

		// Trigger event
		OnAnimStart.Invoke(m_currentLevel, m_progressBar.value);

		// Create animation
		m_xpBarTween = DOTween.To(
			// Getter function
			() => { 
				return m_progressBar.value; 
			}, 

			// Setter function
			(_newValue) => {
				m_progressBar.value = _newValue;
			},

			// Value and speed
			// Speed based, duration representes units/sec
			targetDelta, 0.75f * m_animSpeedMultiplier		// [AOC] Should be synched with dragon unlock bar!
		)

			// Other setup parameters
			.SetSpeedBased(true)
			.SetDelay(_delay * m_animSpeedMultiplier)
			.SetEase(Ease.Linear)

			// Update callback
			.OnUpdate(
				() => {
					// Trigger event
					OnAnimUpdate.Invoke(m_currentLevel, m_progressBar.value);
				}
			)

			// What to do once the anim has finished?
			.OnComplete(
				() => {
					// Was it the target level? We're done!
					if(isTargetLevel) {								
						// Trigger event
						OnAnimEnd.Invoke(m_currentLevel, m_progressBar.value);

						// Set bar into final position
						m_progressBar.value = m_finalDelta;
						// Set texts to final value
						m_currentLevel = m_targetLevel;
						RefreshLevelTexts(false);

						// Give some delay to let the player soak up all the info before moving on to next pill
						DelayedFinish(0.5f * m_animSpeedMultiplier);

						// Stop transfer FX
						if(m_transferFX != null) m_transferFX.Stop(true);

						return;
					}

					// Not the target level, increase level counter and restart animation!
					m_currentLevel++;

					// Set text and animate
					RefreshLevelTexts(true);

					// Launch Level Up FX
					if(m_levelUpFX != null) {
						m_levelUpFX.SetActive(true);
					}

					// Put bar back to the start
					m_progressBar.value = 0f;

					// Trigger event
					OnAnimLevelChanged.Invoke(m_currentLevel, m_progressBar.value);

					// Lose tween reference (will be self-destroyed immediately) and create a new one
					m_xpBarTween = null;
					LaunchXPBarAnim(0f);
				}
			);
	}

	/// <summary>
	/// Refresh the level texts using the m_levelAnimCount var. Optionally launch a level up animation.
	/// </summary>
	/// <param name="_animate">If set to <c>true</c> animate.</param>
	private void RefreshLevelTexts(bool _animate) {
		if (m_isTargetMaxLevel && m_currentLevel == m_targetLevel) {
			m_currentLevelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_currentLevel + 1));
			m_nextLevelText.Localize("TID_MAX");
		} else {
			// Current level
			m_currentLevelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_currentLevel + 1));
			if(_animate) m_currentLevelText.transform.DOScale(1.5f, 0.15f).SetLoops(2, LoopType.Yoyo);

			// Next level
			m_nextLevelText.Localize("TID_LEVEL_ABBR", StringUtils.FormatNumber(m_currentLevel + 2));
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}