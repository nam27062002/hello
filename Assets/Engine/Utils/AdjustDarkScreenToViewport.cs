// AdjustToViewport.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/06/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class AdjustDarkScreenToViewport : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private bool m_executeInEditMode = false;
	private MeshRenderer m_renderer = null;
	public bool m_darkUpdate = true;

	private Camera m_camera = null;

	public Camera targetCamera {
		get { return m_camera; }
        set { m_camera = value; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
        m_renderer = GetComponent<MeshRenderer>();
    }

    /// <summary>
    /// Component has been enabled.
    /// </summary>
    private void OnEnable() {
		FitViewport();
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	#if UNITY_EDITOR
	private void Update() {
		if(!Application.isPlaying && m_executeInEditMode) {
			FitViewport();
		}
	}
	#endif

	/// <summary>
	/// End-of-frame update.
	/// </summary>
	private void LateUpdate() {
		// Apply it by the end of the frame to make sure camera transform is updated!
		if(Application.isPlaying && m_darkUpdate) {
			FitViewport();
		}
	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Make the sprite fit the viewport of the main camera.
    /// </summary>
    public void FitViewport() {
        // Skip if component not enabled
        if (!this.isActiveAndEnabled) return;

        if (m_camera == null)
        {
            // Main camera must be valid
            if (Application.isPlaying && InstanceManager.sceneController != null)
            {
                m_camera = InstanceManager.sceneController.mainCamera;
            }
            else if (m_executeInEditMode)
            {
                m_camera = Camera.main;
            }
        }
        // If camera is not manuall defined, try to automatically get one
        Camera cam = m_camera;

        if (cam == null) return;

        // Find out base sprite's size
        //		m_sprite.transform.localScale = Vector3.one;
        //		float spriteW = m_sprite.sprite.bounds.size.x;
        //		float spriteH = m_sprite.sprite.bounds.size.y;

        // Compute viewport bounds at the Z of the sprite
        // ViewportToWorldPoint(): Viewport space is normalized and relative to the camera. The bottom-left of the viewport is (0,0); the top-right is (1,1). The z position is in world units from the camera.
        // Be generous to make sure all the screen is covered


        float distZ = transform.localPosition.z;

        Vector3 minWorldPos = cam.ViewportToWorldPoint(new Vector3(-0.1f, -0.1f, distZ));
        Vector3 maxWorldPos = cam.ViewportToWorldPoint(new Vector3(1.1f, 1.1f, distZ));

        float scaleX = maxWorldPos.x - minWorldPos.x;
        float scaleY = maxWorldPos.y - minWorldPos.y;

        transform.localScale = new Vector3(scaleX, scaleY, 1.0f);

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}