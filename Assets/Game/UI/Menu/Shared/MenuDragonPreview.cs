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
	public enum Anim {
		IDLE,
		UNLOCKED,
		RESULTS_IN,
		POSE_FLY,
		FLY,

		COUNT
	};

	public static readonly string[] ANIM_TRIGGERS  = {
		"idle",
		"unlocked",
		"results_in",
		"pose_fly",
		"fly"
	};

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; }}

	// Components
	private DragonEquip m_equip = null;
	public DragonEquip equip {
		get {
			if(m_equip == null) {
				m_equip = this.GetComponent<DragonEquip>();
			}
			return m_equip;
		}
	}

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
	/// Apply the given animation to the dragon's animator.
	/// </summary>
	/// <param name="_anim">The animation to be launched.</param>
	public void SetAnim(Anim _anim) {
		if(m_animator != null) {
			m_animator.SetTrigger(ANIM_TRIGGERS[(int)_anim]);
		}
	}

	public void SetFresnelColor( Color col )
	{
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for( int i = 0; i<renderers.Length; i++ )
		{
			Material[] mats = renderers[i].materials;
			for( int j = 0;j<mats.Length; j++ )
			{
				string shaderName = mats[j].shader.name;
				if ( shaderName.Contains("Dragon/Wings") || shaderName.Contains("Dragon/Body") )
				{
					mats[j].SetColor("_FresnelColor", col);
				}
			}
		}
	}
}

