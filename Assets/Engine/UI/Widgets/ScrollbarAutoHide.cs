// ScrollbarAutoHide.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to automatically hide a scrollbar after inactivity.
/// </summary>
[RequireComponent(typeof(Scrollbar))]
[RequireComponent(typeof(ShowHideAnimator))]
public class ScrollbarAutoHide : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed setup
	[SerializeField] private float m_inactivitySeconds = 1f;

	// Internal references
	private Scrollbar m_scrollBar = null;
	private ShowHideAnimator m_anim = null;

	// Internal logic
	private float m_timer = 0f;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_scrollBar = GetComponent<Scrollbar>();
		m_anim = GetComponent<ShowHideAnimator>();
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
		// Start hidden
		m_anim.ForceHide(false, false);

		// Listen to scrollbar activity
		if(m_scrollBar != null) m_scrollBar.onValueChanged.AddListener(OnScrollbarChanged);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		if(m_scrollBar != null) m_scrollBar.onValueChanged.RemoveListener(OnScrollbarChanged);
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Check inactivity timer
		if(m_timer > 0f) {
			m_timer -= Time.unscaledDeltaTime;
			if(m_timer <= 0f) {
				// Hide bar
				m_anim.Hide(true, false);
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

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The scroll position of the target scroll rect has changed.
	/// </summary>
	/// <param name="_newPos">New position.</param>
	private void OnScrollbarChanged(float _newPos) {
		// Make sure bar is visible
		m_anim.Show();

		// Reset timer
		m_timer = m_inactivitySeconds;
	}
}