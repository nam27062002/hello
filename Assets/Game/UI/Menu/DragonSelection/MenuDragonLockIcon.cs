// MenuDragonLockIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 24/01/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

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
/// Simple controller for the lock icon in the dragon selection screen.
/// </summary>
public class MenuDragonLockIcon : MonoBehaviour, IPointerClickHandler {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The input has detected a click over this element.
	/// </summary>
	/// <param name="_event">Data related to the event.</param>
	public void OnPointerClick(PointerEventData _event) {
		// Small animation :P
		this.transform.DOKill(true);
		//this.transform.DOScale(1.25f, 0.1f).SetRecyclable(true).SetLoops(2, LoopType.Yoyo).SetEase(Ease.Linear).SetRecyclable(true);
		this.transform.DOBlendableLocalRotateBy(new Vector3(0f, 0f, -45f), 0.1f).SetLoops(4, LoopType.Yoyo).SetEase(Ease.Linear).SetRecyclable(true);
		_event.Reset();
	}
}