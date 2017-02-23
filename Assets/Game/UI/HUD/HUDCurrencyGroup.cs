// HUDCurrencyGroup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple script to prevent conflicts between multi-layered currency counters (i.e. hud -> shop popup).
/// </summary>
public class HUDCurrencyGroup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private ShowHideAnimator m_animator = null;
	private int m_stackCount = 0;
	private bool m_wasVisible = true;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Gather external references
		m_animator = GetComponent<ShowHideAnimator>();

		// Subscribe to external events
		Messenger.AddListener<bool>(GameEvents.UI_TOGGLE_CURRENCY_COUNTERS, OnToggleCurrencyCounters);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<bool>(GameEvents.UI_TOGGLE_CURRENCY_COUNTERS, OnToggleCurrencyCounters);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The toggle event has been dispatched.
	/// </summary>
	/// <param name="_show">Whether currency counters should be toggled or not.</param>
	private void OnToggleCurrencyCounters(bool _show) {
		// Toggle?
		if(_show) {
			// Decrease stacks
			m_stackCount = Mathf.Max(m_stackCount - 1, 0);	// Never lower than 0!

			// If it's the last stack, restore original state
			if(m_stackCount <= 0) {
				if(m_animator != null) {
					m_animator.Set(m_wasVisible);
				} else {
					this.gameObject.SetActive(m_wasVisible);
				}
			}
		} else {
			// If it's the first stack, store original state
			if(m_stackCount <= 0) {
				if(m_animator != null) {
					m_wasVisible = m_animator.visible;
				} else {
					m_wasVisible = this.gameObject.activeSelf;
				}
			}

			// Hide
			if(m_animator != null) {
				m_animator.Hide();
			} else {
				this.gameObject.SetActive(false);
			}

			// Increase stacks
			m_stackCount++;
		}
	}
}