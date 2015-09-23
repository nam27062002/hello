using UnityEngine;
using System.Collections;

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


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake() {

		m_originalRotation = transform.rotation;
		m_originalScale = transform.localScale;
	}

	void Start() {
		
		m_prey = GetComponent<PreyStats>();
	}

	public override void Initialize() {

		m_isBeingEaten = false;

		transform.rotation = m_originalRotation;
		transform.localScale = m_originalScale;

		// enable colliders
		Collider[] colliders = GetComponents<Collider>();
		foreach (Collider collider in colliders) {
			collider.enabled = true;
		}
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
		}
	}
	
	public Reward Eat(float _time) {

		Reward reward = m_prey.reward;

		if (!m_prey.isGolden) {
			reward.coins = 0;

			//TODO: Drop money event?
		}

		// start go to mouth animation
		m_isBeingEaten = true;
		m_timer = m_time = _time; // amount of time the dragon needs to eat this entity
				
		// disable colliders
		Collider[] colliders = GetComponents<Collider>();
		foreach (Collider collider in colliders) {
			collider.enabled = false;
		}

		return reward;
	}

	public void OnSwallow() {

		// deactivate
		if (m_destroyOnEat) {
			DestroyObject(gameObject);
		} else {
			gameObject.SetActive(false);
		}
	}
}
