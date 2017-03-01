// AOCBezierCurve.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/01/2016.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Class for describing and drawing Bézier Curves.
/// Efficiently handles approzimate length calculation through 'dirty' system
/// Based on https://www.assetstore.unity3d.com/en/#!/content/11278
/// </summary>
[ExecuteInEditMode]
[System.Serializable]
public class BezierCurve : MonoBehaviour, ISerializationCallbackReceiver {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Simple struct to cache sampled segments.
	/// Global world coordinates.
	/// </summary>
	public struct SampledSegment {
		// Line points defining this sample
		public Vector3 p1;
		public Vector3 p2;

		// Control points to where this sample belongs to
		public BezierPoint cp1;
		public BezierPoint cp2;

		// Default constructor
		public SampledSegment(Vector3 _p1, Vector3 _p2, BezierPoint _cp1, BezierPoint _cp2) {
			p1 = _p1;
			p2 = _p2;
			cp1 = _cp1;
			cp2 = _cp2;
		}
	}
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Drawing parameters
	public Color drawColor = Color.white;
	public bool lockZ = false;	// Whether to allow editing the Z value of the points or not - useful for 2D curves

	/// <summary>
	/// Control points of the curve.
	/// </summary>
	[SerializeField] private List<BezierPoint> m_points = new List<BezierPoint>();	// Array of point objects that make up this curve
	public List<BezierPoint> points { 
		get { return m_points; }
	}

	/// <summary>
	/// Segments computed every time the curve is dirty using the target resolution.
	/// </summary>
	private List<SampledSegment> m_sampledSegments = new List<SampledSegment>();
	public List<SampledSegment> sampledSegments { 
		get { return m_sampledSegments; }
	}

	/// <summary>
	/// Whether this <see cref="AOCBezierCurve"/> is dirty or not. Set internally.
	/// </summary>
	public bool dirty {
		get; 
		private set; 
	}

	/// <summary>
	/// The number of mid-points calculated for each pair of Bézier points. Used 
	/// for drawing the curve and to calculate the "length" of the curve.
	/// </summary>
	[SerializeField] private int m_resolution = 30;
	public int resolution {
		get { return m_resolution; }
		set {
			if(value <= 0) return;
			m_resolution = value;
			SetDirty();
		}
	}

	/// <summary>
	/// Used to determine if the curve should be drawn as "closed" in the editor
	/// Used to determine if the curve's length should include the curve between the first and the last points in "points" array
	/// Setting this value will cause the curve to become dirty
	/// </summary>
	[SerializeField] private bool _close;
	public bool closed {
		get { return _close; }
		set {
			if(_close == value) return;
			_close = value;
			SetDirty();
		}
	}

	/// <summary>
	/// Number of points stored in 'points' variable
	///	Set internally
	///	Does not include "handles"
	/// </summary>
	public int pointCount {
		get { return m_points.Count; }
	}

	/// <summary>
	/// The approximate length of the curve.
	/// </summary>
	private float m_length = -1;
	public float length {
		get {
			// If length hasn't yet been computed, do it now
			if(m_length <= 0) ComputeLength();
			return m_length;
		}
	}

	// Auto Smooth
	/// <summary>
	/// If enabled, handle points will be automatically computed.
	/// </summary>
	private bool m_autoSmooth = true;
	public bool autoSmooth {
		get { return m_autoSmooth; }
		set { 
			if(value != m_autoSmooth) SetDirty();
			m_autoSmooth = value;
		}
	}

	/// <summary>
	/// Amount of auto-smoothing.
	/// </summary>
	private float m_autoSmoothFactor = 0.33f;	// 0.33f turns out to be quite balanced
	public float autoSmoothFactor {
		get { return m_autoSmoothFactor; }
		set {
			if(value != m_autoSmoothFactor) SetDirty();
			m_autoSmoothFactor = value;
		}
	}
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Make sure length is ok
		ComputeLength();
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void LateUpdate() {
		// If curve is dirty, recalculate length and sample segments
		if(dirty) {
			// Do it
			ComputeLength();

			// If auto-smooth is enabled, apply it
			if(autoSmooth) AutoSmooth(autoSmoothFactor);

			// Not dirty anymore :)
			dirty = false;
		}
	}

	/// <summary>
	/// Sets this curve to 'dirty'.
	/// Forces the curve to recalculate its length when requested.
	/// </summary>
	public void SetDirty() {
		dirty = true;
	}

