using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ViewControl : MonoBehaviour, ISpawnable {

	public static Color GOLD_TINT = new Color(255.0f / 255.0f, 161 / 255.0f, 0, 255.0f / 255.0f);
    public static Color FREEZE_TINT = new Color(0.0f / 255.0f, 200.0f / 255.0f, 255.0f / 255.0f, 255.0f / 255.0f);
    public static float FREEZE_TIME = 1.0f;

    [Serializable]
	public class SkinData {
		public Material skin;
		[Range(0f, 100f)] public float m_chance = 0f;
	}

	public enum SpecialAnims {
		A = 0,
		B,
		C,

		Count
	}

	public enum EntityTint
	{
		NORMAL,
		GOLD,
		FREEZE,
		NONE
	}

	//-----------------------------------------------
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

	[SeparatorAttribute("Exclamation Mark")]
	[SerializeField] private Transform m_exclamationTransform;

	[SeparatorAttribute("Eaten")]
	[SerializeField] private string m_corpseAsset = "";
	[SerializeField] private ParticleData m_onEatenParticle;
	[SerializeField] private ParticleData m_onEatenFrozenParticle;
	private bool m_useFrozenParticle = false;
	[SerializeField] private string m_onEatenAudio;
	private AudioObject m_onEatenAudioAO;

	[SeparatorAttribute("Water")]
	[SerializeField] private float m_speedToWaterSplash;
	[SerializeField] private ParticleData m_waterSplashParticle;

	[SeparatorAttribute("Burn")]
	[SerializeField] private Transform[] m_firePoints;
	[SerializeField] private ParticleData m_burnParticle;
	[SerializeField] private string m_onBurnAudio;

	[SeparatorAttribute("Explode")]
	[SerializeField] private ParticleData m_explosionParticles; // this will explode when burning
	[SerializeField] private string m_onExplosionAudio;

	[SeparatorAttribute("More Audios")]
	[SerializeField] private string m_onAttackAudio;
	private AudioObject m_onAttackAudioAO;
	protected Vector3 m_attackTargetPosition;
	public Vector3 attackTargetPosition { get{ return m_attackTargetPosition; } set{ m_attackTargetPosition = value; } }
	[SerializeField] private string m_onScaredAudio;
	private AudioObject m_onScaredAudioAO;
	[SerializeField] private string m_onPanicAudio;
	private AudioObject m_onPanicAudioAO;
	[SerializeField] private string m_idleAudio;
	private AudioObject m_idleAudioAO;

	[SeparatorAttribute("Skin")]
	[SerializeField] private List<SkinData> m_skins = new List<SkinData>();


	//-----------------------------------------------
	private Entity m_entity;
	protected Animator m_animator;
	protected float m_disableAnimatorTimer;

	//	private Material m_materialGold;
	private Dictionary<int, Material[]> m_materials;
    // All materials are stored in this list (it contains the same materials as m_materials does) to prevent memory from being allocated when looping through m_materials in entityTint()
    private List<Material> m_allMaterials;
	private List<Color> m_defaultTints;

	protected bool m_boost;
	protected bool m_scared;
	protected bool m_panic; //bite and hold state
	protected bool m_falling;
	protected bool m_jumping;
	protected bool m_attack;
	protected bool m_swim;
	protected bool m_inSpace;
	protected bool m_moving;
	protected bool m_attackingTarget;

	private bool m_isExclamationMarkOn;
	private GameObject m_exclamationMarkOn;

	private float m_desiredBlendX;
	private float m_desiredBlendY;

	private float m_currentBlendX;
	private float m_currentBlendY;

	private bool[] m_specialAnimations;

	private GameObject m_pcTrail = null;

	private PreyAnimationEvents m_animEvents;

	private static int ATTACK_HASH = Animator.StringToHash("Attack");
    // private const int ATTACK_HASH = Animator.StringToHash("Attack");

    //Dragon breath detection
	private DragonBoostBehaviour m_dragonBoost;

	private EntityTint m_entityTint = EntityTint.NONE;

	//
    private float m_freezingLevel = 0;
    private bool m_wasFreezing = true;

    //-----------------------------------------------
    // Use this for initialization
    //-----------------------------------------------
    protected virtual void Awake() {
		m_entity = GetComponent<Entity>();
		m_animator = transform.FindComponentRecursive<Animator>();
		if (m_animator != null)
			m_animator.logWarnings = false;

		m_animEvents = transform.FindComponentRecursive<PreyAnimationEvents>();
		if ( m_animEvents != null){
			m_animEvents.onAttackStart += animEventsOnAttackStart;
			m_animEvents.onAttackEnd += animEventsOnAttackEnd;
			m_animEvents.onAttackDealDamage += animEventsOnAttackDealDamage;
		}

        // Load gold material
        //		m_materialGold = Resources.Load<Material>("Game/Assets/Materials/Gold");

        // keep the original materials, sometimes it will become Gold!
        m_materials = new Dictionary<int, Material[]>();        
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        m_allMaterials = new List<Material>();
		m_defaultTints = new List<Color>();
        if (renderers != null) {
            int count = renderers.Length;
            int matCount;
            Material[] materials;
            for (int i = 0; i < count; i++) {
                // Stores the materials of this renderer in a dictionary for direct access
                materials = renderers[i].materials;
                m_materials[renderers[i].GetInstanceID()] = materials;

                // Stores the materials of this renderer in the list of all materials for sequencial access with no memory allocations
                if (materials != null) {
                    matCount = materials.Length;
                    for (int j = 0; j < matCount; j++) {
                        m_allMaterials.Add(materials[j]);
						if (materials[j].HasProperty("_FresnelColor")) {
							m_defaultTints.Add(materials[j].GetColor("_FresnelColor"));
						} else {
							m_defaultTints.Add(Color.black);
						}
                    }
                }
            }
        }

		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			PoolManager.CreatePool(m_corpseAsset, "Game/Corpses/", 3, true);
		}

		m_isExclamationMarkOn = false;
		if (m_exclamationTransform != null) {
			PoolManager.CreatePool("PF_ExclamationMark", "Game/Entities/", 3, true);
		}

		// particle management
		if (!m_onEatenParticle.IsValid()) {
			// if this entity doesn't have any particles attached, set the standard blood particle
			m_onEatenParticle.name = "PS_Blood_Explosion_Small";
			m_onEatenParticle.path = "Blood/";
			m_onEatenParticle.offset = Vector3.back * 10f;
		}
		// Preload particle
		if (m_onEatenParticle.IsValid()) {
			ParticleManager.CreatePool(m_onEatenParticle.name, m_onEatenParticle.path);
		}

		if (m_onEatenParticle.name.ToLower().Contains("blood")) {
			m_useFrozenParticle = true;
			// only create if on eaten particle is blood
			if ( !m_onEatenFrozenParticle.IsValid())
			{
				m_onEatenFrozenParticle.name = "PS_IceExplosion";
				m_onEatenFrozenParticle.path = "";
			}
			ParticleManager.CreatePool(m_onEatenFrozenParticle.name, m_onEatenFrozenParticle.path);
		}
		else
		{
			m_useFrozenParticle = false;
		}

		m_specialAnimations = new bool[(int)SpecialAnims.Count];

		if (m_burnParticle.IsValid()) {
			ParticleManager.CreatePool(m_burnParticle);
		}

		if (m_explosionParticles.IsValid()) {
			ParticleManager.CreatePool(m_explosionParticles);
		}

		ParticleManager.CreatePool("PS_EntityPCTrail", "Rewards");

        Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
    }

	void Start() {
		if (m_animator != null)
		{ 
			StartEndMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndMachineBehaviour>();
			for( int i = 0; i<behaviours.Length; i++ ){
				behaviours[i].onStart += onStartAnim;
				behaviours[i].onEnd += onEndAnim;
			}
		}

    }

	protected virtual void animEventsOnAttackStart() {
		if ( !string.IsNullOrEmpty( m_onAttackAudio ) )
			m_onAttackAudioAO = AudioController.Play( m_onAttackAudio, transform );
	}

	protected virtual void animEventsOnAttackEnd() {
		if ( m_onAttackAudioAO != null && m_onAttackAudioAO.IsPlaying() && m_onAttackAudioAO.audioItem.Loop != AudioItem.LoopMode.DoNotLoop )
			m_onAttackAudioAO.Stop();
	}

	protected virtual void animEventsOnAttackDealDamage(){}


	void onStartAnim( int stateNameHash ){
		if ( stateNameHash == ATTACK_HASH )
		{
			animEventsOnAttackStart();
		}
	}

	void onEndAnim( int stateNameHash ){
		if ( stateNameHash == ATTACK_HASH )
		{
			animEventsOnAttackEnd();
		}
	}
	//

	public virtual void Spawn(ISpawner _spawner) {
		m_boost = false;
		m_scared = false;
		m_panic = false;
		m_attack = false;
		m_falling = false;
		m_swim = false;
		m_inSpace = false;
		m_moving = false;
		m_attackingTarget = false;

		m_isExclamationMarkOn = false;

		if (m_animator != null) {
			m_animator.enabled = true;
			m_animator.speed = 1f;
		}

		if (m_entity != null) {
			Material altMaterial = null;

            if (m_skins.Count > 0) {				
				for (int i = 0; i < m_skins.Count; i++) {
					float rnd = UnityEngine.Random.Range(0f, 100f);
					if (rnd < m_skins[i].m_chance) {
						altMaterial = m_skins[i].skin;
						break;
					}
				}
			}

			// Restore materials
			Renderer[] renderers = GetComponentsInChildren<Renderer>();
			for (int i = 0; i < renderers.Length; i++) {
				if (altMaterial != null) {				
					Material[] materials = renderers[i].materials;
					for (int m = 0; m < materials.Length; m++) {
						if (!materials[m].shader.name.EndsWith("Additive"))
							materials[m] = altMaterial;
					}
					renderers[i].materials = materials;
				} else {
					if (m_materials.ContainsKey(renderers[i].GetInstanceID()))
						renderers[i].materials = m_materials[renderers[i].GetInstanceID()];
				}
			}

			// Show PC Trail?
			if (m_entity.isPC) {
				// Get an effect instance from the pool
				m_pcTrail = ParticleManager.Spawn("PS_EntityPCTrail", Vector3.zero, "Rewards");
				// Put it in the view's hierarchy so it follows the entity
				if (m_pcTrail != null) {
					m_pcTrail.transform.SetParent(transform);
					m_pcTrail.transform.localPosition = Vector3.zero;
				}
			}

			if (!string.IsNullOrEmpty(m_idleAudio))	{
				m_idleAudioAO = AudioController.Play( m_idleAudio, transform);
			}
		}

		DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
		CheckTint(IsEntityGolden(), dragonBreath.IsFuryOn(), dragonBreath.type);

		m_dragonBoost = InstanceManager.player.dragonBoostBehaviour;
    }

    /*
        void OnEnable()
        {
            Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
        }
    */
    void OnDestroy() {
        Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
		RemoveAudios();
    }

    public virtual void PreDisable() {
		if (m_pcTrail != null) {
			ParticleManager.ReturnInstance(m_pcTrail);
			m_pcTrail = null;
		}

		RemoveAudios();
    }

    private void RemoveAudios()
    {
		if ( ApplicationManager.IsAlive )
    	{
	        if ( m_idleAudioAO != null && m_idleAudioAO.IsPlaying() )
				m_idleAudioAO.Stop();

				// Return parented audio objects if needed
			RemoveAudioParent( m_idleAudioAO );
			RemoveAudioParent( m_onAttackAudioAO );

			RemoveAudioParent( m_onEatenAudioAO );
			RemoveAudioParent( m_onScaredAudioAO );
			RemoveAudioParent( m_onPanicAudioAO );
		}
    }

	void RemoveAudioParent(AudioObject ao)
	{
		if ( ao != null && ao.transform.parent == transform )
		{
			ao.transform.parent = null;		
		}
	}

    void SetEntityTint(EntityTint value)
    {
    	m_entityTint = value;
        if (m_allMaterials != null) {
            int i;
            int count = m_allMaterials.Count;
            for (i = 0; i < count; i++) {
                if (m_allMaterials[i] != null) {
                	switch( value )
                	{
                		case EntityTint.GOLD:
                		{
							m_allMaterials[i].SetColor("_FresnelColor", GOLD_TINT);
                		}break;
                		case EntityTint.FREEZE:
                		{
							m_allMaterials[i].SetColor("_FresnelColor", FREEZE_TINT * m_freezingLevel);
                		}break;
                		case EntityTint.NORMAL:
                		{
							m_allMaterials[i].SetColor("_FresnelColor", m_defaultTints[i]);
                		}break;
                	}
                }
            }
        }       
    }

    void OnFuryToggled(bool _active, DragonBreathBehaviour.Type _type)
    {
		CheckTint(IsEntityGolden(), _active, _type);
    }

    /// <summary>
    /// Determines whether this instance is burnable by player.
    /// </summary>
    /// <returns><c>true</c> if this instance is burnable by player; otherwise, <c>false</c>.</returns>
    private bool IsBurnableByPlayer( DragonBreathBehaviour.Type _fireType )
    {
        if ( m_entity != null )
        {
			switch( _fireType )
	    	{
	    		case DragonBreathBehaviour.Type.Super:
	    			return m_entity.IsBurnable();
				case DragonBreathBehaviour.Type.Standard:
				default:
					return m_entity.IsBurnable( InstanceManager.player.data.tier );
				
	    	}
	    }
    	return false;
    }


	void CheckTint(bool _isGolden, bool _furyActive = false, DragonBreathBehaviour.Type _type = DragonBreathBehaviour.Type.None )
    {
    	EntityTint _tint = ViewControl.EntityTint.NORMAL;
		if ( _isGolden || _furyActive )
    	{
			if ( IsBurnableByPlayer(_type) )
			{	
				_tint = EntityTint.GOLD;
			}
    	}
    	else
    	{
    		if ( m_freezingLevel > 0 )	
    		{
    			_tint = EntityTint.FREEZE;
    		}
    	}

    	if ( _tint != m_entityTint )
    	{
    		SetEntityTint( _tint);
    	}
    }

    public virtual void CustomUpdate() {
		if (m_animator != null) {
			if (m_disableAnimatorTimer > 0) {
				m_disableAnimatorTimer -= Time.deltaTime;
				if (m_disableAnimatorTimer <= 0) {
					m_animator.enabled = false;
				}
			}

			if (m_animator.enabled) {
				if (m_hasNavigationLayer) {
					m_currentBlendX = Util.MoveTowardsWithDamping(m_currentBlendX, m_desiredBlendX, 3f * Time.deltaTime, 0.2f);
					m_animator.SetFloat("direction X", m_currentBlendX);

					m_currentBlendY = Util.MoveTowardsWithDamping(m_currentBlendY, m_desiredBlendY, 3f * Time.deltaTime, 0.2f);
					m_animator.SetFloat("direction Y", m_currentBlendY);
				}

				m_animator.SetBool("swim", m_swim);
				m_animator.SetBool("fly down", m_inSpace);
				if (!m_swim){
					m_animator.SetBool("move", m_moving);
				} else {
					m_animator.SetBool("move", false);
				}
			}
		}


        if (m_freezingLevel > 0)
        {
			m_wasFreezing = true;
            SetEntityTint(EntityTint.FREEZE);
        }
        else if ( m_wasFreezing )
        {
			DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
			CheckTint(IsEntityGolden(), dragonBreath.IsFuryOn(), dragonBreath.type);
        	m_wasFreezing = false;
        }
    }

    bool IsEntityGolden()
    {
    	if ( m_entity != null )
    		return m_entity.isGolden;
    	return false;
    }

	// Queries
	public bool HasCorpseAsset() {
		return !string.IsNullOrEmpty(m_corpseAsset);
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
				m_exclamationMarkOn = PoolManager.GetInstance("PF_ExclamationMark");
				FollowTransform ft = m_exclamationMarkOn.GetComponent<FollowTransform>();
				ft.m_follow = m_exclamationTransform;
			} else {
				PoolManager.ReturnInstance(m_exclamationMarkOn);
				m_exclamationMarkOn = null;
			}

			m_isExclamationMarkOn = _value;
		}
	}


	//Particles
	public void SpawnEatenParticlesAt(Transform _transform) {
        if (FeatureSettingsManager.IsDebugEnabled) {
            // If the debug settings for particles eaten is disabled then they are not spawned
            if (!Prefs.GetBoolPlayer(DebugSettings.INGAME_PARTICLES_EATEN, true))
                return;
        }        

		if ( m_freezingLevel > 0.1f && m_useFrozenParticle && m_onEatenFrozenParticle.IsValid())
		{
			GameObject go = ParticleManager.Spawn( m_onEatenFrozenParticle, transform.position + m_onEatenFrozenParticle.offset);
			if (go != null)	{
				FollowTransform ft = go.GetComponent<FollowTransform>();
				if (ft != null) {
					ft.m_follow = _transform;
					ft.m_offset = m_onEatenFrozenParticle.offset;
				}
			}
		}
		else if ( m_onEatenParticle.IsValid() )
		{
			GameObject go = ParticleManager.Spawn( m_onEatenParticle, transform.position + m_onEatenParticle.offset);
			if (go != null)	{
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
			Vector3 localDir = transform.InverseTransformDirection(_dir);	// todo: replace with direction to target if trying to bite, or during bite?
			m_desiredBlendX = Mathf.Clamp(localDir.x * 3f, -1f, 1f);	// max X bend is about 30 degrees, so *3
			m_desiredBlendY = Mathf.Clamp(localDir.y * 2f, -1f, 1f);	// max Y bend is about 45 degrees, so *2.
		}
	}

	public void RotationLayer(ref Quaternion _from, ref Quaternion _to) {
		if (m_hasRotationLayer) {
			float angle = Quaternion.Angle(_from, _to);
			m_animator.SetBool("rotate left", angle < 0);
			m_animator.SetBool("rotate right", angle > 0);
		}
	}

	public void Aim(float _blendFactor) {
		m_animator.SetFloat("aim", _blendFactor);
	}

	public void Height(float _height) {
		m_animator.SetFloat("height", _height);
	}

	public void Move(float _speed) {
		if (m_panic || m_falling) {
			m_animator.speed = 1f;
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

			m_animator.SetFloat("speed", blendFactor);
			m_moving = true;
			m_animator.speed = Mathf.Lerp(m_animator.speed, animSpeedFactor, Time.deltaTime * 2f);
		} else {
			m_moving = false;
			m_animator.speed = Mathf.Lerp(m_animator.speed, 1f, Time.deltaTime * 2f);
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
			if ( !string.IsNullOrEmpty(m_onScaredAudio) )
			{
				m_onScaredAudioAO = AudioController.Play(m_onScaredAudio, transform);
			}
			m_animator.SetBool("scared", _scared);
		}
	}

	public void Panic(bool _panic, bool _burning) {
		if (m_panic != _panic) {
			m_panic = _panic;

			if (_burning) {
				// lets buuurn!!!
				// will we have a special animation when burning?
				m_animator.speed = 0f;
			} else {
				if ( !string.IsNullOrEmpty(m_onPanicAudio) )
					m_onPanicAudioAO = AudioController.Play( m_onPanicAudio, transform);
				m_animator.SetBool("holded", _panic);
			}
		}
	}

	public void Falling(bool _falling) {
		if (m_falling != _falling) {
			m_falling = _falling;
			m_animator.speed = 1f;
			m_animator.SetBool("falling", _falling);
		}
	}

	public void Jumping(bool _jumping) {
		if (m_jumping != _jumping) {
			m_jumping = _jumping;
			m_animator.speed = 1f;
			m_animator.SetBool("jump", _jumping);
		}
	}
		
	public void Attack(bool _melee, bool _ranged) {
		if (m_panic)
			return;
		
		if (!m_attack) {
			m_attack = true;
			m_animator.SetBool("attack", true);
			m_animator.SetBool("melee",  _melee);
			m_animator.SetBool("ranged", _ranged);
		}
	}

	public void StopAttack() {
		if (m_panic)
			return;
		
		if (m_attack) {
			m_attack = false;
			m_animator.SetBool("attack", false);
			m_animator.SetBool("melee",  false);
			m_animator.SetBool("ranged", false);
		}
	}

	public void StartAttackTarget() {
		m_attackingTarget = true;
		m_animator.SetBool("eat", true);
	}

	public void StopAttackTarget() {
		m_attackingTarget = false;
		m_animator.SetBool("eat", false);
	}

	public void StartEating() {
		m_animator.SetBool("eat", true);
	}

	public void StopEating() {
		if (!m_attackingTarget)
			m_animator.SetBool("eat", false);
	}

	public void Impact() {
		m_animator.SetTrigger("impact");
	}

	public void EnterWater(Collider _other, Vector3 impulse) {
		CreateSplash( _other, Mathf.Abs(impulse.y) );
	}

	public void ExitWater( Collider _other, Vector3 impulse) {
		CreateSplash( _other, Mathf.Abs(impulse.y));
	}

	private void CreateSplash(Collider _other, float verticalImpulse) {
		if (verticalImpulse >= m_speedToWaterSplash && !string.IsNullOrEmpty(m_waterSplashParticle.name)) {
			Vector3 pos = transform.position;
			float waterY =  _other.bounds.center.y + _other.bounds.extents.y;
			pos.y = waterY;
			ParticleManager.Spawn(m_waterSplashParticle.name, transform.position + m_waterSplashParticle.offset, m_waterSplashParticle.path);
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
			switch(_anim) {
				case SpecialAnims.A: m_animator.SetBool(m_animA, _value); break;
				case SpecialAnims.B: m_animator.SetBool(m_animB, _value); break;
				case SpecialAnims.C: m_animator.SetBool(m_animC, _value); break;
			}

			if (_value) OnSpecialAnimationEnter(_anim);
			else 		OnSpecialAnimationExit(_anim);
		}
		m_specialAnimations[(int)_anim] = _value;
	}

	protected virtual void OnSpecialAnimationEnter(SpecialAnims _anim) {}
	protected virtual void OnSpecialAnimationExit(SpecialAnims _anim) {}

	public void Die(bool _eaten = false, bool _burned = false) {
		ShowExclamationMark(false);

		if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying()) {
			m_idleAudioAO.Stop();
		}

		if (!_eaten) {
			if (m_explosionParticles.IsValid()) {
				ParticleManager.Spawn(m_explosionParticles, transform.position + m_explosionParticles.offset);
			}

			if (!string.IsNullOrEmpty(m_onExplosionAudio))
				AudioController.Play(m_onExplosionAudio, transform.position);
		}

		if (!_burned) {
			if (!string.IsNullOrEmpty(m_corpseAsset)) {
				// spawn corpse
				GameObject corpse = PoolManager.GetInstance(m_corpseAsset, true);
				corpse.transform.CopyFrom(transform);

				corpse.GetComponent<Corpse>().Spawn(IsEntityGolden(), m_dragonBoost.IsBoostActive());
			}
		}

		// Stop pc trail effect (if any)
		if (m_pcTrail != null) {
			ParticleManager.ReturnInstance(m_pcTrail);
			m_pcTrail = null;
		}
	}

	/// <summary>
	/// Bite this instance. When someone starts eating this view
	/// </summary>
	public virtual void Bite( Transform _transform )
	{
	}

	public void BeginSwallowed( Transform _transform )
	{
		OnEatenEvent( _transform );
	}

	public void OnEatenEvent( Transform _transform )
	{
		if (m_entity.isOnScreen && !string.IsNullOrEmpty(m_onEatenAudio)) {
			m_onEatenAudioAO = AudioController.Play(m_onEatenAudio, transform);
		}
		SpawnEatenParticlesAt( _transform );
	}

	public void Burn(float _burnAnimSeconds) {
		if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying())
			m_idleAudioAO.Stop();

		if (!m_explosionParticles.IsValid()) {
			if (m_burnParticle.IsValid()) {
				if (m_firePoints != null && m_firePoints.Length > 0) {
					for (int i = 0; i < m_firePoints.Length; i++) {
						SpawnBurnParticle(m_firePoints[i].position, 3f + _burnAnimSeconds);
					}
				} else {
					SpawnBurnParticle(transform.position, 3f + _burnAnimSeconds);
				}
			}
		}

		if (!string.IsNullOrEmpty(m_onBurnAudio)) {
			AudioController.Play(m_onBurnAudio, transform.position);
		}

		if (m_animator != null) {
			m_animator.speed = 1f;
			m_animator.SetTrigger("burn");
			m_disableAnimatorTimer = _burnAnimSeconds;
		} else {
			m_disableAnimatorTimer = 0f;
		}
	}

	private void SpawnBurnParticle(Vector3 _at, float _disableInSeconds) {
		GameObject go = ParticleManager.Spawn(m_burnParticle, _at + m_burnParticle.offset);
		if (go != null) {
			DisableInSeconds dis = go.GetComponent<DisableInSeconds>();
			dis.activeTime = _disableInSeconds;
		}
	}

	public void Freezing( float freezeLevel )
	{
		m_freezingLevel = freezeLevel;
	}

}
