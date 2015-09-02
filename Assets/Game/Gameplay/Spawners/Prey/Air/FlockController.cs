using UnityEngine;
using System.Collections;

public class FlockController : MonoBehaviour {

	public enum GuideFunction{
		SMALL_FLOCK,
		FAST_FLOCK,
		WANDER
	};

	//-----------------------------------------------
	// Properties
	//-----------------------------------------------v

	[SerializeField] private GameObject m_entityPrefab;
	[SerializeField] private int m_numEntities = 4;
	[SerializeField] private float m_activationRange = 5000f;
	[SerializeField] private float m_guideSpeed = 2f;
	[SerializeField] private GuideFunction m_guideFunction = GuideFunction.SMALL_FLOCK;
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	
	private Transform m_player;
	private bool m_playerInRange = false;
	private Bounds m_bounds;
	private float m_activationRangeSqr;

	// Flock control
	private Vector3 m_followPos;
	public Vector3 followPos { get { return m_followPos; } }

	private float m_timer;
	private GameObject[] m_entities;
	public GameObject[] entities { get { return m_entities; } }
	
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {

		InstanceManager.pools.CreatePool(m_entityPrefab);

		m_entities = new GameObject[m_numEntities];
		m_player = GameObject.Find ("Player").transform;

		BoxCollider2D box = GetComponent<BoxCollider2D>();
		if (box != null) {
			box.enabled = true;
			m_bounds = box.bounds;
			box.enabled = false;
		} else {
			m_bounds = new Bounds(transform.position, Vector3.one * 100f);
		}

		m_activationRange += Mathf.Max(m_bounds.extents.x, m_bounds.extents.y);
		m_activationRangeSqr = m_activationRange * m_activationRange;

		m_followPos = m_bounds.center;


	}

	void OnDestroy(){
		for (int i = 0; i < m_numEntities; i++) {			
			if (m_entities[i] != null) 
				m_entities[i].SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (m_playerInRange) {
			// Control flocking
			// Move target for follow behaviour
			m_timer += Time.deltaTime * m_guideSpeed;

			if (m_guideFunction == GuideFunction.SMALL_FLOCK) {
				m_followPos.x = m_bounds.center.x + Mathf.Sin(m_timer * 0.5f) * m_bounds.extents.x * 0.5f + Mathf.Cos(m_timer * 0.25f) * m_bounds.extents.x * 0.5f;
				m_followPos.y = m_bounds.center.y + Mathf.Sin(m_timer * 0.35f) * m_bounds.extents.y * 0.5f + Mathf.Cos(m_timer * 0.65f) * m_bounds.extents.y * 0.5f;
			} else if (m_guideFunction == GuideFunction.FAST_FLOCK) {
				m_followPos.x = m_bounds.center.x + Mathf.Sin(m_timer) * m_bounds.extents.x;
				m_followPos.y = m_bounds.center.y + Mathf.Cos(m_timer) * m_bounds.extents.y;
			} else if (m_guideFunction == GuideFunction.WANDER) {
				if (m_timer > 10f) { // seconds
					m_followPos.x = m_bounds.center.x + Random.Range(-m_bounds.extents.x, m_bounds.extents.x);
					m_followPos.y = m_bounds.center.y + Random.Range(-m_bounds.extents.y, m_bounds.extents.y);
					m_followPos.z = 0;

					m_timer = 0;
				}
			}

			// Check if those entities are active
			for (int i = 0; i < m_numEntities; i++) {
				if (m_entities[i] != null && !m_entities[i].activeInHierarchy) {
					m_entities[i].SetActive(false);
					m_entities[i] = null;
				}
			}
		}

		// Hide/respawn depending on player distance
		float d = (transform.position - m_player.position).sqrMagnitude;
		
		if (d <= m_activationRangeSqr){

			if (!m_playerInRange) {
				m_playerInRange = true;				
				for (int i = 0; i < m_numEntities; i++) {
					m_entities[i] = Instantiate();
				}
			}

		} else if (m_playerInRange) {
			
			m_playerInRange = false;			
			for (int i = 0; i < m_numEntities; i++) {
				if (m_entities[i] != null) {
					m_entities[i].SetActive (false);
					m_entities[i] = null;
				}
			}
			Messenger.Broadcast<GameObject>("SpawnOutOfRange",this.gameObject);
		}
	}
	
	private GameObject Instantiate() {
		GameObject obj = InstanceManager.pools.GetInstance(m_entityPrefab.name);
		obj.GetComponent<FlockBehaviour>().flock = this;
		obj.GetComponent<SpawnableBehaviour>().Spawn(m_bounds);
		obj.transform.position = m_followPos;
		return obj;
	}
}
