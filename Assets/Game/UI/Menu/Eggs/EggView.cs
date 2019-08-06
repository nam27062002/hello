// EggView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main control of a single egg prefab in the menu.
/// </summary>
public class EggView : MonoBehaviour, IBroadcastListener {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//

    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
    [SerializeField] private bool m_showIdleFX = false;

    // Exposed members
    [SerializeField] private Transform m_anchorFX = null;
    public Transform anchorFX {
        get { return m_anchorFX; }
    }

    [SerializeField] private GameObject m_idleFX = null;
    public GameObject idleFX {
        get { return m_idleFX; }
    }

    [Space]
    [SerializeField] private GameObject m_incubatingFX = null;
    [SerializeField] private Range m_incubatingFXIntensityRange = new Range(0f, 1f);

    // Data - can be null
    private Egg m_eggData = null;
    public Egg eggData {
        get { return m_eggData; }
        set { m_eggData = value; }
    }

    // Egg behaviours
    private OpenEggBehaviour m_openBehaviour = null;
    public OpenEggBehaviour openBehaviour {
        get { return m_openBehaviour; }
    }

    private ReadyEggBehaviour m_readyBehaviour = null;
    public ReadyEggBehaviour readyBehaviour {
        get { return m_readyBehaviour; }
    }

    // Setup
    private bool m_forceIdleFX = false;
    public bool forceIdleFX {
        get { return m_forceIdleFX; }
        set { m_forceIdleFX = value; Refresh(); }
    }

    // Internal references
    private Animator m_animator = null;
    private List<Material> m_incubatingFXMaterials = null;

    //------------------------------------------------------------------------//
    // FACTORY METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Create an egg view by its sku.
    /// If the application is running, a new Egg data will be created as well and initialized to SHOWROOM state.
    /// </summary>
    /// <returns>The new egg view. <c>null</c> if the egg view couldn't be created.</returns>
    /// <param name="_eggSku">The sku of the egg in the EGGS definitions category.</param>
    public static EggView CreateFromSku(string _eggSku) {
        // Egg can't be created if definitions are not loaded
        Debug.Assert(ContentManager.ready, "Definitions not yet loaded!");

        // Find and validate definition
        DefinitionNode eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, _eggSku);

        // Use internal method
        EggView newEggView = CreateFromDef(eggDef);

        // If the application is running, create a new egg data and assign it to the new view
        if (newEggView != null && Application.isPlaying) {
            Egg newEgg = Egg.CreateFromDef(eggDef);
            newEgg.ChangeState(Egg.State.SHOWROOM);
            newEggView.eggData = newEgg;
        }

