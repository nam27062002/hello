// ChestTooltip.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control a single tooltip for a chest in the menu.
/// </summary>
public class GoalsScreenChestTooltip : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	// Setup
	[Comment("Position will be anchored to the UIAnchor transform in the corresponding slot of the 3D scene")]
	[SerializeField][Range(0, 4)] private int m_chestIdx = 0;

	// External refs
	[Space]
	[SerializeField] private TextMeshProUGUI m_nameText = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_coinsRewardText = null;
	[SerializeField] private TextMeshProUGUI m_pcRewardText = null;
	[Space]
	[SerializeField] private GameObject m_checkMark = null;

	// Internal
	private Transform m_3dAnchor = null;
	
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
		// Get anchor ref
		MenuSceneController menuController = InstanceManager.menuSceneController;
		MenuScreenScene scene = menuController.screensController.GetScene((int)MenuScreens.GOALS);
		GoalsSceneController goalScene = scene.GetComponent<GoalsSceneController>();
		m_3dAnchor = goalScene.chestSlots[m_chestIdx].uiAnchor;

		// Set name
		// [AOC] HARDCODED!! I'm going to hell for this >_<
		switch(m_chestIdx) {
			case 0: m_nameText.text = "1st"; break;
			case 1: m_nameText.text = "2nd"; break;
			case 2: m_nameText.text = "3rd"; break;
			case 3: m_nameText.text = "4th"; break;
			case 4: m_nameText.text = "5th"; break;
		}

		// Initialize reward info - shouldn't change while alive, so do it at the Start() call
		Chest.RewardData rewardData = ChestManager.GetRewardData(m_chestIdx + 1);
		bool isPC = rewardData.type == Chest.RewardType.PC;

		m_coinsRewardText.gameObject.SetActive(!isPC);
		m_pcRewardText.gameObject.SetActive(isPC);

		if(isPC) {
			m_pcRewardText.text = UIConstants.GetIconString(rewardData.amount, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);
		} else {
			m_coinsRewardText.text = UIConstants.GetIconString(rewardData.amount, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);
		}

		// Do a first refresh!
		Refresh();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.CHESTS_RESET, Refresh);
		Messenger.AddListener(GameEvents.CHESTS_PROCESSED, Refresh);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		Messenger.RemoveListener(GameEvents.CHESTS_RESET, Refresh);
		Messenger.RemoveListener(GameEvents.CHESTS_PROCESSED, Refresh);
	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Keep anchored
		if(isActiveAndEnabled && m_3dAnchor != null) {
			// Get camera and apply the inverse transformation
			if(InstanceManager.sceneController.mainCamera != null) {
				// From http://answers.unity3d.com/questions/799616/unity-46-beta-19-how-to-convert-from-world-space-t.html
				// We can do it that easily because we've adjusted the containers to match the camera viewport coords
				Vector2 posScreen = InstanceManager.sceneController.mainCamera.WorldToViewportPoint(m_3dAnchor.position);
				RectTransform rt = this.transform as RectTransform;
				rt.anchoredPosition = Vector2.zero;
				rt.anchorMin = posScreen;
				rt.anchorMax = posScreen;
			}
		}
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
	/// Refresh all info.
	/// </summary>
	private void Refresh() {
		// Only collected status
		bool collected = ChestManager.collectedAndPendingChests > m_chestIdx;

		// Show/hide checkmark accordingly
		m_checkMark.SetActive(collected);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}