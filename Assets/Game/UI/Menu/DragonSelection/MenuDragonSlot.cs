// MenuDragonSlot.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/02/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Script to identify dragon slots in the dragon selection menu.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class MenuDragonSlot : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
		
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Private references
	private MenuDragonLoader m_dragonLoader;
	private DragonData m_dragonData;

	// Public references
	private MenuDragonPreview m_dragonPreview = null;
	public MenuDragonPreview dragonPreview {
		get {
			if(m_dragonPreview == null) {
				m_dragonPreview = GetComponentInChildren<MenuDragonPreview>();
			}
			return m_dragonPreview;
		}
	}

	private ShowHideAnimator m_animator = null;
	public ShowHideAnimator animator {
		get {
			if(m_animator == null) {
				m_animator = GetComponent<ShowHideAnimator>();
			}
			return m_animator;
		}
	}

	private DragonData.LockState m_currentState = DragonData.LockState.HIDDEN;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_dragonLoader = GetComponentInChildren<MenuDragonLoader>();
		m_dragonData = DragonManager.GetDragonData(m_dragonLoader.dragonSku);

		m_currentState = m_dragonData.lockState;
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		if (m_dragonData != null) {
			DragonData.LockState newState = m_dragonData.lockState;

			if (m_currentState != newState) {
				if (newState == DragonData.LockState.SHADOW ||  newState == DragonData.LockState.REVEAL) {
					m_dragonLoader.useShadowMaterial = true;
				} else if (newState == DragonData.LockState.HIDDEN || newState == DragonData.LockState.TEASE) {
					m_dragonLoader.useShadowMaterial = true;
					m_animator.ForceHide(false);
				}

				m_currentState = newState;
			}
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
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