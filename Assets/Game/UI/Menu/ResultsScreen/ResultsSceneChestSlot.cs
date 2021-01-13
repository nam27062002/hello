// ResultsSceneChestSlot.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single chest slot controller in the results scene.
/// </summary>
public class ResultsSceneChestSlot : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private static readonly string COINS_REWARD_PREFAB = "UI/Metagame/Rewards/PF_ChestCoinsReward";
	private static readonly string PC_REWARD_PREFAB = "UI/Metagame/Rewards/PF_ChestPCReward";
	private static readonly string GF_REWARD_PREFAB = "UI/Metagame/Rewards/PF_ChestGFReward";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references, all required
	[SerializeField] private bool m_uiMode = false;

	// Internal
	private Chest m_chest = null;
	public Chest chest {
		get { return m_chest; }
	}

	private ChestViewController m_chestView = null;
	public ChestViewController chestView {
		get { return m_chestView; }
	}

	private GameObject m_rewardObj = null;
	public GameObject rewardObj {
		get { return m_rewardObj; }
	}

	private Chest.RewardData m_rewardData = null;
	public Chest.RewardData rewardData {
		get { return m_rewardData; }
	}

	private Chest.RewardType m_rewardType = Chest.RewardType.SC;	// Default SC
	public Chest.RewardType rewardType {
		get { return m_rewardType; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Remove placeholder
		this.transform.DestroyAllChildren(false);

		// Instantiate the actual chest
		GameObject chestPrefab = Resources.Load<GameObject>(ChestViewController.PREFAB_PATH);
		GameObject chestObj = GameObject.Instantiate<GameObject>(chestPrefab);
		chestObj.transform.SetParent(this.transform, false);
		chestObj.SetLayerRecursively(this.gameObject.layer);
		m_chestView = chestObj.GetComponentInChildren<ChestViewController>();

		// Subscribe to chest events
		m_chestView.OnChestOpen.AddListener(OnChestOpened);
		m_chestView.OnChestAnimLanded.AddListener(OnChestLanded);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize based on given chest state.
	/// </summary>
	/// <param name="_chest">The target chest.</param>
	/// <param name="_chestRewardData">The reward linked to this chest.</param>
	public void InitFromChest(Chest _chest, Chest.RewardData _chestRewardData) {
		// Store data for further use
		m_chest = _chest;
		m_rewardData = _chestRewardData;
		m_rewardType = (m_rewardData != null) ? m_rewardData.type : Chest.RewardType.SC;

		// If chest data is null, hide and return
		if(_chest == null) {
			this.gameObject.SetActive(false);
			return;
		}

		// Show and init visuals based on state
		this.gameObject.SetActive(true);
		switch(_chest.state) {
			// Already collected, open lid instantly
			case Chest.State.COLLECTED: {
				m_chestView.Open(_chestRewardData == null ? Chest.RewardType.SC : _chestRewardData.type, true);
			} break;

			// Rest of states: lid closed, wait for manual animation trigger
			default: {
				m_chestView.Close();
			} break;
		}
	}

	/// <summary>
	/// Setup and start animation with the chest info provided in the InitFromChest() method.
	/// </summary>
	/// <param name="_doIntro">Whether to also launch the intro animation or not.</param>
	public void LaunchResultsAnim(bool _doIntro) {
		// Skip reward setup if def is not valid
		if(m_rewardData != null && m_chest.state == Chest.State.PENDING_REWARD) {	// Only for chests pending reward!
			// Aux vars
			string rewardPrefabPath = COINS_REWARD_PREFAB;	// [AOC] Let's show coins by default (debug purposes)

			// PC or coins?
			switch(m_rewardType) {
				case Chest.RewardType.SC: {
					rewardPrefabPath = COINS_REWARD_PREFAB;
				} break;

				case Chest.RewardType.PC: {
					rewardPrefabPath = PC_REWARD_PREFAB;
				} break;

				case Chest.RewardType.GF: {
					rewardPrefabPath = GF_REWARD_PREFAB;
				} break;
			}

			// Load and instantiate reward prefab
			GameObject rewardPrefab = Resources.Load<GameObject>(rewardPrefabPath);
			m_rewardObj = GameObject.Instantiate<GameObject>(rewardPrefab);
			m_rewardObj.transform.SetParent(this.transform, false);
			m_rewardObj.SetLayerRecursively(this.gameObject.layer);
			m_rewardObj.SetActive(false);

			// Set text
			TextMeshPro text = m_rewardObj.FindComponentRecursive<TextMeshPro>();
			if(text != null) {
				// Set formatted text
				text.text = "+" + StringUtils.FormatNumber(m_rewardData.amount);

				// Make it look to parent camera
				// [AOC] Make it UI compatible for the new results screen
				if(m_uiMode) {
					Canvas parentCanvas = GetComponentInParent<Canvas>();
					LookAt lookAt = text.GetComponent<LookAt>();
					if(lookAt != null && parentCanvas != null) {
						lookAt.lookAtObject = parentCanvas.worldCamera.transform;
					}
				} else {
					ResultsSceneSetup parentScene = GetComponentInParent<ResultsSceneSetup>();
					LookAt lookAt = text.GetComponent<LookAt>();
					if(lookAt != null && parentScene != null) {
						lookAt.lookAtObject = parentScene.camera.transform;
					}
				}
			}
		}

		// Launch animation sequence
		// Show intro anim?
		if(_doIntro) {
			// Fall from the sky
			m_chestView.ResultsAnim();
		} else {
			// Just open
			m_chestView.Open(m_rewardType, false);
		}
	}

	/// <summary>
	/// Hides and destroys reward object.
	/// </summary>
	public void HideResultsReward() {
		// Skip if reward object is not created
		if(m_rewardObj == null) return;

		// Use the animator!
		m_rewardObj.GetComponent<Animator>().SetTrigger("out");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Event to sync with the open animation.
	/// </summary>
	public void OnChestOpened() {
		// Launch reward animation!
		if(m_rewardObj != null) {
			m_rewardObj.SetActive(true);
			m_rewardObj.GetComponent<Animator>().SetTrigger("in");
		}
	}

	/// <summary>
	/// Event to sync with the animation.
	/// </summary>
	public void OnChestLanded() {
		// Launch the open animation (except for non-collected chests)
		if(m_chest != null && m_chest.collected) {
			m_chestView.Open(m_rewardType, false);
		}
	}
}