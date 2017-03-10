// PolyMesh.cs
// 
// Created by Alger Ortín Castellví on 20/05/2016, imported from HSX project.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Poly mesh.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]	// [AOC] We will have the collider to the same object containing the polymesh, for clarity and keeping the hierarchy clean
public class PolyMesh : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Simple Bezier implementation to auxiliate the poly mesh.
	/// </summary>
	public static class Bezier {
		//--------------------------------------------------------------------//
		// METHODS															  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Curve the specified from, control, to and t.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="control">Control.</param>
		/// <param name="to">To.</param>
		/// <param name="t">T.</param>
		public static float Curve(float from, float control, float to, float t) {
			return from * (1 - t) * (1 - t) + control * 2 * (1 - t) * t + to * t * t;
		}

		/// <summary>
		/// Curve the specified from, control, to and t.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="control">Control.</param>
		/// <param name="to">To.</param>
		/// <param name="t">T.</param>
		public static Vector3 Curve(Vector3 from, Vector3 control, Vector3 to, float t) {
			from.x = Curve(from.x, control.x, to.x, t);
			from.y = Curve(from.y, control.y, to.y, t);
			from.z = Curve(from.z, control.z, to.z, t);
			return from;
		}

		/// <summary>
		/// Control the specified from, to and curve.
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="curve">Curve.</param>
		public static Vector3 Control(Vector3 from, Vector3 to, Vector3 curve) {
			//var center = Vector3.Lerp(from, to, 0.5f);
			//return center + (curve - center) * 2;
			var axis = Vector3.Normalize(to - from);
			var dot = Vector3.Dot(axis, curve - from);
			var linePoint = from + axis * dot;
			return linePoint + (curve - linePoint) * 2;
		}
	}

	/// <summary>
	/// Auxiliar class to the PolyMesh.
	/// </summary>
	public static class Triangulate {
		//--------------------------------------------------------------------//
		// METHODS															  //
		//--------------------------------------------------------------------//
		/// <summary>
		/// Points the specified points.
		/// </summary>
		/// <param name="points">Points.</param>
		public static int[] Points(List<Vector3> points) {
			var indices = new List<int>();

			int n = points.Count;
			if(n < 3)
				return indices.ToArray();

			int[] V = new int[n];
			if(Area(points) > 0) {
				for(int v = 0; v < n; v++)
					V[v] = v;
			}
			else {
				for(int v = 0; v < n; v++)
					V[v] = (n - 1) - v;
			}

			int nv = n;
			int count = 2 * nv;
			for(int m = 0, v = nv - 1; nv > 2;) {
				if((count--) <= 0)
					return indices.ToArray();

				int u = v;
				if(nv <= u)
					u = 0;
				v = u + 1;
				if(nv <= v)
					v = 0;
				int w = v + 1;
				if(nv <= w)
					w = 0;

				if(Snip(points, u, v, w, nv, V)) {
					int a, b, c, s, t;
					a = V[u];
					b = V[v];
					c = V[w];
					indices.Add(a);
					indices.Add(b);
					indices.Add(c);
					m++;
					for(s = v, t = v + 1; t < nv; s++, t++)
						V[s] = V[t];
					nv--;
					count = 2 * nv;
				}
			}

			indices.Reverse();
			return indices.ToArray();
		}

		/// <summary>
		/// Area the specified points.
		/// </summary>
		/// <param name="points">Points.</param>
		static float Area(List<Vector3> points) {
			int n = points.Count;
			float A = 0.0f;
			for(int p = n - 1, q = 0; q < n; p = q++) {
				Vector3 pval = points[p];
				Vector3 qval = points[q];
				A += pval.x * qval.y - qval.x * pval.y;
			}
			return (A * 0.5f);
		}

		/// <summary>
		/// Snip the specified points, u, v, w, n and V.
		/// </summary>
		/// <param name="points">Points.</param>
		/// <param name="u">U.</param>
		/// <param name="v">V.</param>
		/// <param name="w">The width.</param>
		/// <param name="n">N.</param>
		/// <param name="V">V.</param>
		static bool Snip(List<Vector3> points, int u, int v, int w, int n, int[] V) {
			int p;
			Vector3 A = points[V[u]];
			Vector3 B = points[V[v]];
			Vector3 C = points[V[w]];
			if(Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
				return false;
			for(p = 0; p < n; p++) {
				if((p == u) || (p == v) || (p == w))
					continue;
				Vector3 P = points[V[p]];
				if(InsideTriangle(A, B, C, P))
					return false;
			}
			return true;
		}

		/// <summary>
		/// Insides the triangle.
		/// </summary>
		/// <returns><c>true</c>, if triangle was insided, <c>false</c> otherwise.</returns>
		/// <param name="A">A.</param>
		/// <param name="B">B.</param>
		/// <param name="C">C.</param>
		/// <param name="P">P.</param>
		static bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P) {
			float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
			float cCROSSap, bCROSScp, aCROSSbp;

			ax = C.x - B.x;
			ay = C.y - B.y;
			bx = A.x - C.x;
			by = A.y - C.y;
			cx = B.x - A.x;
			cy = B.y - A.y;
			apx = P.x - A.x;
			apy = P.y - A.y;
			bpx = P.x - B.x;
			bpy = P.y - B.y;
			cpx = P.x - C.x;
			cpy = P.y - C.y;

			aCROSSbp = ax * bpy - ay * bpx;
			cCROSSap = cx * apy - cy * apx;
			bCROSScp = bx * cpy - by * cpx;

			return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
		}
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public List<Vector3> keyPoints = new List<Vector3>();
	public List<Vector3> curvePoints = new List<Vector3>();
	public List<bool> isCurve = new List<bool>();
	[Range(0.01f, 1)] public float curveDetail = 0.1f;

	public float colliderDepth = 10;
	public bool buildColliderEdges = true;
	public bool buildColliderFront;
	public Transform colliderParent;

	public Vector2 uvPosition;
	public float uvScale = 1;
	public float uvRotation;

	public bool showNormals = false;
	public bool showOutline = true;
	public float pinkMeshOffset = 0f;
	
	public GameObject mergeObject;
	public int mergeStartPoint;
	public int mergeEndPoint;

	//public MeshCollider meshCollider;		// [AOC] We will have the collider to the same object containing the polymesh, for clarity and keeping the hierarchy clean
	private MeshCollider m_meshCollider = null;
	public MeshCollider meshCollider {
		get {
			if(m_meshCollider == null) {
				m_meshCollider = GetComponent<MeshCollider>();
			}
			return m_meshCollider;
		}
		set
		{
			m_meshCollider = value;
		}
	}

	//------------------------------------------------------------------------//
	// POINT MANAGEMENT METHODS												  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Inverts the points.
	/// </summary>
	public void InvertPoints() {
		keyPoints.Reverse();
		curvePoints.Reverse();
	}

	/// <summary>
	/// Gets the edge points.
	/// </summary>
	/// <returns>The edge points.</returns>
	public List<Vector3> GetEdgePoints() {
		//Build the point list and calculate curves
		var points = new List<Vector3>();
		for(int i = 0; i < keyPoints.Count; i++) {
			if(isCurve[i]) {
				//Get the curve control point
				var a = keyPoints[i];
				var c = keyPoints[(i + 1) % keyPoints.Count];
				var b = Bezier.Control(a, c, curvePoints[i]);
				
				//Build the curve
				var count = Mathf.Ceil(1 / curveDetail);
				for(int j = 0; j < count; j++) {
					var t = (float)j / count;
					points.Add(Bezier.Curve(a, b, c, t));
				}
			}
			else
				points.Add(keyPoints[i]);
		}
		return points;
	}

	//------------------------------------------------------------------------//
	// MESH EDITING METHODS													  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Forces the creation of a new mesh.
	/// </summary>
	public void RebuildMesh() {
		// Clear mesh references from both mesh filter and collider
		MeshFilter meshFilter = GetComponent<MeshFilter>();
		if(meshFilter.sharedMesh != null) meshFilter.sharedMesh = null;

		if(meshCollider != null) {
			if(meshCollider.sharedMesh != null) meshCollider.sharedMesh = null;
		}

		// Build the mesh from scratch
		BuildMesh();
	}

	/// <summary>
	/// Builds the mesh.
	/// </summary>
	public void BuildMesh() {
		var points = GetEdgePoints();
		var vertices = points.ToArray();
		for(int i = 0; i < vertices.Length; i++) {
			vertices[i].z += pinkMeshOffset;
		}

		//Build the index array
		var indices = new List<int>();
		while(indices.Count < points.Count) {
			indices.Add(indices.Count);
		}

		//Build the triangle array
		var triangles = Triangulate.Points(points);
		
		//Build the uv array
		var scale = uvScale != 0 ? (1 / uvScale) : 0;
		var matrix = Matrix4x4.TRS(-uvPosition, Quaternion.Euler(0, 0, uvRotation), new Vector3(scale, scale, 1));
		var uv = new Vector2[points.Count];
		for(int i = 0; i < uv.Length; i++) {
			var p = matrix.MultiplyPoint(points[i]);
			uv[i] = new Vector2(p.x, p.y);
		}
		
		//Find the mesh (create it if it doesn't exist)
		var meshFilter = GetComponent<MeshFilter>();
		var mesh = meshFilter.sharedMesh;
		if(mesh == null) {
			mesh = new Mesh();
			mesh.name = "PolyMesh_Autogenerated_Mesh";
			meshFilter.mesh = mesh;
		}
		
		//Update the mesh
		mesh.Clear();
		mesh.vertices = vertices;
		mesh.uv = uv;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		;
		
		//Update collider after the mesh is updated
		UpdateCollider(points, triangles);
	}

	/// <summary>
	/// Updates the collider.
	/// </summary>
	/// <param name="points">Points.</param>
	/// <param name="tris">Tris.</param>
	void UpdateCollider(List<Vector3> points, int[] tris) {
		//Update the mesh collider if there is one
		if(meshCollider != null) {
			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			
			if(buildColliderEdges) {
				//Build vertices array
				var offset = new Vector3(0, 0, colliderDepth / 2);
				for(int i = 0; i < points.Count; i++) {
					vertices.Add(points[i] + offset);
					vertices.Add(points[i] - offset);
				}
				
				//Build triangles array
				for(int a = 0; a < vertices.Count; a += 2) {
					var b = (a + 1) % vertices.Count;
					var c = (a + 2) % vertices.Count;
					var d = (a + 3) % vertices.Count;
					triangles.Add(a);
					triangles.Add(c);
					triangles.Add(b);
					triangles.Add(c);
					triangles.Add(d);
					triangles.Add(b);
				}
			}
			
			if(buildColliderFront) {
				for(int i = 0; i < tris.Length; i++)
					tris[i] += vertices.Count;
				vertices.AddRange(points);
				triangles.AddRange(tris);
			}
			
			//Find the mesh (create it if it doesn't exist)
			var mesh = meshCollider.sharedMesh;
			if(mesh == null) {
				mesh = new Mesh();
				mesh.name = "PolyMesh_Autogenerated_Collider";
			}

			//Update the mesh
			mesh.Clear();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.RecalculateNormals();
			;
			meshCollider.sharedMesh = null;
			meshCollider.sharedMesh = mesh;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Determines whether this instance is right turn the specified points a b c.
	/// </summary>
	/// <returns><c>true</c> if this instance is right turn the specified points a b c; otherwise, <c>false</c>.</returns>
	/// <param name="points">Points.</param>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	/// <param name="c">C.</param>
	bool IsRightTurn(List<Vector3> points, int a, int b, int c) {
		var ab = points[b] - points[a];
		var bc = points[c] - points[b];
		return (ab.x * bc.y - ab.y * bc.x) < 0;
	}

	/// <summary>
	/// Intersectses the existing lines.
	/// </summary>
	/// <returns><c>true</c>, if existing lines was intersectsed, <c>false</c> otherwise.</returns>
	/// <param name="points">Points.</param>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	bool IntersectsExistingLines(List<Vector3> points, Vector3 a, Vector3 b) {
		for(int i = 0; i < points.Count; i++)
			if(LinesIntersect(points, a, b, points[i], points[(i + 1) % points.Count]))
				return true;
		return false;
	}

	/// <summary>
	/// Lineses the intersect.
	/// </summary>
	/// <returns><c>true</c>, if intersect was linesed, <c>false</c> otherwise.</returns>
	/// <param name="points">Points.</param>
	/// <param name="point1">Point1.</param>
	/// <param name="point2">Point2.</param>
	/// <param name="point3">Point3.</param>
	/// <param name="point4">Point4.</param>
	bool LinesIntersect(List<Vector3> points, Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4) {
		if(point1 == point3 || point1 == point4 || point2 == point3 || point2 == point4)
			return false;
		
		float ua = (point4.x - point3.x) * (point1.y - point3.y) - (point4.y - point3.y) * (point1.x - point3.x);
		float ub = (point2.x - point1.x) * (point1.y - point3.y) - (point2.y - point1.y) * (point1.x - point3.x);
		float denominator = (point4.y - point3.y) * (point2.x - point1.x) - (point4.x - point3.x) * (point2.y - point1.y);
		
		if(Mathf.Abs(denominator) <= 0.00001f) {
			if(Mathf.Abs(ua) <= 0.00001f && Mathf.Abs(ub) <= 0.00001f)
				return true;
		}
		else {
			ua /= denominator;
			ub /= denominator;
			
			if(ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
				return true;
		}
		
		return false;
	}

	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	void OnDrawGizmos() {

		showNormals = true;

		if(showOutline) {
			Gizmos.color = Color.magenta;
			float depth = -pinkMeshOffset * transform.lossyScale.z;
			Vector3 offset = transform.position; // fix for pink outline

			List<Vector3> points = GetEdgePoints();
			// transform points from local space to world space
			for( int i = 0; i< points.Count; i++  )
				points[i] = transform.TransformPoint( points[i]);

			for(int i = 0; i < points.Count - 1; i++) {
				Vector3 point1 = new Vector3(points[i].x, points[i].y, points[i].z - depth);
				Vector3 point2 = new Vector3(points[i + 1].x, points[i + 1].y, points[i + 1].z - depth);

				Gizmos.DrawLine(point1, point2);
				if(showNormals) {
					Gizmos.color = Color.yellow;
					Vector3 n = Vector3.Cross(Vector3.forward, point2 - point1).normalized;
					Gizmos.DrawLine((point1 + point2) / 2.0f, (n * 3.0f) + (point1 + point2) / 2.0f);
					Gizmos.color = Color.magenta;
				}
			}
			int last = points.Count - 1;
			if(last > 0) {
				Vector3 startPoint = new Vector3(points[last].x, points[last].y, points[last].z - depth);
				Vector3 endPoint = new Vector3(points[0].x, points[0].y, points[0].z - depth);
				Gizmos.DrawLine(startPoint, endPoint);
				if(showNormals) {
					Gizmos.color = Color.yellow;
					Vector3 n = Vector3.Cross(Vector3.forward, endPoint - startPoint).normalized;
					Gizmos.DrawLine((startPoint + endPoint) / 2.0f, (n * 3.0f) + (startPoint + endPoint) / 2.0f);
					Gizmos.color = Color.magenta;
				}
			}
		}
	}

	/// <summary>
	/// Merges the meshes.
	/// </summary>
	public void MergeMeshes() {
		if(mergeObject) {
			List<Vector3> points = mergeObject.GetComponent<PolyMesh>().keyPoints;
			
			int myStartPoint = mergeStartPoint; //selectedIndices[0];
			int myEndPoint = mergeEndPoint; //selectedIndices[1];
			
			int otherStartPoint = 0;
			int otherEndPoint = 0;
			for(int i = 1; i < points.Count; i++) {
				if(Vector3.Distance(points[i], keyPoints[myStartPoint]) < Vector3.Distance(points[otherStartPoint], keyPoints[myStartPoint])) {
					otherStartPoint = i;
				}

				if(Vector3.Distance(points[i], keyPoints[myEndPoint]) < Vector3.Distance(points[otherEndPoint], keyPoints[myEndPoint])) {
					otherEndPoint = i;
				}
			}
			
			// remove start and end points (remove points inbetween)
			int count = myEndPoint < myStartPoint ? keyPoints.Count - myStartPoint : myEndPoint - myStartPoint + 1;
			keyPoints.RemoveRange(myStartPoint, count);
			if(myEndPoint < myStartPoint) {
				keyPoints.RemoveRange(0, myEndPoint + 1);
				myStartPoint -= myEndPoint + 1;
			}
			
			// loop through the rest of the points as the start is different
			for(int i = 0; i < points.Count; i++) {
				int ii = (i + otherStartPoint) % points.Count;
				
				// no wrap around
				bool mergePoint = ii >= otherStartPoint && ii <= (otherEndPoint < otherStartPoint ? points.Count - 1 : otherEndPoint);

				// if we have a wrap around
				if(i + otherStartPoint >= points.Count) {
					mergePoint = ii < otherEndPoint;
				}
				
				if(mergePoint) {
					Vector3 mPoint = points[ii];
					//mousePosition = mPoint;
					//nearestLine = NearestLine(out mPoint);
					
					int line = myStartPoint + i;
					if(line == keyPoints.Count + 1) {
						keyPoints.Add(mPoint);
						curvePoints.Add(Vector3.zero);
						isCurve.Add(false);
					}
					else {
						keyPoints.Insert(line, mPoint);
						curvePoints.Insert(line, Vector3.zero);
						isCurve.Insert(line, false);
					}
					
					mPoint = points[ii];
					BuildMesh();
				} else {
					Debug.Log("Chucked Away: " + ii);
				}
			}
		}
	}
}
