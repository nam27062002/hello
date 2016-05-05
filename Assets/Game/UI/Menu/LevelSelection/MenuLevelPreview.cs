// MenuLevelPreview.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Preview of a dragon in the main menu.
/// </summary>
[RequireComponent(typeof(PathFollower))]	// Required by the scroller
public class MenuLevelPreview : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; }}

	// Others
	private DefinitionNode m_def = null;
	public DefinitionNode def {
		get {
			if(m_def == null) m_def = DefinitionsManager.GetDefinition(DefinitionsCategory.LEVELS, sku);
			return m_def;
		}
	}

	// References
	private GameObject m_lockInfoObj = null;
	private GameObject m_debugInfoObj = null;
	private Localizer m_unlockInfoText = null;
	private PathFollower m_follower = null;
	public PathFollower follower {
		get {
			if(m_follower == null) {
				m_follower = GetComponent<PathFollower>();
			}
			return m_follower;
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_lockInfoObj = gameObject.FindObjectRecursive("LockInfo");
		Debug.Assert(m_lockInfoObj != null, "Required child missing!");

		m_unlockInfoText = m_lockInfoObj.FindComponentRecursive<Localizer>("WarningText");
		Debug.Assert(m_unlockInfoText != null, "Required child missing!");

		m_debugInfoObj = gameObject.FindObjectRecursive("DebugInfo");
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.AddListener(GameEvents.DEBUG_UNLOCK_LEVELS, RefreshLockInfo);

		// Make sure all info is updated
		RefreshLockInfo();
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.RemoveListener(GameEvents.DEBUG_UNLOCK_LEVELS, RefreshLockInfo);
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh the lock info corresponding to this level.
	/// </summary>
	private void RefreshLockInfo() {
		// If unkonwn level, don't do anything
		if(def == null) return;

		// Show lock info?
		bool locked = !LevelManager.IsLevelUnlocked(sku);
		m_lockInfoObj.SetActive(locked);

		// Update unlock condition text
		if(locked) {
			// Special case if "coming soon" level
			if(def.GetAsBool("comingSoon")) {
				m_unlockInfoText.Localize("TID_GEN_COMING_SOON");
			} else {
				int dragonsOwned = DragonManager.GetDragonsByLockState(DragonData.LockState.OWNED).Count;
				int remainingDragonsToUnlock = def.GetAsInt("dragonsToUnlock") - dragonsOwned;
				if(remainingDragonsToUnlock == 1) {
					m_unlockInfoText.Localize("TID_LEVEL_UNLOCK_REQUIREMENT_SINGULAR");
				} else {
					m_unlockInfoText.Localize("TID_LEVEL_UNLOCK_REQUIREMENT_PLURAL", StringUtils.FormatNumber(remainingDragonsToUnlock));
				}
			}
		} 

		if(m_debugInfoObj != null) {
			Text txt = m_debugInfoObj.FindComponentRecursive<Text>();
			txt.text = def.Get("tidName");
			m_debugInfoObj.SetActive(!locked);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(DragonData _data) {
		// Refresh lock info
		RefreshLockInfo();
	}
}
