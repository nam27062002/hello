using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireProcController : MonoBehaviour {
    public Renderer m_renderer;
	public FireTypeAutoSelector m_colorSelector;
	private Material m_material; 

	private void Awake() {
        m_material = m_renderer.material;
        m_material.SetFloat(GameConstants.Materials.Property.POWER, 0f);
        m_renderer.material = m_material;
    }

    private void OnEnable() {
        m_material = m_renderer.material;
        m_material.SetFloat(GameConstants.Materials.Property.POWER, 0f);
        m_renderer.material = m_material;
    }

    public void SetPower(float _value) {
		m_material.SetFloat(GameConstants.Materials.Property.POWER, _value);
		m_renderer.material = m_material;
	}

    public void EditorSetPower(float _value) {
        Material material = m_renderer.sharedMaterial;
        material.SetFloat(GameConstants.Materials.Property.POWER, _value);
        m_renderer.sharedMaterial = material;
    }
}
