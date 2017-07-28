using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class RailMeshBuilder : MonoBehaviour {

	[SerializeField] private BSpline.BezierSpline m_spline;
	[SerializeField] private float m_subdivisions = 10f;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update() {
		if (m_spline != null) {
			List<Vector3> verticesLeft = new List<Vector3>();
			List<Vector3> verticesRight = new List<Vector3>();

			for (float t = 0f; t <= 1f; t += 1f / m_subdivisions) {
				Vector3 point = m_spline.GetPoint(t);
				Vector3 forward = m_spline.GetDirection(t);
				Vector3 up = m_spline.GetUpVector(t);

				Vector3 right = Vector3.Cross(forward, up);

				CreateRailAt(point + right * 0.325f, up, -right, ref verticesRight);
				CreateRailAt(point - right * 0.325f, up, -right, ref verticesLeft);
			}

			MeshFilter meshFilter = GetComponent<MeshFilter>();
			if (meshFilter.sharedMesh != null) meshFilter.sharedMesh = null;

			Mesh mesh = meshFilter.sharedMesh;
			if (mesh == null) {
				mesh = new Mesh();
				mesh.name = name + "_Mesh";
				meshFilter.mesh = mesh;
			}

			CombineInstance[] combine = new CombineInstance[2];
			combine[0].mesh = new Mesh();
			combine[0].mesh.vertices = verticesRight.ToArray();
			combine[0].mesh.triangles = Triangulate(verticesRight);
			combine[0].mesh.RecalculateNormals();

			combine[1].mesh = new Mesh();
			combine[1].mesh.vertices = verticesLeft.ToArray();
			combine[1].mesh.triangles = Triangulate(verticesLeft);
			combine[1].mesh.RecalculateNormals();

			mesh.CombineMeshes(combine, true, false);
		}
	}

	private void CreateRailAt(Vector3 _point, Vector3 _up, Vector3 _right, ref List<Vector3> _vertices) {
		_vertices.Add(_point + _up * 0.10f - _right * 0.04f);
		_vertices.Add(_point + _up * 0.10f + _right * 0.04f);
		_vertices.Add(_point + _up * 0.00f + _right * 0.05f);
		_vertices.Add(_point + _up * 0.00f + _right * 0.10f);
		_vertices.Add(_point - _up * 0.05f + _right * 0.10f);
		_vertices.Add(_point - _up * 0.05f - _right * 0.10f);
		_vertices.Add(_point + _up * 0.00f - _right * 0.10f);
		_vertices.Add(_point + _up * 0.00f - _right * 0.05f);
	}

	private int[] Triangulate(List<Vector3> _vertices) {
		List<int> triangles = new List<int>();

		for (int i = 0; i < _vertices.Count - 8; i += 1) {
			triangles.Add(i);
			triangles.Add(i + 8);
			if ((i + 1) % 8 == 0) 
				triangles.Add(i + 1 + 8 - 8); //start of the current loop
			else
				triangles.Add(i + 1 + 8);

			triangles.Add(i);
			if ((i + 1) % 8 == 0)  {
				triangles.Add(i + 1 + 8 - 8);
				triangles.Add(i + 1 - 8);
			} else {
				triangles.Add(i + 1 + 8);
				triangles.Add(i + 1);
			}
		}

		return triangles.ToArray();
	}
}
