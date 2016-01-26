// MenuDragonPreview.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Preview of a dragon in the main menu.
/// </summary>
public class MenuDragonPreview : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] [SkuList(typeof(DragonDef))] private string m_sku;
	public string sku { get { return m_sku; }}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// Enabled.
	/// </summary>
	private void OnEnable() {
		// Make sure dragon has the right scale according to its level
		OnDragonLevelUp(DragonManager.GetDragonData(sku));

		// Subscribe to external events
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnDragonLevelUp);
	}

	/// <summary>
	/// Disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_LEVEL_UP, OnDragonLevelUp);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A dragon has leveled up.
	/// </summary>
	public void OnDragonLevelUp(DragonData _data) {
		// We only care if it's ourselves
		if(_data.def.sku == m_sku) {
			// Make sure dragon has the right scale according to its level
			transform.localScale = Vector3.one * _data.scale;
		}
	}
}

