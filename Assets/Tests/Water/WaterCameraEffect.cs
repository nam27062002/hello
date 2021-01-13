using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCameraEffect : MonoBehaviour {

    public Material m_material;

    private Camera m_originalCamera = null;
    // Use this for initialization
    void Start () {
        m_originalCamera = GetComponent<Camera>();
        m_originalCamera.depthTextureMode = DepthTextureMode.Depth;
    }



    // Update is called once per frame
    public void Update()
    {
//        m_material.SetFloat("_TexelOffset", 1.0f);
//        m_material.SetFloat("_LensOffset", 1.0f);
    }

    void OnPostRender()
    {
        Graphics.Blit(null as RenderTexture, m_material);
    }
/*
    void OnRenderImage(RenderTexture source, RenderTexture dest)
    {
        Graphics.Blit(source, dest, m_material);

    }
*/
}
