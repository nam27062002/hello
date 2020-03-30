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

	[SerializeField] private float m_pauseDuration = 1f;
	public float pauseDuration
	{
		get { return m_pauseDuration; }
        set { m_pauseDuration = value; }
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

		if (m_pauseDuration == 0)
		{
			m_anim.SetTrigger("continue");
		}
		else
		{
			// Make a pause after the clouds enter (fade in)
			UbiBCN.CoroutineManager.DelayedCall(() =>
		       {
		       // After the pause continue with the second part of the animation (fade out)
		       m_anim.SetTrigger("continue");
		       },
			m_transitionDuration + m_pauseDuration);
		}
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