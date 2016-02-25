// AOCBezierPoint.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to AOCBezierCurve for storing and manipulating Bezier Point data.
/// Ensures that handles are in correct relation to one another.
/// Handles adding/removing self from curve point lists.
/// Calls SetDirty() on parent curve when edited.
/// Based on https://www.assetstore.unity3d.com/en/#!/content/11278
/// </summary>
[System.Serializable]
public class BezierPoint {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Relationship between a point's handles
	/// 	- CONNECTED: The point's handles are mirrored across the point
	/// 	- BROKEN: Each handle moves independently of the other
	/// 	- NONE: This point has no handles (both handles are located ON the point)
	/// </summary>
	public enum HandleStyle {
		CONNECTED,
		BROKEN,
		NONE
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private HandleStyle m_handleStyle = HandleStyle.CONNECTED;
	public HandleStyle handleStyle {
		get { return m_handleStyle; }
		set {
			// Store new value
			m_handleStyle = value;

			// Special case for NONE - reset handlers
			if(m_handleStyle == HandleStyle.NONE) {
				handle1 = Vector3.zero;
				handle2 = Vector3.zero;
			}
		}
	}

	// Is this point editable?
	[SerializeField] private bool m_locked = false;
	public bool locked {
		get { return m_locked; }
		set { m_locked = value; }
	}

	/// <summary>
	/// Position of this point relative to its parent curve.
	/// If the point doesn't belong to any curve, world coordinates will be assumed.
	/// </summary>
	[SerializeField] private Vector3 m_position = Vector3.zero;
	public Vector3 position {
		get { return m_position; }
		set {
			// Ignore if locked
			if(m_locked) return;

			// Apply Z-lock
			if(curve != null && curve.lockZ) value.z = m_position.z;

			// Skip if it hasn't changed
			if(m_position == value) return;

			// Store new position and mark curve as dirty
			m_position = value;
			if(curve != null) curve.SetDirty();
		}
	}

	/// <summary>
	/// Gets or sets the position of this point in world coordinates.
	/// Will be stored as local coordinates relative to the parent curve.
	/// If the point doesn't belong to any curve, world coordinates will be assumed.
	/// </summary>
	public Vector3 globalPosition {
		get { return transform ? transform.TransformPoint(m_position) : m_position; }
		set { position = transform ? transform.InverseTransformPoint(value) : value; }
	}

	/// <summary>
	/// Curve this point belongs to.
	/// Changing this value will automatically remove this point from the current curve and add it to the new one
	/// </summary>
	private BezierCurve m_curve;
	public BezierCurve curve {
		get{ return m_curve; }
		set {
			if(m_curve == value) return;
			if(m_curve != null) m_curve.RemovePoint(this);
			m_curve = value;
			m_curve.AddPoint(this);
		}
	}

	/// <summary>
	/// Shortcut to the transform of the parent curve.
	/// </summary>
	private Transform transform {
		get {
			if(m_curve) return m_curve.transform;
			return null;
		}
	}

	/// <summary>
	/// Position of the first handle in relation to the point.
	/// Setting this value will cause the curve to become dirty.
	/// This handle effects the curve generated from this point and the point proceeding it in curve.points.
	/// </summary>
	[SerializeField] private Vector3 m_handle1 = Vector3.zero;
	public Vector3 handle1 {
		get { return m_handle1; }
		set { 
			// Ignore if locked
			if(m_locked) return;

			// Apply Z-lock
			if(curve != null && curve.lockZ) value.z = m_position.z;

			// Skip if not changed
			if(m_handle1 == value) return;
			m_handle1 = value;

			// Adjust second handle according to handling style
			if(m_handleStyle == HandleStyle.NONE) {
				m_handleStyle = HandleStyle.BROKEN;
			} else if(m_handleStyle == HandleStyle.CONNECTED) {
				m_handle2 = -value;
			}
			m_curve.SetDirty();
		}
	}

	/// <summary>
	///	Global position of the first handle
	///	Ultimately stored in the 'handle1' variable
	/// Setting this value will cause the curve to become dirty
	/// This handle effects the curve generated from this point and the point proceeding it in curve.points
	/// </summary>
	public Vector3 globalHandle1 {
		// [AOC] TO TEST!!
		get{ return transform.TransformPoint(m_position + handle1); }
		set{ handle1 = transform.InverseTransformPoint(value) - m_position; }
	}

	/// <summary>
	/// Position of the second handle in relation to the point.
	/// Setting this value will cause the curve to become dirty.
	///	This handle effects the curve generated from this point and the point coming after it in curve.points.
	/// </summary>
	[SerializeField] private Vector3 m_handle2 = Vector3.zero;
	public Vector3 handle2 {
		get { return m_handle2; }
		set {
			// Ignore if locked
			if(m_locked) return;

			// Apply Z-lock
			if(curve != null && curve.lockZ) value.z = m_position.z;

			// Skip if not changed
			if(m_handle2 == value) return;
			m_handle2 = value;

			// Adjust first handle according to handling style
			if(m_handleStyle == HandleStyle.NONE) {
				m_handleStyle = HandleStyle.BROKEN;
			} else if(m_handleStyle == HandleStyle.CONNECTED) {
				m_handle1 = -value;
			}
			m_curve.SetDirty();
		}
	}

	/// <summary>
	/// Global position of the second handle
	///	Ultimately stored in the 'handle2' variable
	///	Setting this value will cause the curve to become dirty
	///	This handle effects the curve generated from this point and the point coming after it in curve.points 
	/// </summary>
	public Vector3 globalHandle2 {
		// [AOC] TO TEST!!
		get{ return transform.TransformPoint(m_position + handle2); }
		set{ handle2 = transform.InverseTransformPoint(value) - m_position; }
	}
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_pos">The local position of this point related to the curve transform.</param>
	public BezierPoint(Vector3 _pos) {
		m_position = _pos;
	}

	//------------------------------------------------------------------//
	// CUSTOM METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Force the specific curve to be the parent curve of this point.
	/// We need to do this every time we serialize/deserialize the curve, otherwise
	/// the reference to it will be lost.
	/// </summary>
	/// <param name="_curve">The curve owning this point.</param>
	public void ConsolidateCurve(BezierCurve _curve) {
		// Don't do any treatment whatsoever, just store the reference to the parent curve
		m_curve = _curve;
	}
}