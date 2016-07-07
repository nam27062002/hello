using UnityEngine;
using System.Collections;


public class FlockController : MonoBehaviour, IGuideFunction {
	
	//http://www.artbylogic.com/parametricart/spirograph/spirograph.htm
	public enum GuideFunction{
		Basic,
		Hypotrochoid,
		Epitrochoid,
		FGOL_Shoal
	};


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private float m_amountSecsInPast = 0.25f;

	[SeparatorAttribute]
	[SerializeField] private GuideFunction m_guideFunction = GuideFunction.Basic;
	[SerializeField] private float m_guideSpeed = 2f;
	[SerializeField] private float m_secondaryGuideSpeed = 2f;
	[SerializeField] private float m_sensePlayer = 0;

	[SerializeField] private float m_innerRadius = 10f; //r
	[SerializeField] private float m_outterRadius = 20f; //R
	[SerializeField] private float m_targetDistance = 5f; //d

	[SeparatorAttribute]
	[SerializeField] private float m_previewStep = 0.1f;
	[SerializeField] private float m_previewMaxTime = 60f;

	private float m_sensePlayerSqr;

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private AreaBounds m_area;

	// Flock control
	private Vector3 m_target;	

	private Vector3 m_movingCircleCenter;
	private float m_timer;
	private float m_secondaryTimer;

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

		ResetTime();
	}

	public Bounds GetBounds() {
		return m_area.bounds;
	}

	public void ResetTime() {
		m_timer = Random.Range(0f, Mathf.PI * 2f);
		m_secondaryTimer = Random.Range(0f, Mathf.PI * 2f);
		UpdateFunction(m_timer, m_secondaryTimer);
	}

	public Vector3 NextPositionAtSpeed(float _speed) {
		UpdateLogic();
		return m_target;
	}

	// Update is called once per frame
	private void UpdateLogic() {	
		// Control flocking
		// Move target for follow behaviour
		if (m_area != null) {
			m_timer += Time.smoothDeltaTime * m_guideSpeed;
			m_secondaryTimer += Time.smoothDeltaTime * m_secondaryGuideSpeed;
			float time = m_timer;

			UpdateFunction(time, m_secondaryTimer);

			if (m_dragonMouth != null) {
				// Check target against player
				Vector3 dist = (Vector2)m_target - (Vector2)m_dragonMouth.position;
				if (Vector2.SqrMagnitude(dist) < m_sensePlayerSqr) {
					m_target = m_dragonMouth.position + dist.normalized * m_sensePlayer;
				}
			}
		}
	}

	void UpdateFunction(float _time, float _secondaryTime) {
		switch (m_guideFunction) {
			case GuideFunction.Basic:
				UpdateBasic(_time);
				break;

			case GuideFunction.Hypotrochoid:
				UpdateHypotrochoid(_time);
				break;

			case GuideFunction.Epitrochoid:
				UpdateEpitrochoid(_time);
				break;

			case GuideFunction.FGOL_Shoal:
				UpdateShoal(_time, _secondaryTime);
				break;
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

	void UpdateShoal( float _timeX, float _timeY )
	{
		float px = m_area.center.x + (Mathf.Sin(_timeX) * m_area.extentsX);
		float py = m_area.center.y + (Mathf.Sin(_timeY) * m_area.extentsY);
		m_target.x = px;
		m_target.y = py;
	}

	void OnDrawGizmos() {
		if (Application.isPlaying) {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_target, 0.5f);
		}
	}

	void OnDrawGizmosSelected() {
		

		if (m_area == null) {
			Area area = GetComponent<Area>();
			if (area != null) {
				m_area = area.bounds;
			}
		}

		if (Application.isPlaying) {
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(m_target, m_sensePlayer);

			Gizmos.color = Color.red;
			Gizmos.DrawLine(m_area.center, m_target);
		} else { 
			float time = 0;
			float secondaryTime = 0;
			float maxTime = m_previewMaxTime;
			float step = Mathf.Max(0.1f, m_previewStep);

			UpdateFunction(time, secondaryTime);
			Vector3 lastTarget = m_target;

			Gizmos.color = Color.white;
			for (time = step; time < maxTime; time += step) {
				secondaryTime += step * (m_secondaryGuideSpeed / m_guideSpeed);
				UpdateFunction(time, secondaryTime);
				Gizmos.DrawLine(lastTarget, m_target);
				lastTarget = m_target;
			}
		}
	}
}
