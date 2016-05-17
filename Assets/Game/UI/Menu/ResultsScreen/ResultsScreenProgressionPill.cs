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
	private static readonly float ANIM_SPEED_MULT = 1f;	// To easily adjust timings
	private static readonly bool UNLOCK_DRAGON_CHEAT = false;
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Slider m_progressBar = null;
	[SerializeField] private Localizer m_infoText = null;
	[SerializeField] private GameObject m_unlockFX = null;
	[Space]
	[SerializeField] private UIScene3DLoader m_nextDragonScene3DLoader = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_progressBar != null, "Required field not initialized!");
		Debug.Assert(m_infoText != null, "Required field not initialized!");
		Debug.Assert(m_unlockFX != null, "Required field not initialized!");
		Debug.Assert(m_nextDragonScene3DLoader != null, "Required field not initialized!");
	}

	//------------------------------------------------------------------------//
	// ResultsScreenCarouselPill IMPLEMENTATION								  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Check whether this pill must be displayed on the carousel or not.
	/// </summary>
	/// <returns><c>true</c> if the pill must be displayed on the carousel, <c>false</c> otherwise.</returns>
	public override bool MustBeDisplayed() {
		// Display only if:
		// a) Next dragon wasn't already unlocked
		// b) There actually is a next dragon
		// c) Next dragon is not already owned (purchased with PC)
		DragonData nextDragonData = DragonManager.nextDragon;
		return RewardManager.dragonInitialUnlockProgress < 1f 
			&& nextDragonData != null 
			&& !nextDragonData.isOwned;
	}

	/// <summary>
	/// Initializes, shows and animates the pill.
	/// The <c>OnFinished</c> event will be invoked once the animation has finished.
	/// </summary>
	protected override void StartInternal() {
		// Aux vars
		DragonData nextDragonData = DragonManager.nextDragon;

		// Bar: animate!
		if(m_progressBar) {
			// Initialize bar
			m_progressBar.minValue = 0;
			m_progressBar.maxValue = 1;
			m_progressBar.value = RewardManager.dragonInitialUnlockProgress;

			// Program animation
			LaunchBarAnim();
		}

		// Hide unlock group
		m_unlockFX.SetActive(false);

		// Load and pose the dragon - will override any existing dragon
		MenuDragonLoader dragonLoader = m_nextDragonScene3DLoader.scene.FindComponentRecursive<MenuDragonLoader>();
		if(dragonLoader != null) {
			dragonLoader.LoadDragonPreview(nextDragonData.def.sku);
			dragonLoader.FindComponentRecursive<Animator>().SetTrigger("idle");
		}

		// Show ourselves!
		gameObject.SetActive(true);
		animator.Show();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launchs the animation of the "unlock next dragon" bar
	/// </summary>
	private void LaunchBarAnim() {
		// Aux vars
		float targetDelta = DragonManager.currentDragon.progression.progressByXp;
		if(UNLOCK_DRAGON_CHEAT) targetDelta = 1f;

		// Launch the tween!
		DOTween.To(
			// Getter function
			() => { 
				return m_progressBar.value; 
			}, 

			// Setter function
			(_newValue) => {
				m_progressBar.value = _newValue;
			},

			// Value and duration
			targetDelta, 2f * ANIM_SPEED_MULT
		)

		// Other setup parameters
			.SetDelay(0.5f * ANIM_SPEED_MULT)	// Give some time for the pill's show animation
			.SetEase(Ease.InOutCubic)

			// What to do once the anim has finished?
			.OnComplete(
				() => {
					// If we reached max delta, a dragon has been unlocked!
					if(targetDelta >= 1f || UNLOCK_DRAGON_CHEAT) {
						LaunchDragonUnlockAnimation();
					} else {
						// Notify finish after some delay
						DelayedFinish(0.5f * ANIM_SPEED_MULT);
					}
				}
			);
	}

	/// <summary>
	/// Launches the dragon unlock animation.
	/// </summary>
	private void LaunchDragonUnlockAnimation() {
		// Aux vars
		DragonData nextDragonData = DragonManager.nextDragon;

		// Prepare for animation
		m_unlockFX.SetActive(true);
		m_unlockFX.transform.localScale = Vector3.zero;

		// Animation
		DOTween.Sequence()
			// Initial Pause
			.SetId(this)
			.AppendInterval(0.25f * ANIM_SPEED_MULT)

			// Scale up
			.Append(m_unlockFX.transform.DOScale(1f, 0.25f * ANIM_SPEED_MULT).SetEase(Ease.OutBack))

			// Change bar text as well
			.AppendCallback(() => {
				if(m_infoText != null) {
					m_infoText.Localize("TID_RESULTS_DRAGON_UNLOCKED", nextDragonData.def.GetLocalized("tidName"));
				}
			})

			// Pause
			.AppendInterval(1f * ANIM_SPEED_MULT)

			// Scale out
			.Append(m_unlockFX.transform.DOScale(2f, 0.5f * ANIM_SPEED_MULT).SetEase(Ease.OutCubic))

			// Fade out
			.Join(m_unlockFX.GetComponent<CanvasGroup>().DOFade(0f, 0.5f * ANIM_SPEED_MULT).SetEase(Ease.OutCubic))

			// Disable object once the sequence is completed
			.OnComplete(() => {
				m_unlockFX.SetActive(false);
				DelayedFinish(1f * ANIM_SPEED_MULT);
			})

			// Go!!!
			.Play();

		// Update text with the name of the unlocked dragon
		Localizer unlockText = m_unlockFX.FindComponentRecursive<Localizer>("UnlockText");
		if(unlockText != null) unlockText.Localize("TID_RESULTS_DRAGON_UNLOCKED", nextDragonData.def.GetLocalized("tidName"));

		// Play cool dragon animation!
		/*if(m_nextDragonScene3D != null) {
			Animator anim = m_nextDragonScene3D.FindComponentRecursive<Animator>();
			if(anim != null) {
				anim.SetTrigger("unlocked");
			}
		}*/
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}