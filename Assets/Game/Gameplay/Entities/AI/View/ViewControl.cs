
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ViewControl : IViewControl, IBroadcastListener {

    public static bool sm_allowGoldenMaterial = true;

    public static Material sm_goldenMaterial = null;
    public static Material sm_goldenFreezeMaterial = null;
    public static Material sm_goldenInloveMaterial = null;
    private static ulong sm_id = 0;

    public static float FREEZE_TIME = 1.0f;

    [Serializable]
    public class SkinData {
        public Material skin;
        [Range(0f, 100f)] public float chance = 0f;
    }

    public enum SpecialAnims {
        A = 0,
        B,
        C,

        Count
    }

    public enum MaterialType {
        NORMAL,
        GOLD,
        GOLD_FREEZE,
        GOLD_INLOVE,
        FREEZE,
        INLOVE,
        NONE
    }

    //-----------------------------------------------
    [SeparatorAttribute("Place Holder Setup")]
    [SerializeField] private bool m_isPlaceHolder = false;

    [SeparatorAttribute("Animation playback speed")]
    [SerializeField] private float m_walkSpeed = 1f;
    [SerializeField] private float m_runSpeed = 1f;
    [SerializeField] private float m_minPlaybakSpeed = 1f;
    [SerializeField] private float m_maxPlaybakSpeed = 1.5f;
    [SerializeField] private bool m_onBoostMaxPlaybackSpeed = false;


    [SeparatorAttribute("Animation blending")]
    [SerializeField] private bool m_hasNavigationLayer = false;
    public bool hasNavigationLayer { get { return m_hasNavigationLayer; } }
    [SerializeField] private bool m_hasRotationLayer = false;

    [SeparatorAttribute("Special Actions Animations")] // map a special action from the pilot to a specific animation.
    [SerializeField] private string m_animA = "";
    [SerializeField] private string m_animB = "";
    [SerializeField] private string m_animC = "";

    [SeparatorAttribute("Render")]
    [SerializeField] private List<Renderer> m_materialChangeExceptions = new List<Renderer>();

    [SeparatorAttribute("Freeze Particle")]
    [SerializeField] private bool m_useFreezeParticle = false;
    [SerializeField] private float m_freezeParticleScale = 1.0f;
    override public float freezeParticleScale { get { return m_freezeParticleScale; } }

    [SeparatorAttribute("In Love")]
    [SerializeField] private bool m_useMoveAnimInLove = false;

    [SeparatorAttribute("Exclamation Mark")]
    [SerializeField] private Transform m_exclamationTransform;

    [SeparatorAttribute("Water")]
    [SerializeField] private float m_speedToWaterSplash;
    [SerializeField] private ParticleData m_waterSplashParticle;

    [SeparatorAttribute("Damage")]
    [SerializeField] private bool m_showDamageFeedback = false;
    [SerializeField] private Color m_damageColor = Color.red;
    [SerializeField] private float m_damageTime = 2f;

    [SeparatorAttribute("Eaten")]
    [SerializeField] private string m_corpseAsset;
    [SerializeField] private ParticleData m_onEatenParticle;
    [SerializeField] private ParticleData m_onEatenFrozenParticle;
    private bool m_useFrozenParticle = false;
    [SerializeField] private string m_onEatenAudio;
    private AudioObject m_onEatenAudioAO;

    [SeparatorAttribute("Burn")]
    [SerializeField] private Transform[] m_firePoints;
    [SerializeField] private ParticleData m_burnParticle;
    [SerializeField] private string m_onBurnAudio;

    [SeparatorAttribute("Explode")]
    [SerializeField] private ParticleData m_explosionParticles;
    [SerializeField] private string m_onExplosionAudio;
    [SerializeField] private bool m_explodeWhenBurned = true;

    [SeparatorAttribute("More Audios")]
    [SerializeField] protected string m_onAttackAudio;
    private AudioObject m_onAttackAudioAO;
    [SerializeField] protected string m_onAttackDealDamageAudio;
    private AudioObject m_onAttackDealDamageAudioAO;
    protected Vector3 m_attackTargetPosition;
    public Vector3 attackTargetPosition { get { return m_attackTargetPosition; } set { m_attackTargetPosition = value; } }
    [SerializeField] private string m_onScaredAudio;
    private AudioObject m_onScaredAudioAO;
    [SerializeField] private string m_onPanicAudio;
    private AudioObject m_onPanicAudioAO;
    [SerializeField] private string m_idleAudio;
    private AudioObject m_idleAudioAO;

    [SeparatorAttribute("Skin")]
    [SerializeField] protected List<SkinData> m_skins = new List<SkinData>();


    //-----------------------------------------------
    [SerializeField]
    protected Entity m_entity;
    [SerializeField]
    protected Animator m_animator;
    protected float m_animatorSpeed;
    [SerializeField]
    protected bool m_isAnimatorAvailable;
    protected float m_disableAnimatorTimer;

    [SerializeField]
    private Renderer[] m_renderers;
    private List<Material[]> m_rendererMaterials;

    private Dictionary<int, List<Material>> m_materials;
    private Dictionary<int, List<Material>> m_materialsFrozen;
    private Dictionary<int, List<Material>> m_materialsInlove;
    protected List<Material> m_materialList;


    private int m_vertexCount;
    override public int vertexCount { get { return m_vertexCount; } }

    private int m_rendererCount;
    override public int rendererCount { get { return m_rendererCount; } }

    protected bool m_boost;
    protected bool m_scared;
    protected bool m_panic; //bite and hold state
    protected bool m_upsideDown;
    protected bool m_falling;
    protected bool m_jumping;
    protected bool m_attack;
    protected bool m_swim;
    protected bool m_inSpace;
    protected bool m_moving;
    protected bool m_attackingTarget;
    protected float m_aim;

    private bool m_hitAnimOn;
    private float m_damageFeedbackTimer;

    private bool m_isExclamationMarkOn;
    private GameObject m_exclamationMarkOn;

    private ParticleHandler m_corpseHandler;
    private ParticleHandler m_exclamationHandler;

    private float m_desiredBlendX;
    private float m_desiredBlendY;

    private float m_currentBlendX;
    private float m_currentBlendY;

    [SerializeField]
    private bool[] m_specialAnimations;
    [SerializeField]
    private int m_animA_Hash;
    [SerializeField]
    private int m_animB_Hash;
    [SerializeField]
    private int m_animC_Hash;

    [SerializeField]
    protected PreyAnimationEvents m_animEvents;
    override public PreyAnimationEvents animationEvents { get { return m_animEvents; } }

    private static int ATTACK_HASH = Animator.StringToHash("Attack");
    // private const int ATTACK_HASH = Animator.StringToHash("Attack");

    //Dragon breath detection
    private DragonBoostBehaviour m_dragonBoost;

    private MaterialType m_materialType = MaterialType.NONE;

    private Transform[] m_fireParticles;
    private Transform[] m_fireParticlesParents;

    //
    private float m_freezingLevel = 0;
    private GameObject m_freezingParticle;

    private ParticleData m_stunParticle;
    private GameObject m_stunParticleInstance;

    private ParticleData m_onEatenInloveParticle;
    private ParticleData m_inLoveParticle;
    private GameObject m_inLoveParticleInstance;
    protected bool m_inLove = false;
    protected bool m_bubbled = false;




    [SerializeField]
    private Transform m_transform;

    private ulong m_id;
    
    [SerializeField]
    private Transform m_view;
    // local backup
    private Vector3 m_viewPosition;
    private Quaternion m_viewRotation;
    private Vector3 m_viewScale;
    private bool m_applyRootMotion;
    private AnimatorCullingMode m_animatorCullingMode;

    private GameCamera m_camera;


    //-----------------------------------------------
    // Use this for initialization
    //-----------------------------------------------
    protected virtual void Awake() {
        //----------------------------
        m_id = sm_id;
        sm_id++;
        //----------------------------
        if (sm_goldenMaterial == null) sm_goldenMaterial = new Material(Resources.Load("Game/Materials/NPC_Golden") as Material);
        if (sm_goldenFreezeMaterial == null) sm_goldenFreezeMaterial = new Material(Resources.Load("Game/Materials/NPC_GoldenFreeze") as Material);
        if (sm_goldenInloveMaterial == null) sm_goldenInloveMaterial = new Material(Resources.Load("Game/Materials/NPC_GoldenInlove") as Material);
        //---------------------------- 

        if (m_animator != null) {
            m_isAnimatorAvailable = true;
            m_animator.logWarnings = false;
            m_applyRootMotion = m_animator.applyRootMotion;
            m_animatorCullingMode = m_animator.cullingMode;
        } else {
            m_isAnimatorAvailable = false;
            m_applyRootMotion = false;
            m_animatorCullingMode = AnimatorCullingMode.CullCompletely;
        }


        if (m_animEvents != null) {
            m_animEvents.onAttackStart += animEventsOnAttackStart;
            m_animEvents.onAttackEnd += animEventsOnAttackEnd;
            m_animEvents.onAttackDealDamage += animEventsOnAttackDealDamage;
            m_animEvents.onHitEnd += OnHitAnimEnd;
        }

        // keep the original materials, sometimes it will become Gold!
        m_materials = new Dictionary<int, List<Material>>();
        m_materialsFrozen = new Dictionary<int, List<Material>>();
        m_materialsInlove = new Dictionary<int, List<Material>>();
        m_materialList = new List<Material>();
        m_rendererMaterials = new List<Material[]>();

        m_vertexCount = 0;
        m_rendererCount = 0;

        // Add equiped stuff to the renderers list
        if (m_entity != null && m_entity.equip != null && m_entity.equip.HasSomethingEquiped()){
            Renderer[] moreRenderers = m_entity.equip.EquipedRenderers();
            int moreRenderersLength = moreRenderers.Length;
            int initialLength = m_renderers.Length; 
            int totalSize = moreRenderersLength + initialLength;
            Renderer[] newRenderesList = new Renderer[totalSize];
            for (int i = 0; i < initialLength; i++){
                newRenderesList[i] = m_renderers[i];
            }
            for (int i = 0; i < moreRenderersLength; i++){
                newRenderesList[ initialLength + i] = moreRenderers[i];
            }
            m_renderers = newRenderesList;
        }

        Renderer[] renderers = m_renderers;

        if (renderers != null) {
            int renderersCount = renderers.Length;
            for (int i = 0; i < renderersCount; i++) {
                Renderer renderer = renderers[i];
                Material[] materials = renderer.sharedMaterials;

                // Stores the materials of this renderer in a dictionary for direct access//
                int renderID = renderer.GetInstanceID();
                m_materials[renderID] = new List<Material>();
                m_materialsFrozen[renderID] = new List<Material>();
                m_materialsInlove[renderID] = new List<Material>();

                for (int m = 0; m < materials.Length; ++m) {
                    Material mat = materials[m];
                    if (m_showDamageFeedback) mat = new Material(materials[m]);

                    if (mat != null) {
                        m_materialList.Add(mat);
                    }

                    m_materials[renderID].Add(mat);
                    if (mat != null) {
                        m_materialsFrozen[renderID].Add(FrozenMaterialManager.GetFrozenMaterialFor(mat));
                        m_materialsInlove[renderID].Add(InloveMaterialManager.GetInloveMaterialFor(mat));
                    } else {
                        m_materialsFrozen[renderID].Add(null);
                        m_materialsInlove[renderID].Add(null);
                    }

                    materials[m] = null; // remove all materials to avoid instantiation.
                }
                renderer.sharedMaterials = materials;
                m_rendererMaterials.Add(materials);
            }
            m_rendererCount = m_renderers.Length;
        }

        if (!string.IsNullOrEmpty(m_corpseAsset)) {
            m_corpseHandler = ParticleManager.CreatePool(m_corpseAsset, "Corpses/");
        }

        m_isExclamationMarkOn = false;
        if (m_exclamationTransform != null) {
            m_exclamationHandler = ParticleManager.CreatePool("PF_ExclamationMark");
        }

        // Preload particle
        m_onEatenParticle.CreatePool();
        m_burnParticle.CreatePool();
        m_explosionParticles.CreatePool();

        if (m_onEatenParticle.name.ToLower().Contains("blood")) {
            m_useFrozenParticle = true;
            // only create if on eaten particle is blood
            if (!m_onEatenFrozenParticle.IsValid()) {
                m_onEatenFrozenParticle.name = "PS_IceExplosion";
                m_onEatenFrozenParticle.path = "";
            }
            m_onEatenFrozenParticle.CreatePool();
        } else {
            m_useFrozenParticle = false;
        }



        m_fireParticles = new Transform[Mathf.Max(1, m_firePoints.Length)];
        m_fireParticlesParents = new Transform[m_fireParticles.Length];

        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);


        if (m_stunParticle == null) {
            m_stunParticle = new ParticleData("FX_Stun", "", GameConstants.Vector3.zero);
        }
        m_stunParticle.CreatePool();

        m_onEatenInloveParticle = new ParticleData("PS_ValentinPetBrokenHeart", "", GameConstants.Vector3.zero);
        m_onEatenInloveParticle.orientate = true;
        m_onEatenInloveParticle.CreatePool();

        if (m_inLoveParticle == null) {
            m_inLoveParticle = new ParticleData("PS_ValentinPetLoop", "", GameConstants.Vector3.zero);
        }
        m_inLoveParticle.orientate = true;
        m_inLoveParticle.CreatePool();

        // Backup view values
        m_viewPosition = m_view.localPosition;
        m_viewRotation = m_view.localRotation;
        m_viewScale = m_view.localScale;
    }

    [ContextMenu("Get References")]
    public void GetReferences() {
        m_entity = GetComponent<Entity>();
        m_transform = transform;
        m_view = m_transform.FindObjectRecursive("view").transform;
        m_animator = m_view.GetComponent<Animator>();
        m_animEvents = m_transform.FindComponentRecursive<PreyAnimationEvents>();
        List<Renderer> renderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
        for (int i = renderers.Count - 1; i >= 0; i--) {
            if (m_materialChangeExceptions.Contains(renderers[i])) {
                renderers.RemoveAt(i);
            }
        }
        m_renderers = renderers.ToArray();


        m_specialAnimations = new bool[(int)SpecialAnims.Count];
        m_animA_Hash = UnityEngine.Animator.StringToHash(m_animA);
        m_animB_Hash = UnityEngine.Animator.StringToHash(m_animB);
        m_animC_Hash = UnityEngine.Animator.StringToHash(m_animC);
    }

    void Start() {
        if (m_isAnimatorAvailable) {
            StartEndMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndMachineBehaviour>();
            for (int i = 0; i < behaviours.Length; i++) {
                behaviours[i].onStart += onStartAnim;
                behaviours[i].onEnd += onEndAnim;
            }
        }
    }

    protected virtual void animEventsOnAttackStart() {
        if (!string.IsNullOrEmpty(m_onAttackAudio)) {
            m_onAttackAudioAO = AudioController.Play(m_onAttackAudio, m_transform);
            if (m_onAttackAudioAO != null)
                m_onAttackAudioAO.completelyPlayedDelegate = OnAttackAudioCompleted;
        }
    }

    protected virtual void animEventsOnAttackEnd() {
        if (m_onAttackAudioAO != null && m_onAttackAudioAO.IsPlaying() && m_onAttackAudioAO.audioItem.Loop != AudioItem.LoopMode.DoNotLoop)
            m_onAttackAudioAO.Stop();
        RemoveAudioParent(ref m_onAttackAudioAO);
    }

    void OnAttackAudioCompleted(AudioObject ao) {
        RemoveAudioParent(ref m_onAttackAudioAO);
    }

    protected virtual void animEventsOnAttackDealDamage() {
        if (!string.IsNullOrEmpty(m_onAttackDealDamageAudio)) {
            m_onAttackDealDamageAudioAO = AudioController.Play(m_onAttackDealDamageAudio, m_transform);
        }
    }


    void onStartAnim(int stateNameHash) {
        if (stateNameHash == ATTACK_HASH) {
            animEventsOnAttackStart();
        }
    }

    void onEndAnim(int stateNameHash) {
        if (stateNameHash == ATTACK_HASH) {
            animEventsOnAttackEnd();
        }
    }

    protected virtual void OnHitAnimEnd() {
        m_hitAnimOn = false;
    }
    //

    override public void Spawn(ISpawner _spawner) {
        if (m_camera == null && Camera.main != null) {
            m_camera = Camera.main.GetComponent<GameCamera>();
        }

        if (m_scared) { AnimatorSetBool(GameConstants.Animator.SCARED, false); m_scared = false; }
        if (m_panic) { AnimatorSetBool(GameConstants.Animator.HOLDED, false); m_panic = false; }
        if (m_upsideDown) { AnimatorSetBool(GameConstants.Animator.UPSIDE_DOWN, false); m_upsideDown = false; }
        if (m_falling) { AnimatorSetBool(GameConstants.Animator.FALLING, false); m_falling = false; }
        if (m_jumping) { AnimatorSetBool(GameConstants.Animator.JUMP, false); m_jumping = false; }
        if (m_attack) { AnimatorSetBool(GameConstants.Animator.ATTACK, false); m_attack = false; }
        if (m_swim) { AnimatorSetBool(GameConstants.Animator.SWIM, false); m_swim = false; }
        if (m_inSpace) { AnimatorSetBool(GameConstants.Animator.FLY_DOWN, false); m_inSpace = false; }
        if (m_moving) { AnimatorSetBool(GameConstants.Animator.MOVE, false); m_moving = false; }

        if (m_specialAnimations[0]) { AnimatorSetBool(m_animA_Hash, false); m_specialAnimations[0] = false; }
        if (m_specialAnimations[1]) { AnimatorSetBool(m_animB_Hash, false); m_specialAnimations[1] = false; }
        if (m_specialAnimations[2]) { AnimatorSetBool(m_animC_Hash, false); m_specialAnimations[2] = false; }

        AnimatorSetBool(GameConstants.Animator.EAT, false);

        m_boost = false;
        m_attackingTarget = false;
        m_hitAnimOn = false;
        m_isExclamationMarkOn = false;
        m_inLove = false;
        m_bubbled = false;

        m_aim = 0f;
        m_damageFeedbackTimer = 0f;

        m_disableAnimatorTimer = 0f;
        m_animatorSpeed = 1f;
        if (m_isAnimatorAvailable) {
            m_animator.enabled = true;
            m_animator.speed = m_animatorSpeed;
        }

        if (m_entity != null) {
            if (!string.IsNullOrEmpty(m_idleAudio)) {
                m_idleAudioAO = AudioController.Play(m_idleAudio, m_transform);
            }
        }

        DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
        SetMaterialType(GetMaterialType(IsEntityGolden(), dragonBreath.IsFuryOn(), dragonBreath.type));

        if (m_showDamageFeedback) {
            for (int i = 0; i < m_materialList.Count; ++i)
                m_materialList[i].DisableKeyword(GameConstants.Materials.Keyword.TINT);
        }

        if (m_corpseHandler != null && !m_corpseHandler.isValid) {
            m_corpseHandler = ParticleManager.CreatePool(m_corpseAsset, "Corpses/");
        }

        m_dragonBoost = InstanceManager.player.dragonBoostBehaviour;
    }

    private void AnimatorSetBool(int _param, bool _value) {
        if (m_isAnimatorAvailable) {
            m_animator.SetBool(_param, _value);
        }
    }

    void OnDestroy() {
        Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
        RemoveAudios();
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.FURY_RUSH_TOGGLED: {
                    FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                    OnFuryToggled(furyRushToggled.activated, furyRushToggled.type);
                }
                break;
        }
    }


    override public void PreDisable() {
        for (int i = 0; i < m_fireParticles.Length; ++i) {
            if (m_fireParticles[i] != null) {
                if (((m_firePoints.Length == 0) && (m_fireParticles[i].parent == m_transform)) ||
                    ((m_firePoints.Length > 0) && (m_fireParticles[i].parent == m_firePoints[i]))) {

                    m_fireParticles[i].SetParent(m_fireParticlesParents[i], true);
                }
                m_fireParticles[i] = null;
            }
        }
        if (m_stunParticleInstance) {
            m_stunParticle.ReturnInstance(m_stunParticleInstance);
            m_stunParticleInstance = null;
        }

        if (m_freezingParticle && FreezingObjectsRegistry.instance != null) {
            FreezingObjectsRegistry.instance.ForceReturnInstance(m_freezingParticle);
            FreezingObjectsRegistry.instance.freezeParticle.ReturnInstance(m_freezingParticle);
            m_freezingParticle = null;
        }

        if (m_inLoveParticleInstance) {
            m_inLoveParticle.ReturnInstance(m_inLoveParticleInstance);
            m_inLoveParticleInstance = null;
        }

        RemoveAudios();
    }

    protected virtual void RemoveAudios() {
        if (ApplicationManager.IsAlive) {
            if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying()) {
                m_idleAudioAO.Stop();
            }

            // Return parented audio objects if needed
            RemoveAudioParent(ref m_idleAudioAO);
            RemoveAudioParent(ref m_onAttackAudioAO);
            RemoveAudioParent(ref m_onAttackDealDamageAudioAO);
            RemoveAudioParent(ref m_onEatenAudioAO);

            RemoveAudioParent(ref m_onScaredAudioAO);
            RemoveAudioParent(ref m_onPanicAudioAO);
        }
    }

    protected void RemoveAudioParent(ref AudioObject ao) {
        if (ao != null && ao.transform.parent == m_transform) {
            ao.transform.parent = null;
            ao.completelyPlayedDelegate = null;
            if (ao.IsPlaying() && ao.audioItem.Loop != AudioItem.LoopMode.DoNotLoop)
                ao.Stop();
        }
        ao = null;
    }

    public void SetMaterialType(MaterialType _type) {
        if (m_isPlaceHolder) {
            _type = MaterialType.NORMAL;
        }
        m_materialType = _type;

        // Restore materials
        if (m_renderers != null) {
            for (int i = 0; i < m_rendererCount; i++) {
                int id = m_renderers[i].GetInstanceID();
                Material[] materials = m_rendererMaterials[i];
                for (int m = 0; m < materials.Length; m++) {
                    switch (_type) {
                        case MaterialType.GOLD: materials[m] = sm_goldenMaterial; break;
                        case MaterialType.GOLD_FREEZE: materials[m] = sm_goldenFreezeMaterial; break;
                        case MaterialType.GOLD_INLOVE: materials[m] = sm_goldenInloveMaterial; break;
                        case MaterialType.FREEZE: materials[m] = m_materialsFrozen[id][m]; break;
                        case MaterialType.INLOVE: materials[m] = m_materialsInlove[id][m]; break;
                        case MaterialType.NORMAL: {
                                Material mat = m_materials[id][m];
                                if (m_skins.Count > 0) {
                                    for (int s = 0; s < m_skins.Count; s++) {
                                        float rnd = UnityEngine.Random.Range(0f, 100f);
                                        if (rnd < m_skins[s].chance) {
                                            mat = m_skins[s].skin;
                                            break;
                                        }
                                    }
                                }
                                materials[m] = mat;
                            }
                            break;
                    }
                }
                m_renderers[i].materials = materials;
            }
        }
    }

    protected void RefreshMaterial() {
        SetMaterialType(m_materialType);
    }

    void OnFuryToggled(bool _active, DragonBreathBehaviour.Type _type) {
        RefreshMaterialType();
    }

    /// <summary>
    /// Determines whether this instance is burnable by player.
    /// </summary>
    /// <returns><c>true</c> if this instance is burnable by player; otherwise, <c>false</c>.</returns>
    private bool IsBurnableByPlayer(DragonBreathBehaviour.Type _fireType) {
        if (m_entity != null) {
            switch (_fireType) {
                case DragonBreathBehaviour.Type.Mega:
                return m_entity.IsBurnable();
                case DragonBreathBehaviour.Type.Standard:
                default:
                return m_entity.IsBurnable(InstanceManager.player.data.tier);

            }
        }
        return false;
    }

    MaterialType GetMaterialType(bool _isGolden, bool _furyActive = false, DragonBreathBehaviour.Type _type = DragonBreathBehaviour.Type.None) {
        MaterialType matType = ViewControl.MaterialType.NORMAL;
        if (sm_allowGoldenMaterial && (_isGolden || _furyActive)) {
            if (IsBurnableByPlayer(_type)) {
                matType = MaterialType.GOLD;
            }
        }

        // Check Freezing. It Has priority over inlove
        if (m_freezingLevel > 0) {
            if (sm_allowGoldenMaterial && matType == MaterialType.GOLD) {
                matType = MaterialType.GOLD_FREEZE;
            } else {
                matType = MaterialType.FREEZE;
            }
        } else if (m_inLove) {
            if (sm_allowGoldenMaterial && matType == MaterialType.GOLD) {
                matType = MaterialType.GOLD_INLOVE;
            } else {
                matType = MaterialType.INLOVE;
            }
        }
        return matType;
    }

    public void RefreshMaterialType() {
        DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
        CheckMaterialType(IsEntityGolden(), dragonBreath.IsFuryOn(), dragonBreath.type);
    }

    override public void ForceGolden() {
        RefreshMaterialType();
    }

    void CheckMaterialType(bool _isGolden, bool _furyActive = false, DragonBreathBehaviour.Type _type = DragonBreathBehaviour.Type.None) {
        MaterialType matType = GetMaterialType(_isGolden, _furyActive, _type);
        if (matType != m_materialType) {
            SetMaterialType(matType);
        }
    }

    override public void CustomUpdate() {
        UnityEngine.Profiling.Profiler.BeginSample("ViewControl");
        if (m_isAnimatorAvailable) {
            if (m_disableAnimatorTimer > 0) {
                m_disableAnimatorTimer -= Time.deltaTime;
                if (m_disableAnimatorTimer < 0) {
                    m_animator.enabled = false;
                }
            }

            if (m_animator.enabled && !m_bubbled) {

                m_animator.speed = m_animatorSpeed * Mathf.Max(0.25f, 1f - m_freezingLevel);

                if (m_hasNavigationLayer) {
                    m_currentBlendX = Util.MoveTowardsWithDamping(m_currentBlendX, m_desiredBlendX, Time.deltaTime, 0.2f);
                    m_animator.SetFloat(GameConstants.Animator.DIR_X, m_currentBlendX);

                    m_currentBlendY = Util.MoveTowardsWithDamping(m_currentBlendY, m_desiredBlendY, Time.deltaTime, 0.2f);
                    m_animator.SetFloat(GameConstants.Animator.DIR_Y, m_currentBlendY);
                }

                if (m_inLove && m_useMoveAnimInLove) {
                    m_moving = true;
                }

                m_animator.SetBool(GameConstants.Animator.SWIM, m_swim);
                m_animator.SetBool(GameConstants.Animator.FLY_DOWN, m_inSpace);
                if (!m_swim) {
                    m_animator.SetBool(GameConstants.Animator.MOVE, m_moving);
                } else {
                    m_animator.SetBool(GameConstants.Animator.MOVE, false);
                }
            }
        }

        if (m_damageFeedbackTimer > 0f) {
            m_damageFeedbackTimer -= Time.deltaTime;

            if (m_damageFeedbackTimer <= 0f) {
                for (int i = 0; i < m_materialList.Count; ++i)
                    m_materialList[i].DisableKeyword(GameConstants.Materials.Keyword.TINT);
            }

            Color damageColor = m_damageColor * (m_damageFeedbackTimer / m_damageTime) + Color.white * (1f - (m_damageFeedbackTimer / m_damageTime));
            SetColorAdd(damageColor);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    void SetColorAdd(Color _c) {
        _c.a = 0;
        for (int i = 0; i < m_materialList.Count; ++i)
            m_materialList[i].SetColor(GameConstants.Materials.Property.TINT, _c);
    }

    bool IsEntityGolden() {
        if (m_entity != null)
            return m_entity.isGolden;
        return false;
    }

    // Queries
    public bool HasCorpseAsset() {
        return !string.IsNullOrEmpty(m_corpseAsset);
    }

    public bool isHitAnimOn() {
        return m_hitAnimOn;
    }

    public bool canAttack() {
        return !m_attack;
    }

    public bool hasAttackEnded() {
        return false;
    }
    //

    public void ShowExclamationMark(bool _value) {
        if (m_exclamationTransform != null && m_isExclamationMarkOn != _value) {
            if (_value) {
                m_exclamationMarkOn = m_exclamationHandler.Spawn(null);
                if (m_exclamationMarkOn) {
                    FollowTransform ft = m_exclamationMarkOn.GetComponent<FollowTransform>();
                    ft.m_follow = m_exclamationTransform;
                }
            } else {
                if (m_exclamationMarkOn != null) {
                    m_exclamationHandler.ReturnInstance(m_exclamationMarkOn);
                    m_exclamationMarkOn = null;
                }
            }
            m_isExclamationMarkOn = _value;
        }
    }

    //Particles
    public void SpawnEatenParticlesAt(Transform _transform) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            // If the debug settings for particles eaten is disabled then they are not spawned
            if (!DebugSettings.ingameParticlesEaten)
                return;
        }

        if (m_freezingLevel > 0.1f && m_useFrozenParticle) {
            GameObject go = m_onEatenFrozenParticle.Spawn(m_transform.position + m_onEatenFrozenParticle.offset);
            if (go != null) {
                FollowTransform ft = go.GetComponent<FollowTransform>();
                if (ft != null) {
                    ft.m_follow = _transform;
                    ft.m_offset = m_onEatenFrozenParticle.offset;
                }
            }
        } else if (m_inLove) {
            Vector3 pos = m_transform.position;
            Quaternion rot = GameConstants.Quaternion.identity;
            if (m_inLoveParticleInstance) {
                pos = m_inLoveParticleInstance.transform.position;
                rot = m_inLoveParticleInstance.transform.rotation;
            }
            GameObject go = m_onEatenInloveParticle.Spawn(pos + m_onEatenInloveParticle.offset, rot);
        } else {
            GameObject go = m_onEatenParticle.Spawn(m_transform.position + m_onEatenParticle.offset);
            if (go != null) {
                FollowTransform ft = go.GetComponent<FollowTransform>();
                if (ft != null) {
                    ft.m_follow = _transform;
                    ft.m_offset = m_onEatenParticle.offset;
                }
            }
        }
    }


    // Animations
    public void NavigationLayer(Vector3 _dir) {
        if (m_hasNavigationLayer) {
            Vector3 localDir = m_transform.InverseTransformDirection(_dir); // todo: replace with direction to target if trying to bite, or during bite?
            m_desiredBlendX = Mathf.Clamp(localDir.x * 3f, -1f, 1f);    // max X bend is about 30 degrees, so *3
            m_desiredBlendY = Mathf.Clamp(localDir.y * 2f, -1f, 1f);    // max Y bend is about 45 degrees, so *2.
        }
    }

    public void RotationLayer(ref Quaternion _from, ref Quaternion _to) {
        if (m_hasRotationLayer && m_isAnimatorAvailable) {
            float angle = Quaternion.Angle(_from, _to);
            m_animator.SetBool(GameConstants.Animator.ROTATE_LEFT, angle < 0);
            m_animator.SetBool(GameConstants.Animator.ROTATE_RIGHT, angle > 0);
        }
    }

    public void Aim(float _blendFactor) {
        m_aim = _blendFactor;
        if (m_isAnimatorAvailable)
            m_animator.SetFloat(GameConstants.Animator.AIM, _blendFactor);
    }

    public void Height(float _height) {
        if (m_isAnimatorAvailable)
            m_animator.SetFloat(GameConstants.Animator.HEIGHT, _height);
    }

    public void Move(float _speed) {
        if (m_isAnimatorAvailable) {
            if (m_panic || m_falling) {
                m_animatorSpeed = 1;
                return;
            }

            if (_speed > 0.01f) {
                // 0- walk  1- run, blending in between
                float blendFactor = 0f;
                float animSpeedFactor = 1f;

                if (_speed <= m_walkSpeed) {
                    blendFactor = 0f;
                    animSpeedFactor = Mathf.Max(m_minPlaybakSpeed, _speed / m_walkSpeed);
                } else if (_speed >= m_runSpeed) {
                    blendFactor = 1f;
                    animSpeedFactor = Mathf.Min(m_maxPlaybakSpeed, _speed / m_runSpeed);
                } else {
                    blendFactor = 0f + (_speed - m_walkSpeed) * ((1f - 0f) / (m_runSpeed - m_walkSpeed));
                }

                if (m_boost && m_onBoostMaxPlaybackSpeed) {
                    animSpeedFactor = m_maxPlaybakSpeed;
                }

                m_moving = true;

                m_animator.SetFloat(GameConstants.Animator.SPEED, blendFactor);
                m_animatorSpeed = Mathf.Lerp(m_animatorSpeed, animSpeedFactor, Time.deltaTime * 2f);
                // m_animator.speed = 
            } else {
                m_moving = false;
                m_animatorSpeed = Mathf.Lerp(m_animatorSpeed, 1f, Time.deltaTime * 2f);
            }
        }
    }

    public void Boost(bool _boost) {
        if (m_panic)
            return;

        if (m_boost != _boost) {
            m_boost = _boost;
        }
    }

    public void Scared(bool _scared) {
        if (m_panic)
            return;

        if (m_scared != _scared) {
            m_scared = _scared;
            if (_scared) {
                if (!string.IsNullOrEmpty(m_onScaredAudio)) {
                    m_onScaredAudioAO = AudioController.Play(m_onScaredAudio, m_transform);
                    if (m_onScaredAudioAO != null)
                        m_onScaredAudioAO.completelyPlayedDelegate = OnScaredAudioEnded;
                }
            } else {
                if (m_onScaredAudioAO != null && m_onScaredAudioAO.IsPlaying() && m_onScaredAudioAO.audioItem.Loop != AudioItem.LoopMode.DoNotLoop) {
                    m_onScaredAudioAO.Stop();
                    RemoveAudioParent(ref m_onScaredAudioAO);
                }
            }

            if (m_isAnimatorAvailable)
                m_animator.SetBool(GameConstants.Animator.SCARED, _scared);

        }
    }

    void OnScaredAudioEnded(AudioObject ao) {
        RemoveAudioParent(ref m_onScaredAudioAO);
    }

    public void UpsideDown(bool _upsideDown) {
        if (m_upsideDown != _upsideDown) {
            m_upsideDown = _upsideDown;
            m_animator.SetBool(GameConstants.Animator.UPSIDE_DOWN, _upsideDown);
        }
    }

    public void Panic(bool _panic, bool _burning) {
        if (m_panic != _panic) {
            m_panic = _panic;

            if (_burning) {
                // lets buuurn!!!
                // will we have a special animation when burning?
                if (m_isAnimatorAvailable)
                    m_animatorSpeed = 0f;
            } else {
                if (_panic) {
                    if (!string.IsNullOrEmpty(m_onPanicAudio))
                        m_onPanicAudioAO = AudioController.Play(m_onPanicAudio, m_transform);
                    if (m_onPanicAudioAO != null)
                        m_onPanicAudioAO.completelyPlayedDelegate = OnPanicAudioEnded;
                } else {
                    if (m_onPanicAudioAO != null && m_onPanicAudioAO.IsPlaying() && m_onPanicAudioAO.audioItem.Loop != AudioItem.LoopMode.DoNotLoop)
                        m_onPanicAudioAO.Stop();
                    RemoveAudioParent(ref m_onPanicAudioAO);
                }

                if (m_isAnimatorAvailable)
                    m_animator.SetBool(GameConstants.Animator.HOLDED, _panic);
            }
        }
    }

    void OnPanicAudioEnded(AudioObject ao) {
        RemoveAudioParent(ref m_onPanicAudioAO);
    }

    public void Hit() {
        m_hitAnimOn = true;

        if (m_isAnimatorAvailable)
            m_animator.SetTrigger(GameConstants.Animator.HIT);

        if (m_showDamageFeedback) {
            m_damageFeedbackTimer = m_damageTime;

            for (int i = 0; i < m_materialList.Count; ++i)
                m_materialList[i].EnableKeyword(GameConstants.Materials.Keyword.TINT);
        }
    }

    public void Falling(bool _falling) {
        if (m_falling != _falling) {
            m_falling = _falling;
            if (m_isAnimatorAvailable) {
                m_animatorSpeed = 1;
                m_animator.SetBool(GameConstants.Animator.FALLING, _falling);
            }
        }
    }

    public void Jumping(bool _jumping) {
        if (m_jumping != _jumping) {
            m_jumping = _jumping;
            if (m_isAnimatorAvailable) {
                m_animatorSpeed = 1f;
                m_animator.SetBool(GameConstants.Animator.JUMP, _jumping);
            }
        }
    }

    public void JumpDown(bool _down) {
        if (m_isAnimatorAvailable){
            m_animator.SetBool("jump_down", _down);
        }

        
    }

    public void Attack(bool _melee, bool _ranged) {
        if (m_panic)
            return;

        if (!m_attack) {
            m_attack = true;
            if (m_isAnimatorAvailable) {
                m_animator.SetBool(GameConstants.Animator.ATTACK, true);
                m_animator.SetBool(GameConstants.Animator.MELEE, _melee);
                m_animator.SetBool(GameConstants.Animator.RANGED, _ranged);

            }
        }
    }

    public void StopAttack() {
        if (m_panic)
            return;

        if (m_attack) {
            m_attack = false;
            if (m_isAnimatorAvailable) {
                m_animator.SetBool(GameConstants.Animator.ATTACK, false);
                m_animator.SetBool(GameConstants.Animator.MELEE, false);
                m_animator.SetBool(GameConstants.Animator.RANGED, false);
            }
        }
    }

    public void StartAttackTarget() {
        m_attackingTarget = true;

        if (m_isAnimatorAvailable)
            m_animator.SetBool(GameConstants.Animator.EAT, true);
    }

    public void StopAttackTarget() {
        m_attackingTarget = false;
        if (m_isAnimatorAvailable)
            m_animator.SetBool(GameConstants.Animator.EAT, false);
    }

    public void StartEating() {
        m_animator.SetBool(GameConstants.Animator.EAT, true);
    }

    public void StopEating() {
        if (!m_attackingTarget)
            m_animator.SetBool(GameConstants.Animator.EAT, false);
    }

    public void Impact() {
        if (m_isAnimatorAvailable)
            m_animator.SetTrigger(GameConstants.Animator.IMPACT);

        if (m_showDamageFeedback) {
            m_damageFeedbackTimer = m_damageTime;
            for (int i = 0; i < m_materialList.Count; ++i)
                m_materialList[i].EnableKeyword(GameConstants.Materials.Keyword.TINT);
        }
    }

    public void EnterWater(Collider _other, Vector3 impulse) {
        CreateSplash(_other, Mathf.Abs(impulse.y));
    }

    public void ExitWater(Collider _other, Vector3 impulse) {
        CreateSplash(_other, Mathf.Abs(impulse.y));
    }

    private void CreateSplash(Collider _other, float verticalImpulse) {
        if (verticalImpulse >= m_speedToWaterSplash && !string.IsNullOrEmpty(m_waterSplashParticle.name)) {
            Vector3 pos = m_transform.position;
            float waterY = _other.bounds.center.y + _other.bounds.extents.y;
            pos.y = waterY;
            m_waterSplashParticle.Spawn(m_transform.position + m_waterSplashParticle.offset);
        }
    }

    public void StartSwimming() {
        m_swim = true;
    }

    public void StopSwimming() {
        m_swim = false;
    }

    public void FlyToSpace() {
        m_inSpace = true;
    }

    public void ReturnFromSpace() {
        m_inSpace = false;
    }

    public void SpecialAnimation(SpecialAnims _anim, bool _value) {
        if (m_specialAnimations[(int)_anim] != _value) {
            switch (_anim) {
                case SpecialAnims.A: m_animator.SetBool(m_animA_Hash, _value); break;
                case SpecialAnims.B: m_animator.SetBool(m_animB_Hash, _value); break;
                case SpecialAnims.C: m_animator.SetBool(m_animC_Hash, _value); break;
            }

            if (_value) OnSpecialAnimationEnter(_anim);
            else OnSpecialAnimationExit(_anim);
        }
        m_specialAnimations[(int)_anim] = _value;
    }

    protected virtual void OnSpecialAnimationEnter(SpecialAnims _anim) { }
    protected virtual void OnSpecialAnimationExit(SpecialAnims _anim) { }

    public void Die(bool _eaten = false, bool _burned = false) {
        ShowExclamationMark(false);

        if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying()) {
            m_idleAudioAO.Stop();
            RemoveAudioParent(ref m_idleAudioAO);
        }

        bool other = !_eaten && !_burned;

        // if burned or explode for no reason
        if ((m_explodeWhenBurned && _burned) || other) {
            PlayExplosion();
        }

        // if eaten or not burned
        if (_eaten || other) {
            if (m_corpseHandler != null) {
                // spawn corpse
                GameObject corpse = m_corpseHandler.Spawn(null);
                if (corpse != null) {
                    corpse.transform.CopyFrom(m_transform);
                    corpse.GetComponent<Corpse>().Spawn(IsEntityGolden(), m_dragonBoost.IsBoostActive());
                }
            }
        }
    }

    public void PlayExplosion() {
        if (m_camera.IsInsideCameraFrustrum(m_transform.position)) {
            GameObject go = m_explosionParticles.Spawn(m_transform.position + m_explosionParticles.offset, m_transform.rotation);

            if (!string.IsNullOrEmpty(m_onExplosionAudio))
                AudioController.Play(m_onExplosionAudio, m_transform.position);
        }
    }

    /// <summary>
    /// Bite this instance. When someone starts eating this view
    /// </summary>
    public virtual void Bite(Transform _transform) {
        if (m_inLoveParticleInstance) {
            m_inLoveParticleInstance.transform.parent = m_transform.parent;
        }
    }

    public void BeginSwallowed(Transform _transform) {
        OnEatenEvent(_transform);
    }

    public void OnEatenEvent(Transform _transform) {
        if (m_entity.isOnScreen && !string.IsNullOrEmpty(m_onEatenAudio)) {
            m_onEatenAudioAO = AudioController.Play(m_onEatenAudio, m_transform);
        }
        SpawnEatenParticlesAt(_transform);
    }

    public void Burn(float _burnAnimSeconds, FireColorSetupManager.FireColorType _burnColor) {
        if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying()) {
            m_idleAudioAO.Stop();
            RemoveAudioParent(ref m_idleAudioAO);
        }

        if (!m_explosionParticles.IsValid()) {
            if (m_burnParticle.IsValid()) {
                if (m_firePoints != null && m_firePoints.Length > 0) {
                    for (int i = 0; i < m_firePoints.Length; i++) {
                        SpawnBurnParticle(m_firePoints[i], i, _burnAnimSeconds, _burnColor);
                    }
                } else {
                    SpawnBurnParticle(m_transform, 0, _burnAnimSeconds, _burnColor);
                }
            }
        }

        if (!string.IsNullOrEmpty(m_onBurnAudio)) {
            AudioController.Play(m_onBurnAudio, m_transform.position);
        }

        if (m_isAnimatorAvailable) {
            m_animatorSpeed = 1f;
            m_animator.SetTrigger(GameConstants.Animator.BURN);
            m_disableAnimatorTimer = _burnAnimSeconds + 0.1f;
        } else {
            m_disableAnimatorTimer = 0.1f;
        }
    }

    private void SpawnBurnParticle(Transform _parent, int _index, float _disableInSeconds, FireColorSetupManager.FireColorType _burnColor) {
        GameObject go = m_burnParticle.Spawn();
        if (go != null) {
            Transform t = go.transform;

            m_fireParticles[_index] = t;
            m_fireParticlesParents[_index] = t.parent;

            t.SetParent(_parent, false);
            t.localPosition = m_burnParticle.offset;

            DisableInSeconds dis = go.GetComponent<DisableInSeconds>();
            dis.activeTime = _disableInSeconds;

            FireTypeAutoSelector fireTypeAutoSelector = go.GetComponent<FireTypeAutoSelector>();
            if (fireTypeAutoSelector != null)
                fireTypeAutoSelector.m_fireType = _burnColor;
        } else {
            m_fireParticles[_index] = null;
            m_fireParticlesParents[_index] = null;
        }
    }

    /// <summary>
    /// Freezing the specified freezeLevel. 0 -> no freezing, 1 -> completely frozen
    /// </summary>
    /// <param name="freezeLevel">Freeze level.</param>
    override public void Freezing(float freezeLevel) {
        if ((m_freezingLevel <= 0 && freezeLevel > 0) || (m_freezingLevel > 0 && freezeLevel <= 0)) {
            m_freezingLevel = freezeLevel;
            RefreshMaterialType();
            if (m_freezingLevel > 0) {
                AudioController.Play("freeze", m_transform.position);
            }

            // Check Particle
            if (m_useFreezeParticle) {
                if (m_freezingLevel > 0) {
                    if (m_freezingParticle == null) {
                        m_freezingParticle = FreezingObjectsRegistry.instance.freezeParticle.Spawn(m_transform);
                    }
                    if (m_freezingParticle) {
                        m_freezingParticle.transform.position = m_entity.circleArea.center;
                        FreezingObjectsRegistry.instance.ScaleUpParticle(m_freezingParticle, m_freezeParticleScale);
                    }
                } else {
                    if (m_freezingParticle != null) {
                        FreezingObjectsRegistry.instance.ScaleDownParticle(m_freezingParticle, FreezeScaleDownCallback);
                    }
                }
            }
        }
        m_freezingLevel = freezeLevel;
    }

    public void FreezeScaleDownCallback() {
        FreezingObjectsRegistry.instance.freezeParticle.ReturnInstance(m_freezingParticle);
        m_freezingParticle = null;
    }


    public void SetStunned(bool stunned) {
        if (stunned) {
            if (m_isAnimatorAvailable)
                m_animator.enabled = false;
            // if no stunned particle -> stun
            if (m_stunParticleInstance == null) {
                m_stunParticleInstance = m_stunParticle.Spawn(m_transform);
            }
        } else {
            if (m_isAnimatorAvailable)
                m_animator.enabled = true;
            // if stunned particle -> remove stun
            if (m_stunParticleInstance) {
                m_stunParticle.ReturnInstance(m_stunParticleInstance);
                m_stunParticleInstance = null;
            }
        }
    }


    public void SetInLove(bool inlove) {
        if (m_inLove != inlove) {
            m_inLove = inlove;
            RefreshMaterialType();
            if (inlove) {
                if (m_inLoveParticleInstance == null) {
                    Vector3 pos = m_transform.position;
                    Quaternion rot = m_transform.rotation;
                    m_inLoveParticleInstance = m_inLoveParticle.Spawn(pos + m_inLoveParticle.offset, rot);
                }
                m_moving = m_useMoveAnimInLove;
            } else {
                if (m_inLoveParticleInstance) {
                    m_inLoveParticle.ReturnInstance(m_inLoveParticleInstance);
                    m_inLoveParticleInstance = null;
                }
                m_moving = false;
            }
        }
    }

    public void SetBubbled(bool _bubbled) {
        m_bubbled = _bubbled;
    }

}
