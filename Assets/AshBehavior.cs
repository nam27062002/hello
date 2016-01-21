using UnityEngine;
using System.Collections;

public class AshBehavior : MonoBehaviour {

	[SerializeField] private string m_ashesAsset;
	[SerializeField] private float m_dissolveTime = 3f;
	[SerializeField] private bool m_loop = true;

	Material m_material;
	GameObject m_psGO;
	ParticleSystem m_ps;
	float m_timer;

	// Use this for initialization
	void Start () {
		SkinnedMeshRenderer sMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		m_material = sMeshRenderer.material;
		m_material.SetFloat("_AshLevel", 0);

		m_psGO = GameObject.Instantiate((GameObject)Resources.Load("Particles/Ashes/" + m_ashesAsset));
		m_psGO.transform.position = sMeshRenderer.transform.position;
		m_psGO.transform.rotation = sMeshRenderer.transform.rotation;
		m_psGO.transform.localScale = sMeshRenderer.transform.localScale;

		m_ps = m_psGO.GetComponent<ParticleSystem>();
		m_ps.Clear();
		m_ps.Play();

		m_timer = 0;
	}
	
	// Update is called once per frame
	void Update () {

		if (m_timer < m_dissolveTime) {
			m_material.SetFloat("_AshLevel", m_timer / m_dissolveTime);
			m_timer += Time.deltaTime;
		} else {
			m_material.SetFloat("_AshLevel", 1f);

			if (m_loop && !m_psGO.activeInHierarchy) {
				m_ps.Clear();
				m_ps.Play();
				m_timer = 0;

				m_psGO.SetActive(true);
			}
		}

	}
}
