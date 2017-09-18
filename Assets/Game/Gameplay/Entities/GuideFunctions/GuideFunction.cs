﻿using UnityEngine;
using System.Collections;


public class GuideFunction : MonoBehaviour, IGuideFunction {
	
	//http://www.artbylogic.com/parametricart/spirograph/spirograph.htm
	public enum FunctionType{
		Hypotrochoid,
		Epitrochoid,
		Spiral,
		Infinity
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
	[SerializeField] private Vector3 m_boundsScale = Vector3.zero;

	[SeparatorAttribute]
	[SerializeField] private bool m_drawPreview = false;
	[SerializeField] private float m_previewStep = 0.1f;
	[SerializeField] private float m_previewMaxTime = 60f;


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private Vector3 m_target = Vector3.zero;
	private Vector3 m_centerOffset = Vector3.zero;
	private float m_time;

	private float m_size;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	public AreaBounds GetBounds() {
		float rDiff = 0f;
		float depth = m_depthAmplitude;
		Vector3 center = Vector3.zero;

		switch (m_guideFunction) {
			case FunctionType.Hypotrochoid:	rDiff = Mathf.Abs(m_outterRadius - m_innerRadius); 	break;
			case FunctionType.Epitrochoid:	rDiff = Mathf.Abs(m_outterRadius + m_innerRadius); 	break;
			case FunctionType.Spiral:		depth = m_targetDistance * 0.5f; center.z += depth; break;
		}

		Vector3 size = Vector3.zero;

		if (m_guideFunction == FunctionType.Infinity) {
			size = ((new Vector3(m_innerRadius * 1.1414f,  m_innerRadius * 0.5f * 1.1414f,  depth)) + Vector3.one);
		} else {
			size = ((new Vector3(rDiff + m_targetDistance,  rDiff + m_targetDistance,  depth)) + Vector3.one) * 2f;
		}


		size += m_boundsScale;
		size = Quaternion.Euler(m_rotation) * Vector3.Scale(m_scale, size);
		center = Quaternion.Euler(m_rotation) * Vector3.Scale(m_scale, center);

		size.x = Mathf.Abs(size.x);
		size.y = Mathf.Abs(size.y);
		size.z = Mathf.Abs(size.z);

		m_size = Mathf.Max(size.x, size.y);

		return new RectAreaBounds(transform.position + m_centerOffset + center, size);
	}

	public void ResetTime() {
		m_time = 0f;
		UpdateFunction(0f);
		m_centerOffset = -m_target;
	}

	public Vector3 NextPositionAtSpeed(float _speed) {

		float speedFactor = 0.185f + ((0.125f - 0.185f) * ((m_size - 20f) / (40f - 20f)));

		m_time += _speed * Time.deltaTime * speedFactor * 2f;
		if (UpdateFunction(m_time)) {
			m_time = 0f;
		}
		return m_target + transform.position + m_centerOffset;
	}

	private bool UpdateFunction(float _t) {
		bool resetTime = false;
		switch (m_guideFunction) {
			case FunctionType.Hypotrochoid:
				UpdateHypotrochoid(_t);
				break;

			case FunctionType.Epitrochoid:
				UpdateEpitrochoid(_t);
				break;

			case FunctionType.Spiral:
				resetTime = UpdateSpiral(_t);
				break;

			case FunctionType.Infinity:
				UpdateInfinity(_t);
				break;
		}

		m_target = Quaternion.Euler(m_rotation) * Vector3.Scale(m_scale, m_target);
		return resetTime;
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

	private bool UpdateSpiral(float _a) {
		//x(t) = at cos(t), y(t) = at sin(t), 
		Vector3 oldTarget = m_target;

		m_target = Vector3.zero;
		m_target.x = m_outterRadius * _a * Mathf.Cos(_a);
		m_target.y = m_innerRadius  * _a * Mathf.Sin(_a);
		m_target.z = (m_depthAmplitude * _a) + Mathf.Cos(m_depthFrequency * _a);
			
		if (Mathf.Abs(m_outterRadius * _a) 	 > m_targetDistance ||
			Mathf.Abs(m_innerRadius * _a) 	 > m_targetDistance || 
			Mathf.Abs(m_depthAmplitude * _a) > m_targetDistance) {
			return true;
		}

		return false;
	}

	private void UpdateInfinity(float _t) {
		float cosT = Mathf.Cos(_t);
		float sinT = Mathf.Sin(_t);
		float sin2T = sinT * sinT;

		float aSqrt2 = m_innerRadius * 1.1414f; //a * Sqrt(2f)

		m_target = Vector3.zero;
		m_target.x = (aSqrt2 * cosT) / (sin2T + 1f);
		m_target.y = (aSqrt2 * cosT * sinT) / (sin2T + 1f);
		m_target.z = (m_depthAmplitude * _t) + Mathf.Cos(m_depthFrequency * _t);
	}

	//-------------------------------------------------------------------------------------
	//-------------------------------------------------------------------------------------
	private void OnDrawGizmosSelected() {
		UpdateFunction(0);
		m_centerOffset = -m_target;

		if (LevelEditor.LevelEditor.settings.previewPaths && m_drawPreview) {
			float time = 0;
			float maxTime = m_previewMaxTime;
			float step = Mathf.Max(0.1f, m_previewStep);

			UpdateFunction(time);
			Vector3 lastTarget = m_target + transform.position + m_centerOffset;

			Gizmos.color = Color.white;
			for (time = step; time < maxTime; time += step) {
				if (UpdateFunction(time)) {
					break;
				}
				Vector3 target = m_target + transform.position + m_centerOffset;
				Gizmos.DrawLine(lastTarget, target);
				lastTarget = target;
			}
		}

		//
		AreaBounds bounds = GetBounds();
		Gizmos.color = Color.red;
		Gizmos.DrawWireCube(bounds.center, bounds.bounds.size);
	}
}
