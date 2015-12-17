using UnityEngine;
using System.Collections;

public class FogController : MonoBehaviour {

	[SerializeField] private Color m_outsideColor 	= new Color(0.045f, 0.85f, 1f);
	[SerializeField] private Color m_caveColor 		= new Color(0.15f, 0.15f, 0.15f);

	[SerializeField] private float m_startDistance 		= 140f;
	[SerializeField] private float m_endDistance 		= 300f;
	[SerializeField] private float m_caveDistanceOffset	= 120f;	

	[SerializeField] private float m_skyLine 	= 50f;
	[SerializeField] private float m_caveLine 	= -50f;

	/*
	 * 				 sky
	 * -------------------------------- <- disable fog completelly
	 * 				ground
	 * -------------------------------- <- fog changed to cave color
	 * 				 cave
	 * */

	private float m_distanceOffset = 0f; // cave fog will start near the camera

	// Use this for initialization
	void Start () {
		RenderSettings.fog = true; // disable on slow devices?
		RenderSettings.fogColor = m_outsideColor;
		RenderSettings.fogMode = FogMode.Linear;
		RenderSettings.fogStartDistance = m_startDistance;
		RenderSettings.fogEndDistance = m_endDistance;
	}
	
	// Update is called once per frame
	void Update () {
	
		// lerp fog color from/to outside - cave
		float y = transform.position.y;

		if (RenderSettings.fog) {
			float t = 0f;

			// 0 -> outside color
			// caveline -> cave color
			if (y <= 0) {
				t = Mathf.Abs(y / m_caveLine);
			}

			m_distanceOffset = Mathf.Lerp(0, m_caveDistanceOffset, t);

			RenderSettings.fogStartDistance = m_startDistance - m_distanceOffset;
			RenderSettings.fogEndDistance = m_endDistance - m_distanceOffset;
			RenderSettings.fogColor = Color.Lerp(m_outsideColor, m_caveColor, t);
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.grey;

		Vector3 skyLine = new Vector3(0, m_skyLine, 0);
		Gizmos.DrawLine(skyLine + Vector3.left * 500f, skyLine + Vector3.right * 500f);

		Vector3 caveLine = new Vector3(0, m_caveLine, 0);
		Gizmos.DrawLine(caveLine + Vector3.left * 500f, caveLine + Vector3.right * 500f);
	}
}
