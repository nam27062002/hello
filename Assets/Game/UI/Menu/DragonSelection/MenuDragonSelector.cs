// MenuDragonSelector.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Select the current dragon in the menu screen.
/// </summary>
public class MenuDragonSelector : UISelectorTemplate<IDragonData>, IPointerClickHandler {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[Separator]
	[SerializeField] private IDragonData.Type m_dragonType = IDragonData.Type.CLASSIC;

	// Dragon animation
	[Separator("Dragon tap animation")]
	[List("scale", "move", "rotate")]
	[SerializeField] private string m_animId = "scale";
	[SerializeField] private float m_animDuration = 1f;
	[SerializeField] private Vector3 m_animValue = Vector3.one;
	[SerializeField] private Ease m_animEase = Ease.OutBack;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to events
		OnSelectionChanged.AddListener(OnSelectedDragonChanged);

        // Initialize items list with all the dragons
        enableEvents = false;
        Init(DragonManager.GetDragonsByOrder(IDragonData.Type.ALL));

        // Figure out initial index
        string selectedSku = InstanceManager.menuSceneController.selectedDragon;
        for (int i = 0; i < m_items.Count; i++) {
            if (selectedSku == m_items[i].def.sku) {
                SelectItem(i);
                break;
            }
        }
        enableEvents = true;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Subscribe to events
		OnSelectionChanged.RemoveListener(OnSelectedDragonChanged);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Changes dragon selected to the given one.
	/// </summary>
	/// <param name="_sku">The sku of the dragon we want to be the current one.</param>
	public void SetSelectedDragon(string _sku) {
		// Get data belonging to this sku
		IDragonData data = DragonManager.GetDragonData(_sku);
		if(data == null) return;
		SelectItem(data);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon has been changed.
	/// </summary>
	/// <param name="_oldDragon">Data of the previously selected dragon.</param>
	/// <param name="_newDragon">Data of the new dragon.</param>
	public void OnSelectedDragonChanged(IDragonData _oldDragon, IDragonData _newDragon) {
		if(_newDragon != null) {
			// Notify game
			Messenger.Broadcast<string>(MessengerEvents.MENU_DRAGON_SELECTED, _newDragon.def.sku);

			// Play some SFX!
			AudioController.Play("hd_arrow_left");
		}
	}

	/// <summary>
	/// The selected object on the scroll list has changed.
	/// </summary>
	/// <param name="_newSelectedPoint">The new selected node object of the scrolllist.</param>
	[System.Obsolete("To be used if the dragon scroll was to be handled by a SnappingScroll rect (which currently is not)")]
	public void OnScrollSelectedDragonChanged(ScrollRectSnapPoint _newSelectedPoint) {
		// Skip if null (shouldn't happen)
		if(_newSelectedPoint == null) return;

		// We know the new selected object must have a MenuDragonPreview component somewhere, use it to define the new selected dragon
		MenuDragonPreview dragonPreview = _newSelectedPoint.GetComponentInChildren<MenuDragonPreview>();
		if(dragonPreview != null) {
			SetSelectedDragon(dragonPreview.sku);
		}
	}

	/// <summary>
	/// OnMouseUpAsButton is only called when the mouse is released over the same 
	/// GUIElement or Collider as it was pressed.
	/// </summary>
	public void OnPointerClick(PointerEventData _eventData) {
		// Find all object intersecting with the raycast, looking for dragons and pets
		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(_eventData, results);

		// Ignore if there is only one hit (that's ourselves)
		if(results.Count <= 1) return;
			
		// Find pets and dragons!
		MenuScreen targetScreen = MenuScreen.NONE;
		MenuPetPreview pet = null;
		MenuDragonPreview dragon = null;
		for(int i = 0; i < results.Count; i++) {
			// Get pets and dragons
			pet = results[i].gameObject.FindComponentInParents<MenuPetPreview>();
			dragon = results[i].gameObject.FindComponentInParents<MenuDragonPreview>();

			// Is it a pet?
			// Look for pets first, since pets are children of dragons and looking for dragons will result in a false positive!
			if(pet != null) {
				// Yes! Go to the pet screen
				targetScreen = InstanceManager.menuSceneController.GetPetScreenForCurrentMode();	// [AOC] Lab or regular pet screen?

				// Do a fun animation on the pet!
				pet.SetAnim(MenuPetPreview.Anim.IN);
				break;
			}

			// Is it a dragon?
			else if(dragon != null) {
				// a) Goto Game!
				// Only if enabled!
				if(DebugSettings.menuEnableShortcuts) {
					// Only owned dragons!
					if(DragonManager.GetDragonData(dragon.sku).isOwned) {
						// Menu scene controller will manage it
						InstanceManager.menuSceneController.GoToGame();
					}
				}

				// b) Goto Disguises Screen
				// Yes! Go to the disguises screen
				//targetScreen = MenuScreen.SKINS;

				// Do a fun animation on the dragon!
				// Only owned dragons!
				if(DragonManager.GetDragonData(dragon.sku).isOwned) {
					//dragon.SetAnim(MenuDragonPreview.Anim.FUN);
					/*
					dragon.transform.DOKill(true);
					switch(m_animId) {
						case "scale": {
							dragon.transform.DOScale(Vector3.Scale(dragon.transform.localScale, m_animValue), m_animDuration).SetEase(m_animEase).SetLoops(2, LoopType.Yoyo);
						} break;

						case "move": {
							dragon.transform.DOLocalMove(dragon.transform.localPosition + m_animValue, m_animDuration).SetEase(m_animEase).SetLoops(2, LoopType.Yoyo);
						} break;

						case "rotate": {
							dragon.transform.DOBlendableRotateBy(m_animValue, m_animDuration, RotateMode.FastBeyond360).SetEase(m_animEase);
						} break;
					}
					*/
					// dragon.ForcePreferedAltAnimation();
				}
				break;
			}
		}

		// Go to the target screen, if any
		// Only if enabled!
		if(DebugSettings.menuEnableShortcuts) {
			if(targetScreen != MenuScreen.NONE) {
				// Check conditions
				MenuSceneController menuController = InstanceManager.menuSceneController;

				// a) Current dragon is owned
				if(!DragonManager.GetDragonData(menuController.selectedDragon).isOwned) return;

				// b) Camera is not tweening (scrolling between dragons)
				if(menuController.isTweening) return;

				// c) We're not scrolling between dragons
				if(menuController.dragonScroller.cameraAnimator.isTweening) return;

				// Everything ok! Go to the disguises screen
				menuController.GoToScreen(targetScreen);
			}
		}
	}
}

