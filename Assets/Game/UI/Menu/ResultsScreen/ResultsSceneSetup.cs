// ResultsSceneSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Setup to define a 3D area in the level to use for the results screen.
/// </summary>
public class ResultsSceneSetup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references, all required
	[SerializeField] private Camera m_camera = null;
	public Camera camera {
		get { return m_camera; }
	}

	[Comment("DragonLoader should be set to \"CURRENT\" mode", 10)]
	[SerializeField] private MenuDragonLoader m_dragonSlot = null;
	[SerializeField] private Transform m_eggSlot = null;
	[SerializeField] private Animator m_goldMountainAnimator = null;

	[Comment("Sort chest slots from left to right, chests will be spawned from the center depending on how many were collected.\nAlways 5 slots, please.", 10)]
	[SerializeField] private ResultsSceneChestSlot[] m_chestSlots = new ResultsSceneChestSlot[5];

	[Comment("Fog Settings used", 10)]
	[SerializeField] FogManager.FogAttributes m_fog;

	// Internal
	private List<ResultsSceneChestSlot> m_rewardedSlots = new List<ResultsSceneChestSlot>();	// The slots that we'll be actually using, sorted in order of appereance
	private bool m_eggFound = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// A change has occurred on the inspector. Validate its values.
	/// </summary>
	private void OnValidate() {
		// There must be exactly 5 chest slots
		if(m_chestSlots.Length != 5) {
			// Create a new array with exactly 5 slots and copy as many values as we can
			ResultsSceneChestSlot[] chestSlots = new ResultsSceneChestSlot[5];
			for(int i = 0; i < m_chestSlots.Length && i < chestSlots.Length; i++) {
				chestSlots[i] = m_chestSlots[i];
			}
			m_chestSlots = chestSlots;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the scene and leave it ready for the LaunchAnim() call.
	/// </summary>
	public void Init() {
		// How many chests?
		int preCollectedChests = 0;
		int pendingChests = 0;
		int collectedAndPendingChests = 0;
		List<Chest> sortedChests = new List<Chest>();
		List<Chest> sourceChests = ChestManager.sortedChests;
		if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.NONE) {
			// Real logic
			for(int i = 0; i < sourceChests.Count; i++) {
				sortedChests.Add(sourceChests[i]);
				switch(sourceChests[i].state) {
					case Chest.State.COLLECTED: {
						preCollectedChests++; 
					} break;

					case Chest.State.PENDING_REWARD: {
						pendingChests++; 
					}break;
				}
			}
			collectedAndPendingChests = preCollectedChests + pendingChests;
		} else {
			// [AOC] DEBUG ONLY!!
			pendingChests = CPResultsScreenTest.chestsMode - CPResultsScreenTest.ChestTestMode.FIXED_0;
			if(CPResultsScreenTest.chestsMode == CPResultsScreenTest.ChestTestMode.RANDOM) {
				pendingChests = Random.Range(0, 5);
			}

			// Adjust number of previously collected chests to prevent overflowing max chests
			preCollectedChests = ChestManager.collectedChests;
			collectedAndPendingChests = preCollectedChests + pendingChests;
			if(collectedAndPendingChests > ChestManager.NUM_DAILY_CHESTS) {
				collectedAndPendingChests = ChestManager.NUM_DAILY_CHESTS;
				preCollectedChests = collectedAndPendingChests - pendingChests;
			}

			// Either reuse actual chest or create a fake new one
			for(int i = 0; i < sourceChests.Count; i++) {
				if(i < preCollectedChests) {
					sortedChests.Add(sourceChests[i]);
				} else if(i < collectedAndPendingChests) {
					Chest newChest = new Chest();
					newChest.ChangeState(Chest.State.PENDING_REWARD);
					sortedChests.Add(newChest);
				} else {
					Chest newChest = new Chest();
					newChest.ChangeState(Chest.State.NOT_COLLECTED);
					sortedChests.Add(newChest);
				}
			}
		}

		// Initialize chest slots
		// Testing different layouts!
		switch(CPResultsScreenTest.chestsLayout) {
			// Option A) Show the chests in the center, left to right
			case CPResultsScreenTest.ChestsLayout.ONLY_COLLECTED_CHESTS: {
				m_rewardedSlots.Clear();
				int startIdx = (Mathf.CeilToInt(m_chestSlots.Length/2f) - Mathf.FloorToInt(pendingChests/2f)) - 1;	// -1 for 0-based index
				int endIdx = startIdx + pendingChests;
				int chestIdx = preCollectedChests + 1;
				for(int i = startIdx; i < endIdx; i++) {
					m_rewardedSlots.Add(m_chestSlots[i]);
					m_chestSlots[i].InitFromChest(sortedChests[chestIdx], ChestManager.GetRewardData(chestIdx + 1));
				}

				// Hide all slots
				for(int i = 0; i < m_chestSlots.Length; i++) {
					m_chestSlots[i].gameObject.SetActive(false);
				}
			} break;

			// Option B) Show the daily chest progression, linear order (0-1-2-3-4) left to right
			case CPResultsScreenTest.ChestsLayout.FULL_PROGRESSION: {
				// Using all chest slots
				m_rewardedSlots.Clear();
				for(int i = 0; i < m_chestSlots.Length; i++) {
					// Initialize based on chest state
					m_chestSlots[i].InitFromChest(sortedChests[i], ChestManager.GetRewardData(i + 1));
					if(sortedChests[i].state == Chest.State.PENDING_REWARD) {
						m_rewardedSlots.Add(m_chestSlots[i]);
					}
				}
			} break;
		}

		// Egg found?
		m_eggFound = false;
		switch(CPResultsScreenTest.eggMode) {
			case CPResultsScreenTest.EggTestMode.FOUND: {
				m_eggFound = true; 
			} break;

			case CPResultsScreenTest.EggTestMode.NOT_FOUND: {
				m_eggFound = false; 
			} break;

			case CPResultsScreenTest.EggTestMode.RANDOM: {
				m_eggFound = (Random.Range(0f, 1f) > 0.5f); 
			} break;

			case CPResultsScreenTest.EggTestMode.NONE: {
				m_eggFound = EggManager.collectibleEgg != null && EggManager.collectibleEgg.collected;
			} break;
		}

		// Hide egg slot
		m_eggSlot.gameObject.SetActive(false);

		// Hide dragon slot
		m_dragonSlot.gameObject.SetActive(false);

		if ( InstanceManager.fogManager != null )
			InstanceManager.fogManager.ForceAttributes( m_fog );
	}

	/// <summary>
	/// Launches the dragon intro animation.
	/// </summary>
	public void LaunchDragonAnim() {
		// Launch gold mountain animation
        if (m_goldMountainAnimator != null)
		    m_goldMountainAnimator.SetTrigger("Intro");

		// Show and trigger dragon animation
		m_dragonSlot.gameObject.SetActive(true);
		m_dragonSlot.dragonInstance.SetAnim(MenuDragonPreview.Anim.RESULTS_IN);
	}

	/// <summary>
	/// Setup and launch results animation based on current game stats (RewardManager, etc.).
	/// </summary>
	/// <returns>The total duration of the animation.</returns>
	/// <param name="_carouselPill">The carousel's pill, used to sync both animations.</param>
	public float LaunchRewardsAnim(ResultsScreenChestsPill _carouselPill) {
		// Make things easy with a sequence!
		Sequence seq = DOTween.Sequence();
		seq.AppendInterval(_carouselPill.animator.tweenDuration);	// Initial delay (time for the pill to appear)
		seq.AppendInterval(UIConstants.resultsChestsAndEggMinDuration * 0.5f);	// Extra delay - half here, half by the end of the whole sequence

		// Program animation of selected slots
		for(int i = 0; i < m_rewardedSlots.Count; i++) {
			// In order to be able to use inline functions within a loop, we must store looped vars into a copy
			ResultsSceneChestSlot slot = m_rewardedSlots[i];

			// Launch anim
			seq.AppendCallback(() => {
				// Do it
				slot.gameObject.SetActive(true);
				slot.LaunchRewardAnim();
			});

			seq.AppendInterval(UIConstants.resultsChestDuration * 0.4f);	// Arbitrary, to sync with animation
			seq.AppendCallback(() => {
				// Update pill
				_carouselPill.IncreaseChestCount();
			});

			// Add arbitrary delay (time to finish the animation + delay to next chest)
			seq.AppendInterval(UIConstants.resultsChestDuration * 0.6f);
		}

		// Program egg animation (if required!)
		if(m_eggFound) {
			// Precompute durations (must add up to 1f)
			float eggAnimDuration = UIConstants.resultsEggDuration;
			float inDelay = eggAnimDuration * 0.1f;
			float inDuration = eggAnimDuration * 0.7f;
			float outDuration = eggAnimDuration * 0.2f;

			// Initialize
			float eggSlotScale = m_eggSlot.transform.localScale.x;
			m_eggSlot.gameObject.SetActive(true);
			m_eggSlot.transform.SetLocalScale(0f);
			seq.AppendInterval(inDelay);	// Initial delay

			// Up
			seq.Append(m_eggSlot.DOScale(eggSlotScale, inDuration * 0.9f).SetEase(Ease.OutBack));
			seq.Join(m_eggSlot.DOLocalMoveY(0.25f, inDuration * 0.9f).SetRelative(true).SetEase(Ease.OutBack));
			seq.Join(m_eggSlot.DOBlendableLocalRotateBy(Vector3.up * 360f, inDuration, RotateMode.FastBeyond360).SetEase(Ease.OutCubic));

			// Down
			seq.Append(m_eggSlot.DOLocalMoveY(-0.25f, outDuration).SetRelative(true).SetEase(Ease.OutQuad));
		}

		// Final delay
		seq.AppendInterval(UIConstants.resultsChestsAndEggMinDuration * 0.5f);	// Final delay (time to process everything and let the last chest animation finish

		// Done!
		seq.Play();
		return seq.Duration();
	}
}