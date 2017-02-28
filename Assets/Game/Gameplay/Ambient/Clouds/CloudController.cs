using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum DebugType {
		NONE,
		SOLID,
		FRAME,
		CONTROL_POINTS_ONLY
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	// Refereneces
	[InfoBox(
		"Start and end points mark the left and right limits.\n" +
		"Use negative speeds for right->left, positive for left->right direction.\n" + 
		"Speed will be interpolated for each cloud based on its Z position."
	)]
	public Transform m_controlPoint1 = null;
	public Transform m_controlPoint2 = null;
	public Transform m_cloudContainer = null;

	public GameObject[] m_prefabs = new GameObject[0];

	// Main Setup
	public int m_amount = 10;
	public Range m_zNearSpeedRange = new Range(10f, 15f);
	public Range m_zFarSpeedRange = new Range(1f, 5f);

	// Transformations
	public Range m_xScaleRange = new Range(1f, 1.5f);
	public Range m_yScaleRange = new Range(1f, 1.5f);
	public Range m_aspectRatioRange = new Range(0.9f, 1.1f);

	[Range(0f, 1f)] public float m_xFlipChance = 0.5f;
	[Range(0f, 1f)] public float m_yFlipChance = 0f;

	// Distribution
	public AnimationCurve m_xDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	public AnimationCurve m_yDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
	public AnimationCurve m_zDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	// Editor
	public bool m_livePreview = true;
	public bool m_liveEdit = true;

	// Debug
	public DebugType m_debugType = DebugType.FRAME;
	public Color m_boxColor = Colors.WithAlpha(Colors.silver, 0.5f);
	public Color m_controlPointsColor = Colors.pink;

	// Internal
	public List<GameObject> m_clouds = new List<GameObject>();
	public List<float> m_speeds = new List<float>();
	public List<SpriteRenderer> m_renderers = new List<SpriteRenderer>();
	public Range m_xRange = new Range();
	public Range m_yRange = new Range();
	public Range m_zRange = new Range();

	//------------------------------------------------------------------------//
	// DEFAULT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start()  {
		
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void LateUpdate()  {
		DoUpdate(Time.deltaTime);
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
		Gizmos.color = m_boxColor;
		switch(m_debugType) {
			case DebugType.FRAME: {
				Gizmos.DrawWireCube(
					new Vector3(m_xRange.center, m_yRange.center, m_zRange.center),
					new Vector3(m_xRange.distance, m_yRange.distance, m_zRange.distance)
				);
			} break;

			case DebugType.SOLID: {
				Gizmos.DrawCube(
					new Vector3(m_xRange.center, m_yRange.center, m_zRange.center),
					new Vector3(m_xRange.distance, m_yRange.distance, m_zRange.distance)
				);
			} break;
		}

		// Draw control points
		Gizmos.color = m_controlPointsColor;
		switch(m_debugType) {
			case DebugType.NONE: {
				// Do nothing
			} break;

			default: {
				if(m_controlPoint1 != null) Gizmos.DrawSphere(m_controlPoint1.position, 2f);
				if(m_controlPoint2 != null) Gizmos.DrawSphere(m_controlPoint2.position, 2f);
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// PUBLIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Actually do the update stuff.
	/// Separate method to be able to run it outside play mode.
	/// </summary>
	/// <param name="_deltaTime">Time elapsed since last frame.</param>
	public void DoUpdate(float _deltaTime) {
		// Compute area
		UpdateArea();

		// Aux vars
		Transform t;
		Vector3 pos;
		float distToEdge;
		float deltaToEdge;
		for(int i = 0; i < m_clouds.Count; i++) {
			// Get references
			t = m_clouds[i].transform;
			pos = t.position;

			// Update position
			pos.x += m_speeds[i] * _deltaTime;

			// Reset position if end reached (either left or right)
			if(m_speeds[i] > 0) {
				if(pos.x > m_xRange.max) {
					pos.x = m_xRange.min;
				}
			} else {
				if(pos.x < m_xRange.min) {
					pos.x = m_xRange.max;
				}
			}

			// Apply new position
			t.position = pos;

			// Fade out at the edges (to avoid popping)
			if(m_renderers[i] != null) {
				distToEdge = Mathf.Min(Mathf.Abs(m_xRange.min - pos.x), Mathf.Abs(m_xRange.max - pos.x));	// Distance to closest edge
				deltaToEdge = Mathf.InverseLerp(0f, 0.1f * m_xRange.distance, distToEdge);	// Scale distToEdge between 0f and 0.1f of the total range
				m_renderers[i].color = Colors.WithAlpha(m_renderers[i].color, Mathf.Lerp(0f, 1f, deltaToEdge));
			}
		}
	}

	/// <summary>
	/// Compute the area.
	/// </summary>
	public void UpdateArea() {
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