        // Return the newly created instance
        return newEggView;
    }

    /// <summary>
    /// Create an egg view for the given egg data.
    /// </summary>
    /// <returns>The new egg view. <c>null</c> if the egg view couldn't be created.</returns>
    /// <param name="_eggData">The egg data to be used to initialize this egg.</param>
    public static EggView CreateFromData(Egg _eggData) {
        // Ignore if data not valid
        if (_eggData == null) return null;

        // Use internal method
        EggView newEggView = CreateFromDef(_eggData.def);

        // Assign the given egg data to the newly created egg view
        if (newEggView != null) {
            newEggView.eggData = _eggData;
        }

        // Return the newly created instance
        return newEggView;
    }

    /// <summary>
    /// Create an egg view given an egg definition. Internal usage only.
    /// </summary>
    /// <returns>The new egg view. <c>null</c> if the egg view couldn't be created.</returns>
    /// <param name="_def">The egg definition.</param>
    private static EggView CreateFromDef(DefinitionNode _def) {
        // Def must be valid!
        if (_def == null) return null;

        // Create new egg view from the definition
        // Load the prefab for this egg as defined in the definition
        GameObject prefabObj = Resources.Load<GameObject>(Egg.PREFAB_PATH + _def.GetAsString("prefabPath"));
        Debug.Assert(prefabObj != null, "The prefab defined to egg " + _def.sku + " couldn't be found");

        // Create a new instance and obtain the egg view component
        GameObject newInstance = GameObject.Instantiate<GameObject>(prefabObj);
        EggView newEggView = newInstance.GetComponent<EggView>();

        // Return the newly created instance
        return newEggView;
    }

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        // Get external references
        m_openBehaviour = GetComponent<OpenEggBehaviour>();
        m_readyBehaviour = GetComponent<ReadyEggBehaviour>();
        m_animator = GetComponentInChildren<Animator>();

        // Subscribe to external events
        Broadcaster.AddListener(BroadcastEventType.EGG_STATE_CHANGED, this);
    }

    /// <summary>
    /// First update.
    /// </summary>
    private void Start() {
        // Make sure we're updated
        Refresh();
    }

    /// <summary>
    /// Component has been enabled.
    /// </summary>
    private void OnEnable() {
        // Make sure we're updated
        Refresh();
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    private void OnDestroy() {
        // Unsubscribe to external events
        Broadcaster.RemoveListener(BroadcastEventType.EGG_STATE_CHANGED, this);
    }


    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.EGG_STATE_CHANGED: {
                    EggStateChanged info = (EggStateChanged)broadcastEventInfo;
                    OnEggStateChanged(info.egg, info.from, info.to);
                }
                break;
        }
    }


    /// <summary>
    /// Called every frame.
    /// </summary>
    private void Update() {
        // If the egg is incubating, setup Idle VFX intensity
        if (m_incubatingFX != null && m_eggData != null && m_eggData.state == Egg.State.INCUBATING) {
            float intensity = m_incubatingFXIntensityRange.Lerp(m_eggData.incubationProgress);
            SetIncubatingVFXIntensity(intensity);
        }
    }

    //------------------------------------------------------------------//
    // INTERNAL METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Refresh this object based on egg's current state.
    /// </summary>
    private void Refresh() {
        // Aux vars
        int step = 0;

        // If we don't have valid data, simulate SHOWROOM state
        Egg.State state = Egg.State.SHOWROOM;
        if (m_eggData != null) {
            state = m_eggData.state;
        }

        // Enable/disable behaviours based on current egg's state
        if (m_openBehaviour != null) m_openBehaviour.enabled = (state == Egg.State.OPENING);
        if (m_readyBehaviour != null) m_readyBehaviour.enabled = (state == Egg.State.READY);

        // Set animator's parameters
        if (m_animator != null) {
            // Egg state
            m_animator.SetInteger(GameConstants.Animator.EGG_STATE, (int)state);

            // Collect steps
            if (m_openBehaviour != null) {
                step = Mathf.Clamp(m_openBehaviour.tapCount, 0, OpenEggBehaviour.TAPS_TO_OPEN);
                m_animator.SetInteger(GameConstants.Animator.COLLECT_STEP, step);
            }

            // Rarity
            if (m_eggData != null && m_eggData.rewardData != null) {
                m_animator.SetInteger(GameConstants.Animator.RARITY, (int)m_eggData.rewardData.rarity);
            } else {
                m_animator.SetInteger(GameConstants.Animator.RARITY, (int)Metagame.Reward.Rarity.COMMON);
            }

            // Animation intensity - reset to default if state is different than collected
            if (state != Egg.State.COLLECTED) {
                m_animator.SetFloat(GameConstants.Animator.INTENSITY, 1f);
            }
        }

        // Stuff depending on egg state
        bool showIdleFX = false;
        bool showIncubatingFX = false;
        float incubatingFXIntensity = m_incubatingFXIntensityRange.max;
        switch (state) {
            case Egg.State.INCUBATING: {
                    showIdleFX = false;
                    showIncubatingFX = true;
                    incubatingFXIntensity = m_incubatingFXIntensityRange.min;
                }
                break;

            case Egg.State.READY: {
                    showIdleFX = true;
                    showIncubatingFX = true;
                }
                break;

            case Egg.State.OPENING: {
                    if (step <= 0) {    // Show only while no tap has been done
                        showIdleFX = true;
                        showIncubatingFX = true;
                    }
                }
                break;

            case Egg.State.SHOWROOM: {
                    // Only for premium eggs (or if forced)
                    if (m_forceIdleFX || m_showIdleFX) {
                        showIdleFX = true;
                    }
                }
                break;

            default: {
                    // Hide for the rest of cases
                    showIdleFX = false;
                    showIncubatingFX = false;
                }
                break;
        }


        if (m_idleFX != null) {
            m_idleFX.SetActive(showIdleFX);
        }

        if (m_incubatingFX != null) {
            m_incubatingFX.SetActive(showIncubatingFX);
            if (showIncubatingFX) SetIncubatingVFXIntensity(incubatingFXIntensity);
        }
    }

    /// <summary>
    /// Sets the intensity of the incubating VFX.
    /// </summary>
    /// <param name="_intensity">The intensity to be applied</param>
    private void SetIncubatingVFXIntensity(float _intensity) {
        // Skip if we have no FX assigned
        if (m_incubatingFX == null) return;

        // Do we have the material references?
        if (m_incubatingFXMaterials == null) {
            m_incubatingFXMaterials = new List<Material>();
            Renderer[] renderers = m_incubatingFX.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; ++i) {
                for (int j = 0; j < renderers[i].materials.Length; ++j) {
                    m_incubatingFXMaterials.Add(renderers[i].materials[j]);
                }
            }
        }

        // Set intensity in all registered materials
        for (int i = 0; i < m_incubatingFXMaterials.Count; ++i) {
            m_incubatingFXMaterials[i].SetFloat(GameConstants.Materials.Property.OPACITY_SATURATION, _intensity);
        }
    }

    //------------------------------------------------------------------//
    // CALLBACKS														//
    //------------------------------------------------------------------//
    /// <summary>
    /// An egg's state has changed.
    /// </summary>
    /// <param name="_egg">The egg whose state has changed.</param>
    /// <param name="_from">Previous state.</param>
    /// <param name="_to">New state.</param>
    private void OnEggStateChanged(Egg _egg, Egg.State _from, Egg.State _to) {
        // If it's this egg, refresh
        if (_egg == m_eggData) {
            // Update animator parameters
            Refresh();

            // If going to "collected", launch extra FX
            if (_to == Egg.State.COLLECTED) {
                // Increase intensity over time
                // Super-easy to do with DOTween library!
                DOVirtual.Float(1f, UIConstants.openEggSpinIntensity, UIConstants.openEggExplosionDuration,
                    (float _value) => { m_animator.SetFloat(GameConstants.Animator.INTENSITY, _value); }
                )
                .SetEase(UIConstants.openEggSpinEase)
                .SetDelay(0f)
                .OnComplete(
                        () => { m_animator.SetFloat(GameConstants.Animator.INTENSITY, 1f); }
                );
            }
        }
    }

    /// <summary>
    /// The egg has been tapped.
    /// To be called by child behaviours.
    /// </summary>
    /// <param name="_tapCount">Total tap count (including this one).</param>
    public void OnTap(int _tapCount) {
        // Update animator parameters
        Refresh();
    }
}

