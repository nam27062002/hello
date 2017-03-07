// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
//[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private GameObject m_spinner = null;
	[SerializeField] private UI3DScaler m_container = null;

	[FileListAttribute("Resources/UI/Menu/Dragons", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
	[SerializeField] private string[] m_resourcesList = new string[0];

	private ResourceRequest m_loadingRequest = null;
	private GameObject m_instance = null;
	private int m_resIdx = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Hide spinner
		if(m_spinner) m_spinner.SetActive(false);

		// Load first dragon
		OnTestButton();
	}

	/// <summary>
	/// Something changed on the inspector.
	/// </summary>
	private void OnValidate() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void Update() {
		// If we're loading, wait for the loading process to end
		if(m_loadingRequest != null) {
			if(m_loadingRequest.isDone) {
				// Done! Instantiate loaded asset
				m_instance = GameObject.Instantiate<GameObject>((GameObject)m_loadingRequest.asset, m_container.transform, false);
				if(m_instance != null) {
					m_instance.SetLayerRecursively(m_container.gameObject.layer);
					m_container.Refresh(true, true);
				}

				// Hide spinner
				m_spinner.SetActive(false);

				// Drop loading request
				m_loadingRequest = null;
			}
		}
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		// Ignore if not playing or not enabled
		if(!Application.isPlaying || !isActiveAndEnabled) {
			Debug.LogError("Only while playing!"); return;
		}

		// If we have something loaded, destroy it
		if(m_instance != null) {
			GameObject.Destroy(m_instance);
			m_instance = null;
		}

		// Ignore if resources list is empty
		if(m_resourcesList.Length <= 0) return;

		// We don't care if we're already loading another asset, it will be ignored once done loading
		string path = m_resourcesList[m_resIdx];
		m_resIdx = (m_resIdx + 1) % m_resourcesList.Length;
		m_loadingRequest = Resources.LoadAsync<GameObject>(path);

		// Show spinner
		if(m_loadingRequest != null && m_spinner != null) {
			m_spinner.SetActive(true);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {

	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//

}