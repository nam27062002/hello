// ReadyEggBehaviour.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/03/2016.
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
/// Controls a single egg when it's ready for collection.
/// </summary>
[RequireComponent(typeof(EggController))]
public class ReadyEggBehaviour : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

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

		// Subscribe to mouse events on the collider
		MouseEventsPropagator mouseEvents = GetComponentInChildren<MouseEventsPropagator>(true);
		if(mouseEvents != null) {
			mouseEvents.onMouseUpAsButton.AddListener(OnMouseUpAsButton);
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The egg has been tapped.
	/// </summary>
	public void OnMouseUpAsButton() {
		// Ignore if component is disabled
		if(!enabled) return;

		// Do some funny anim
		DOTween.Kill("eggTap");	// Kill any existing tween
		Vector3 targetScale = new Vector3(transform.localScale.x * 1.1f, transform.localScale.y * 0.9f, transform.localScale.z * 1.1f);
		DOTween.Sequence().SetId("eggTap").SetAutoKill(true)
			.Append(transform.DOScale(targetScale, 0.1f).SetEase(Ease.InQuad))
			.Append(transform.DOScale(transform.localScale, 0.4f).SetEase(Ease.OutElastic))
			.Play();

		// Incubator screen will take care of it
		MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
		IncubatorScreenController incubatorScreen = screensController.GetScreen((int)MenuScreens.INCUBATOR).GetComponent<IncubatorScreenController>();
		if(incubatorScreen != null) {
			incubatorScreen.OpenCurrentEgg();
		}
	}
}

