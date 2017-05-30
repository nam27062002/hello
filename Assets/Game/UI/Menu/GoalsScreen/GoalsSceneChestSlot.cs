// ChestTooltip.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/10/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Control a single chest slot in the Goals Screen 3D scene.
/// </summary>
public class GoalsSceneChestSlot : MonoBehaviour, IPointerClickHandler {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// External refs
	[SerializeField] private Transform m_uiAnchor = null;
	public Transform uiAnchor {
		get { return m_uiAnchor; }
	}

	// Jump setup
	[Space]
	[SerializeField] private int m_numJumps = 1;
	[SerializeField] private float m_jumpForce = 0.15f;
	[SerializeField] private Ease m_jumpEase = Ease.OutQuad;
	[SerializeField] private float m_jumpDuration = 0.25f;

	// Internal
	private ChestViewController m_view = null;
	public ChestViewController view {
		get { return m_view; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Instantiate the chest view
		GameObject chestPrefab = Resources.Load<GameObject>(ChestViewController.PREFAB_PATH);
		GameObject chestObj = GameObject.Instantiate<GameObject>(chestPrefab);
		chestObj.transform.SetParent(this.transform, false);
		m_view = chestObj.GetComponentInChildren<ChestViewController>();
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
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	public void OnPointerClick(PointerEventData _eventData) {
		// Just do a small bounce animation :D
		this.transform.DOKill(true);
		this.transform
			.DOLocalJump(this.transform.localPosition, m_jumpForce, m_numJumps, m_jumpDuration)
			.SetEase(m_jumpEase);
	}
}