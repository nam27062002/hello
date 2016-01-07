using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PreyStats))]
public class EdibleBehaviour : Initializable {
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private DragonTier m_edibleFromTier = 0;
	public DragonTier edibleFromTier { get { return m_edibleFromTier; } }
	
	[SerializeField][Range(1, 10)] private float m_size = 1f;
	public float size { get { return m_size; } }

	[SerializeField] private bool m_isBig = false;
	public bool isBig { get { return m_isBig; } }
	
	[SerializeField] private bool m_destroyOnEat = false;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private PreyStats m_prey;
	private Animator m_animator;
	private bool m_isBeingEaten;
	public bool isBeingEaten { get { return m_isBeingEaten; } }

	private Quaternion m_originalRotation;
	private Vector3 m_originalScale;

	private DragonMotion m_dragon;
	private DragonEatBehaviour m_dragonEat;
	private Transform m_dragonMouth;
	private CircleArea2D m_Bounds;


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
		m_prey = GetComponent<PreyStats>();
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_dragonEat = m_dragon.GetComponent<DragonEatBehaviour>();
		m_dragonMouth = m_dragon.tongue;
	}

	public override void Initialize() {
		m_isBeingEaten = false;

		transform.rotation = m_originalRotation;
		transform.localScale = m_originalScale;
	}

	void Update() {	
		// go to mouth
		if (m_isBeingEaten) {
			
		} else if (m_dragonEat.enabled) {
			// check if this prey is in front of the mouth
			Vector3 heading = m_Bounds.center - m_dragonMouth.transform.position;
			float dot = Vector3.Dot(heading.normalized, m_dragon.GetDirection());

			// check distance to dragon mouth
			if (dot > 0) {
				float distanceSqr = m_Bounds.DistanceSqr( m_dragonMouth.transform.position );
				if (distanceSqr <= m_dragonEat.eatDistanceSqr) {
					m_isBeingEaten = m_dragonEat.Eat(this);
					if (m_isBeingEaten) {
						m_animator.SetTrigger("being eaten");
					}
				}
			}
		}
	}
	
	public Reward OnSwallow() {
		// Get the reward to be given from the prey stats
		Reward reward = m_prey.GetOnKillReward();

		// Dispatch global event
		Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, this.transform, reward);

		// deactivate
		if (m_destroyOnEat) {
			Destroy(gameObject);
		} else {
			gameObject.SetActive(false);
		}

		return reward;
	}
}
