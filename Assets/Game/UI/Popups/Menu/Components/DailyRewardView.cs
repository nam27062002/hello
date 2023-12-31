// DailyRewardView.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 12/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// View controller for a single daily reward.
/// </summary>
public class DailyRewardView : MetagameRewardView
{
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//
    public enum State
    {
        IDLE = 0,
        COOLDOWN,
        CURRENT,
        COLLECTED
    }

    private DailyRewardViewSettings m_settings = null;
    protected virtual DailyRewardViewSettings settings
    {
        get
        {
            if (m_settings == null)
            {
                m_settings = Resources.Load<DailyRewardViewSettings>(DailyRewardViewSettings.PATH);
            }
            return m_settings;
        }
    }

    // Auxiliar struct
    [System.Serializable]
    public class OffsetPerType
    {
        [RewardTypeList]
        public string type = "";
        public Vector2 offset = GameConstants.Vector2.zero;
        public float scale = 1f;

        public void Apply(RectTransform _rt)
        {
            _rt.anchoredPosition = offset;
            _rt.SetLocalScale(scale);
        }
    }

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    // Exposed members
    [Separator("DailyRewardView Custom Fields")]
    [Comment("Mandatory:")]
    [SerializeField] private Animator m_stateAnimator = null;
    [SerializeField] private UIGradient m_backgroundGradient = null;
    [Space]
    [Comment("Optional depending on reward type / index:")]
    [SerializeField] private Localizer m_dayText = null;
    [SerializeField] private MenuDragonLoader m_dragonLoader = null;
    [SerializeField] private MenuPetLoader m_petLoader = null;
    [SerializeField] private MenuEggLoader m_eggLoader = null;
    [Space]
    [SerializeField] private UITooltipTrigger m_tooltipTrigger = null;
    [SerializeField] private DragonInfoTooltip m_dragonTooltip = null;
    [SerializeField] private PowerTooltip_Generic m_powerTooltip = null;
    [Space]
    [Comment("VFX (optional)")]
    [SerializeField] private PrefabLoader m_currentGlowFX = null;
    [SerializeField] private PrefabLoader m_collectFX = null;
    [Space]
    [Comment("Extra Setup")]
    [SerializeField] private OffsetPerType[] m_iconOffsets = new OffsetPerType[0];

    // Data
    private DailyReward m_dailyReward = null;
    public DailyReward dailyReward
    {
        get { return m_dailyReward; }
    }

    private int m_rewardIdx = -1;
    public int rewardIdx
    {
        get { return m_rewardIdx; }
    }

