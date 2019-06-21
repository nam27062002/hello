using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProcController : MonoBehaviour {
	public Renderer m_renderer;
	public FireTypeAutoSelector m_colorSelector;
	private Material m_material; 

	private void Awake() {
		m_material = m_renderer.material;
		m_material.SetFloat("_Power", 0f);
		m_renderer.material = m_material;
	}

	public void SetPower(float _value) {
		m_material = m_renderer.material;
		m_material.SetFloat("_Power", _value);
		m_renderer.material = m_material;
	}
}
