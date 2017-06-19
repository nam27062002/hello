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

	private bool m_collected = false;
	private Chest.RewardData m_rewardData = null;

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
	/// <summary>
	/// Initialize this slot with the given chest data.
	/// </summary>
	/// <param name="_collected">Was the chest collected?.</param>
	/// <param name="_rewardData">Reward corresponding to this chest.</param>
	public void Init(bool _collected, Chest.RewardData _rewardData) {
		// Store new data
		m_collected = _collected;
		m_rewardData = _rewardData;

		// Update view
		if(m_collected) {
			// Figure out reward type to show the proper FX
			view.Open(m_rewardData.type, false);
		} else {
			view.Close();
		}
	}

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

		// Trigger UI tooltip as well
		RectTransform parent = InstanceManager.menuSceneController.GetScreen(MenuScreens.GOALS).transform as RectTransform;
		string tid = m_collected ? "TID_CHEST_TIP_COLLECTED" : "TID_CHEST_TIP_NOT_COLLECTED";
		UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize(tid), new Vector2(0.5f, 0.4f), parent);
	}
}