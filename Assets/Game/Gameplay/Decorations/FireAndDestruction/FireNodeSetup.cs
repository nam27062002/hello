using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireNodeSetup : MonoBehaviour {

	[SerializeField] private int m_boxelSize = 2;
	[SeparatorAttribute]
	[SerializeField] private string m_breathHitParticle = "PF_FireHit";
	[SerializeField] private bool m_hitParticleMatchDirection = false;
	[SerializeField] private float m_hitRadius = 1.5f;


	private MeshFilter	 m_meshFilter;
	private MeshRenderer m_meshRenderer;

	private List<Vector3>[,,] m_boxels;
	private Vector3 m_size;
	private Vector3 m_center;


	// Use this for initialization
	public void Init() {
		m_meshFilter = transform.FindComponentRecursive<MeshFilter>();
		m_meshRenderer = transform.FindComponentRecursive<MeshRenderer>();

		m_boxels = null;
	}
	
	public void Build() {
		m_boxels = null;

		Bounds bounds = m_meshRenderer.bounds;
		float maxSize = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
		int size = Mathf.CeilToInt(maxSize / m_boxelSize);
		if (size > 20) {
			m_boxelSize = Mathf.CeilToInt(maxSize / 20);
		}

		int sizeX = Mathf.CeilToInt(bounds.size.x / m_boxelSize);
		int sizeY = Mathf.CeilToInt(bounds.size.y / m_boxelSize);
		int sizeZ = Mathf.CeilToInt(bounds.size.z / m_boxelSize);

		m_boxels = new List<Vector3>[sizeX, sizeY, sizeZ];
		m_size = new Vector3(sizeX, sizeY, sizeZ);
		m_center = bounds.center;

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
		Vector3[] vertices = m_meshFilter.sharedMesh.vertices;

		for (int v = 0; v < vertices.Length; v++) {			
			EnableBoxelAt(transform.TransformVector(vertices[v]) + transform.position);
		}
	}

	private void EnableBoxelAt(Vector3 _v) {
		Vector3 offset = (_v - (m_center - (m_size * m_boxelSize * 0.5f))) / m_boxelSize;
		int x = Mathf.FloorToInt(offset.x);
		int y = Mathf.FloorToInt(offset.y); 
		int z = Mathf.FloorToInt(offset.z);

		m_boxels[x, y, z].Add(_v);
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
		Transform fireNodes = transform.FindChild("FireNodes");

		if (fireNodes != null) {
			fireNodes.parent = null;
			GameObject.DestroyImmediate(fireNodes.gameObject);
		}

		GameObject obj = new GameObject("FireNodes");
		obj.transform.SetParent(transform, false);
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
						FireNode fireNode = fireNodeObj.AddComponent<FireNode>();
						fireNode.transform.position = position;
						fireNode.transform.SetParent(fireNodes, true);
						fireNode.Setup(m_breathHitParticle, m_hitParticleMatchDirection, m_hitRadius);
					}
				}
			}
		}
	}

	private Vector3 GetBoxelCenter(int _x, int _y, int _z) {
		Vector3 offset = new Vector3(_x, _y, _z);
		return (offset * m_boxelSize) + (m_center - ((m_size - Vector3.one) * m_boxelSize * 0.5f));
	}

	void OnDrawGizmosSelected() {
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
			Gizmos.DrawWireCube(m_center, m_size * m_boxelSize);
		}

		if (m_meshFilter != null) {
			Vector3[] vertices = m_meshFilter.sharedMesh.vertices;

			for (int v = 0; v < vertices.Length; v++) {			
				Gizmos.DrawCube(transform.TransformVector(vertices[v]) + transform.position, Vector3.one * 0.1f);
			}
		}
	}
}
