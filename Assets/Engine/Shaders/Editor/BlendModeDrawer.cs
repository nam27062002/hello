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
public class BlendModeDrawer : MaterialPropertyDrawer {
    //------------------------------------------------------------------//
    // CONSTANTS														//
    //------------------------------------------------------------------//
    public enum BlendMode
    {
        Additive,
        SoftAdditive,
        Premultiply,
        AlphaBlend
    }

    private BlendMode m_blendmode = BlendMode.Premultiply;

    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
/*
    public BlendModeDrawer(int i)
    {
    }
*/
    //    private MaterialProperty m_BlendSrc;
     //    private MaterialProperty m_BlendDst;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//

    /// <summary>
    /// Draw the property inside the given rect.
    /// </summary>
    override public void OnGUI(Rect _rect, MaterialProperty _prop, string _label, MaterialEditor _editor) {

        Vector4 value = _prop.vectorValue;
		EditorGUI.BeginChangeCheck();
//		EditorGUI.showMixedValue = _prop.hasMixedValue;

		Rect pos = new Rect(_rect.x, _rect.y, _rect.width, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
        m_blendmode = (BlendMode)EditorGUI.EnumPopup(pos, "Blend Mode", m_blendmode);
		// Apply changed values
//		EditorGUI.showMixedValue = false;
		if(EditorGUI.EndChangeCheck()) {
            switch(m_blendmode)
            {
                case BlendMode.Additive:
                    value = new Vector4((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float)UnityEngine.Rendering.BlendMode.One, 0.0f, 0.0f);
                    break;
                case BlendMode.SoftAdditive:
                    value = new Vector4((float)UnityEngine.Rendering.BlendMode.One, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcColor, 0.0f, 0.0f);
                    break;
                case BlendMode.Premultiply:
                    value = new Vector4((float)UnityEngine.Rendering.BlendMode.One, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, 0.0f, 0.0f);
                    break;
                case BlendMode.AlphaBlend:
                    value = new Vector4((float)UnityEngine.Rendering.BlendMode.SrcAlpha, (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, 0.0f, 0.0f);
                    break;
            }
//            _prop.vectorValue = value;
            _prop.vectorValue = value;
            Apply(_prop);
        }
//        base.OnGUI(_rect, _prop, _label, _editor);
    }

	/// <summary>
	/// Gets the height of the property drawer.
	/// </summary>
	/// <returns>The height required by this property drawer.</returns>
	/// <param name="_property">The property we're drawing.</param>
	/// <param name="_label">The label of the property.</param>
//	override public float GetPropertyHeight(MaterialProperty _prop, string _label, MaterialEditor _editor) {
//		return EditorGUIUtility.singleLineHeight * 5 + EditorGUIUtility.standardVerticalSpacing * 5;	// label + x-y-z-w + spacing
//	}

    public override void Apply(MaterialProperty prop)
    {
        Debug.Log("Applying");
        base.Apply(prop);
    }
}