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
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls a single egg on the open egg menu.
/// </summary>
[RequireComponent(typeof(EggController))]
public class OpenEggBehaviour : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly int TAPS_TO_OPEN = 3;

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Internal logic
	private int m_tapCount = 0;
	public int tapCount {
		get { return m_tapCount; }
	}

	// References
	private Sequence m_shakeAnim = null;

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

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Start shake animation
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Stop shake animation
		//m_shakeAnim.DOComplete();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	private void OnMouseUpAsButton() {
		// Ignore if component is not enabled
		if(!this.enabled) return;

		// Increase tap count!
		m_tapCount++;

		// If we've reached the required amount of taps, open egg!
		if(m_tapCount >= TAPS_TO_OPEN) {
			// Do it!
			// Open FX are managed externally
			GetComponent<EggController>().eggData.Collect();
			PersistenceManager.Save();

			// Stop shake animation
			//m_shakeAnim.Pause();
		}

		// Otherwise show some FX
		else {
			// Kill any existing tween
			DOTween.Kill("eggTap");

			// Scale down a bit every time xD
			// Use relative scaling
			float delta = m_tapCount/(float)(TAPS_TO_OPEN - 1);
			Vector3 targetScale = new Vector3(transform.localScale.x * 1.1f, transform.localScale.y * 0.9f, transform.localScale.z * 1.1f);
			transform.DOScale(targetScale, 0.5f).SetEase(Ease.OutElastic).SetId("eggTap");
		}
	}
}