	/// <summary>
	/// Computes the total length of the curve using designated resolution.
	/// Computes sample segments.
	/// Do not abuse calling this method!
	/// </summary>
	private void ComputeLength() {
		// Compute new length and sample new segments
		// Add length between every control point
		// Only if the curve has enough control points
		m_length = 0;
		m_sampledSegments = new List<SampledSegment>();
		if(m_points.Count > 1) {
			// Some aux vars
			BezierPoint p1;
			BezierPoint p2;
			float fResolution = (float)resolution;	// To avoid doing the cast every time
			int samplingLoops = resolution + 1;

			// Loop through all control points
			int numSections = closed ? m_points.Count : m_points.Count - 1;	// If closed, compute segment between last and first point as well
			for(int i = 0; i < numSections; i++){
				// Aux vars
				p1 = m_points[i];
				p2 = m_points[(i + 1) % m_points.Count];

				// Add length
				m_length += ApproximateLength(p1, p2, resolution);

				// Compute samples between this point and the next one
				Vector3 currentPos = Vector3.zero;
				Vector3 lastPos = p1.globalPosition;
				for(int j = 0; j < samplingLoops; j++) {
					// Do it!
					currentPos = GetValue(p1, p2, j/fResolution);
					m_sampledSegments.Add(new SampledSegment(lastPos, currentPos, p1, p2));
					lastPos = currentPos;
				}
			}
		}
	}

	/// <summary>
	/// Automatically adjust all handlers to get a smooth curve.
	/// Roughly based on http://devmag.org.za/2011/06/23/bzier-path-algorithms/
	/// </summary>
	/// <param name="_factor">Curvature factor.</param>
	public void AutoSmooth(float _factor) {
		// Figure out handles position based on previous and next point
		int numPoints = points.Count;
		int i0 = numPoints - 1;
		int i1 = 0;
		int i2 = 1;
		BezierPoint p0 = null;
		BezierPoint p1 = null;
		BezierPoint p2 = null;
		for(int i = 0; i < numPoints; i++) {
			// Based on http://devmag.org.za/2011/06/23/bzier-path-algorithms/
			// Get target point, previous one and next one
			p0 = GetPoint(i0);
			p1 = GetPoint(i1);
			p2 = GetPoint(i2);

			// Force unlock and connected style
			bool wasLocked = p1.locked;
			p1.locked = false;

			// Handle 2 is automatically computed (CONNECTED style forced)
			// Special cases for first and last points (if the curve is not closed)
			if(i1 == 0 && !closed) {	// First point
				p1.handleStyle = BezierPoint.HandleStyle.BROKEN;
				Vector3 tangent = p2.position - p1.position;
				p1.handle1 = _factor * tangent;
				p1.handle2 = Vector3.zero;
			} else if(i1 == numPoints - 1 && !closed) {	// Last point
				p1.handleStyle = BezierPoint.HandleStyle.BROKEN;
				Vector3 tangent = p1.position - p0.position;
				p1.handle1 = -_factor * tangent;
				p1.handle2 = Vector3.zero;
			} else {	// Rest of the points
				p1.handleStyle = BezierPoint.HandleStyle.CONNECTED;
				Vector3 tangent = (p2.position - p0.position).normalized;
				p1.handle1 = -_factor * tangent * (p1.position - p0.position).magnitude;
			}

			// Restore lock state
			p1.locked = wasLocked;

			// Increase indexes
			i0 = (i0 + 1) % numPoints;
			i1 = (i1 + 1) % numPoints;
			i2 = (i2 + 1) % numPoints;
		}
	}

	//------------------------------------------------------------------//
	// POINT MANAGEMENT													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Clear the points list in this curve.
	/// </summary>
	public void Clear() {
		// Just do it
		m_points.Clear();
	}

	/// <summary>
	/// Add a point with the given 3D position to the curve at the given index.
	/// </summary>
	/// <param name="_pos">World position of the new point.</param>
	/// <param name="_idx">Index where to be inserted. Default at the end of the list.</param>
	public BezierPoint AddPoint(Vector3 _pos, int _idx = -1) {
		// Just create a new point using the given position
		// Transform to local coords first
		_pos = transform.InverseTransformPoint(_pos);
		BezierPoint newPoint = new BezierPoint(_pos);
		AddPoint(newPoint, _idx);
		return newPoint;
	}

	/// <summary>
	/// Add a point to the curve at the given index.
	/// </summary>
	/// <param name="_pos">The point to be added.</param>
	/// <param name="_idx">Index where to be inserted. Default at the end of the list.</param>
	public void AddPoint(BezierPoint _point, int _idx = -1) {
		// Check point
		if(_point == null) return;

		// If point is already on the curve, remove it first
		m_points.Remove(_point);

		// If index not valid, insert at the end of the list
		if(_idx < 0 || _idx >= m_points.Count) {
			m_points.Add(_point);
		} else {
			m_points.Insert(_idx, _point);
		}

		// Update point's curve. Mark curve as dirty.
		_point.curve = this;
		dirty = true;
	}

