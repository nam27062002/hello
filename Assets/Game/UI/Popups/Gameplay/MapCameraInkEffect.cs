using UnityEngine;
using System.Collections;

public class MapCameraInkEffect : MonoBehaviour {

    public float m_outlineStrength = 1.0f;
    public float m_stepMargin = 0.3f;
    public Color m_outlineColor = Color.black;
    public Color m_paperColor = Color.white;
    public Shader m_shader = null;

    private Camera m_mapCamera;
    private Material m_material;

	// Use this for initialization
	void Start () {
        m_material = new Material(m_shader);
//        m_mapCamera.SetReplacementShader(m_replacementShader, null);
//        m_replacementShader.
	}
	
	// Update is called once per frame
//	void Update () {
//	
//	}

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        m_material.SetFloat("_outlineStrength", m_outlineStrength);
        m_material.SetFloat("_stepMargin", m_stepMargin);
        m_material.SetColor("_inkColor", m_outlineColor);
        m_material.SetColor("_paperColor", m_paperColor);

        Graphics.Blit(source, destination, m_material);
    }

}
