// CPCameraZoom.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Several debug options to control the game camera.
/// </summary>
public class CPCameraZoom : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	private GameCameraController m_camController = null;
	private Slider m_slider = null;
	public enum ZoomType
	{
		DEFAULT,
		FAR
	}
	public ZoomType m_zoomType;

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		m_slider = GetComponent<Slider>();
	}

	/// <summary>
	/// Component was enabled.
	/// </summary>
	void OnEnable() {
		// If camera ref wasn't initialized, try to do it now
		if(m_camController == null) OnLevelWasLoaded();
	}

	/// <summary>
	/// Sets the zoom offset of the camera.
	/// </summary>
	/// <param name="_value">The new zoom offset of the camera.</param>
	public void SetZoomOffset(float _value) {
		
		if(m_camController != null) 
		{
			switch(m_zoomType)
			{
				case ZoomType.DEFAULT:
				{
					m_camController.defaultZoom = _value;
				}break;
				case ZoomType.FAR:
				{
					m_camController.farZoom = _value;
				}break;
			}
		}
	}

	/// <summary>
	/// A new scene has been loaded.
	/// </summary>
	private void OnLevelWasLoaded() {
		// Reset camera reference to the camera in the new scene
		m_camController = GameObject.FindObjectOfType<GameCameraController>();

		// Initialize slider with new scene's camera values
		m_slider.interactable = (m_camController != null);
		m_slider.value = 0;	// Initial value
	}
}
