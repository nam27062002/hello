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
	public MenuDragonLoader dragonLoader {
		get{
			if(m_dragonLoader == null) {
				m_dragonLoader = GetComponentInChildren<MenuDragonLoader>();
			} 
			return m_dragonLoader; 
		}
	}
	private IDragonData m_dragonData;

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

	private IDragonData.LockState m_currentState = IDragonData.LockState.HIDDEN;
	public IDragonData.LockState currentState{
		get{ return m_currentState; }
	}

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
		if (m_currentState <= IDragonData.LockState.REVEAL) {
			m_dragonLoader.useShadowMaterial = true;
		}
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
			IDragonData.LockState newState = m_dragonData.lockState;

			if (m_currentState != newState) {
				if (newState == IDragonData.LockState.SHADOW ||  newState == IDragonData.LockState.REVEAL) {
					m_dragonLoader.useShadowMaterial = true;
				} else if (newState == IDragonData.LockState.HIDDEN || newState == IDragonData.LockState.TEASE) {
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
	/// <summary>
	/// The slot's show animation has just finished.
	/// </summary>
	public void OnShowPostAnimation() {
		// Rescale all particles
		if ( m_dragonPreview )
		{
			ParticleScaler[] scalers = m_dragonPreview.GetComponentsInChildren<ParticleScaler>();
			for(int i = 0;i<scalers.Length; ++i) {
				scalers[i].DoScale();
			}
		}
	}
}