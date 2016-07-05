using UnityEngine;
using System.Collections;


public class GuideFunction : MonoBehaviour {
	
	//http://www.artbylogic.com/parametricart/spirograph/spirograph.htm
	public enum FunctionType{
		Hypotrochoid,
		Epitrochoid
	};

	// more equations
	// http://wiki.roblox.com/index.php?title=Parametric_equations


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SeparatorAttribute]
	[SerializeField] private FunctionType m_guideFunction = FunctionType.Hypotrochoid;

	[SerializeField] private float m_innerRadius = 10f;   //r
	[SerializeField] private float m_outterRadius = 20f;  //R
	[SerializeField] private float m_targetDistance = 5f; //d

	[SerializeField] private float m_depthAmplitude = 0f;
	[SerializeField] private float m_depthFrequency = 0f;

	[SeparatorAttribute]
	[SerializeField] private Vector3 m_scale = Vector3.one;
	[SerializeField] private Vector3 m_rotation = Vector3.zero;

	[SeparatorAttribute]
	[SerializeField] private bool m_forcePreview = false;
	[SerializeField] private float m_previewStep = 0.1f;
	[SerializeField] private float m_previewMaxTime = 60f;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private Vector3 m_target = Vector3.zero;
	private Vector3 m_centerOffset = Vector3.zero;
	private float m_time;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	public Bounds GetBounds() {
		Bounds bounds = new Bounds();

		float rDiff = 0f;
		switch (m_guideFunction) {
			case FunctionType.Hypotrochoid:	rDiff = Mathf.Abs(m_outterRadius - m_innerRadius); break;
			case FunctionType.Epitrochoid:	rDiff = Mathf.Abs(m_outterRadius + m_innerRadius); break;
		}

		Vector3 max = new Vector3(rDiff + m_targetDistance,  rDiff + m_targetDistance,  m_depthAmplitude);

		bounds.min = transform.position + m_centerOffset + Quaternion.Euler(m_rotation) * Vector3.Scale(m_scale, -max);
		bounds.max = transform.position + m_centerOffset + Quaternion.Euler(m_rotation) * Vector3.Scale(m_scale,  max);

		return bounds;
	}

	public void ResetTime() {
		m_time = 0f;
		UpdateFunction(0f);
		m_centerOffset = -m_target;
	}

	public Vector3 NextPositionAtSpeed(float _speed) {
		m_time += _speed * Time.deltaTime;
		UpdateFunction(m_time);
		return m_target + transform.position + m_centerOffset;
	}

	private void UpdateFunction(float _t) {
		switch (m_guideFunction) {
			case FunctionType.Hypotrochoid:
				UpdateHypotrochoid(_t);
				break;

			case FunctionType.Epitrochoid:
				UpdateEpitrochoid(_t);
				break;
		}

		m_target = Quaternion.Euler(m_rotation) * Vector3.Scale(m_scale, m_target);
	}

	private void UpdateHypotrochoid(float _a) {
		float rDiff = (m_outterRadius - m_innerRadius);
		float tAngle = (rDiff / m_innerRadius) * _a;

		m_target = Vector3.zero;
		m_target.x += rDiff * Mathf.Cos(_a) + m_targetDistance * Mathf.Cos(tAngle);
		m_target.y += rDiff * Mathf.Sin(_a) - m_targetDistance * Mathf.Sin(tAngle);
		m_target.z += m_depthAmplitude * Mathf.Cos(m_depthFrequency * _a);
	}

	private void UpdateEpitrochoid(float _a) {
		float rSum = (m_outterRadius + m_innerRadius);
		float tAngle = (rSum / m_innerRadius) * _a;

		m_target = Vector3.zero;
		m_target.x += rSum * Mathf.Cos(_a) - m_targetDistance * Mathf.Cos(tAngle);
		m_target.y += rSum * Mathf.Sin(_a) - m_targetDistance * Mathf.Sin(tAngle);
		m_target.z += m_depthAmplitude * Mathf.Cos(m_depthFrequency * _a);
	}

	//-------------------------------------------------------------------------------------
	//-------------------------------------------------------------------------------------
	private void OnDrawGizmosSelected() {

		UpdateFunction(0f);
		m_centerOffset = -m_target;

		if (m_forcePreview || !Application.isPlaying) {
			float time = 0;
			float maxTime = m_previewMaxTime;
			float step = Mathf.Max(0.1f, m_previewStep);

			UpdateFunction(time);
			Vector3 lastTarget = m_target + transform.position + m_centerOffset;

			Gizmos.color = Color.white;
			for (time = step; time < maxTime; time += step) {
				UpdateFunction(time);
				Vector3 target = m_target + transform.position + m_centerOffset;
				Gizmos.DrawLine(lastTarget, target);
				lastTarget = target;
			}
		}

		//
		Bounds bounds = GetBounds();
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(bounds.center, bounds.size);
	}
}
