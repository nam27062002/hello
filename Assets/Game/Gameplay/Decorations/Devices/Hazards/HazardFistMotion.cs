using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HazardFistMotion : MonoBehaviour {

	[SerializeField] private Renderer m_staticColumn;
	[SerializeField] private Renderer m_fist;
	[SeparatorAttribute]
	[SerializeField] private float m_distance;
	[SerializeField] private float m_speed;
	[SeparatorAttribute]
	[SerializeField] ParticleData m_hitGroundParticles;

	// Size
	private float m_initialPosY;
	private float m_columnHeight;

	// Transforms
	private Transform m_staticColumnTransform;
	private Transform m_fistTransform;
	private Transform[] m_mobileColumnTransforms;

	// Motion
	private float m_realDistance;
	private float m_time;

	//
	private bool m_spawnParticles;


	//
	// Use this for initialization
	void Awake() {
		if (m_speed > 0) {
			m_realDistance = m_distance * transform.localScale.x;

			m_columnHeight = m_staticColumn.bounds.size.y * 0.95f;

			m_staticColumnTransform = m_staticColumn.transform;
			m_fistTransform = m_fist.transform;

			m_initialPosY = m_staticColumnTransform.localPosition.y;

			int auxColumnsCount = Mathf.FloorToInt(m_realDistance / m_columnHeight);
			m_mobileColumnTransforms = new Transform[auxColumnsCount];

			float yScale = (m_realDistance / auxColumnsCount) / m_columnHeight;
			m_columnHeight *= yScale;

			float scale = 1f;
			for (int i = 0; i < auxColumnsCount; ++i) {
				GameObject go = GameObject.Instantiate(m_staticColumn.gameObject);
				Transform tr = go.transform;
				tr.SetParent(transform);
				tr.CopyFrom(m_staticColumnTransform);
				if (i > 0) { // the column next to the hand won't be scaled
					tr.localScale = tr.localScale.x * (new Vector3(scale, yScale, scale));
				}
				m_mobileColumnTransforms[i] = tr;

				scale -= 0.1f;
			}

			m_staticColumnTransform.localScale = m_staticColumnTransform.localScale.x * (new Vector3(scale, yScale, scale));

			m_hitGroundParticles.CreatePool();
			m_spawnParticles = true;

			m_time = 0f;
		}
	}
	
	// Update is called once per frame
	void Update() {
		if (m_speed > 0) {
			m_time += Time.deltaTime;
			float sinValue = (Mathf.Sin(m_time * m_speed) - 1);
			Vector3 position = Vector3.up * (sinValue * m_realDistance * 0.5f);

			m_fistTransform.localPosition = position;
			for (int i = 0; i < m_mobileColumnTransforms.Length; ++i) {
				Vector3 p = position + Vector3.up * (m_initialPosY + (m_columnHeight * i));
				if (p.y > m_initialPosY) {
					p.y = m_initialPosY;
				}
				m_mobileColumnTransforms[i].localPosition = p;
			}

			if (sinValue < -1.9f) {
				if (m_spawnParticles) {
					m_hitGroundParticles.Spawn(m_fistTransform.position);
					m_spawnParticles = false;
				}
			} else if (sinValue > -0.15f) {
				m_spawnParticles = true;
			}
		}
	}
}
