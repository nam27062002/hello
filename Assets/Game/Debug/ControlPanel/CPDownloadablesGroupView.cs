// CPDownloadablesGroupView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class CPDownloadablesGroupView : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private GameObject m_bundleViewPrefab = null;
	[SerializeField] private Transform m_bundlesRoot = null;
	[Space]
	[SerializeField] private TMP_Text m_nameText = null;
	[SerializeField] private TMP_Text m_priorityText = null;
	[SerializeField] private Image m_background = null;
	[Space]
	[SerializeField] private Toggle m_permissionRequestedToggle = null;
	[SerializeField] private Toggle m_permissionGrantedToggle = null;

	// Public properties
	public Downloadables.CatalogGroup Group { get; private set; }

	// Internal
	private int m_priority = -1;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Make sure priority is updated
		Refresh(false);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this view with a given group data.
	/// </summary>
	/// <param name="_group">Group to be used.</param>
	public void InitWithGroup(Downloadables.CatalogGroup _group) {
		// Store data
		Group = _group;

		// Initialize bundle views
		if(Group.EntryIds != null && Group.EntryIds.Count > 0) {
			// Shortcut to catalog
			Dictionary<string, Downloadables.CatalogEntryStatus> catalog = AssetBundlesManager.Instance.GetDownloadablesCatalog();

			// Create a new view instance for each bundle in the group
			CPDownloadablesBundleView bundleView = null;
			GameObject newInstance = null;
			Downloadables.CatalogEntryStatus entryStatus = null;
			int count = Group.EntryIds.Count;
			for(int i = 0; i < count; ++i) {
				// Get entry data
				catalog.TryGetValue(Group.EntryIds[i], out entryStatus);
				if(entryStatus != null) {
					// Create new instance
					newInstance = Instantiate<GameObject>(m_bundleViewPrefab, m_bundlesRoot);
					newInstance.SetActive(true);

					// Initialize view
					bundleView = newInstance.GetComponent<CPDownloadablesBundleView>();
					bundleView.InitWithData(entryStatus);
				}
			}
		}

		// Disable prefab instance
		m_bundleViewPrefab.SetActive(false);

		// Perform a first refresh
		Refresh(true);
	}

	/// <summary>
	/// Update everything with latest data from the group.
	/// </summary>
	/// <param name="_forced">If set to <c>false</c>, visuals will only be changed if values are different from the displayed ones.</param>
	private void Refresh(bool _forced) {
		// Name
		m_nameText.text = Group.Id;

		// Priority
		if(_forced || m_priority != Group.Priority) {
			m_priority = Group.Priority;
			m_priorityText.text = m_priority.ToString();
		}

		// Permission toggles
		if(_forced || m_permissionRequestedToggle.isOn != Group.PermissionRequested) {
			m_permissionRequestedToggle.isOn = Group.PermissionRequested;
		}

		if(_forced || m_permissionGrantedToggle.isOn != Group.PermissionOverCarrierGranted) {
			m_permissionGrantedToggle.isOn = Group.PermissionOverCarrierGranted;
		}
	}

	/// <summary>
	/// Define the background color for this group.
	/// </summary>
	/// <param name="_color">Color.</param>
	public void SetBackgroundColor(Color _color) {
		m_background.color = _color;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The permission request toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnPermissionRequestToggle(bool _newValue) {
		Group.PermissionRequested = _newValue;
	}

	/// <summary>
	/// The permission granted toggle has been changed.
	/// </summary>
	/// <param name="_newValue">New value.</param>
	public void OnPermissionGrantedToggle(bool _newValue) {
		Group.PermissionOverCarrierGranted = _newValue;
	}
}