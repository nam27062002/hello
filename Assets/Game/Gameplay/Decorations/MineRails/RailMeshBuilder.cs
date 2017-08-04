using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(BSpline.BezierSpline))]
public class RailMeshBuilder : MonoBehaviour {

	[SerializeField] private BSpline.BezierSpline m_spline;
	[SerializeField] private float m_subdivisions = 10f;
	[SerializeField] private float m_distancePerGameObject = 3f;
	[SeparatorAttribute]
	[SerializeField] private int m_tieCountPerGameObject = 1;
	[SerializeField] private float m_meshScale = 1f;
	[SerializeField] private float m_uvScale = 1f;
	[SerializeField] private Material m_material;

	private float m_currentSubdivisions = 0f;
	private int	  m_currentTieCountPerGameObject = 0;
	private float m_currentMeshScale = 0f;
	private float m_currentUVscale = 0f;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update() {
		if (Application.isEditor) {
			if (m_spline != null) {
				bool isDirty = m_spline.isDirty3D || (m_currentSubdivisions != m_subdivisions) || (m_currentTieCountPerGameObject != m_tieCountPerGameObject) || (m_currentMeshScale != m_meshScale) || (m_currentUVscale != m_uvScale);

				if (isDirty) {					
					m_currentTieCountPerGameObject = m_tieCountPerGameObject;
					m_currentSubdivisions = m_subdivisions;
					m_currentMeshScale = m_meshScale;
					m_currentUVscale = m_uvScale;

					while (transform.childCount > 0) {
						Transform child = transform.GetChild(0);
						child.parent = null;
						GameObject.DestroyImmediate(child.gameObject);
					}

					float dist = 0;
					while ((dist + m_distancePerGameObject) < m_spline.length) {
						BuildRail(dist, dist + m_distancePerGameObject, m_subdivisions);
						dist += m_distancePerGameObject;
					}
					BuildRail(dist, m_spline.length, m_subdivisions * 0.5f);
				}
			}
		}
	}

	void BuildRail(float _fromDist, float _toDist, float _subdivisions) {
		// Create game object
		GameObject obj = new GameObject("Rail");
		obj.transform.SetParent(transform, false);
	
		MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
		//

		float totalDistance = _toDist - _fromDist;

		_subdivisions = Mathf.Max(1f, _subdivisions);

		List<Vector3> verticesLeft = new List<Vector3>();
		List<Vector3> verticesRight = new List<Vector3>();

		float distance = 0f;
		float step = totalDistance / _subdivisions;
		for (distance = _fromDist; distance < _toDist; distance += step) {
			Vector3 dir = Vector3.zero;
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 point = m_spline.GetPointAtDistance(distance, ref dir, ref up, ref right, true);

			CreateRailAt(point + right * 0.325f * m_meshScale, up, right, ref verticesRight);
			CreateRailAt(point - right * 0.325f * m_meshScale, up, right, ref verticesLeft);
		}

		Vector3 forwardLast = Vector3.zero;
		Vector3 upLast = Vector3.zero;
		Vector3 rightLast = Vector3.zero;
		Vector3 pointLast = m_spline.GetPointAtDistance(_toDist, ref forwardLast, ref upLast, ref rightLast, true);

		CreateRailAt(pointLast + rightLast * 0.325f * m_meshScale, upLast, rightLast, ref verticesRight);
		CreateRailAt(pointLast - rightLast * 0.325f * m_meshScale, upLast, rightLast, ref verticesLeft);

		if (meshFilter.sharedMesh != null) meshFilter.sharedMesh = null;

		Mesh mesh = meshFilter.sharedMesh;
		if (mesh == null) {
			mesh = new Mesh();
			mesh.name = name + "_Mesh";
			meshFilter.mesh = mesh;
		}

		int tieCount = m_tieCountPerGameObject;
		float distBetweenTies = (m_distancePerGameObject / (float)(m_tieCountPerGameObject + 1));
		tieCount = (int)(totalDistance / distBetweenTies);

		CombineInstance[] combine = new CombineInstance[2 + tieCount];
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
	
		float dist = _fromDist + distBetweenTies;
		for (int i = 0; i < tieCount; ++i) {
			Vector3 dir = Vector3.zero;
			Vector3 up = Vector3.zero;
			Vector3 right = Vector3.zero;
			Vector3 point = m_spline.GetPointAtDistance(dist, ref dir, ref up, ref right, true);

			combine[2 + i].mesh = CreateWoodTie(point, up, right, dir);

			dist += distBetweenTies;
		}

		mesh.CombineMeshes(combine, true, false);

		renderer.sharedMaterial = m_material;
	}

	private void CreateRailAt(Vector3 _point, Vector3 _up, Vector3 _right, ref List<Vector3> _vertices) {
		_vertices.Add(_point + (_up * 0.10f - _right * 0.04f) * m_meshScale);
		_vertices.Add(_point + (_up * 0.10f + _right * 0.04f) * m_meshScale);
		_vertices.Add(_point + (_up * 0.00f + _right * 0.05f) * m_meshScale);
		_vertices.Add(_point + (_up * 0.00f + _right * 0.10f) * m_meshScale);
		_vertices.Add(_point + (-_up * 0.05f + _right * 0.10f) * m_meshScale);
		_vertices.Add(_point + (-_up * 0.05f - _right * 0.10f) * m_meshScale);
		_vertices.Add(_point + (_up * 0.00f - _right * 0.10f) * m_meshScale);
		_vertices.Add(_point + (_up * 0.00f - _right * 0.05f) * m_meshScale);
	}

	private List<Vector2> CreateRailUVs(int vCount) {
		List<Vector2> uvs = new List<Vector2>();
		float vStep = (1f / m_uvScale);
		float v = 0;

		for (int i = 0; i < vCount; i += 8) {
			uvs.Add(new Vector2(0.175f, v));
			uvs.Add(new Vector2(0.330f, v));
			uvs.Add(new Vector2(0.450f, v));
			uvs.Add(new Vector2(0.500f, v));
			uvs.Add(new Vector2(0.45f, v));
			uvs.Add(new Vector2(0.020f, v));
			uvs.Add(new Vector2(0.060f, v));
			uvs.Add(new Vector2(0.055f, v));

			v += vStep;
			if (v > 1f)
				v = 0f;
		}

		return uvs;
	}

	private Mesh CreateWoodTie(Vector3 _point, Vector3 _up, Vector3 _right, Vector3 _dir) {
		List<Vector3> vertices = new List<Vector3>();
		// top
		vertices.Add(_point + (-_up * 0.05f - _right * 0.70f + _dir * 0.22f) * m_meshScale);
		vertices.Add(_point + (-_up * 0.05f + _right * 0.70f + _dir * 0.22f) * m_meshScale);
		vertices.Add(_point + (-_up * 0.05f + _right * 0.70f - _dir * 0.22f) * m_meshScale);
		vertices.Add(_point + (-_up * 0.05f - _right * 0.70f - _dir * 0.22f) * m_meshScale);

		// bottom
		vertices.Add(_point + (-_up * 0.12f - _right * 0.70f + _dir * 0.22f) * m_meshScale);
		vertices.Add(_point + (-_up * 0.12f + _right * 0.70f + _dir * 0.22f) * m_meshScale);
		vertices.Add(_point + (-_up * 0.12f + _right * 0.70f - _dir * 0.22f) * m_meshScale);
		vertices.Add(_point + (-_up * 0.12f - _right * 0.70f - _dir * 0.22f) * m_meshScale);

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
