using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Entity))]
public class EdibleBehaviour : Initializable {

	public enum EatenFrom {
		All = 0,
		Front,
		Back
	};

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private EatenFrom m_vulnerable = EatenFrom.All;

	private Entity m_entity;
	private MotionInterface m_motion;
	private Animator m_animator;
	private bool m_isBeingEaten;
	public bool isBeingEaten { get { return m_isBeingEaten; } }

	private Quaternion m_originalRotation;
	private Vector3 m_originalScale;

	public List<string> m_onEatenParticles = new List<string>();
	[Range(0f, 100f)]
	public float m_onEatenSoundProbability = 50.0f;
	public List<string> m_onEatenSounds = new List<string>();

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake() {
		m_originalRotation = transform.rotation;
		m_originalScale = transform.localScale;
	}

	void Start() {
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_entity = GetComponent<Entity>();
		m_motion = GetComponent<MotionInterface>();
	}

	public override void Initialize() {
		m_isBeingEaten = false;

		transform.rotation = m_originalRotation;
		transform.localScale = m_originalScale;

		enabled = true;
	}

	public bool CanBeEaten(Vector3 _eaterDirection) {
		if (enabled) {
			if (m_motion == null || m_vulnerable == EatenFrom.All) {
				return true;
			} else {
				float dot = Vector2.Dot(m_motion.direction, _eaterDirection);
				return (m_vulnerable == EatenFrom.Front && dot < -0.25f) || (m_vulnerable == EatenFrom.Back && dot > 0.25f);
			}
		}

		return false;
	}

	public void OnEatByPet() {
		m_isBeingEaten = true;
		OnEatBehaviours(false);
		m_animator.SetTrigger("being eaten");

		EntityManager.instance.Unregister(GetComponent<Entity>());
	}

	public void OnEat() {
		m_isBeingEaten = true;
		OnEatBehaviours(false);
		if ( m_animator != null )
			m_animator.SetTrigger("being eaten");
		if ( m_onEatenSounds.Count > 0 && Random.Range(0, 100) <= m_onEatenSoundProbability)
		{
			// Play sound!
			string soundName = m_onEatenSounds[ Random.Range( 0, m_onEatenSounds.Count ) ];
			if (!string.IsNullOrEmpty( soundName ))
			{
				AudioManager.instance.PlayClip( soundName );
			}
		}

		EntityManager.instance.Unregister(GetComponent<Entity>());
	}
	
	public void OnSwallow( Transform _transform ) {
		// Get the reward to be given from the entity
		Reward reward = m_entity.GetOnKillReward(false);

		// Dispatch global event
		Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, this.transform, reward);

		// Particles
		if ( m_onEatenParticles.Count <= 0 )
		{
			GameObject go = ParticleManager.Spawn("bloodchurn-large", transform.position);
					FollowTransform ft = go.GetComponent<FollowTransform>();
					if ( ft != null )
						ft.m_follow = _transform;
		}
		else
		{
			for( int i = 0; i<m_onEatenParticles.Count; i++ )
			{
				if ( !string.IsNullOrEmpty(m_onEatenParticles[i]) )
				{
					GameObject go = ParticleManager.Spawn(m_onEatenParticles[i], transform.position);
					FollowTransform ft = go.GetComponent<FollowTransform>();
					if ( ft != null )
						ft.m_follow = _transform;
				}
			}
		}

		OnEatBehaviours(true);

		// deactivate
		gameObject.SetActive(false);
	}

	void OnEatBehaviours( bool _enable)
	{
		PreyMotion pm = GetComponent<PreyMotion>();
		if ( pm != null )
			pm.enabled = _enable;

		PreyOrientation po = GetComponent<PreyOrientation>();
		if ( po != null )
			po.enabled = _enable;
	}
}
