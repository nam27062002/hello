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
	private bool m_isBeingEaten;

	private float m_timer;
	private float m_time;

	private Quaternion m_originalRotation;
	private Vector3 m_originalScale;

	private DragonEatBehaviour m_dragon;
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

		m_prey = GetComponent<PreyStats>();
		m_dragon = InstanceManager.player.GetComponent<DragonEatBehaviour>();
		m_dragonMouth = m_dragon.GetComponent<DragonMotion>().mouth;
	}

	public override void Initialize() {

		m_isBeingEaten = false;

		transform.rotation = m_originalRotation;
		transform.localScale = m_originalScale;
	}

	void Update() {
	
		// go to mouth
		if (m_isBeingEaten) {

			if (m_timer > 0f) {
				m_timer -= Time.deltaTime;
				float t = 1f - (m_timer / m_time);

				// make it small
				Vector3 scale = Vector3.Lerp(transform.localScale, Vector3.one * 0.75f, t);
				transform.localScale = scale;
			}
		} else if (m_dragon.enabled) {
			// check distance to dragon mouth
			Vector2 v = (m_Bounds.bounds.bounds.center - m_dragonMouth.transform.position);
			float distanceSqr = v.sqrMagnitude - (m_Bounds.radius * m_Bounds.radius);
			if (distanceSqr <= m_dragon.eatDistanceSqr) {
				m_isBeingEaten = m_dragon.Eat(this);
			}
		}
	}
	
	public Reward OnSwallow(float _time) {
		// Create a copy of the base rewards and tune them
		Reward reward = m_prey.reward;
		if(!m_prey.isGolden) {
			reward.coins = 0;
		}

		// Dispatch global event
		Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_EATEN, this.transform, reward);

		// start go to mouth animation
		m_timer = m_time = _time; // amount of time the dragon needs to eat this entity

		// deactivate
		if (m_destroyOnEat) {
			Destroy(gameObject);
		} else {
			gameObject.SetActive(false);
		}

		return reward;
	}
}
