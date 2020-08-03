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
    new public Camera camera {
        get { return m_camera; }
    }

    [Comment("DragonLoader should be set to \"CURRENT\" mode", 10)]
    [SerializeField] private MenuDragonLoader m_dragonSlot = null;
    [SerializeField] private Transform m_dragonSlotViewPosition = null;
    public MenuDragonLoader dragonSlot {
        get { return m_dragonSlot; }
    }

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

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        if (Application.isPlaying) {
            // Hide dragon slot
            m_dragonSlot.gameObject.SetActive(false);

            if (InstanceManager.fogManager != null) {
                InstanceManager.fogManager.ForceAttributes(m_fog);
                InstanceManager.fogManager.Update();
            } else {
                if (m_fog.texture == null) {
                    m_fog.CreateTexture();
                    m_fog.RefreshTexture();
                }
                m_fog.FogSetup();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void Update() {
        if (!Application.isPlaying) {
            if (recolocate) {
                m_dragonSlot.SetViewPosition(m_dragonSlotViewPosition.position);
                m_dragonSlot.dragonInstance.transform.rotation = m_dragonSlot.transform.rotation;
                m_dragonSlot.dragonInstance.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }

            if (m_fog.texture == null) {
                m_fog.CreateTexture();
                Shader.SetGlobalTexture(GameConstants.Materials.Property.FOG_TEXTURE, m_fog.texture);
            }
            m_fog.RefreshTexture();
            Shader.SetGlobalFloat(GameConstants.Materials.Property.FOG_START, m_fog.m_fogStart);
            Shader.SetGlobalFloat(GameConstants.Materials.Property.FOG_END, m_fog.m_fogEnd);
        }
    }

    /// <summary>
    /// A change has occurred on the inspector. Validate its values.
    /// </summary>
    private void OnValidate() {
        // There must be exactly 5 chest slots
        if (m_chestSlots.Length != 5) {
            // Create a new array with exactly 5 slots and copy as many values as we can
            ResultsSceneChestSlot[] chestSlots = new ResultsSceneChestSlot[5];
            for (int i = 0; i < m_chestSlots.Length && i < chestSlots.Length; i++) {
                chestSlots[i] = m_chestSlots[i];
            }
            m_chestSlots = chestSlots;
        }
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Prepare the scene for a results sequence.
    /// </summary>
    public void Init() {
        // Toggle chests based on game mode
        for (int i = 0; i < m_chestSlots.Length; ++i) {
            m_chestSlots[i].gameObject.SetActive(SceneController.mode == SceneController.Mode.DEFAULT);
        }

        // Toggle egg based on game mode
        m_eggSlot.gameObject.SetActive(SceneController.mode == SceneController.Mode.DEFAULT);
    }

    /// <summary>
    /// Launches the dragon intro animation.
    /// </summary>
    public void LaunchDragonAnim() {
        // Launch gold mountain animation

        // Show and trigger dragon animation
        m_dragonSlot.gameObject.SetActive(true);
        IDragonData dragonData = null;
        if (SceneController.mode == SceneController.Mode.TOURNAMENT) {
            dragonData = HDLiveDataManager.tournament.tournamentData.tournamentDef.dragonData;
        } else {
            dragonData = DragonManager.CurrentDragon;
        }
        m_dragonSlot.LoadDragon(dragonData.sku, dragonData.disguise);
        m_dragonSlot.dragonInstance.SetAnim(MenuDragonPreview.Anim.RESULTS);
        m_dragonSlot.dragonInstance.DisableMovesOnResults();
        m_dragonSlot.dragonInstance.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        m_dragonSlot.SetViewPosition(m_dragonSlotViewPosition.position);
        m_dragonSlot.dragonInstance.transform.rotation = m_dragonSlot.transform.rotation;

        // Trigger confetti anim
        // Avoid colliding with music
        LaunchConfettiFX(!GameSettings.Get(GameSettings.MUSIC_ENABLED));
    }

    /// <summary>
    /// Launches the disguise purchased FX on the selected dragon.
    /// </summary>
    /// <param name="_playSFX">Whether to trigger sound FX or not.</param>
    public void LaunchConfettiFX(bool _playSFX) {
        // Restart effect
        m_confettiFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        m_confettiFX.Play(true);

        // Restart SFX
        if (_playSFX) {
            string audioId = "hd_unlock_dragon";
            AudioController.Stop(audioId);
            AudioController.Play(audioId);
        }
    }
}