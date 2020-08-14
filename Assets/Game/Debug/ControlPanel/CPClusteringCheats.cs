// CPClusteringCheats.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/08/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Debug class to provide cheats to test the clustering feature.
/// </summary>
public class CPClusteringCheats : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Add here any new cluster
	public enum ClusterIdOptions {
		CLUSTER_NONE = 0,
		CLUSTER_GENERIC,
		CLUSTER_NON_PAYER,
		CLUSTER_2,
		CLUSTER_5,
		CLUSTER_10,
		COUNT
	};

	// Should match the skus in the clusterDefinitions content table
	private static readonly string[] CLUSTER_IDS = new string[] {
		"",
		ClusteringManager.CLUSTER_GENERIC,
		"CLUSTER_NON_PAYER",
		"CLUSTER_2",
		"CLUSTER_5",
		"CLUSTER_10"
	};

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private CanvasGroup m_root = null;
	[Space]
	[SerializeField] private Toggle m_syncedToggle = null;
	[Space]
	[SerializeField] private TMP_InputField m_clusterIdInput = null;
	[SerializeField] private TMP_Dropdown m_clusterIdDropdown = null;
	[SerializeField] private Button m_setCustomButton = null;

	// Internal
	private bool m_uiLocked = false;

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Init toggle view
		RefreshSyncedView();

		// Init dropdown options
		if(m_clusterIdDropdown != null) {
			List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
			for(int i = 0; i < (int)ClusterIdOptions.COUNT; ++i) {
				string label = CLUSTER_IDS[i];
				if(string.IsNullOrEmpty(label)) label = "-";	// Show something for empty
				options.Add(new TMP_Dropdown.OptionData(label));
			}
			m_clusterIdDropdown.AddOptions(options);
		}

		// Detect cluster ID changes
		if(m_clusterIdInput != null) m_clusterIdInput.onValueChanged.AddListener(OnClusterIdChangedInput);
		if(m_clusterIdDropdown != null) m_clusterIdDropdown.onValueChanged.AddListener(OnClusterIdChangedDropdown);

		// Init cluster ID view
		RefreshClusterIdView(true);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from events
		if(m_clusterIdInput != null) m_clusterIdInput.onValueChanged.RemoveListener(OnClusterIdChangedInput);
		if(m_clusterIdDropdown != null) m_clusterIdDropdown.onValueChanged.RemoveListener(OnClusterIdChangedDropdown);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Keep synced view updated
		RefreshSyncedView();
	}

	/// <summary>
	/// Refreshes the Cluster ID input field.
	/// </summary>
	/// <param name="_updateText">Set the text to the current Cluster Id?</param>
	public void RefreshClusterIdView(bool _updateText) {
		// Aux vars
		string currentClusterId = GetCurrentClusterId();
		bool isNewValue = false;

		// Support both input textfield and dropdown
		// A) Input textfield
		if(m_clusterIdInput != null) {
			// Set text?
			if(_updateText) {
				m_clusterIdInput.text = currentClusterId;
			}

			// Use different color if textfield value doesn't match current value
			isNewValue = m_clusterIdInput.text != currentClusterId;
			if(isNewValue) {
				m_clusterIdInput.textComponent.color = Colors.yellow;
			} else {
				m_clusterIdInput.textComponent.color = Color.white;
			}
		}

		// B) Dropdown
		if(m_clusterIdDropdown != null) {
			// Set text?
			if(_updateText) {
				// Find matching index
				for(int i = 0; i < CLUSTER_IDS.Length; ++i) {
					if(CLUSTER_IDS[i] == currentClusterId) {
						// Set it as the selected value
						m_clusterIdDropdown.value = i;
						break;
					}
				}
			}

			// Use different color if textfield value doesn't match current value
			isNewValue = CLUSTER_IDS[m_clusterIdDropdown.value] != currentClusterId;
			if(isNewValue) {
				m_clusterIdDropdown.captionText.color = Colors.orange;
			} else {
				m_clusterIdDropdown.captionText.color = Color.black;
			}
		}

		// Only enable Set button if ID is different
		m_setCustomButton.interactable = isNewValue;
	}

	/// <summary>
	/// Refreshes the Synced toggle.
	/// </summary>
	public void RefreshSyncedView() {
		// Just reflect current status
		m_syncedToggle.isOn = UsersManager.currentUser.clusterSynced;
	}

	/// <summary>
	/// Lock the UI (i.e. waiting for server response).
	/// </summary>
	/// <param name="_lock">Whether to lock or unlock the UI.</param>
	/// <returns>Whether the action was successfully applied or not.</returns>
	private bool LockUI(bool _lock) {
		// Locking the UI
		if(_lock) {
			// Is it already locked?
			if(m_uiLocked) {
				// Yes! Show feedback and do nothing
				ControlPanel.LaunchTextFeedback("Don't spam! Waiting for server response...", Color.red);
				return false;
			} else {
				// No! Lock it!
				m_uiLocked = true;
				m_root.interactable = false;
				m_root.alpha = 0.5f;
				return true;
			}
		}

		// Unlocking the UI
		else {
			// Is it already unlocked?
			if(!m_uiLocked) {
				// Yes! Nothing to do
				return false;
			} else {
				// No! Unlock it!
				m_uiLocked = false;
				m_root.interactable = true;
				m_root.alpha = 1f;
				return true;
			}
		}
	}

	/// <summary>
	/// Obtain the cluster Id assigned to the current user.
	/// </summary>
	/// <returns>The cluster Id assigned to the current user.</returns>
	private string GetCurrentClusterId() {
		return UsersManager.currentUser.GetClusterId(false);
	}

	/// <summary>
	/// Perform all the needed stuff to set a new cluster ID for the current user.
	/// </summary>
	/// <param name="_clusterId">The new cluster ID.</param>
	private void SetClusterId(string _clusterId) {
		// Store new values and launch a server sync
		UsersManager.currentUser.SetClusterId(_clusterId);
		UsersManager.currentUser.clusterSynced = false;
		ClusteringManager.Instance.SyncWithServer();

		// Refresh view
		RefreshClusterIdView(true);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The cluster id has changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnClusterIdChangedInput(string _newValue) {
		// Refresh visuals
		RefreshClusterIdView(false);
	}

	/// <summary>
	/// The cluster id has changed.
	/// </summary>
	/// <param name="_newValueIdx">Index of the new selected option.</param>
	public void OnClusterIdChangedDropdown(int _newValueIdx) {
		// Refresh visuals
		RefreshClusterIdView(false);
	}

	/// <summary>
	/// Reset cluster ID button.
	/// </summary>
	public void OnResetClusterId() {
		// Just do it!
		SetClusterId("");
	}

	/// <summary>
	/// Set custom generic cluster ID button.
	/// </summary>
	public void OnSetGenericClusterId() {
		// Just do it!
		SetClusterId(ClusteringManager.CLUSTER_GENERIC);
	}

	/// <summary>
	/// Set custom cluster ID button.
	/// </summary>
	public void OnSetCustomClusterId() {
		// Read from input field / dropdown and do it!
		if(m_clusterIdInput != null && m_clusterIdInput.isActiveAndEnabled) {
			SetClusterId(m_clusterIdInput.text);
		}

		else if(m_clusterIdDropdown != null && m_clusterIdDropdown.isActiveAndEnabled) {
			SetClusterId(CLUSTER_IDS[m_clusterIdDropdown.value]);
		}
	}
}