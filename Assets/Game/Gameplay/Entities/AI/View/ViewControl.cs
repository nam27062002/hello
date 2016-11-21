using System;
using UnityEngine;
using System.Collections.Generic;

public class ViewControl : MonoBehaviour, ISpawnable {

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

	//-----------------------------------------------
	[SeparatorAttribute("Animation playback speed")]
	[SerializeField] private float m_walkSpeed = 1f;
	[SerializeField] private float m_runSpeed = 1f;
	[SerializeField] private float m_minPlaybakSpeed = 1f;
	[SerializeField] private float m_maxPlaybakSpeed = 1.5f;
	[SerializeField] private bool m_onBoostMaxPlaybackSpeed = false;


	[SeparatorAttribute("Animation blending")]
	[SerializeField] private bool m_hasNavigationLayer = false;
	[SerializeField] private bool m_hasRotationLayer = false;

	[SeparatorAttribute("Special Actions Animations")] // map a special action from the pilot to a specific animation.
	[SerializeField] private string m_animA = "";
	[SerializeField] private string m_animB = "";
	[SerializeField] private string m_animC = "";

	[SeparatorAttribute("Eaten")]
	[SerializeField] private List<ParticleData> m_onEatenParticles = new List<ParticleData>();
	[SerializeField] private string m_onEatenAudio;

	[SeparatorAttribute("Water")]
	[SerializeField] private float m_speedToWaterSplash;
	[SerializeField] private ParticleData m_waterSplashParticle;

	[SeparatorAttribute("Burn")]
	[SerializeField] private ParticleData m_burnParticle;
	[SerializeField] private string m_onBurnAudio;

	[SeparatorAttribute("Explode")]
	[SerializeField] private ParticleData m_explosionParticles; // this will explode when burning
	[SerializeField] private string m_onExplosionAudio;

	[SeparatorAttribute("More Audios")]
	[SerializeField] private string m_onAttackAudio;
	private AudioObject m_onAttackAudioAO;
	[SerializeField] private string m_onScaredAudio;
	[SerializeField] private string m_onPanicAudio;
	[SerializeField] private string m_idleAudio;
	private AudioObject m_idleAudioAO;

	[SeparatorAttribute("Skin")]
	[SerializeField] private List<SkinData> m_skins = new List<SkinData>();


	//-----------------------------------------------
	private Entity m_entity;
	private Animator m_animator;
	private Material m_materialGold;
	private Dictionary<int, Material[]> m_materials;

	private bool m_boost;
	private bool m_scared;
	private bool m_panic; //bite and hold state
	private bool m_falling;
	private bool m_attack;
	private bool m_swim;
	private bool m_inSpace;
	private bool m_moving;
	private bool m_attackingTarget;

	private float m_desiredBlendX;
	private float m_desiredBlendY;

	private float m_currentBlendX;
	private float m_currentBlendY;

	private bool[] m_specialAnimations;

	private GameObject m_pcTrail = null;

	private PreyAnimationEvents m_animEvents;

	private static int ATTACK_HASH = Animator.StringToHash("Attack");
	// private const int ATTACK_HASH = Animator.StringToHash("Attack");

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
		m_materialGold = Resources.Load<Material>("Game/Assets/Materials/Gold");

