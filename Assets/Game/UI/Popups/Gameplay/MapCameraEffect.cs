using UnityEngine;
using System.Collections;

public class MapCameraEffect : MonoBehaviour {


    public Texture2D m_TextureEffect;

    public float m_offMultiply = 0.01f;
    public float m_outlineStrength = 1.0f;
    public float m_stepMargin = 0.3f;

    private Camera m_mapCamera;
    private Material m_material;

	// Use this for initialization
	void Start () {
        m_mapCamera = transform.GetComponent<Camera>();

        m_material = new Material(Shader.Find("Hidden/Minimap ink effect"));
        m_material.SetTexture("_Trace", m_TextureEffect);
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
        if (m_offMultiply == 0.0f)
        {
            Graphics.Blit(source, destination);
            return;
        }

        m_material.SetFloat("_offMultiply", m_offMultiply);
        m_material.SetFloat("_outlineStrength", m_outlineStrength);
        m_material.SetFloat("_stepMargin", m_stepMargin);

        Graphics.Blit(source, destination, m_material);
    }

}
