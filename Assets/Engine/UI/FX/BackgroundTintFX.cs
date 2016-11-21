﻿using UnityEngine;
using System.Collections;

public class BackgroundTintFX : MonoBehaviour {

    public Color m_Tint = Color.white;
    public Shader m_shader = null;

//    private Camera m_mapCamera;
    private Material m_material;

    // Use this for initialization
    void Start()
    {
        m_material = new Material(m_shader);
        setTint(m_Tint);
    }

    public void setTint(Color col)
    {
        m_material.SetColor("_Tint", col);
    }

    private RenderTexture m_buffer = null;
    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTexture m_buffer = RenderTexture.GetTemporary(source.width, source.height, 24);
        Graphics.SetRenderTarget(m_buffer.colorBuffer, m_buffer.depthBuffer);
        setTint(m_Tint);
        Graphics.Blit(source, m_buffer, m_material);
        Graphics.Blit(m_buffer, destination);
//        Graphics.Blit(source, destination, m_material);
        RenderTexture.ReleaseTemporary(m_buffer);
    }

    void OnPostRender()
    {
//        RenderTexture.ReleaseTemporary(m_buffer);
//        Debug.Log("Release background tint texture");
    }

}
