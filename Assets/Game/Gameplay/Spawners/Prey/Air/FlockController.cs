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
	[SerializeField] private float m_amountSecsInPast = 0.25f;

	[SeparatorAttribute]
	[SerializeField] private GuideFunction m_guideFunction = GuideFunction.Basic;
	[SerializeField] private float m_guideSpeed = 2f;

	[SerializeField] private float m_innerRadius = 10f; //r
	[SerializeField] private float m_outterRadius = 20f; //R
	[SerializeField] private float m_targetDistance = 5f; //d

	public float m_sensePlayer = 0;
	private float m_sensePlayerSqr;

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private AreaBounds m_area;

	// Flock control
	private Vector3 m_target;	

	private Vector3 m_movingCircleCenter;
	private float m_timer;

	private Transform m_dragonMouth;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
	
	}

	public void Init() {
		m_dragonMouth = InstanceManager.player.GetComponent<DragonMotion>().tongue;
		m_sensePlayerSqr = m_sensePlayer * m_sensePlayer;
		Area area = GetComponent<Area>();
		if (area != null) {
			m_area = area.bounds;
		}
		m_target = transform.position;	

		m_timer = Random.Range(0f, Mathf.PI * 2f);
	}

	public Vector2 GetTarget() {
		return m_target;
	}

	// Update is called once per frame
	void Update () {	
		// Control flocking
		// Move target for follow behaviour
		if (m_area != null) {
			m_timer += Time.smoothDeltaTime * m_guideSpeed;

			float time = m_timer;
			switch (m_guideFunction) {
				case GuideFunction.Basic:
					UpdateBasic(time);
				 	break;

				case GuideFunction.Hypotrochoid:
					UpdateHypotrochoid(time);
					break;

				case GuideFunction.Epitrochoid:
					UpdateEpitrochoid(time);
					break;
			}
			// Check target against player
			Vector3 dist = (Vector2)m_target - (Vector2)m_dragonMouth.position;
			if ( Vector2.SqrMagnitude( dist ) < m_sensePlayerSqr )
			{
				m_target = m_dragonMouth.position + dist.normalized * m_sensePlayer;
			}
		}
	}

	void UpdateBasic(float _a) 
	{
		m_target = m_area.center;
		m_target.x += (Mathf.Sin(_a * 0.75f) * 0.5f + Mathf.Cos(_a * 0.25f) * 0.5f) * m_area.extentsX;
		m_target.y += (Mathf.Sin(_a * 0.35f) * 0.5f + Mathf.Cos(_a * 0.65f) * 0.5f) * m_area.extentsY;
		// m_target.z +=  Mathf.Sin(_a) * m_area.bounds.extents.z;

	}

	void UpdateHypotrochoid(float _a) 
	{
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

	void UpdateShoal()
	{
		float dt = Time.deltaTime;
		// m_movePhase += m_guideSpeed * Time.deltaTime;
		// timer should be 2
		/*
		float px = m_area.center.x + (Mathf.Sin(m_movePhase.x) * m_area.extentsX);
		float py = m_area.center.y + (Mathf.Sin(m_movePhase.y) * m_area.extentsY);
		m_target.x = px;
		m_target.y = py;
		*/
	}

	void OnDrawGizmos() {
		if (Application.isPlaying) {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_target, 0.5f);
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(m_target, m_sensePlayer);
		if (m_guideFunction != GuideFunction.Basic) {
			if (m_area == null) {
				Area area = GetComponent<Area>();
				if (area != null) {
					m_area = area.bounds;
				}
			}

			if (!Application.isPlaying) {
				m_timer += 0.25f;

				float time = m_timer; // go back to the past :3
				switch (m_guideFunction) {
					case GuideFunction.Hypotrochoid:
						UpdateHypotrochoid(time);
						break;

					case GuideFunction.Epitrochoid:
						UpdateEpitrochoid(time);
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


		}

	}
}
