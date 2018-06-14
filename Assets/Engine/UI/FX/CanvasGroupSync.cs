// CanvasGroupSync.cs
// Hungry Dragon
// 
// Created by Diego Campos on 08/06/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class CanvasGroupSync : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    private CanvasGroup m_canvasObj;
    private List<Material> m_materialList = new List<Material>();

    private static int PID_TintColor;

    private float m_oldAlpha = 0.0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
        //We will read alpha from this CanvasGroup component and propagate to children
        m_canvasObj = GetComponent<CanvasGroup>();
        PID_TintColor = Shader.PropertyToID("_GlobalAlpha");
    }

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
        ParticleSystemRenderer[] m_prenderers = GetComponentsInChildren<ParticleSystemRenderer>();
        foreach (ParticleSystemRenderer psr in m_prenderers)
        {
            Material[] m = psr.materials;
            for (int c = 0; c < m.Length; c++)
            {
                if (m[c].shader.name.Contains("Transparent particles standard"))
                {
                    m_materialList.Add(m[c]);
                }
            }
        }
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
        float alpha = m_canvasObj.alpha;

        if (alpha == m_oldAlpha) return;

        m_oldAlpha = alpha;
//        Debug.Log("CanvasGroup alpha: " + alpha);

        for (int c = 0; c < m_materialList.Count; c++)
        {
            m_materialList[c].SetFloat(PID_TintColor, alpha);
        }

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}