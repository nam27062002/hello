// UISafeAreaDebug.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 29/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class UISafeAreaDebug : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private UISafeAreaSetter[] m_areasToDebug = new UISafeAreaSetter[0];
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		Messenger.AddListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, OnCPBoolChanged);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		Refresh();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Messenger.RemoveListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, OnCPBoolChanged);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Refresh items.
	/// </summary>
	private void Refresh() {
		// Show? Never if it's not a debug build!
		bool show = DebugSettings.showSafeArea && UnityEngine.Debug.isDebugBuild;
		this.gameObject.SetActive(show);

		// Refresh bounds
		if(show) {
			for(int i = 0; i < m_areasToDebug.Length; ++i) {
				m_areasToDebug[i].Apply();
			}
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A Control Panel bool value has been changed.
	/// </summary>
	/// <param name="_id">Property ID.</param>
	/// <param name="_value">New value.</param>
	private void OnCPBoolChanged(string _id, bool _value) {
		if(_id == DebugSettings.SHOW_SAFE_AREA) {
			Refresh();
		}
	}
}