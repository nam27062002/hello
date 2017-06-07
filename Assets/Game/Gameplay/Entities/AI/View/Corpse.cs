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
	[SerializeField] private float m_fadeDelay = 0.5f;
	[SerializeField] private float m_fadeTime = 1f;
	[SerializeField] private float m_forceExplosion = 175f;
	[SerializeField] private Transform m_view;
	[SerializeField] private ParticleData m_blood;
	[SerializeField] private Transform[] m_bloodPoints;

	private Rigidbody[] m_gibs;
	private List<Material> m_materials;
	private List<Color> m_defaultTints;
	private List<SimpleTransform> m_originalTransforms;
	private List<Vector3> m_forceDirection;

	private bool m_spawned;
	private float m_time;
	private float m_delay;


	// Use this for initialization
	void Awake() {		
		m_spawned = false;

		m_originalTransforms = new List<SimpleTransform>();
		m_forceDirection = new List<Vector3>();
		m_materials = new List<Material>();
		m_defaultTints = new List<Color>();

		m_gibs = m_view.GetComponentsInChildren<Rigidbody>();

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

		Renderer[] renderers = m_view.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			Material[] materials = renderers[i].materials;
			for (int m = 0; m < materials.Length; m++) {
				m_materials.Add(materials[m]);
				if (materials[m].HasProperty("_FresnelColor")) {
					m_defaultTints.Add(materials[m].GetColor("_FresnelColor"));
				} else {
					m_defaultTints.Add(Color.black);
				}
			}
		}

		m_blood.CreatePool();
	}

	void OnDisable() {
		m_spawned = false;
	}

	public void Spawn(bool _isGold, bool _hasBoost) {
		if (m_gibs != null) {

			float forceFactor = _hasBoost ? 1.25f : 1f;

			for (int i = 0; i < m_gibs.Length; i++) {
				m_gibs[i].transform.position = Vector3.zero;

				m_gibs[i].transform.localPosition = m_originalTransforms[i].localPosition;
				m_gibs[i].transform.localRotation = m_originalTransforms[i].localRotation;
				m_gibs[i].transform.localScale = m_originalTransforms[i].localScale;

				m_gibs[i].position = m_gibs[i].transform.position;
				m_gibs[i].velocity = Vector3.zero;

				m_gibs[i].AddForce(m_forceDirection[i] * m_forceExplosion * forceFactor, ForceMode.Impulse);
			}

			if (!string.IsNullOrEmpty(m_blood.name) && m_bloodPoints != null) {
				for (int i = 0; i < m_bloodPoints.Length; i++) {
					GameObject ps = m_blood.Spawn();

					if (ps != null) {
						FollowTransform ft = ps.GetComponent<FollowTransform>();
						if (ft != null) {
							ft.m_follow = m_bloodPoints[i].transform;
						} else {
							ps.transform.localPosition = Vector3.zero;
						}
					}
				}
			}
		}

		m_time = m_fadeTime;
		Color tint = Color.white;
		for (int i = 0; i < m_materials.Count; i++) {
			if (_isGold) {
				m_materials[i].SetColor("_FresnelColor", ViewControl.GOLD_TINT);                        
			} else {
				m_materials[i].SetColor("_FresnelColor", m_defaultTints[i]);
			}
			m_materials[i].SetColor("_Tint", tint);
		}

		m_delay = m_fadeDelay;
		m_spawned = true;
	}

	public void SwitchDragonTextures( Texture bodyTexture, Texture wingsTexture )
	{
		Color tint = Color.white;
		for (int i = 0; i < m_materials.Count; i++) {
			if (m_materials[i].name.Contains("body")) {
				m_materials[i].mainTexture = bodyTexture;
			} else if (m_materials[i].name.Contains("wings")) {
				m_materials[i].mainTexture = wingsTexture;
			}
		}
	}

	void Update() {
		if (m_spawned) {
			m_delay -= Time.deltaTime;
			if (m_delay <= 0) {
				Color tint = Color.white;
				tint.a = m_time / m_fadeTime;
				for (int i = 0; i < m_materials.Count; i++) {
					m_materials[i].SetColor("_Tint", tint);
				}

				m_time -= Time.deltaTime;
				if (m_time <= 0) m_time = 0f;
			}

			for (int i = 0; i < m_gibs.Length; i++) {
				m_gibs[i].AddForce(Vector3.down * 25f);
			}
		}
	}
}