	/// <summary>
	/// Remove a given point from the curve. If the point doesn't belong to the curve,
	/// nothing will happen.
	/// </summary>
	/// <param name="_point">The point to be removed.</param>
	public void RemovePoint(BezierPoint _point) {
		if(m_points.Remove(_point)) {
			dirty = true;	// [AOC] CHECK!! Original puts that to false
		}
	}

	/// <summary>
	/// Remove the point at the given index.
	/// If index not valid, last point in the list will be removed.
	/// </summary>
	/// <param name="_idx">The index to be removed.</param>
	public void RemovePoint(int _idx = -1) {
		// Skip if there are no points to remove
		if(m_points.Count == 0) return;

		// If index is not valid, remove last point
		if(_idx < 0 || _idx >= m_points.Count) _idx = m_points.Count - 1;

		// Do it
		m_points.RemoveAt(_idx);
		dirty = true;	// [AOC] CHECK!! Original puts that to false
	}

	/// <summary>
	/// Get the point at the given index.
	/// </summary>
	/// <returns>The <see cref="AOCBezierPoint"/> at the given index. <c>null</c> if index not valid.</returns>
	/// <param name="_idx">Index of the point to be returned.</param>
	public BezierPoint GetPoint(int _idx) {
		if(_idx < 0 || _idx >= m_points.Count) return null;
		return m_points[_idx];
	}

	/// <summary>
	/// Find out the index of a given point in this curve.
	/// </summary>
	/// <returns>The point index, -1 if the point is not found.</returns>
	/// <param name="_point">The point to search for.</param>
	public int GetPointIdx(BezierPoint _point) {
		return m_points.IndexOf(_point);
	}

	/// <summary>
	/// Find the control point closest to a given curve delta.
	/// </summary>
	/// <returns>The index of the point closest to the given delta value.</returns>
	/// <param name="_t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%).</param>
	public int GetPointAt(float _t) {
		// Check params
		if(_t <= 0f) return 0;
		if(_t >= 1f) return closed ? 0 : m_points.Count - 1;	// If closed, last point is the first one!

		// Manual search
		float d2 = GetDelta(0);
		float d1 = d2;
		for(int i = 1; i < m_points.Count; i++) {
			// Get next delta
			d1 = d2;
			d2 = GetDelta(i);

			// Is target _t between these points?
			if(_t >= d1 && _t < d2) {
				// YES! Find closest point
				if(_t - d1 < d2 - _t) {
					return i - 1;
				} else {
					return i;
				}
			}
		}

		// If the curve is closed and we didn't find the target points, it means that the given delta belongs to the last segment of curve (between last and first points)
		if(closed) {
			d1 = d2;	// Last point's delta, computed during the previous loop
			d2 = 1f;	// First point has delta 1

			// Find closest point
			if(_t - d1 < d2 - _t) {
				return m_points.Count - 1;
			} else {
				return 0;	// First point
			}
		}

		// This point should never be reached
		return m_points.Count - 1;
	}

	//------------------------------------------------------------------//
	// CURVE NAVIGATION													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Gets position at 't' percent along this curve.
	/// </summary>
	/// <returns>>Returns the world position of point at 't' percent in the curve.</returns>
	/// <param name="_t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%).</param>
	public Vector3 GetValue(float _t) {
		// Check params
		if(_t <= 0f) return m_points[0].globalPosition;
		if(_t >= 1f) return closed ? m_points[0].globalPosition : m_points[m_points.Count - 1].globalPosition;	// If closed, last point is the first one

		// Here starts the black magic from the original script
		// Aux vars
		float processedDelta = 0f;
		float segmentDelta = 0f;
		float segmentLength = 0f;
		BezierPoint p1 = null;
		BezierPoint p2 = null;

		// Iterate all the points up to the last one
		for(int i = 0; i < m_points.Count - 1; i++) {
			// Compute percentage equivalent to the curve between current point and the next one
			segmentLength = ApproximateLength(m_points[i], m_points[i + 1], resolution);
			segmentDelta = segmentLength / length;

			// Have we reached requested delta?
			if(processedDelta + segmentDelta >= _t) {
				// Yes!! Store target points
				p1 = m_points[i];
				p2 = m_points[i + 1];
				break;
			} else {
				// No! Increase traversed percentage and keep looping
				processedDelta += segmentDelta;
			}
		}

		// If the curve is closed and we didn't find the target points, it means that the given delta belongs to the last segment of curve (between last and first points)
		if(closed && p1 == null) {
			// Select points
			p1 = m_points[m_points.Count - 1];
			p2 = m_points[0];

			// Compute required data
			segmentLength = ApproximateLength(p1, p2, resolution);
			segmentDelta = segmentLength / length;
		}

		// Compute relative remaining percent between the two points and get the value at that segment
		_t -= processedDelta;
		return GetValue(p1, p2, _t/segmentDelta);
	}

