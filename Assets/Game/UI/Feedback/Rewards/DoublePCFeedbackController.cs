// DoublePCFeedbackController.cs
// Hungry Dragon
// 
// Created by  on 29/06/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

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
public class DoublePCFeedbackController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[SerializeField]
	private MenuPetLoader m_menuPetLoader;

	[SerializeField]
	private Canvas textCanvas;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

		BabyDragon babyDragon = InstanceManager.player.GetBabyDragon();

		if (babyDragon != null && ! string.IsNullOrEmpty(babyDragon.sku) )
		{
			m_menuPetLoader.Load(babyDragon.sku);

		}

        if (textCanvas != null)
        {   
            // Assing the text canvas to the UI camera. Otherwise the text wont be rendered properly
			textCanvas.worldCamera = InstanceManager.uiCamera.unityCamera;
			textCanvas.planeDistance = 1f;
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