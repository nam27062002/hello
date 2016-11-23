using UnityEngine;
using System.Collections;

public class BackgroundTintFX : MonoBehaviour {

    public Color m_Tint = Color.white;
    public Color m_Tint2 = Color.white;
    public Shader m_shader = null;

//    private Camera m_mapCamera;
    private Material m_material;

    private bool m_tintActive;

    // Use this for initialization
    void Start()
    {
        m_material = new Material(m_shader);
        setTint(m_Tint, m_Tint2);

        Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
    }

    void OnFuryToggled(bool active, DragonBreathBehaviour.Type type)
    {
        m_tintActive = active;
    }

    public void setTint(Color col)
    {
        m_material.SetColor("_Tint", col);
        m_material.SetColor("_Tint2", col);
    }
    public void setTint(Color col, Color col2)
    {
        m_material.SetColor("_Tint", col);
        m_material.SetColor("_Tint2", col2);
    }

    private RenderTexture m_buffer = null;

    // Postprocess the image
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m_tintActive)
        {
            m_buffer = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            //        Graphics.SetRenderTarget(m_buffer.colorBuffer, m_buffer.depthBuffer, 0);
            Graphics.SetRenderTarget(source.colorBuffer, source.depthBuffer, 0);
            //        setTint(m_Tint, m_Tint2);
            Graphics.Blit(source, m_buffer, m_material);
            //        Graphics.Blit(m_buffer, destination, m_material);
            Graphics.Blit(m_buffer, destination);

            RenderTexture.ReleaseTemporary(m_buffer);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }

/*
    void OnPostRender()
    {
//        m_material.SetTexture("_MainTex", m_buffer);
        GL.PushMatrix();

        m_material.SetPass(0);

        GL.LoadOrtho();

        // draw a quad over whole screen
        GL.Begin(GL.TRIANGLES);
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0, 0, 1);
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1, 0, 1);
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1, 1, 1);
//        GL.MultiTexCoord2(0, 0.0f, 1.0f);
//        GL.Vertex3(0, 1, 0);
        GL.End();

        GL.PopMatrix();
        //        Debug.Log("Release background tint texture");
    }
*/

}
