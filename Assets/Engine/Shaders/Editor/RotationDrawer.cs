// BlendModeDrawer.cs
// 
// Created by Diego Campos Martinez on 31/10/2017
// Copyright (c) 2017 Ubisoft. All rights reserved.
//
// Headers and MaterialPropertyDrawer model based on
// LightDirectionDrawer.cs
// Created by Alger Ortín Castellví

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEditor;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Custom inspector for a Blend Mode property on a custom shader.
/// It should be equivalent to a Vector4 where the xyz represent the light position
/// and the w the intensity.
/// Use with "[LightDirection]" before a float shader property
/// </summary>
public class RotationDrawer : MaterialPropertyDrawer {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//
    static private Material matCircle = null;
    static private Material matQuad = null;
    static private Texture2D txCircle = null;
    static private bool isDragged = false;
    static private Color currentColor = Color.white;
    static private Vector4 currentTarget = Vector4.zero;
    static private float specularPower = 2.0f;
    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//

    static public void setColor(Color col)
    {
        currentColor = col;
    }

    static public void setTargetPoint(float x, float y)
    {
        currentTarget.x = x;
        currentTarget.y = y;
    }
    static public void setSpecularPow(float sp)
    {
        specularPower = sp;
    }


    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//

    void DrawQuad(float px, float py, float sx, float sy, Color col)
    {
        GL.Begin(GL.QUADS);
        GL.Color(col);
        GL.TexCoord2(0.0f, 0.0f);
        GL.Vertex3(px, py, 0.0f);
        GL.TexCoord2(1.0f, 0.0f);
        GL.Vertex3(px + sx, py, 0.0f);
        GL.TexCoord2(1.0f, 1.0f);
        GL.Vertex3(px + sx, py + sy, 0.0f);
        GL.TexCoord2(0.0f, 1.0f);
        GL.Vertex3(px, py + sy, 0.0f);
        GL.End();
    }

    bool isInside(Vector2 mpos, Vector2 tpos, float rad)
    {
        Vector2 d = mpos - tpos;
        return d.x > 0.0f && d.x < rad && d.y > 0.0f && d.y < rad;
    }

    /// <summary>
    /// Draw the property inside the given rect.
    /// </summary>
    override public void OnGUI(Rect _rect, MaterialProperty _prop, string _label, MaterialEditor _editor) {

        Vector3 value = Vector3.Normalize(_prop.vectorValue);
        currentTarget.x = value.x;
        currentTarget.y = value.y;
        value.y = -value.y;
		EditorGUI.BeginChangeCheck();

        Rect pos = _rect;

        float bs = _rect.height;
        float bh = bs * 0.5f;
        Vector2 tv = new Vector2(value.x, value.y) * bh;
        Vector2 ipos = new Vector2(_rect.width * 0.5f, 0.0f);

        if (Event.current.type == EventType.Repaint)
        {
            if (matCircle == null)
            {
                Shader sh = Shader.Find("Hidden/RotationDrawer");                
                matCircle = new Material(sh);
                matQuad = new Material(matCircle);
                txCircle = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/UI/Common/Generic/circle_mask.png");
                matCircle.SetTexture("_MainTex", txCircle);
            }

            matCircle.SetColor("_TintColor", currentColor);
            matCircle.SetVector("_TargetPos", currentTarget);
            matCircle.SetFloat("_SpecPow", specularPower);

            GUI.BeginClip(pos);
            GL.PushMatrix();
            GL.Clear(true, false, Color.black);

            matCircle.SetPass(0);

            DrawQuad(ipos.x, ipos.y, bs, bs, Color.white);

            GL.PopMatrix();
            GUI.EndClip();

            GUI.Label(_rect, _label);
        }
        else if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) && Event.current.button == 0)
        {
            ipos.x += _rect.x;
            ipos.y += _rect.y;
            Vector2 mp = Event.current.mousePosition;
            isDragged = isInside(mp, ipos, bs);
        }
        else if (Event.current.type == EventType.MouseDrag)
        {
            ipos.x += _rect.x;
            ipos.y += _rect.y;
            Vector2 mp = Event.current.mousePosition;
            if (isDragged)
            {
                Vector2 d = ((mp - ipos) / bs) - (Vector2.one * 0.5f);
                float l = Mathf.Clamp01(d.magnitude * 2.0f);
                float z = Mathf.Sin(Mathf.Acos(l));
                value.x = d.x;
                value.y = -d.y;
                value.z = -z;

                _prop.vectorValue = value.normalized;
                _editor.Repaint();
            }
        }
    }

	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
	override public float GetPropertyHeight(MaterialProperty _prop, string _label, MaterialEditor _editor) {
        return EditorGUIUtility.singleLineHeight * 5;
	}
}