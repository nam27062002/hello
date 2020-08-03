using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CagePart : MonoBehaviour {
	
	[SerializeField] private float m_fadeTime;
	[SerializeField] private Transform m_referenceTransform;

	private List<Material> m_materials;

	private float m_time;


	void Awake() {
		m_materials = new List<Material>();

		Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			Material[] materials = renderers[i].materials;
			for (int m = 0; m < materials.Length; m++) {
				m_materials.Add(materials[m]);
			}
		}
	}

	void OnEnable() {
		if (m_referenceTransform != null) {
			transform.CopyFrom(m_referenceTransform);
		}

		Color tint = Color.white;
		for (int i = 0; i < m_materials.Count; i++) {			
			m_materials[i].SetColor( GameConstants.Materials.Property.TINT , tint);
		}

		m_time = m_fadeTime;
	}

	// Update is called once per frame
	void Update () {
		if (m_time > 0f) {
			m_time -= Time.deltaTime;

			if (m_time <= 0f) {
				gameObject.SetActive(false);
			} else {
				Color tint = Color.white;
				tint.a = m_time / m_fadeTime;
				for (int i = 0; i < m_materials.Count; i++) {
					m_materials[i].SetColor( GameConstants.Materials.Property.TINT , tint);
				}
			}
		}
	}
}
