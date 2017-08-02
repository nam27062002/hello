using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class RailMeshBuilder : MonoBehaviour {

	[SerializeField] private BSpline.BezierSpline m_spline;
	[SerializeField] private float m_subdivisions = 10f;
	[SerializeField] private float m_uvScale = 1f;

	private float m_currentSubdivisions = 0f;
	private float m_currentUVscale = 0f;





	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update() {
		if (m_spline != null) {
			bool isDirty = m_spline.isDirty3D || (m_currentSubdivisions != m_subdivisions) || (m_currentUVscale != m_uvScale);

			if (isDirty) {
				m_currentSubdivisions = m_subdivisions;
				m_currentUVscale = m_uvScale;

				List<Vector3> verticesLeft = new List<Vector3>();
				List<Vector3> verticesRight = new List<Vector3>();

				float distance = 0f;
				float step = m_spline.length / m_subdivisions;
				for (distance = 0f; distance < m_spline.length; distance += step) {
					Vector3 dir = Vector3.zero;
					Vector3 up = Vector3.zero;
					Vector3 right = Vector3.zero;
					Vector3 point = m_spline.GetPointAtDistance(distance, ref dir, ref up, ref right);

					CreateRailAt(point + right * 0.325f, up, right, ref verticesRight);
					CreateRailAt(point - right * 0.325f, up, right, ref verticesLeft);
				}

				Vector3 forwardLast = Vector3.zero;
				Vector3 upLast = Vector3.zero;
				Vector3 rightLast = Vector3.zero;
				Vector3 pointLast = m_spline.GetPointAtDistance(distance, ref forwardLast, ref upLast, ref rightLast);

				CreateRailAt(pointLast + rightLast * 0.325f, upLast, rightLast, ref verticesRight);
				CreateRailAt(pointLast - rightLast * 0.325f, upLast, rightLast, ref verticesLeft);

				MeshFilter meshFilter = GetComponent<MeshFilter>();
				if (meshFilter.sharedMesh != null) meshFilter.sharedMesh = null;

				Mesh mesh = meshFilter.sharedMesh;
				if (mesh == null) {
					mesh = new Mesh();
					mesh.name = name + "_Mesh";
					meshFilter.mesh = mesh;
				}

				float distanceBetweenTies = 1f;
				int woodTieCount = (int)(m_spline.length / distanceBetweenTies);

				CombineInstance[] combine = new CombineInstance[2 + woodTieCount];
				combine[0].mesh = new Mesh();
				combine[0].mesh.vertices = verticesRight.ToArray();
				combine[0].mesh.triangles = Triangulate(verticesRight);
				combine[0].mesh.RecalculateNormals();
				combine[0].mesh.RecalculateBounds();
				combine[0].mesh.SetUVs(0, CreateRailUVs(verticesRight.Count));

				combine[1].mesh = new Mesh();
				combine[1].mesh.vertices = verticesLeft.ToArray();
				combine[1].mesh.triangles = Triangulate(verticesLeft);
				combine[1].mesh.RecalculateNormals();
				combine[1].mesh.RecalculateBounds();
				combine[1].mesh.SetUVs(0, CreateRailUVs(verticesLeft.Count));

				float dist = distanceBetweenTies * 0.5f;
				for (int i = 0; i < woodTieCount; ++i) {
					Vector3 dir = Vector3.zero;
					Vector3 up = Vector3.zero;
					Vector3 right = Vector3.zero;
					Vector3 point = m_spline.GetPointAtDistance(dist, ref dir, ref up, ref right);

					combine[2 + i].mesh = CreateWoodTie(point, up, right, dir);

					dist += distanceBetweenTies;
				}

				mesh.CombineMeshes(combine, true, false);
			}
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

	private List<Vector2> CreateRailUVs(int vCount) {
		List<Vector2> uvs = new List<Vector2>();
		float vStep = (1f / m_uvScale);
		float v = 0;

		for (int i = 0; i < vCount; i += 8) {

			v += vStep;
			if (v > 1f)
				v -= 1f;

			for (int c = 0; c < 8; c += 2) {
				uvs.Add(new Vector2(0, v));
				uvs.Add(new Vector2(0.5f, v));
			}
		}

		return uvs;
	}

	private Mesh CreateWoodTie(Vector3 _point, Vector3 _up, Vector3 _right, Vector3 _dir) {
		List<Vector3> vertices = new List<Vector3>();
		// top
		vertices.Add(_point - _up * 0.05f - _right * 0.70f + _dir * 0.22f);
		vertices.Add(_point - _up * 0.05f + _right * 0.70f + _dir * 0.22f);
		vertices.Add(_point - _up * 0.05f + _right * 0.70f - _dir * 0.22f);
		vertices.Add(_point - _up * 0.05f - _right * 0.70f - _dir * 0.22f);

		// bottom
		vertices.Add(_point - _up * 0.12f - _right * 0.70f + _dir * 0.22f);
		vertices.Add(_point - _up * 0.12f + _right * 0.70f + _dir * 0.22f);
		vertices.Add(_point - _up * 0.12f + _right * 0.70f - _dir * 0.22f);
		vertices.Add(_point - _up * 0.12f - _right * 0.70f - _dir * 0.22f);

		int[] triangles = {	0, 1, 2, 2, 3, 0, // top
							5, 4, 6, 6, 4, 7, // bottom
							1, 0, 4, 5, 1, 4,
							3, 2, 6, 6, 7, 3,
							2, 1, 5, 5, 6, 2,
							0, 3, 7, 7, 4, 0};

		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		mesh.SetUVs(0, CreateTieUVs());

		return mesh;
	}

	private List<Vector2> CreateTieUVs() {
		List<Vector2> uvs = new List<Vector2>();
		uvs.Add(new Vector2(1.0f, 0.0f));
		uvs.Add(new Vector2(1.0f, 1.0f));
		uvs.Add(new Vector2(0.5f, 1.0f));
		uvs.Add(new Vector2(0.5f, 0.0f));

		uvs.Add(new Vector2(1.0f, 0.0f));
		uvs.Add(new Vector2(1.0f, 1.0f));
		uvs.Add(new Vector2(0.5f, 1.0f));
		uvs.Add(new Vector2(0.5f, 0.0f));

		return uvs;
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