    private State m_state = State.IDLE;
    public State state
    {
        get { return m_state; }
        set { m_state = value; Refresh(false); }
    }

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    public void Awake()
    {
        // Subscribe to external events
        if (m_dragonLoader != null)
        {
            m_dragonLoader.onDragonLoaded += OnDragonLoaded;
        }
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    public void OnDestroy()
    {
        // Unsubscribe from external events
        if (m_tooltipTrigger != null)
        {
            m_tooltipTrigger.OnTooltipOpen.RemoveListener(OnTooltipOpen);
        }

        if (m_dragonLoader != null)
        {
            m_dragonLoader.onDragonLoaded -= OnDragonLoaded;
        }
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialize this view with the given data.
    /// </summary>
    /// <param name="_reward">Reward object.</param>
    /// <param name="_rewardIdx">Reward index within the sequence.</param>
    /// <param name="_state">Which state should the view be on?</param>
    public void InitFromData(DailyReward _reward, int _rewardIdx, State _state)
    {
        // Store data
        m_dailyReward = _reward;
        m_rewardIdx = _rewardIdx;
        m_state = _state;

        // Init metagame reward
        base.InitFromReward(_reward.reward);

        // Perform a first refresh!
        Refresh(true);
    }

    /// <summary>
    /// Force a refresh of all the visuals.
    /// </summary>
    public override void Refresh()
    {
        // Created to hide parent basically
        Refresh(true);
    }

    /// <summary>
    /// Force a refresh of all the visuals.
    /// </summary>
    /// <param name="_reloadPreview">Whether to reload the preview or not.</param>
    public void Refresh(bool _reloadPreview)
    {
        // Let parent do its thing
        base.Refresh();

        // Day Text
        if (m_dayText != null)
        {
            m_dayText.Localize(m_dayText.tid, StringUtils.FormatNumber(m_rewardIdx + 1));
        }

        // Depending on reward type, toggle different objects on/off
        bool isDragon = false;
        bool isPet = false;
        bool isEgg = false;
        if (m_reward != null)
        {
            isDragon = m_reward.type == Metagame.RewardDragon.TYPE_CODE;
            isPet = m_reward.type == Metagame.RewardPet.TYPE_CODE;
            isEgg = m_reward.type == Metagame.RewardEgg.TYPE_CODE;
        }

        // 2D Preview: Disable if a 3D preview is available
        if (m_icon != null)
        {
            if ((isDragon && m_dragonLoader != null)
            || (isPet && m_petLoader != null)
            || (isEgg && m_eggLoader != null))
            {
                m_icon.gameObject.SetActive(false);
            }
            else
            {
                m_icon.gameObject.SetActive(true);

                // [AOC] HACK!! Add some offset depending on reward type
                OffsetPerType offset = null;
                for (int i = 0; i < m_iconOffsets.Length; ++i)
                {
                    if (m_iconOffsets[i].type == m_reward.type)
                    {
                        offset = m_iconOffsets[i];
                        break;  // Stop looping
                    }
                }

                // If no offset was found, use default values
                if (offset == null)
                {
                    offset = new OffsetPerType();
                }

                // Apply offset
                offset.Apply(m_icon.rectTransform);
            }
        }

        // 3D Previews
        // [AOC] We must delay them because initializing the 3D views at the same time that the popup is being instantiated results in weird behaviours
        UbiBCN.CoroutineManager.DelayedCallByFrames(() =>
        {
            // Dragon Loader
            if (m_dragonLoader != null)
            {
                // Activate?
                m_dragonLoader.gameObject.SetActive(isDragon);

                // Reload preview if required
                if (_reloadPreview)
                {
                    if (isDragon)
                    {
                        m_dragonLoader.LoadDragon(m_reward.sku);
                    }
                    else
                    {
                        m_dragonLoader.UnloadDragon();
                    }
                }
            }

            // Pet Loader
            if (m_petLoader != null)
            {
                // Activate?
                m_petLoader.gameObject.SetActive(isPet);

                // Reload preview if required
                if (_reloadPreview)
                {
                    if (isPet)
                    {
                        m_petLoader.Load(m_reward.sku);
                    }
                    else
                    {
                        m_petLoader.Unload();
                    }
                }
            }

            // Egg Loader
            if (m_eggLoader != null)
            {
                // Activate?
                m_eggLoader.gameObject.SetActive(isEgg);

                // Reload preview if required
                if (_reloadPreview)
                {
                    if (isEgg)
                    {
                        m_eggLoader.Load(m_reward.sku);
                    }
                    else
                    {
                        m_eggLoader.Unload();
                    }
                }
            }
        }, 1);

        // Tooltip trigger
        if (m_tooltipTrigger != null)
        {
            // Assign proper tooltip reference based on reward type
            UITooltip targetTooltip = null;
            switch (m_reward.type)
            {
                case Metagame.RewardDragon.TYPE_CODE:
                    {
                        targetTooltip = m_dragonTooltip;
                    }
                    break;

                case Metagame.RewardPet.TYPE_CODE:
                    {
                        targetTooltip = m_powerTooltip;
                    }
                    break;
            }

            // Activate? Only when type supports tooltip
            m_tooltipTrigger.gameObject.SetActive(targetTooltip != null);

            // Listen to events only if active
            m_tooltipTrigger.OnTooltipOpen.RemoveListener(OnTooltipOpen);
            if (targetTooltip != null)
            {
                m_tooltipTrigger.OnTooltipOpen.AddListener(OnTooltipOpen);

                // Assign target tooltip
                m_tooltipTrigger.tooltip = targetTooltip;
            }
        }

        // Background gradient color
        // [AOC] Unfortunately, Unity doesn't allow animating custom serializable 
        //       properties (Gradient4), so we need to do that manually -_-
        if (m_backgroundGradient != null)
        {
            // Select gradient based on reward state
            bool isSpecial = isDragon || isPet || isEgg;
            Gradient4 targetGradient = settings.defaultGradient;
            switch (m_state)
            {
                case State.IDLE:
                    {
                        targetGradient = isSpecial ? settings.specialGradient : settings.defaultGradient;
                    }
                    break;

                case State.COOLDOWN:
                    {
                        targetGradient = settings.cooldownGradient;
                    }
                    break;

                case State.CURRENT:
                    {
                        targetGradient = settings.currentGradient;
                    }
                    break;

                case State.COLLECTED:
                    {
                        targetGradient = settings.collectedGradient;
                    }
                    break;
            }

            // Apply!
            m_backgroundGradient.gradient.Set(targetGradient);
            m_backgroundGradient.enabled = false;
            m_backgroundGradient.enabled = true;
        }

        // Current glow FX
        if (m_currentGlowFX != null)
        {
            if (m_state == State.CURRENT)
            {
                m_currentGlowFX.Load();
            }
            else if (m_currentGlowFX.loadedInstance != null)
            {
                m_currentGlowFX.Unload();
            }
        }

        // State animator
        if (m_stateAnimator != null)
        {
            m_stateAnimator.SetInteger("state", (int)m_state);
            m_stateAnimator.SetBool("isSpecial", isDragon || isPet || isEgg);
        }

        // If the reward is in Current or Cooldown state, render on top so the decorations 
        // don't get hidden by other rewards in the sequence
        if (m_state == State.COOLDOWN || m_state == State.CURRENT)
        {
            this.transform.SetAsLastSibling();
        }
    }

    /// <summary>
    /// Trigger the collect FX.
    /// Won't perform any checks.
    /// </summary>
    public void LaunchCollectFX()
    {
        // VFX
        if (m_collectFX != null)
        {
            m_collectFX.Load();
            if (m_collectFX.loadedInstance != null)
            {
                ParticleSystem ps = m_collectFX.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }
        }

        // SFX
        AudioController.Play("hd_lab_power_upgraded");
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// A tooltip is about to be opened.
    /// </summary>
    /// <param name="_tooltip">Tooltip that will be opened.</param>
    /// <param name="_trigger">The trigger opening the tooltip.</param>
    public void OnTooltipOpen(UITooltip _tooltip, UITooltipTrigger _trigger)
    {
        // Initialize with this reward's info
        // Depends on reward type (non-supported types shouldn't receive this event anyways)
        switch (m_reward.type)
        {
            case Metagame.RewardDragon.TYPE_CODE:
                {
                    // Initialize tooltip
                    DragonInfoTooltip dragonTooltip = _tooltip as DragonInfoTooltip;
                    if (dragonTooltip != null)
                    {
                        dragonTooltip.InitFromDefinition(m_reward.def);
                    }
                }
                break;

            case Metagame.RewardPet.TYPE_CODE:
                {
                    // Gather definition of this pet's power
                    DefinitionNode powerDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_reward.def.GetAsString("powerup"));

                    // Initialize tooltip - it should have a PowerTooltip_OLD component attached
                    PowerTooltip_Generic powerTooltip = _tooltip as PowerTooltip_Generic;
                    if (powerTooltip != null)
                    {
                        powerTooltip.InitTooltip(powerDef, m_reward.def, PowerIcon.Mode.PET, PowerIcon.DisplayMode.PREVIEW);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Dragon preview has been loaded.
    /// </summary>
    /// <param name="_loader">The loader that triggered the event.</param>
    public void OnDragonLoaded(MenuDragonLoader _loader)
    {
        // Remove rigidbody from the dragon so it gets properly positioned (WTF)
        MenuDragonPreview dragonInstance = _loader.dragonInstance;
        if (dragonInstance != null)
        {
            // Destroy rigidbody
            Rigidbody rb = dragonInstance.GetComponent<Rigidbody>();
            if (rb != null) DestroyImmediate(rb);

            // Reset position
            dragonInstance.transform.localPosition = Vector3.zero;
        }
    }

    //------------------------------------------------------------------------//
    // DEBUG																  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Simulate a debug state.
    /// </summary>
    /// <param name="_state">State to simulate.</param>
    /// <param name="_rewardIdx">Reward index within the sequence.</param>
    public void DEBUG_Init(State _state, int _rewardIdx)
    {
        // Simulate some visuals
        // Check Mark
        GameObject checkMarkObj = this.FindObjectRecursive("CheckMark");
        if (checkMarkObj != null)
        {
            checkMarkObj.SetActive(_state == State.COLLECTED);
        }

        // Day Text
        if (m_dayText != null)
        {
            m_dayText.text.text = "Day " + StringUtils.FormatNumber(_rewardIdx + 1);
        }

        // Background gradient color
        if (m_backgroundGradient != null)
        {
            // Select gradient based on reward state
            bool isSpecial = _rewardIdx == DailyRewardsSequence.SEQUENCE_SIZE - 1;
            Gradient4 targetGradient = settings.defaultGradient;
            switch (_state)
            {
                case State.IDLE:
                    {
                        targetGradient = isSpecial ? settings.specialGradient : settings.defaultGradient;
                    }
                    break;

                case State.COOLDOWN:
                    {
                        targetGradient = settings.cooldownGradient;
                    }
                    break;

                case State.CURRENT:
                    {
                        targetGradient = settings.currentGradient;
                    }
                    break;

                case State.COLLECTED:
                    {
                        targetGradient = settings.collectedGradient;
                    }
                    break;
            }

            // Apply!
            m_backgroundGradient.gradient.Set(targetGradient);
            m_backgroundGradient.enabled = false;
            m_backgroundGradient.enabled = true;
        }

        // Simulate what the animator would do
        AnimationClip[] clips = m_stateAnimator.runtimeAnimatorController.animationClips;

        //string stateId = "AN_DailyRewardView_Layout_DEFAULT";
        for (int i = 0; i < clips.Length; ++i)
        {
            switch (clips[i].name)
            {
                // Frame color layer
                case "AN_DailyRewardView_FrameColor_CURRENT":
                    {
                        if (_state == State.CURRENT)
                        {
                            clips[i].SampleAnimation(this.gameObject, 0f);
                        }
                    }
                    break;

                case "AN_DailyRewardView_FrameColor_IDLE":
                    {
                        if (_state != State.CURRENT)
                        {
                            clips[i].SampleAnimation(this.gameObject, 0f);
                        }
                    }
                    break;

                // Layout layer
                case "AN_DailyRewardView_Layout_COLLECTED":
                    {
                        if (_state == State.COLLECTED)
                        {
                            clips[i].SampleAnimation(this.gameObject, 0f);
                        }
                    }
                    break;

                case "AN_DailyRewardView_Layout_DEFAULT":
                    {
                        if (_state != State.COLLECTED)
                        {
                            clips[i].SampleAnimation(this.gameObject, 0f);
                        }
                    }
                    break;

                // Cooldown Flag layer
                case "AN_DailyRewardView_Flag_COOLDOWN":
                    {
                        if (_state == State.COOLDOWN)
                        {
                            clips[i].SampleAnimation(this.gameObject, 0f);
                        }
                    }
                    break;

                case "AN_DailyRewardView_Flag_DEFAULT":
                    {
                        if (_state != State.COOLDOWN)
                        {
                            clips[i].SampleAnimation(this.gameObject, 0f);
                        }
                    }
                    break;
            }
        }
    }
}