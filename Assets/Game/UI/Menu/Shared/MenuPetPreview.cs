// MenuPetPreview.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Preview of a pet in the main menu.
/// </summary>
public class MenuPetPreview : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Anim {
		IDLE,
		IN,
		OUT,

		COUNT
	};

	public static readonly string[] ANIM_TRIGGERS  = {
		"idle",
		"in",
		"out"
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; }}

	// Internal
	private Animator m_animator = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_animator = GetComponentInChildren<Animator>();
	}

	/// <summary>
	/// Apply the given animation to the pet's animator.
	/// </summary>
	/// <param name="_anim">The animation to be launched.</param>
	public void SetAnim(Anim _anim) {
		if(m_animator != null) {
			m_animator.SetTrigger(ANIM_TRIGGERS[(int)_anim]);
		}
	}
}