		// keep the original materials, sometimes it will become Gold!
		m_materials = new Dictionary<int, Material[]>(); 
		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			m_materials[renderers[i].GetInstanceID()] = renderers[i].materials;
		}

		// particle management
		if (m_onEatenParticles.Count <= 0) {
			// if this entity doesn't have any particles attached, set the standard blood particle
			ParticleData data = new ParticleData("PS_Blood_Explosion_Small", "Blood/", Vector3.back * 10f);
			m_onEatenParticles.Add(data);
		}

		m_specialAnimations = new bool[(int)SpecialAnims.Count];

		// Preload particles
		for( int i = 0; i < m_onEatenParticles.Count; i++) {
			ParticleData data = m_onEatenParticles[i];
			if (!string.IsNullOrEmpty(data.name)) {
				ParticleManager.CreatePool(data.name, data.path);
			}
		}

		if (m_burnParticle.IsValid()) {
			ParticleManager.CreatePool(m_burnParticle, 20);
		}

		if (m_explosionParticles.IsValid()) {
			ParticleManager.CreatePool(m_explosionParticles);
		}

		ParticleManager.CreatePool("PS_EntityPCTrail", "Rewards", 5);
	}

	void Start()
	{
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
			m_onAttackAudioAO = AudioController.Play( m_onAttackAudio, transform.position );
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

		if (m_animator != null) {
			m_animator.enabled = true;
			m_animator.speed = 1f;
		}

		if (m_entity != null) {
			Material altMaterial = null;

			if (m_entity.isGolden) {
				altMaterial = m_materialGold;
			} else if (m_skins.Count > 0) {				
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
			if(m_entity.isPC) {
				// Get an effect instance from the pool
				m_pcTrail = ParticleManager.Spawn("PS_EntityPCTrail", Vector3.zero, "Rewards");
				// Put it in the view's hierarchy so it follows the entity
				if(m_pcTrail != null) {
					m_pcTrail.transform.SetParent(transform);
					m_pcTrail.transform.localPosition = Vector3.zero;
				}
			}

			if (!string.IsNullOrEmpty(m_idleAudio))
			{
				m_idleAudioAO = AudioController.Play( m_idleAudio, transform);
			}
		}
	}

	void OnDisable() {
		if ( m_idleAudioAO != null && m_idleAudioAO.IsPlaying() )
			m_idleAudioAO.Stop();
	}

	protected virtual void Update() {
		if (m_animator != null) {
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


	// Queries
	public bool canAttack() {
		return !m_attack;
	}

	public bool hasAttackEnded() {
		return false;
	}
	//


	//Particles
	public void SpawnEatenParticlesAt(Transform _transform) {
#if !PRODUCTION
        // If the debug settings for particles eaten is disabled then they are not spawned
        if (!Prefs.GetBoolPlayer(DebugSettings.INGAME_PARTICLES_EATEN))
            return;
#endif

        for ( int i = 0; i < m_onEatenParticles.Count; i++) {
			ParticleData data = m_onEatenParticles[i];
			if (!string.IsNullOrEmpty(data.name)) {
				GameObject go = ParticleManager.Spawn(data.name, transform.position + data.offset, data.path);
				if (go != null)	{
					FollowTransform ft = go.GetComponent<FollowTransform>();
					if (ft != null) {
						ft.m_follow = _transform;
						ft.m_offset = data.offset;
					}
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
				AudioController.Play(m_onScaredAudio, transform.position);
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
					AudioController.Play( m_onPanicAudio, transform.position);
				m_animator.SetBool("hold", _panic);
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
		
	public void Attack() {
		if (m_panic)
			return;
		
		if (!m_attack) {
			m_attack = true;
			m_animator.SetBool("attack", true);
		}
	}

	public void StopAttack() {
		if (m_panic)
			return;
		
		if (m_attack) {
			m_attack = false;
			m_animator.SetBool("attack", false);
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

	public void Die(bool _eaten = false) {

		if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying()) {
			m_idleAudioAO.Stop();
		}

		if (m_explosionParticles.IsValid()) {
			ParticleManager.Spawn(m_explosionParticles, transform.position + m_explosionParticles.offset);
		}

		if (_eaten) {
			if (!string.IsNullOrEmpty(m_onEatenAudio))
				AudioController.Play(m_onEatenAudio, transform.position);
		} else {
			if (!string.IsNullOrEmpty(m_onExplosionAudio))
				AudioController.Play(m_onExplosionAudio, transform.position);
		}

		// Stop pc trail effect (if any)
		if (m_pcTrail != null) {
			ParticleManager.ReturnInstance(m_pcTrail);
			m_pcTrail = null;
		}
	}

	public void Burn() {
		if (m_idleAudioAO != null && m_idleAudioAO.IsPlaying())
			m_idleAudioAO.Stop();

		if (!m_explosionParticles.IsValid()) {
			if (m_burnParticle.IsValid()) {
				ParticleManager.Spawn(m_burnParticle, transform.position + m_burnParticle.offset);
			}
		}

		if (!string.IsNullOrEmpty(m_onBurnAudio)) {
			AudioController.Play(m_onBurnAudio, transform.position);
		}

		if (m_animator != null) {
			m_animator.enabled = false;
		}
	}
}
