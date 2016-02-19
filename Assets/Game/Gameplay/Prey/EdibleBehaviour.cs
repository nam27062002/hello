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

	private Quaternion m_originalRotation;
	private Vector3 m_originalScale;

	private DragonMotion m_dragon;

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
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_prey = GetComponent<Entity>();

		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
	}

	public override void Initialize() {
		m_isBeingEaten = false;

		transform.rotation = m_originalRotation;
		transform.localScale = m_originalScale;

		enabled = true;
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
