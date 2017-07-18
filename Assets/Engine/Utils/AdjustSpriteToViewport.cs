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
[RequireComponent(typeof(SpriteRenderer))]
public class AdjustSpriteToViewport : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private bool m_executeInEditMode = false;
	private SpriteRenderer m_sprite = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_sprite = GetComponent<SpriteRenderer>();
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
		if(Application.isPlaying) {
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
		if(!this.isActiveAndEnabled) return;

		// Main camera must be valid
		Camera cam = null;
		if(Application.isPlaying) {
			cam = InstanceManager.sceneController.mainCamera;
		} else if(m_executeInEditMode) {
			cam = Camera.main;
		}
		if(cam == null) return;

		// Find out base sprite's size
		m_sprite.transform.localScale = Vector3.one;
		float spriteW = m_sprite.sprite.bounds.size.x;
		float spriteH = m_sprite.sprite.bounds.size.y;

		// Compute viewport bounds at the Z of the sprite
		// ViewportToWorldPoint(): Viewport space is normalized and relative to the camera. The bottom-left of the viewport is (0,0); the top-right is (1,1). The z position is in world units from the camera.
		// Be generous to make sure all the screen is covered
		Vector3 minWorldPos = cam.ViewportToWorldPoint(new Vector3(-0.1f, -0.1f, m_sprite.transform.position.z));
		Vector3 maxWorldPos = cam.ViewportToWorldPoint(new Vector3(1.1f, 1.1f, m_sprite.transform.position.z));
		Rect viewportBounds = new Rect(
			minWorldPos.x,
			minWorldPos.y,
			maxWorldPos.x - minWorldPos.x,
			maxWorldPos.y - minWorldPos.y
		);

		// Change sprite's scale to fit the new size
		m_sprite.transform.localScale = new Vector3(
			viewportBounds.width / spriteW,
			viewportBounds.height / spriteH,
			1f
		);

		// Change sprite's position to be centered on the screen
		m_sprite.transform.position = new Vector3(
			viewportBounds.center.x,
			viewportBounds.center.y,
			m_sprite.transform.position.z
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}