	/// <summary>
	/// Obtain the delta [0..1] corresponding to the given control point index.
	/// </summary>
	/// <returns>The relative position within the curve where the point with the given index is.</returns>
	/// <param name="_pointIdx">The point whose delta we want to know.</param>
	public float GetDelta(int _pointIdx) {
		// Check params
		if(_pointIdx < 0) return 0f;
		if(_pointIdx >= m_points.Count) return 1f;

		// Compute the length from the beginning up to this point
		float totalLength = 0f;
		for(int i = 1; i <= _pointIdx; i++) {
			totalLength += ApproximateLength(m_points[i - 1], m_points[i], resolution);
		}

		// Compare it to total length
		return totalLength/length;
	}

	//------------------------------------------------------------------//
	// ISerializationCallbackReceiver IMPLEMENTATION					//
	//------------------------------------------------------------------//
	/// <summary>
	/// Serialization is about to start.
	/// </summary>
	public void OnBeforeSerialize() {
		// Nothing to do
	}

	/// <summary>
	/// Deserialization just finished.
	/// </summary>
	public void OnAfterDeserialize() {
		// Make sure all points in the list are valid and have the correct reference to the parent curve
		for(int i = 0; i < m_points.Count; i++) {
			if(m_points[i] == null) {
				m_points.RemoveAt(i);
				i--;
			} else {
				m_points[i].ConsolidateCurve(this);
			}
		}
		SetDirty();
	}

	//------------------------------------------------------------------//
	// PUBLIC STATIC METHODS											//
	//------------------------------------------------------------------//
	/// <summary>
	/// Compute the global position equivalent to a given position between two control points.
	/// Automatically calculates for the number of relevant handlers to be used.
	/// </summary>
	/// <returns>The global position of the point 't' percent along the curve segment between <paramref name="_p1"/> amd <paramref name="_p2"/>.</returns>
	/// <param name="_p1">The Bézier point at the beginning of the curve segment.</param>
	/// <param name="_p2">The Bézier point at the end of the curve segment.</param>
	/// <param name="_t">Value between 0 and 1 representing the percent along the curve segment (0 = 0%, 1 = 100%) to be computed.</param>
	public static Vector3 GetValue(BezierPoint _p1, BezierPoint _p2, float _t) {
		// Use different methods depending on point handlers
		// Black magic from the source
		if(_p1.handle2 != Vector3.zero) {
			if(_p2.handle1 != Vector3.zero){
				return GetCubicCurvePoint(_p1.globalPosition, _p1.globalHandle2, _p2.globalHandle1, _p2.globalPosition, _t);
			} else {
				return GetQuadraticCurvePoint(_p1.globalPosition, _p1.globalHandle2, _p2.globalPosition, _t);
			}
		} else {
			if(_p2.handle1 != Vector3.zero) {
				return GetQuadraticCurvePoint(_p1.globalPosition, _p2.globalHandle1, _p2.globalPosition, _t);
			} else {
				return GetLinearPoint(_p1.globalPosition, _p2.globalPosition, _t);
			}
		}
	}

	/// <summary>
	/// Gets the point 't' percent along a third-order curve.
	/// </summary>
	/// <returns>The global position of the point 't' percent along the curve formed by the input points.</returns>
	/// <param name="_p1">The point at the beginning of the curve.</param>
	/// <param name="_p2">The second point along the curve.</param>
	/// <param name="_p3">The third point along the curve.</param>
	/// <param name="_p4">The point at the end of the curve.</param>
	/// <param name="_t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%).</param>
	public static Vector3 GetCubicCurvePoint(Vector3 _p1, Vector3 _p2, Vector3 _p3, Vector3 _p4, float _t) {
		// Check params
		_t = Mathf.Clamp01(_t);

		// Black magic from the source
		Vector3 part1 = Mathf.Pow(1 - _t, 3) * _p1;
		Vector3 part2 = 3 * Mathf.Pow(1 - _t, 2) * _t * _p2;
		Vector3 part3 = 3 * (1 - _t) * Mathf.Pow(_t, 2) * _p3;
		Vector3 part4 = Mathf.Pow(_t, 3) * _p4;
		return part1 + part2 + part3 + part4;
	}

