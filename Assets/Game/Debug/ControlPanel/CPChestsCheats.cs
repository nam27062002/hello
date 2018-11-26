// CPChestsCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/01/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control Panel Daily Chests section.
/// </summary>
public class CPChestsCheats : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private CanvasGroup m_chestsNavigationGroup = null;
	[SerializeField] private TextMeshProUGUI m_chestNavigationText = null;

	// Internal
	private int m_chestNavigationIdx = 0;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);

		// Initialize text
		if(m_chestNavigationText != null) {
			m_chestNavigationText.text = (m_chestNavigationIdx + 1).ToString() + "/" + ChestManager.NUM_DAILY_CHESTS.ToString();
		}

		// Interactable
		m_chestsNavigationGroup.interactable = InstanceManager.player != null;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType)
        {
            case BroadcastEventType.GAME_ENDED:
            {
                OnGameEnded();
            }break;
        }
    }



	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Move the player to a target chest.
	/// </summary>
	/// <param name="_chestOffset">Offset from current chest index.</param>
	private void GoToChest(int _chestOffset) {
		// Only during play mode!
		if(InstanceManager.player == null) {
			UIFeedbackText.CreateAndLaunch(
				"In-game only!",
				new Vector2(0.5f, 0.5f),
				this.GetComponentInParent<Canvas>().transform as RectTransform
			);
			return;
		}

		// Figure out chest index
		m_chestNavigationIdx += _chestOffset;
		if(m_chestNavigationIdx < 0) {
			m_chestNavigationIdx = ChestManager.NUM_DAILY_CHESTS - 1;
		} else if(m_chestNavigationIdx >= ChestManager.NUM_DAILY_CHESTS) {
			m_chestNavigationIdx = m_chestNavigationIdx % ChestManager.NUM_DAILY_CHESTS;
		}

		// Move player to position
		InstanceManager.player.transform.position = CollectiblesManager.chests[m_chestNavigationIdx].transform.position + (Vector3.left * 7f);

		// Update text
		if(m_chestNavigationText != null) {
			m_chestNavigationText.text = (m_chestNavigationIdx + 1).ToString() + "/" + ChestManager.NUM_DAILY_CHESTS.ToString();
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Game has started.
	/// </summary>
	private void OnGameStarted() {
		m_chestsNavigationGroup.interactable = true;
	}

	/// <summary>
	/// Game has ended.
	/// </summary>
	private void OnGameEnded() {
		m_chestsNavigationGroup.interactable = false;
	}

	/// <summary>
	/// Navigate to previous collectible chest.
	/// </summary>
	public void OnPrevChest() {
		GoToChest(-1);
	}

	/// <summary>
	/// Navigate to next collectible chest.
	/// </summary>
	public void OnNextChest() {
		GoToChest(1);
	}

	/// <summary>
	/// Simulates daily chest collection (no menu refresh for now, reload menu for that).
	/// </summary>
	public void OnAddDailyChest() {
		// Find the first non-collected chest
		Chest ch = null;
		foreach(Chest chest in ChestManager.dailyChests) {
			if(!chest.collected) {
				ch = chest;
				break;
			}
		}

		// Mark it as collected and process rewards
		if(ch != null) {
			ch.ChangeState(Chest.State.PENDING_REWARD);
			ChestManager.ProcessChests();
		}
	}

	/// <summary>
	/// Simulate daily chests timer expiration.
	/// </summary>
	public void OnResetDailyChests() {
		ChestManager.Reset();
	}
}