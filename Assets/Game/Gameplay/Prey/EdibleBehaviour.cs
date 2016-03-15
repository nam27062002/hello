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

	public string m_onEatenParticle = "";
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
		if ( m_onEatenSounds.Count > 0)
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
	
	public void OnSwallow() {
		// Get the reward to be given from the entity
		Reward reward = m_entity.GetOnKillReward(false);

		// Dispatch global event
		Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, this.transform, reward);

		// Particles
		if ( !string.IsNullOrEmpty(m_onEatenParticle) )
			ParticleManager.Spawn(m_onEatenParticle, transform.position);

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