	/// <summary>
	/// Gets the point 't' percent along a second-order curve.
	/// </summary>
	/// <returns>The global position of the point 't' percent along the curve formed by the input points.</returns>
	/// <param name="_p1">The point at the beginning of the curve.</param>
	/// <param name="_p2">The second point along the curve.</param>
	/// <param name="_p3">The point at the end of the curve.</param>
	/// <param name="_t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%).</param>
	public static Vector3 GetQuadraticCurvePoint(Vector3 _p1, Vector3 _p2, Vector3 _p3, float _t) {
		// Check params
		_t = Mathf.Clamp01(_t);

		// Black magic from the source
		Vector3 part1 = Mathf.Pow(1 - _t, 2) * _p1;
		Vector3 part2 = 2 * (1 - _t) * _t * _p2;
		Vector3 part3 = Mathf.Pow(_t, 2) * _p3;
		return part1 + part2 + part3;
	}

	/// <summary>
	/// Gets the point 't' percent along a linear "curve" (straight line).
	/// This is exactly equivalent to Vector3.Lerp.
	/// </summary>
	/// <returns>The global position of the point 't' percent along the curve formed by the input points.</returns>
	/// <param name="_p1">The point at the beginning of the curve.</param>
	/// <param name="_p2">The point at the end of the curve.</param>
	/// <param name="_t">Value between 0 and 1 representing the percent along the curve (0 = 0%, 1 = 100%).</param>
	public static Vector3 GetLinearPoint(Vector3 _p1, Vector3 _p2, float _t) {
		// Check params
		_t = Mathf.Clamp01(_t);

		// Black magic from the source
		return _p1 + ((_p2 - _p1) * _t);
	}

	/// <summary>
	/// Approximates the length of the curve formed by two points.
	/// </summary>
	/// <returns>The approximated length of the curve formed by <paramref name="_p1"/> and <paramref name="_p2"/>.</returns>
	/// <param name="_p1">The bezier point at the start of the curve.</param>
	/// <param name="_p2">The bezier point at the end of the curve.</param>
	/// <param name="_resolution">The number of points along the curve used to create measurable segments.</param>
	public static float ApproximateLength(BezierPoint _p1, BezierPoint _p2, int _resolution = 10) {
		// Black magic from source (not so black :D)
		float _res = _resolution;
		float total = 0;
		Vector3 lastPosition = _p1.globalPosition;
		Vector3 currentPosition;
		for(int i = 0; i < _resolution + 1; i++) {
			currentPosition = GetValue(_p1, _p2, i / _res);
			total += (currentPosition - lastPosition).magnitude;
			lastPosition = currentPosition;
		}

		return total;
	}

	//------------------------------------------------------------------//
	// OPERATORS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Get the point at the given index.
	///	Does not allow direct set, set internally.
	/// </summary>
	/// <returns>The <see cref="AOCBezierPoint"/> at the given index. <c>null</c> if index not valid.</returns>
	/// <param name="_idx">Index of the point to be returned.</param>
	public BezierPoint this[int _idx] {
		get { return GetPoint(_idx); }
	}

	//------------------------------------------------------------------//
	// DRAWING															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Scene gizmos drawing
	/// </summary>
	private void OnDrawGizmos() {
		if(!isActiveAndEnabled) return;

		#if UNITY_EDITOR
		// Draw line
		Handles.color = drawColor;
		Vector3[] sampledPoints = new Vector3[m_sampledSegments.Count];
		for(int i = 0; i < m_sampledSegments.Count; i++) {
			sampledPoints[i] = m_sampledSegments[i].p1;
		}
	 	Handles.DrawAAPolyLine(5f, sampledPoints);

		// Draw points and handlers
		BezierPoint p;
		for(int i = 0; i < pointCount; i++) {
			p = GetPoint(i);

			// The point itself
			Handles.color = Colors.white;
			Handles.SphereCap(0, p.globalPosition, Quaternion.identity, 1f);

			// Handlers
			if(p.handleStyle != BezierPoint.HandleStyle.NONE) {
				Handles.color = Colors.skyBlue;

				// Handler 1
				Handles.DrawLine(p.globalPosition, p.globalHandle1);
				Handles.SphereCap(0, p.globalHandle1, Quaternion.identity, 0.5f);

				// Handler 2
				Handles.DrawLine(p.globalPosition, p.globalHandle2);
				Handles.SphereCap(0, p.globalHandle2, Quaternion.identity, 0.5f);
			}

			// Label
			Handles.Label(p.globalPosition, i.ToString(), CustomEditorStyles.bigSceneLabel);
		}
		#endif
	}
}