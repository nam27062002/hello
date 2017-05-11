using System.Collections.Generic;
using UnityEngine;

public class FireNodeSetup {

	private int m_boxelSize = 2;

	private Transform	  m_parent;
	private List<Vector3> m_vertices;
	private Bounds 	  	  m_bounds;

	private List<Vector3>[,,] m_boxels;
	private Vector3 m_size;


	// Use this for initialization
	public void Init(Transform _parent) {
		m_parent = _parent;
		m_bounds = new Bounds();
		m_vertices = new List<Vector3>();

		Renderer[] renderers = _parent.GetComponentsInChildren<Renderer>();

		for (int i = 0; i < renderers.Length; ++i) {
			Renderer renderer = renderers[i];
			m_bounds.Encapsulate(renderer.bounds);

			Vector3[] vertices = null;
			if (renderer.GetType() == typeof(SkinnedMeshRenderer)) {
				vertices = (renderer as SkinnedMeshRenderer).sharedMesh.vertices;
			} else if (renderer.GetType() == typeof(MeshRenderer)) {
				MeshFilter filter = renderer.GetComponent<MeshFilter>();
				if (filter != null) {
					vertices = filter.sharedMesh.vertices;
				}
			}

			for (int v = 0; v < vertices.Length; ++v) {
				m_vertices.Add(renderer.transform.TransformPoint(vertices[v]));
			}
		}

		m_boxels = null;
	}
	
	public void Build(int _boxelSize) {
		m_boxelSize = _boxelSize;
		m_boxels = null;

		float maxSize = Mathf.Max(m_bounds.size.x, Mathf.Max(m_bounds.size.y, m_bounds.size.z));
		int size = Mathf.CeilToInt(maxSize / m_boxelSize);
		if (size > 20) {
			m_boxelSize = Mathf.CeilToInt(maxSize / 20);
		}

		int sizeX = Mathf.CeilToInt(m_bounds.size.x / m_boxelSize);
		int sizeY = Mathf.CeilToInt(m_bounds.size.y / m_boxelSize);
		int sizeZ = Mathf.CeilToInt(m_bounds.size.z / m_boxelSize);

		m_boxels = new List<Vector3>[sizeX, sizeY, sizeZ];
		m_size = new Vector3(sizeX, sizeY, sizeZ);

		for (int x = 0; x < m_size.x; x++) {
			for (int y = 0; y < m_size.y; y++) {
				for (int z = 0; z < m_size.z; z++) {
					m_boxels[x, y, z] = new List<Vector3>();
				}
			}
		}

		EnableBoxels();
		DisableInternalBoxels();
		BuildFireNodes();
	}

	private void EnableBoxels() {
		for (int v = 0; v < m_vertices.Count; v++) {			
			EnableBoxelAt(m_vertices[v]);
		}
	}

	private void EnableBoxelAt(Vector3 _v) {
		Vector3 offset = (_v - (m_bounds.center - (m_size * m_boxelSize * 0.5f))) / m_boxelSize;
		int x = Mathf.FloorToInt(offset.x);
		int y = Mathf.FloorToInt(offset.y); 
		int z = Mathf.FloorToInt(offset.z);

		if (x >= 0 && x < m_size.x && 
			y >= 0 && y < m_size.y &&
			z >= 0 && z < m_size.z) {
			m_boxels[x, y, z].Add(_v);
		}
	}

	private void DisableInternalBoxels() {
		List<List<Vector3>> clear = new List<List<Vector3>>();

		for (int x = 1; x < m_size.x - 1; x++) {
			for (int y = 1; y < m_size.y - 1; y++) {
				for (int z = 1; z < m_size.z - 1; z++) {
					if (m_boxels[x, y, z].Count > 0) {
						if (m_boxels[x - 1, y, z].Count > 0 &&
							m_boxels[x + 1, y, z].Count > 0 &&
							m_boxels[x, y - 1, z].Count > 0 &&
							m_boxels[x, y + 1, z].Count > 0 &&
							m_boxels[x, y, z - 1].Count > 0 &&
							m_boxels[x, y, z + 1].Count > 0) {
							clear.Add(m_boxels[x, y, z]);
						}
					}
				}
			}
		}

		for (int i = 0; i < clear.Count; i++) {
			clear[i].Clear();
		}
	}

	private void BuildFireNodes() {
		Transform fireNodes = m_parent.transform.FindChild("FireNodes");

		if (fireNodes != null) {
			fireNodes.parent = null;
			GameObject.DestroyImmediate(fireNodes.gameObject);
		}

		GameObject obj = new GameObject("FireNodes");
		obj.transform.SetParent(m_parent.transform, false);
		fireNodes = obj.transform;

		
		for (int x = 0; x < m_size.x; x++) {
			for (int y = 0; y < m_size.y; y++) {
				for (int z = 0; z < m_size.z; z++) {
					if (m_boxels[x, y, z].Count > 0) {
						List<Vector3> boxel = m_boxels[x, y, z];

						Vector3 position = Vector3.zero;
						for (int i = 0; i < boxel.Count; i++) {
							position += boxel[i];
						}
						position /= boxel.Count;

						GameObject fireNodeObj = new GameObject("FireNode");
						fireNodeObj.transform.position = position;
						fireNodeObj.transform.SetParent(fireNodes, true);
						fireNodeObj.AddComponent<FireNode>();
					}
				}
			}
		}
	}

	private Vector3 GetBoxelCenter(int _x, int _y, int _z) {
		Vector3 offset = new Vector3(_x, _y, _z);
		return (offset * m_boxelSize) + (m_bounds.center - ((m_size - Vector3.one) * m_boxelSize * 0.5f));
	}

	public void OnDrawGizmosSelected() {
		if (m_boxels != null) {
			Gizmos.color = Colors.slateBlue;
			for (int x = 0; x < m_size.x; x++) {
				for (int y = 0; y < m_size.y; y++) {
					for (int z = 0; z < m_size.z; z++) {
						if (m_boxels[x, y, z].Count > 0) {
							Gizmos.DrawWireCube(GetBoxelCenter(x, y, z), Vector3.one * m_boxelSize);
						}
					}
				}
			}

			Gizmos.color = Colors.paleYellow;
			Gizmos.DrawWireCube(m_bounds.center, m_size * m_boxelSize);
		}
				
		for (int v = 0; v < m_vertices.Count; v++) {					
			Gizmos.DrawCube(m_vertices[v], Vector3.one * 0.1f);
		}
	}
}
