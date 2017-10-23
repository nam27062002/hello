// ResultsSceneSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 22/09/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// PREPROCESSOR																  //
//----------------------------------------------------------------------------//

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Setup to define a 3D area in the level to use for the results screen.
/// </summary>
[ExecuteInEditMode]
public class ResultsSceneSetup : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references, all required
	[SerializeField] private Camera m_camera = null;
	public Camera camera {
		get { return m_camera; }
	}

	[Comment("DragonLoader should be set to \"CURRENT\" mode", 10)]
	[SerializeField] private MenuDragonLoader m_dragonSlot = null;
	[SerializeField] private Transform m_dragonSlotViewPosition = null;
	[SerializeField] private ResultsSceneEggSlot m_eggSlot = null;
	public ResultsSceneEggSlot eggSlot {
		get { return m_eggSlot; }
	}

	[SerializeField] private ParticleSystem m_confettiFX = null;

	[Comment("Sort chest slots from left to right, chests will be spawned from the center depending on how many were collected.\nAlways 5 slots, please.", 10)]
	[SerializeField] private ResultsSceneChestSlot[] m_chestSlots = new ResultsSceneChestSlot[5];
	public ResultsSceneChestSlot[] chestSlots {
		get { return m_chestSlots; }
	}

	[Comment("Fog Settings used", 10)]
	[SerializeField] FogManager.FogAttributes m_fog;

	// Test To recolocate the dragons view!
	[Comment("Only to test the editor")]
	public bool recolocate = false; //"run" or "generate" for example
	void Update()
	{
		if ( !Application.isPlaying )
		{
			if (recolocate)
			{
				m_dragonSlot.SetViewPosition( m_dragonSlotViewPosition.position );
				m_dragonSlot.dragonInstance.transform.rotation = m_dragonSlot.transform.rotation;
				if ( m_dragonSlot.dragonSku == "dragon_chinese" || m_dragonSlot.dragonSku == "dragon_reptile" || m_dragonSlot.dragonSku == "dragon_balrog")
				{
					m_dragonSlot.dragonInstance.transform.Rotate(Vector3.up * -45);
				}
			}

			if ( m_fog.texture == null )
			{
				m_fog.CreateTexture();
				Shader.SetGlobalTexture("_FogTexture", m_fog.texture);
			}
			m_fog.RefreshTexture();
			Shader.SetGlobalFloat("_FogStart", m_fog.m_fogStart);
			Shader.SetGlobalFloat("_FogEnd", m_fog.m_fogEnd);
		}
	}


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		if (Application.isPlaying )
		{
			// Hide dragon slot
			m_dragonSlot.gameObject.SetActive(false);

			if ( InstanceManager.fogManager != null )
			{
				InstanceManager.fogManager.ForceAttributes( m_fog );
				InstanceManager.fogManager.Update();
			}
			else
			{
				if (m_fog.texture == null)
				{
					m_fog.CreateTexture();
					m_fog.RefreshTexture();
				}
				m_fog.FogSetup();
			}
		}
	}

	/// <summary>
	/// A change has occurred on the inspector. Validate its values.
	/// </summary>
	private void OnValidate() {
		// There must be exactly 5 chest slots
		if(m_chestSlots.Length != 5) {
			// Create a new array with exactly 5 slots and copy as many values as we can
			ResultsSceneChestSlot[] chestSlots = new ResultsSceneChestSlot[5];
			for(int i = 0; i < m_chestSlots.Length && i < chestSlots.Length; i++) {
				chestSlots[i] = m_chestSlots[i];
			}
			m_chestSlots = chestSlots;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Launches the dragon intro animation.
	/// </summary>
	public void LaunchDragonAnim() {
		// Launch gold mountain animation

		// Show and trigger dragon animation
		m_dragonSlot.gameObject.SetActive(true);
		m_dragonSlot.dragonInstance.SetAnim(MenuDragonPreview.Anim.RESULTS_IN);
		m_dragonSlot.SetViewPosition( m_dragonSlotViewPosition.position );
		m_dragonSlot.dragonInstance.transform.rotation = m_dragonSlot.transform.rotation;
		if ( m_dragonSlot.dragonSku == "dragon_chinese" || m_dragonSlot.dragonSku == "dragon_reptile" || m_dragonSlot.dragonSku == "dragon_balrog")
		{
			m_dragonSlot.dragonInstance.transform.Rotate(Vector3.up * -45);
		}

		// Trigger confetti anim
		LaunchConfettiFX();
	}

	/// <summary>
	/// Launches the disguise purchased FX on the selected dragon.
	/// </summary>
	public void LaunchConfettiFX() {
		// Restart effect
		m_confettiFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		m_confettiFX.Play(true);

		// Restart SFX
		string audioId = "hd_unlock_dragon";
		AudioController.Stop(audioId);
		AudioController.Play(audioId);
	}
}