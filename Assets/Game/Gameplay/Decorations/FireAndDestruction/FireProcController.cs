using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProcController : MonoBehaviour {
	private Renderer m_renderer;
	private Material m_material;

	private void Awake() {
		m_renderer = transform.GetFirstComponentInChildren<Renderer>();
		m_material = m_renderer.material;
		m_material.SetFloat("_Power", 0f);
		m_renderer.material = m_material;
	}

	public void SetPower(float _value) {
		m_material.SetFloat("_Power", _value);
		m_renderer.material = m_material;
	}
}
