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
using UnityEngine.EventSystems;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controls a single egg when it's ready for collection.
/// </summary>
[RequireComponent(typeof(EggView))]
public class ReadyEggBehaviour : MonoBehaviour, IPointerClickHandler {
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
		MenuSceneController sceneController = InstanceManager.menuSceneController;
		if(sceneController == null) {
			this.enabled = false;
			return;
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The egg has been tapped.
	/// </summary>
	public void OnPointerClick(PointerEventData _eventData) {
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
		InstanceManager.sceneController.GetComponent<MenuScreensController>().StartOpenEggFlow(GetComponent<EggView>().eggData);
	}
}

