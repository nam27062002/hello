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
	private PreyMotion m_motion;
	private Animator m_animator;
	private bool m_isBeingEaten;
	public bool isBeingEaten { get { return m_isBeingEaten; } }

	private Quaternion m_originalRotation;
	private Vector3 m_originalScale;

	public List<string> m_onEatenParticles = new List<string>();
	[Range(0f, 100f)]
	public float m_onEatenSoundProbability = 50.0f;
	public List<string> m_onEatenSounds = new List<string>();

	public float biteResistance { get { return m_entity.biteResistance; }}

	bool m_beingHeld = false;

	private List<Transform> m_holdPreyPoints = new List<Transform>();
	public List<Transform> holdPreyPoints
	{
		get{ return m_holdPreyPoints; }
	}

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake() 
	{
		m_originalRotation = transform.rotation;
		m_originalScale = transform.localScale;
		// Find all hold prey opints
		HoldPreyPoint[] holdPoints = transform.GetComponentsInChildren<HoldPreyPoint>();
		if ( holdPoints != null )
		{
			for( int i = 0;i<holdPoints.Length; i++ )
			{
				m_holdPreyPoints.Add( holdPoints[i].transform);
			}
		}
	}

	void Start() {
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_entity = GetComponent<Entity>();
		m_motion = GetComponent<PreyMotion>();
	}

	void Update()
	{
		if ( m_beingHeld )
		{
			m_motion.Stop();
		}
	}

	public override void Initialize() {
		m_isBeingEaten = false;
		m_beingHeld = false;
		transform.rotation = m_originalRotation;
		transform.localScale = m_originalScale;

		enabled = true;
	}

	public bool CanBeEaten(Vector3 _eaterDirection) {
		if (enabled && m_entity.isEdible) {
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

		EntityManager.instance.Unregister(GetComponent<Entity>());
	}

	public void OnEat() {
		m_isBeingEaten = true;
		OnEatBehaviours(false);

		TryOnEatSound();
		EntityManager.instance.Unregister(GetComponent<Entity>());
	}

	private void TryOnEatSound()
	{
		if ( m_onEatenSounds.Count > 0 && Random.Range(0, 100) <= m_onEatenSoundProbability)
		{
			// Play sound!
			string soundName = m_onEatenSounds[Random.Range(0, m_onEatenSounds.Count)];
			if (!string.IsNullOrEmpty(soundName))
			{
				AudioManager.instance.PlayClip( soundName );
			}
		}
	}
	
	public void OnSwallow( Transform _transform, bool tryOnEatSound = false ) 
	{
		
		if ( tryOnEatSound )
			TryOnEatSound();

		// Get the reward to be given from the entity
		Reward reward = m_entity.GetOnKillReward(false);

		// Dispatch global event
		Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, this.transform, reward);

		// Particles
		if ( m_onEatenParticles.Count <= 0 )
		{
			GameObject go = ParticleManager.Spawn("PS_Blood_Explosion_Small", transform.position + (Vector3.back * 10), "Blood/");
			if ( go != null)
			{
				FollowTransform ft = go.GetComponent<FollowTransform>();
				if ( ft != null )
				{
					ft.m_follow = _transform;
					ft.m_offset = Vector3.back * 10;
				}
			}
		}
		else
		{
			for( int i = 0; i<m_onEatenParticles.Count; i++ )
			{
				if ( !string.IsNullOrEmpty(m_onEatenParticles[i]) )
				{
					GameObject go = ParticleManager.Spawn(m_onEatenParticles[i], transform.position);
					if ( go != null )
					{
						FollowTransform ft = go.GetComponent<FollowTransform>();
						if ( ft != null )
							ft.m_follow = _transform;
					}
				}
			}
		}

		OnEatBehaviours(true);

		// deactivate
		GetComponent<SpawnBehaviour>().EatOrBurn();
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

	public void OnHoldBy( EatBehaviour holder )
	{
		m_beingHeld = true;
		m_animator.SetBool("hold", true);
		// OnEatBehaviours(false);
	}

	public void ReleaseHold()
	{
		m_beingHeld = false;
		m_animator.SetBool("hold", false);
		// OnEatBehaviours(true);
	}

	public bool IsBeingHeld()
	{
		return m_beingHeld;
	}

	public void HoldingDamage( float damage )
	{
		m_entity.Damage( damage );
	}

	public bool isDead()
	{
		return m_entity.health <= 0;
	}

}
