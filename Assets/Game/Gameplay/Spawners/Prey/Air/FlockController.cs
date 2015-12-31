using UnityEngine;
using System.Collections;

public class FlockController : MonoBehaviour {

	//http://www.artbylogic.com/parametricart/spirograph/spirograph.htm
	public enum GuideFunction{
		Basic,
		Hypotrochoid,
		Epitrochoid
	};


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------

	[SerializeField] private float m_guideSpeed = 2f;
	[SerializeField] private GuideFunction m_guideFunction = GuideFunction.Basic;

	[SerializeField] private float m_innerRadius = 10f; //r
	[SerializeField] private float m_outterRadius = 20f; //R
	[SerializeField] private float m_targetDistance = 5f; //d

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


	private Vector3 m_movingCircleCenter;


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
		}
		m_target = transform.position;	
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
			m_timer += Time.smoothDeltaTime * m_guideSpeed;

			switch (m_guideFunction) {
				case GuideFunction.Basic:
					m_target = m_area.center;
					m_target.x += (Mathf.Sin(m_timer * 0.75f) * 0.5f + Mathf.Cos(m_timer * 0.25f) * 0.5f) * m_area.extentsX;
					m_target.y += (Mathf.Sin(m_timer * 0.35f) * 0.5f + Mathf.Cos(m_timer * 0.65f) * 0.5f) * m_area.extentsY;
					m_target.z +=  Mathf.Sin(m_timer) * m_area.bounds.extents.z;
				 	break;

				case GuideFunction.Hypotrochoid:
					UpdateHypotrochoid(m_timer);
					break;

				case GuideFunction.Epitrochoid:
					UpdateEpitrochoid(m_timer);
					break;
			}
		}
	}

	void UpdateHypotrochoid(float _a) {
		float rDiff = (m_outterRadius - m_innerRadius);
		float tAngle = (rDiff / m_innerRadius) * _a;

		m_movingCircleCenter = m_area.center;
		m_movingCircleCenter.x += rDiff * Mathf.Cos(_a);
		m_movingCircleCenter.y += rDiff * Mathf.Sin(_a);

		m_target = m_movingCircleCenter;
		m_target.x += m_targetDistance * Mathf.Cos(tAngle);
		m_target.y -= m_targetDistance * Mathf.Sin(tAngle);
	}

	void UpdateEpitrochoid(float _a) {
		float rSum = (m_outterRadius + m_innerRadius);
		float tAngle = (rSum / m_innerRadius) * _a;

		m_movingCircleCenter = m_area.center;
		m_movingCircleCenter.x += rSum * Mathf.Cos(_a);
		m_movingCircleCenter.y += rSum * Mathf.Sin(_a);

		m_target = m_movingCircleCenter;
		m_target.x -= m_targetDistance * Mathf.Cos(tAngle);
		m_target.y -= m_targetDistance * Mathf.Sin(tAngle);
	}

	void OnDrawGizmos() {
		if (Application.isPlaying) {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_target, 0.5f);
		}
	}

	void OnDrawGizmosSelected() {
		if (m_guideFunction != GuideFunction.Basic) {
			if (m_area == null) {
				Area area = GetComponent<Area>();
				if (area != null) {
					m_area = area.bounds;
				}
			}

			if (!Application.isPlaying) {
				m_timer += 0.25f;
				switch (m_guideFunction) {
					case GuideFunction.Hypotrochoid:
						UpdateHypotrochoid(m_timer);
						break;

					case GuideFunction.Epitrochoid:
						UpdateEpitrochoid(m_timer);
						break;
				}
			}

			Color white = Color.blue;
			white.a = 0.75f;

			Gizmos.color = white;
			Gizmos.DrawSphere(m_area.center, m_outterRadius);
			Gizmos.DrawSphere(m_movingCircleCenter, m_innerRadius);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(m_movingCircleCenter, m_target);
			Gizmos.DrawSphere(m_target, 0.5f);
		}
	}
}
