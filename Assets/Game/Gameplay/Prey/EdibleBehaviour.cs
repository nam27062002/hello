using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Entity))]
public class EdibleBehaviour : Initializable {
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private Entity m_prey;
	private Animator m_animator;
	private bool m_isBeingEaten;
	public bool isBeingEaten { get { return m_isBeingEaten; } }

	private bool m_petTarget;

	private Quaternion m_originalRotation;
	private Vector3 m_originalScale;

	private DragonMotion m_dragon;
	private DragonEatBehaviour m_dragonEat;
	private DragonPetEatBehaviour m_dragonPetEat;
	private Transform m_dragonMouth;
	private CircleArea2D m_Bounds;

	private float m_lastEatingDistance = float.MaxValue;

	public string onEatenParticle = "";


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake() {
		m_originalRotation = transform.rotation;
		m_originalScale = transform.localScale;
	}

	void Start() {
		m_Bounds = GetComponent<CircleArea2D>();

		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_prey = GetComponent<Entity>();

		m_dragonPetEat = InstanceManager.pet.GetComponent<DragonPetEatBehaviour>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_dragonEat = m_dragon.GetComponent<DragonEatBehaviour>();
		m_dragonMouth = m_dragon.tongue;
	}

	public override void Initialize() {
		m_isBeingEaten = false;
		m_petTarget = false;

		transform.rotation = m_originalRotation;
		transform.localScale = m_originalScale;

		enabled = true;
	}

	void Update() {	
		// go to mouth
		/*
		if (m_isBeingEaten) {
			
		} else {
			if (m_dragonPetEat != null) {
				if (!m_petTarget) {
					m_petTarget = m_dragonPetEat.RegisterTarget(this);
				} else {
					m_petTarget = m_dragonPetEat.UnRegisterTarget(this, false);
				}
			}

			if (m_dragonEat.enabled) {
				// check if this prey is in front of the mouth
				Vector3 heading = m_Bounds.center - m_dragonMouth.transform.position;
				float dot = Vector3.Dot(heading.normalized, m_dragon.GetDirection());

				// check distance to dragon mouth
				if (dot > 0) {
					float distanceSqr = m_Bounds.DistanceSqr( m_dragonMouth.transform.position );

					if (distanceSqr <= m_dragonEat.eatDistanceSqr) { 
						m_isBeingEaten = m_dragonEat.Eat(this);

						if (m_isBeingEaten)  {
							m_dragonPetEat.UnRegisterTarget(this, true);
							OnEatBehaviours(false);
							m_animator.SetTrigger("being eaten");
						}
						m_lastEatingDistance = float.MaxValue;
					} else {
						if (distanceSqr < m_dragonEat.eatDistanceSqr * 5) {
							if (distanceSqr > m_lastEatingDistance) {
								// I'm Escaping!!!!
								m_dragonEat.AlmostEat(this);
								m_lastEatingDistance = float.MaxValue;
							} else {
								m_lastEatingDistance = distanceSqr;
							}
						} else {
							m_lastEatingDistance = float.MaxValue;
						}
					}
				} else {
					m_lastEatingDistance = float.MaxValue;
				}
			}
		}*/
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
		m_animator.SetTrigger("being eaten");

		EntityManager.instance.Unregister(GetComponent<Entity>());
	}
	
	public void OnSwallow() {
		// Get the reward to be given from the prey stats
		Reward reward = m_prey.GetOnKillReward();

		// Dispatch global event
		Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, this.transform, reward);

		// Particles
		if ( !string.IsNullOrEmpty(onEatenParticle) )
			ParticleManager.Spawn(onEatenParticle, transform.position);

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
