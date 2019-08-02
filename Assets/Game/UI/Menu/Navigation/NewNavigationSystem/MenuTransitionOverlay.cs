// MenuTransitionOverlay.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 31/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
public class MenuTransitionOverlay : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Animator m_anim = null;

	[Tooltip("Adjust to the total duration of the animation")]
	[SerializeField] private float m_transitionDuration = 0.5f;
	public float transitionDuration {
		get { return m_transitionDuration; }
	}
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Start hidden
		Stop();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launch the animation.
	/// </summary>
	public void Play() {
		// Make sure we are on top
		this.transform.SetAsLastSibling();

		// Trigger animation
		m_anim.SetTrigger("play");
	}

	/// <summary>
	/// Interrupt the animation and disable the game object.
	/// </summary>
	public void Stop() {
		m_anim.SetTrigger("stop");
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}