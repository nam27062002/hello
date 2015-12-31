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
	[SerializeField] private int m_targetTimeSlots = 5;
	[SerializeField] private float m_amountSecsInPast = 0.25f;

	[SeparatorAttribute]
	[SerializeField] private GuideFunction m_guideFunction = GuideFunction.Basic;
	[SerializeField] private float m_guideSpeed = 2f;

	[SerializeField] private float m_innerRadius = 10f; //r
	[SerializeField] private float m_outterRadius = 20f; //R
	[SerializeField] private float m_targetDistance = 5f; //d




	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private AreaBounds m_area;

	// Flock control
	private Vector3[] m_target;	
	private GameObject[] m_entities;
	public GameObject[] entities { get { return m_entities; } }

	private Vector3 m_movingCircleCenter;
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
		}

		m_target = new Vector3[m_targetTimeSlots];
		for (int i = 0; i < m_targetTimeSlots; i++) {
			m_target[i] = transform.position;	
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

	public Vector2 GetTarget(int _index) {
		return m_target[_index % m_targetTimeSlots];
	}

	// Update is called once per frame
	void Update () {	
		// Control flocking
		// Move target for follow behaviour
		if (m_area != null) {
			m_timer += Time.smoothDeltaTime * m_guideSpeed;

			for (int i = 0; i < m_targetTimeSlots; i++) {
				float time = m_timer - i * m_amountSecsInPast; // go back to the past :3
				switch (m_guideFunction) {
					case GuideFunction.Basic:
						UpdateBasic(time, i);
					 	break;

					case GuideFunction.Hypotrochoid:
						UpdateHypotrochoid(time, i);
						break;

					case GuideFunction.Epitrochoid:
						UpdateEpitrochoid(time, i);
						break;
				}
			}
		}
	}

	void UpdateBasic(float _a, int _index) {
		m_target[_index] = m_area.center;
		m_target[_index].x += (Mathf.Sin(_a * 0.75f) * 0.5f + Mathf.Cos(_a * 0.25f) * 0.5f) * m_area.extentsX;
		m_target[_index].y += (Mathf.Sin(_a * 0.35f) * 0.5f + Mathf.Cos(_a * 0.65f) * 0.5f) * m_area.extentsY;
		m_target[_index].z +=  Mathf.Sin(_a) * m_area.bounds.extents.z;
	}

	void UpdateHypotrochoid(float _a, int _index) {
		float rDiff = (m_outterRadius - m_innerRadius);
		float tAngle = (rDiff / m_innerRadius) * _a;

		m_movingCircleCenter = m_area.center;
		m_movingCircleCenter.x += rDiff * Mathf.Cos(_a);
		m_movingCircleCenter.y += rDiff * Mathf.Sin(_a);

		m_target[_index] = m_movingCircleCenter;
		m_target[_index].x += m_targetDistance * Mathf.Cos(tAngle);
		m_target[_index].y -= m_targetDistance * Mathf.Sin(tAngle);
	}

	void UpdateEpitrochoid(float _a, int _index) {
		float rSum = (m_outterRadius + m_innerRadius);
		float tAngle = (rSum / m_innerRadius) * _a;

		m_movingCircleCenter = m_area.center;
		m_movingCircleCenter.x += rSum * Mathf.Cos(_a);
		m_movingCircleCenter.y += rSum * Mathf.Sin(_a);

		m_target[_index] = m_movingCircleCenter;
		m_target[_index].x -= m_targetDistance * Mathf.Cos(tAngle);
		m_target[_index].y -= m_targetDistance * Mathf.Sin(tAngle);
	}

	void OnDrawGizmos() {
		if (Application.isPlaying) {
			Gizmos.color = Color.red;
			for (int i = 0; i < m_targetTimeSlots; i++) {
				Gizmos.DrawSphere(m_target[i], 0.5f - i * 0.05f);
			}
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

			if (m_target == null || m_target.Length != m_targetTimeSlots) {
				m_target = new Vector3[m_targetTimeSlots];
			}

			if (!Application.isPlaying) {
				m_timer += 0.25f;
				for (int i = 0; i < m_targetTimeSlots; i++) {
					float time = m_timer - i * m_amountSecsInPast; // go back to the past :3
					switch (m_guideFunction) {
						case GuideFunction.Hypotrochoid:
							UpdateHypotrochoid(time, i);
							break;

						case GuideFunction.Epitrochoid:
							UpdateEpitrochoid(time, i);
							break;
					}
				}
			}

			Color white = Color.blue;
			white.a = 0.75f;

			Gizmos.color = white;
			Gizmos.DrawSphere(m_area.center, m_outterRadius);
			Gizmos.DrawSphere(m_movingCircleCenter, m_innerRadius);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(m_movingCircleCenter, m_target[0]);
			for (int i = 0; i < m_targetTimeSlots; i++) {
				Gizmos.DrawSphere(m_target[i], 0.5f - i * 0.05f);
			}
		}
	}
}
