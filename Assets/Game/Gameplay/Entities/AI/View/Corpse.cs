using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour {
	//------------------------------------
	private struct SimpleTransform {
		public Vector3 localPosition;
		public Vector3 localScale;
		public Quaternion localRotation;
	}
	//------------------------------------
	[SerializeField] private float m_fadeTime = 1f;
	[SerializeField] private float m_forceExplosion = 175f;
	[SerializeField] private ParticleData m_blood;
	[SerializeField] private Transform[] m_bloodPoints;

	private Rigidbody[] m_gibs;
	private Renderer[] m_renderers;
	private List<SimpleTransform> m_originalTransforms;
	private List<Vector3> m_forceDirection;

	private float m_time;


	// Use this for initialization
	void Awake() {		
		m_originalTransforms = new List<SimpleTransform>();
		m_forceDirection = new List<Vector3>();

		Transform view = transform.FindChild("view");
		m_gibs = view.GetComponentsInChildren<Rigidbody>();
		m_renderers = view.GetComponentsInChildren<Renderer>();

		for (int i = 0; i < m_gibs.Length; i++) {
			SimpleTransform t = new SimpleTransform();
			t.localPosition = m_gibs[i].transform.localPosition;
			t.localScale = m_gibs[i].transform.localScale;
			t.localRotation = m_gibs[i].transform.localRotation;
			m_originalTransforms.Add(t);

			Vector3 dir = m_gibs[i].transform.position - transform.position;
			dir.Normalize();
			m_forceDirection.Add(dir);
		}
	}

	void OnEnable() {
		if (m_gibs != null) {
			for (int i = 0; i < m_gibs.Length; i++) {
				m_gibs[i].transform.position = Vector3.zero;
				m_gibs[i].transform.rotation = Quaternion.identity;

				m_gibs[i].transform.localPosition = m_originalTransforms[i].localPosition;
				m_gibs[i].transform.localRotation = m_originalTransforms[i].localRotation;
				m_gibs[i].transform.localScale = m_originalTransforms[i].localScale;

				m_gibs[i].position = m_gibs[i].transform.position;
				m_gibs[i].rotation = Quaternion.identity;
				m_gibs[i].velocity = Vector3.zero;

				m_gibs[i].AddForce(m_forceDirection[i] * m_forceExplosion, ForceMode.Impulse);
			}

			if (!string.IsNullOrEmpty(m_blood.name) && m_bloodPoints != null) {
				for (int i = 0; i < m_bloodPoints.Length; i++) {
					GameObject ps = ParticleManager.Spawn(m_blood.name, Vector3.zero, m_blood.path);
					ps.transform.SetParent(m_bloodPoints[i].transform);
					ps.transform.localPosition = Vector3.zero;
				}
			}
		}

		m_time = m_fadeTime;
	}

	void Update() {
		float alpha = m_time / m_fadeTime;

		m_time -= Time.deltaTime;
		if (m_time <= 0) m_time = 0f;
	}
}
