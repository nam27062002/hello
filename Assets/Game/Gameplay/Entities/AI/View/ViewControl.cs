using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ViewControl : MonoBehaviour, IViewControl, ISpawnable {

	private static Material sm_goldenMaterial = null;

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

	public enum MaterialType
	{
		NORMAL,
		GOLD,
		FREEZE,
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
	[SerializeField] private ParticleData m_explosionParticles; // this will explode when burning
	[SerializeField] private string m_onExplosionAudio;

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
	protected Entity m_entity;
	protected Animator m_animator;
	protected float m_disableAnimatorTimer;

	private Renderer[] m_renderers;
	private Dictionary<int, List<Material>> m_materials;
	private Dictionary<int, List<Material>> m_materialsFrozen;
	private List<Material> m_materialList;


	private int m_vertexCount;
	public int vertexCount { get { return m_vertexCount; } }

	private int m_rendererCount;
	public int rendererCount { get { return m_rendererCount; } }

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

	private bool[] m_specialAnimations;

	protected PreyAnimationEvents m_animEvents;

	private static int ATTACK_HASH = Animator.StringToHash("Attack");
    // private const int ATTACK_HASH = Animator.StringToHash("Attack");

    //Dragon breath detection
	private DragonBoostBehaviour m_dragonBoost;

	private MaterialType m_materialType = MaterialType.NONE;

	private Transform[] m_fireParticles;
	private Transform[] m_fireParticlesParents;

	//
    private float m_freezingLevel = 0;
    private bool m_wasFreezing = true;

    private ParticleData m_stunParticle;
    private GameObject m_stunParticleInstance;

    //-----------------------------------------------
    // Use this for initialization
    //-----------------------------------------------
    protected virtual void Awake() {
		//----------------------------
		if (sm_goldenMaterial == null) sm_goldenMaterial = new Material(Resources.Load("Game/Materials/NPC_Golden") as Material);
		//---------------------------- 

		m_entity = GetComponent<Entity>();
		m_animator = transform.FindComponentRecursive<Animator>();
		if (m_animator != null)
			m_animator.logWarnings = false;

		m_animEvents = transform.FindComponentRecursive<PreyAnimationEvents>();
		if (m_animEvents != null) {
			m_animEvents.onAttackStart += animEventsOnAttackStart;
			m_animEvents.onAttackEnd += animEventsOnAttackEnd;
			m_animEvents.onAttackDealDamage += animEventsOnAttackDealDamage;
			m_animEvents.onHitEnd += OnHitAnimEnd;
		}

        // keep the original materials, sometimes it will become Gold!
        m_materials = new Dictionary<int, List<Material>>();
		m_materialsFrozen = new Dictionary<int, List<Material>>();
		m_materialList = new List<Material>();
		m_renderers = GetComponentsInChildren<Renderer>();
        
		m_vertexCount = 0;
		m_rendererCount = 0;

		if (m_renderers != null) {
			m_rendererCount = m_renderers.Length;
                        
			for (int i = 0; i < m_rendererCount; i++) {
				Renderer renderer = m_renderers[i];

				// Keep the vertex count (for DEBUG)
				if (renderer.GetType() == typeof(SkinnedMeshRenderer)) {
					m_vertexCount += (renderer as SkinnedMeshRenderer).sharedMesh.vertexCount;
				} else if (renderer.GetType() == typeof(MeshRenderer)) {
					MeshFilter filter = renderer.GetComponent<MeshFilter>();
					if (filter != null) {
						m_vertexCount += filter.sharedMesh.vertexCount;
					}
				}

				Material[] materials = renderer.sharedMaterials;

				// Stores the materials of this renderer in a dictionary for direct access//
				int renderID = renderer.GetInstanceID();
				m_materials[renderID] = new List<Material>();
				m_materialsFrozen[renderID] = new List<Material>();

				for (int m = 0; m < materials.Length; ++m) {
					Material mat = materials[m];
					if (m_showDamageFeedback) mat = new Material(materials[m]);

					m_materialList.Add(mat);
					m_materials[renderID].Add(mat);
					m_materialsFrozen[renderID].Add(FrozenMaterialManager.GetFrozenMaterialFor(mat));

					materials[m] = null; // remove all materials to avoid instantiation.
				}
				renderer.sharedMaterials = materials;
            }
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

		m_specialAnimations = new bool[(int)SpecialAnims.Count];

		m_fireParticles = new Transform[Mathf.Max(1, m_firePoints.Length)];
		m_fireParticlesParents = new Transform[m_fireParticles.Length];

        Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
        if (m_stunParticle == null)
        {
        	m_stunParticle = new ParticleData("PS_Stun","",Vector3.one);
        }
		m_stunParticle.CreatePool();

    }

	void Start() {
		if (m_animator != null) { 
			StartEndMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndMachineBehaviour>();
			for( int i = 0; i<behaviours.Length; i++ ){
				behaviours[i].onStart += onStartAnim;
				behaviours[i].onEnd += onEndAnim;
			}
		}
    }

	protected virtual void animEventsOnAttackStart() {
		if (!string.IsNullOrEmpty(m_onAttackAudio))
			m_onAttackAudioAO = AudioController.Play( m_onAttackAudio, transform );
	}

	protected virtual void animEventsOnAttackEnd() {
		if ( m_onAttackAudioAO != null && m_onAttackAudioAO.IsPlaying() && m_onAttackAudioAO.audioItem.Loop != AudioItem.LoopMode.DoNotLoop )
			m_onAttackAudioAO.Stop();
	}

	protected virtual void animEventsOnAttackDealDamage(){
		if (!string.IsNullOrEmpty(m_onAttackDealDamageAudio)){
			m_onAttackDealDamageAudioAO = AudioController.Play( m_onAttackDealDamageAudio, transform );
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

	public virtual void Spawn(ISpawner _spawner) {
		m_boost = false;
		m_scared = false;
		m_panic = false;
		m_upsideDown = false;
		m_falling = false;
		m_jumping = false;
		m_attack = false;
		m_swim = false;
		m_inSpace = false;
		m_moving = false;
		m_attackingTarget = false;
		m_hitAnimOn = false;
		m_isExclamationMarkOn = false;

		m_aim = 0f;
		m_damageFeedbackTimer = 0f;

		m_disableAnimatorTimer = 0f;
		if (m_animator != null) {
			m_animator.enabled = true;
			m_animator.speed = 1f;
		}

		if (m_entity != null) {			
			if (!string.IsNullOrEmpty(m_idleAudio))	{
				m_idleAudioAO = AudioController.Play( m_idleAudio, transform);
			}
		}

		DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
		SetMaterialType(GetMaterialType(IsEntityGolden(), dragonBreath.IsFuryOn(), dragonBreath.type));

		if (m_showDamageFeedback) {
			for (int i = 0; i < m_materialList.Count; ++i)	
				m_materialList[i].DisableKeyword("TINT");
		}

		m_dragonBoost = InstanceManager.player.dragonBoostBehaviour;
    }

	void OnDestroy() {
        Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
		RemoveAudios();
    }

    public virtual void PreDisable() {
		for (int i = 0; i < m_fireParticles.Length; ++i) {
			if (m_fireParticles[i] != null) {
				if (((m_firePoints.Length == 0) && (m_fireParticles[i].parent == transform)) ||
					((m_firePoints.Length > 0) && (m_fireParticles[i].parent == m_firePoints[i]))) {

					m_fireParticles[i].SetParent(m_fireParticlesParents[i], true);
				}
				m_fireParticles[i] = null;
			}
		}
		if ( m_stunParticleInstance )
		{
			m_stunParticle.ReturnInstance( m_stunParticleInstance );
			m_stunParticleInstance = null;
		}
		RemoveAudios();
    }

    protected virtual void RemoveAudios() {
		if (ApplicationManager.IsAlive) {
	        if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying())
				m_idleAudioAO.Stop();

			// Return parented audio objects if needed
			RemoveAudioParent( m_idleAudioAO );
			RemoveAudioParent( m_onAttackAudioAO );
			RemoveAudioParent( m_onAttackDealDamageAudioAO );
			RemoveAudioParent( m_onEatenAudioAO );

			RemoveAudioParent( m_onScaredAudioAO );
			RemoveAudioParent( m_onPanicAudioAO );
		}
    }

	protected void RemoveAudioParent(AudioObject ao)
	{
		if ( ao != null && ao.transform.parent == transform )
		{
			ao.transform.parent = null;		
		}
	}

    public void SetMaterialType(MaterialType _type) {
		if (m_isPlaceHolder) {
			_type = MaterialType.NORMAL;
		}
		m_materialType = _type;
        
		// Restore materials
		for (int i = 0; i < m_renderers.Length; i++) {
			int id = m_renderers[i].GetInstanceID();
			Material[] materials = m_renderers[i].sharedMaterials;
			for (int m = 0; m < materials.Length; m++) {
				switch (_type) {
					case MaterialType.GOLD: 	materials[m] = sm_goldenMaterial;  		 break;
					case MaterialType.FREEZE:	materials[m] = m_materialsFrozen[id][m]; break;						
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
						}	break;
				}
			}
			m_renderers[i].sharedMaterials = materials;
		}
    }

	protected void RefreshMaterial() {
		SetMaterialType(m_materialType);
	}

    void OnFuryToggled(bool _active, DragonBreathBehaviour.Type _type) {
		CheckMaterialType(IsEntityGolden(), _active, _type);
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
	    		case DragonBreathBehaviour.Type.Mega:
	    			return m_entity.IsBurnable();
				case DragonBreathBehaviour.Type.Standard:
				default:
					return m_entity.IsBurnable( InstanceManager.player.data.tier );
				
	    	}
	    }
    	return false;
    }

	MaterialType GetMaterialType(bool _isGolden, bool _furyActive = false, DragonBreathBehaviour.Type _type = DragonBreathBehaviour.Type.None) {
		MaterialType matType = ViewControl.MaterialType.NORMAL;
		if (_isGolden || _furyActive) {
			if (IsBurnableByPlayer(_type)) {	
				matType = MaterialType.GOLD;
			}
		} else {
			if (m_freezingLevel > 0) {
				matType = MaterialType.FREEZE;
			}
		}
		return matType;
	}

	void CheckMaterialType(bool _isGolden, bool _furyActive = false, DragonBreathBehaviour.Type _type = DragonBreathBehaviour.Type.None) {
		MaterialType matType = GetMaterialType(_isGolden, _furyActive, _type);
		if (matType != m_materialType) {
			SetMaterialType(matType);
    	}
    }

    public virtual void CustomUpdate() {
		if (m_animator != null) {
			if (m_disableAnimatorTimer > 0) {
				m_disableAnimatorTimer -= Time.deltaTime;
				if (m_disableAnimatorTimer < 0) {
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

        if (m_freezingLevel > 0) {
			m_wasFreezing = true;
            SetMaterialType(MaterialType.FREEZE);
        } else if (m_wasFreezing) {
			DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
			CheckMaterialType(IsEntityGolden(), dragonBreath.IsFuryOn(), dragonBreath.type);
        	m_wasFreezing = false;
        }

		if (m_damageFeedbackTimer > 0f) {
			m_damageFeedbackTimer -= Time.deltaTime;

			if (m_damageFeedbackTimer <= 0f) {
				for (int i = 0; i < m_materialList.Count; ++i)	
					m_materialList[i].DisableKeyword("TINT");
			}

			Color damageColor = m_damageColor * (m_damageFeedbackTimer / m_damageTime) + Color.white * (1f - (m_damageFeedbackTimer / m_damageTime));
			SetColorAdd(damageColor);
		}
    }

	void SetColorAdd(Color _c) {
		_c.a = 0;
		for (int i = 0; i < m_materialList.Count; ++i)	
			m_materialList[i].SetColor("_Tint", _c);
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
            if (!Prefs.GetBoolPlayer(DebugSettings.INGAME_PARTICLES_EATEN, true))
                return;
        }        

		if ( m_freezingLevel > 0.1f && m_useFrozenParticle)
		{
			GameObject go = m_onEatenFrozenParticle.Spawn(transform.position + m_onEatenFrozenParticle.offset);
			if (go != null)	{
				FollowTransform ft = go.GetComponent<FollowTransform>();
				if (ft != null) {
					ft.m_follow = _transform;
					ft.m_offset = m_onEatenFrozenParticle.offset;
				}
			}
		}
		else 
		{
			GameObject go = m_onEatenParticle.Spawn(transform.position + m_onEatenParticle.offset);
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
		if (m_hasRotationLayer && m_animator != null) {
			float angle = Quaternion.Angle(_from, _to);
			m_animator.SetBool("rotate left", angle < 0);
			m_animator.SetBool("rotate right", angle > 0);
		}
	}

	public void Aim(float _blendFactor) {
		m_aim = _blendFactor;
		if (m_animator != null)
			m_animator.SetFloat("aim", _blendFactor);
	}

	public void Height(float _height) {
		if (m_animator != null)
			m_animator.SetFloat("height", _height);
	}

	public void Move(float _speed) {
		if (m_animator != null) {
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

				m_moving = true;

				m_animator.SetFloat("speed", blendFactor);
				m_animator.speed = Mathf.Lerp(m_animator.speed, animSpeedFactor, Time.deltaTime * 2f);
			} else {
				m_moving = false;
				m_animator.speed = Mathf.Lerp(m_animator.speed, 1f, Time.deltaTime * 2f);
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
			if ( !string.IsNullOrEmpty(m_onScaredAudio) )
			{
				m_onScaredAudioAO = AudioController.Play(m_onScaredAudio, transform);
			}
			if (m_animator != null)
				m_animator.SetBool("scared", _scared);
		}
	}

	public void UpsideDown(bool _upsideDown) {
		if (m_upsideDown != _upsideDown) {
			m_upsideDown = _upsideDown;
			m_animator.SetBool("upside down", _upsideDown);
		}
	}

	public void Panic(bool _panic, bool _burning) {
		if (m_panic != _panic) {
			m_panic = _panic;

			if (_burning) {
				// lets buuurn!!!
				// will we have a special animation when burning?
				if (m_animator != null)
					m_animator.speed = 0f;
			} else {
				if ( !string.IsNullOrEmpty(m_onPanicAudio) )
					m_onPanicAudioAO = AudioController.Play( m_onPanicAudio, transform);
				if (m_animator != null)
					m_animator.SetBool("holded", _panic);
			}
		}
	}

	public void Hit() {
		m_hitAnimOn = true;

		if (m_animator != null)
			m_animator.SetTrigger("hit");
		
		if (m_showDamageFeedback) {
			m_damageFeedbackTimer = m_damageTime;

			for (int i = 0; i < m_materialList.Count; ++i)	
				m_materialList[i].EnableKeyword("TINT");
		}
	}

	public void Falling(bool _falling) {
		if (m_falling != _falling) {
			m_falling = _falling;
			if (m_animator != null) {
				m_animator.speed = 1f;
				m_animator.SetBool("falling", _falling);
			}
		}
	}

	public void Jumping(bool _jumping) {
		if (m_jumping != _jumping) {
			m_jumping = _jumping;
			if (m_animator != null) {
				m_animator.speed = 1f;
				m_animator.SetBool("jump", _jumping);
			}
		}
	}
		
	public void Attack(bool _melee, bool _ranged) {
		if (m_panic)
			return;
		
		if (!m_attack) {
			m_attack = true;
			if (m_animator != null) {
				m_animator.SetBool("attack", true);
				m_animator.SetBool("melee",  _melee);
				m_animator.SetBool("ranged", _ranged);
			}
		}
	}

	public void StopAttack() {
		if (m_panic)
			return;
		
		if (m_attack) {
			m_attack = false;
			if (m_animator != null) {
				m_animator.SetBool("attack", false);
				m_animator.SetBool("melee",  false);
				m_animator.SetBool("ranged", false);
			}
		}
	}

	public void StartAttackTarget() {
		m_attackingTarget = true;
		if (m_animator != null)
			m_animator.SetBool("eat", true);
	}

	public void StopAttackTarget() {
		m_attackingTarget = false;
		if (m_animator != null)
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
		if (m_animator != null)
			m_animator.SetTrigger("impact");

		if (m_showDamageFeedback) {
			m_damageFeedbackTimer = m_damageTime;
			for (int i = 0; i < m_materialList.Count; ++i)	
				m_materialList[i].EnableKeyword("TINT");
		}
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
			m_waterSplashParticle.Spawn(transform.position + m_waterSplashParticle.offset);
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
			PlayExplosion();
		}

		if (!_burned) {
			if (m_corpseHandler != null) {
				// spawn corpse
				GameObject corpse = m_corpseHandler.Spawn(null);
				if (corpse != null) {
					corpse.transform.CopyFrom(transform);
					corpse.GetComponent<Corpse>().Spawn(IsEntityGolden(), m_dragonBoost.IsBoostActive());
				}
			}
		}
	}

	public void PlayExplosion()
	{
		m_explosionParticles.Spawn(transform.position + m_explosionParticles.offset);

		if (!string.IsNullOrEmpty(m_onExplosionAudio))
			AudioController.Play(m_onExplosionAudio, transform.position);
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
						SpawnBurnParticle(m_firePoints[i], i, _burnAnimSeconds);
					}
				} else {
					SpawnBurnParticle(transform, 0, _burnAnimSeconds);
				}
			}
		}

		if (!string.IsNullOrEmpty(m_onBurnAudio)) {
			AudioController.Play(m_onBurnAudio, transform.position);
		}

		if (m_animator != null) {
			m_animator.speed = 1f;
			m_animator.SetTrigger("burn");
			m_disableAnimatorTimer = _burnAnimSeconds + 0.1f;
		} else {
			m_disableAnimatorTimer = 0.1f;
		}
	}

	private void SpawnBurnParticle(Transform _parent, int _index, float _disableInSeconds) {
		GameObject go = m_burnParticle.Spawn();
		if (go != null) {
			Transform t = go.transform;

			m_fireParticles[_index] = t;
			m_fireParticlesParents[_index] = t.parent;

			t.SetParent(_parent, false);
			t.localPosition = m_burnParticle.offset;

			DisableInSeconds dis = go.GetComponent<DisableInSeconds>();
			dis.activeTime = _disableInSeconds;
		} else {
			m_fireParticles[_index] = null;
			m_fireParticlesParents[_index] = null;
		}
	}

	public void Freezing( float freezeLevel ){
		m_freezingLevel = freezeLevel;
	}

	public void SetStunned( bool stunned ){
		if ( stunned ){
			if (m_animator != null)
				m_animator.enabled = false;
			// if no stunned particle -> stun
			if (m_stunParticleInstance == null)
			{
				m_stunParticleInstance = m_stunParticle.Spawn(transform);
			}
		}else{
			if (m_animator != null)
				m_animator.enabled = true;
			// if stunned particle -> remove stun
			if ( m_stunParticleInstance )
			{
				m_stunParticle.ReturnInstance( m_stunParticleInstance );
				m_stunParticleInstance = null;
			}
		}
	}

}
