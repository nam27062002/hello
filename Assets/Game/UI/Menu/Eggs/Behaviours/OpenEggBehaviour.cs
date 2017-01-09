// OpenEggBehaviour.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls a single egg on the open egg menu.
/// </summary>
[RequireComponent(typeof(EggController))]
public class OpenEggBehaviour : MonoBehaviour, IPointerClickHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int TAPS_TO_OPEN = 2;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Internal logic
	private int m_tapCount = 0;
	public int tapCount {
		get { return m_tapCount; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// If we are not at the menu scene, disable this component
		MenuSceneController sceneController = InstanceManager.GetSceneController<MenuSceneController>();
		if(sceneController == null) {
			this.enabled = false;
			return;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	public void OnPointerClick(PointerEventData _eventData) {
		// Ignore if component is not enabled
		if(!this.enabled) return;
		if(m_tapCount >= TAPS_TO_OPEN) return;

		// Aux vars
		EggController eggController = GetComponent<EggController>();

		// Increase tap count!
		m_tapCount++;

		// If we've reached the required amount of taps, open egg!
		if(m_tapCount >= TAPS_TO_OPEN) {
			// Do it!
			// Open FX are managed externally
			eggController.eggData.Collect();

			// Reset count
			m_tapCount = 0;
		}

		// Otherwise show some FX
		else {
			/*// Kill any existing tween
			DOTween.Kill("eggTap");

			// Scale down a bit every time xD
			// Use relative scaling
			Vector3 targetScale = new Vector3(transform.localScale.x * 1.1f, transform.localScale.y * 0.9f, transform.localScale.z * 1.1f);
			transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutElastic).SetId("eggTap");*/
		}

		// Refresh view
		eggController.Refresh();
	}
}

