using UnityEngine;
using System.Collections;

public class CloudController : MonoBehaviour 
{
	// Exposed members
	[InfoBox(
		"Start and end points mark the left and right limits.\n" +
		"Use negative speeds for right->left, positive for left->right direction.\n" + 
		"Speed will be interpolated for each cloud based on its Z position."
	)]
	public Transform m_controlPoint1;
	public Transform m_controlPoint2;

	[Space]
	public float m_closeSpeed = 10;
	public float m_farSpeed = 1;

	[Space]
	public Color m_debugColor = Colors.silver;

	// Internal
	private Renderer[] m_renderers;
	private Range m_xRange = new Range();
	private Range m_yRange = new Range();
	private Range m_zRange = new Range();

	// Use this for initialization
	void Start ()  {
		m_renderers = GetComponentsInChildren<Renderer>();
	}
	
	// Update is called once per frame
	void LateUpdate ()  {
		// Compute area
		UpdateArea();

		// Aux vars
		Renderer r;
		Vector3 pos;
		float delta;
		float speed;
		for(int i = 0; i<m_renderers.Length; i++) {
			// Get references
			r = m_renderers[i];
			pos = r.transform.position;

			// Update speed
			//delta = ( pos.z - m_closeSpeedZ)/(m_farSpeedZ-m_closeSpeedZ);
			//speed = m_closeSpeed + ( m_farSpeed - m_closeSpeed ) * delta;
			delta = Mathf.InverseLerp(m_zRange.min, m_zRange.max, pos.z);
			speed = Mathf.Lerp(m_closeSpeed, m_farSpeed, delta);

			// Update position
			pos.x += speed * Time.deltaTime;

			// Reset position if end reached (either left or right)
			if(speed > 0) {
				if(pos.x > m_xRange.max) {
					pos.x = m_xRange.min;
				}
			} else {
				if(pos.x < m_xRange.min) {
					pos.x = m_xRange.max;
				}
			}

			// Apply new position
			r.transform.position = pos;
		}
	}

	/// <summary>
	/// Debug utils.
	/// </summary>
	private void OnDrawGizmos() {
		// Ignore if not active
		if(!isActiveAndEnabled) return;

		// If not playing, update area (otherwise it will be done in the LateUpdate call, we don't want to do it twice)
		if(!Application.isPlaying) {
			UpdateArea();
		}

		// Setup gizoms
		Gizmos.matrix = Matrix4x4.identity;		// Global coords

		// Draw area
		Gizmos.color = m_debugColor;
		Gizmos.DrawWireCube(
			new Vector3(m_xRange.center, m_yRange.center, m_zRange.center),
			new Vector3(m_xRange.distance, m_yRange.distance, m_zRange.distance)
		);

		// Draw control points
		Gizmos.color = m_debugColor;
		if(m_controlPoint1 != null) Gizmos.DrawSphere(m_controlPoint1.position, 2f);
		if(m_controlPoint2 != null) Gizmos.DrawSphere(m_controlPoint2.position, 2f);
	}

	/// <summary>
	/// Compute the area.
	/// </summary>
	private void UpdateArea() {
		// Both CP needed
		if(m_controlPoint1 == null || m_controlPoint2 == null) return;

		// Do it
		m_xRange.Set(
			Mathf.Min(m_controlPoint1.position.x, m_controlPoint2.position.x),
			Mathf.Max(m_controlPoint1.position.x, m_controlPoint2.position.x)
		);
		m_yRange.Set(
			Mathf.Min(m_controlPoint1.position.y, m_controlPoint2.position.y),
			Mathf.Max(m_controlPoint1.position.y, m_controlPoint2.position.y)
		);
		m_zRange.Set(
			Mathf.Min(m_controlPoint1.position.z, m_controlPoint2.position.z),
			Mathf.Max(m_controlPoint1.position.z, m_controlPoint2.position.z)
		);
	}
}
