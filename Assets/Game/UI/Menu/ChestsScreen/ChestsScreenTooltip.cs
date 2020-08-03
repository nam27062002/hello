// ChestsScreenTooltip.cs
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
public class ChestsScreenTooltip : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------8------------//
	// Setup
	[Comment("Position will be anchored to the UIAnchor transform in the corresponding slot of the 3D scene")]
	[SerializeField][Range(0, 4)] private int m_chestIdx = 0;

	// External refs
	[Space]
	[SerializeField] private Localizer m_nameText = null;
	[Space]
	[SerializeField] private TextMeshProUGUI m_rewardText = null;
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
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.CHESTS_RESET, RefreshCollected);
		Messenger.AddListener(MessengerEvents.CHESTS_PROCESSED, RefreshCollected);
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Get anchor ref
		MenuSceneController menuController = InstanceManager.menuSceneController;
		MenuScreenScene scene = menuController.GetScreenData(MenuScreen.CHESTS).scene3d;
		ChestsSceneController chestsScene = scene.GetComponent<ChestsSceneController>();
		m_3dAnchor = chestsScene.chestSlots[m_chestIdx].uiAnchor;

		// Set name
		/* [AOC] Replacing with generic "REWARD" text, already set in the Localizer component
		string tid = "";
		switch(m_chestIdx) {
			case 0: tid = "TID_GEN_ORDER_1"; break;
			case 1: tid = "TID_GEN_ORDER_2"; break;
			case 2: tid = "TID_GEN_ORDER_3"; break;
			case 3: tid = "TID_GEN_ORDER_4"; break;
			case 4: tid = "TID_GEN_ORDER_5"; break;
		}
		m_nameText.Localize(tid);
		*/

		// Do a first refresh!
		RefreshReward();
		RefreshCollected();
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
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.CHESTS_RESET, RefreshCollected);
		Messenger.RemoveListener(MessengerEvents.CHESTS_PROCESSED, RefreshCollected);
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh collected state.
	/// </summary>
	private void RefreshCollected() {
		// Only collected status
		bool collected = ChestManager.collectedAndPendingChests > m_chestIdx;

		// Show/hide checkmark accordingly
		m_checkMark.SetActive(collected);
	}

	/// <summary>
	/// Refresh reward text.
	/// </summary>
	private void RefreshReward() {
		// Initialize reward info
		Chest.RewardData rewardData = ChestManager.GetRewardData(m_chestIdx + 1);
		
        switch(rewardData.type) {
            case Chest.RewardType.SC: m_rewardText.text = UIConstants.GetIconString(rewardData.amount, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);             break;
            case Chest.RewardType.PC: m_rewardText.text = UIConstants.GetIconString(rewardData.amount, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);                break;
            case Chest.RewardType.GF: m_rewardText.text = UIConstants.GetIconString(rewardData.amount, UIConstants.IconType.GOLDEN_FRAGMENTS, UIConstants.IconAlignment.LEFT);  break;
        }
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A dragon has been acquired.
	/// </summary>
	/// <param name="_data">Data of the dragon that has just been acquired.</param>
	private void OnDragonAcquired(IDragonData _data) {
		// Reward scales with the biggest owned dragon. Update it.
		RefreshReward();
	}
}