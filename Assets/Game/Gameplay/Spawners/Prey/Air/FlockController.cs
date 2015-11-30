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

	[SerializeField] private float m_guideSpeed = 2f;
	[SerializeField] private GuideFunction m_guideFunction = GuideFunction.SMALL_FLOCK;
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------

	private AreaBounds m_area;

	// Flock control
	private Vector3 m_target;
	public Vector3 target { get { return m_target; } }
	
	private GameObject[] m_entities;
	public GameObject[] entities { get { return m_entities; } }

	private float m_timer;


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
	
	}

	public void Init(int _maxEntities) {

		m_entities = new GameObject[_maxEntities];
		
		m_area = GetComponent<Area>().bounds;

		m_target = m_area.bounds.center;		
		m_timer = 0;
	}

	public void Add(GameObject _entity) {

		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] == null) {
				m_entities[i] = _entity;
				break;
			}
		}
	}

	public void Remove(GameObject _entity) {

		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] == _entity) {
				m_entities[i] = null;
				break;
			}
		}
	}
	private int count = 0;
	// Update is called once per frame
	void Update () {
	
		// Control flocking
		// Move target for follow behaviour
		m_timer += Time.deltaTime * m_guideSpeed;

		if (m_guideFunction == GuideFunction.SMALL_FLOCK) {
			m_target.x = m_area.bounds.center.x + (Mathf.Sin(m_timer * 0.75f) * 0.5f + Mathf.Cos(m_timer * 0.25f) * 0.5f) * m_area.bounds.extents.x;
			m_target.y = m_area.bounds.center.y + (Mathf.Sin(m_timer * 0.35f) * 0.5f + Mathf.Cos(m_timer * 0.65f) * 0.5f) * m_area.bounds.extents.y;
		} else if (m_guideFunction == GuideFunction.FAST_FLOCK) {
			m_target.x = m_area.bounds.center.x + Mathf.Sin(m_timer) * m_area.bounds.extents.x;
			m_target.y = m_area.bounds.center.y + Mathf.Cos(m_timer) * m_area.bounds.extents.y;
		} else if (m_guideFunction == GuideFunction.WANDER) {
			if (m_timer >= 10f) { // seconds
				m_target = m_area.RandomInside();
				m_target.z = 0;

				m_timer = 0;
				count++;
			}
		}
	}

	void OnDrawGizmos() {

		if (Application.isPlaying) {
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(m_target, 0.25f);
		}
	}
}
