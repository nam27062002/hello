using UnityEngine;
using System.Collections;

public class FlockController : MonoBehaviour {

	public enum GuideFunction{
		SMALL_FLOCK
	};


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------

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
		
		Area area = GetComponent<Area>();
		if (area != null) {
			m_area = area.bounds; 
			m_target = m_area.bounds.center;
		} else {
			m_target = transform.position;
		}
	
		m_timer = Random.Range(0f, Mathf.PI * 2f);
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


	// Update is called once per frame
	void Update () {	
		// Control flocking
		// Move target for follow behaviour
		if (m_area != null) {
			m_timer += Time.deltaTime * m_guideSpeed;

			if (m_guideFunction == GuideFunction.SMALL_FLOCK) {
				m_target.x = m_area.bounds.center.x + (Mathf.Sin(m_timer * 0.75f) * 0.5f + Mathf.Cos(m_timer * 0.25f) * 0.5f) * m_area.bounds.extents.x;
				m_target.y = m_area.bounds.center.y + (Mathf.Sin(m_timer * 0.35f) * 0.5f + Mathf.Cos(m_timer * 0.65f) * 0.5f) * m_area.bounds.extents.y;
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
