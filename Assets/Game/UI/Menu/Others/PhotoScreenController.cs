// PhotoScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 01/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Main controller for the photo menu screen.
/// </summary>
public class PhotoScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private Localizer m_dragonName = null;
	[SerializeField] private Localizer m_dragonDesc = null;
	[SerializeField] private Image m_dragonTierIcon = null;
	[Space]
	[SerializeField] private DragControlRotation m_dragController = null;
	[SerializeField] private DOTweenAnimation m_flashFX = null;
	[Space]
	[SerializeField] private List<GameObject> m_objectsToHide = new List<GameObject>();

	// Internal
	private Texture2D m_picture = null;
	private List<GameObject> m_objectsToShow = new List<GameObject>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

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
		m_picture = null;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Does a screenshot and saves it into the picture texture, overriding its previous content.
	/// </summary>
	/// <returns>The coroutine.</returns>
	private IEnumerator TakePicture() {
		// Hide all UI elements
		m_objectsToShow.Clear();	// Only those that were actually active will be restored
		for(int i = 0; i < m_objectsToHide.Count; i++) {
			if(m_objectsToHide[i].activeSelf) {
				m_objectsToHide[i].SetActive(false);
				m_objectsToShow.Add(m_objectsToHide[i]);
			}
		}

		// Hide HUD as well
		InstanceManager.menuSceneController.hud.gameObject.SetActive(false);

		// Wait until the end of the frame so the "hide" is actually applied
		yield return new WaitForEndOfFrame();

		// Take the screenshot!
		// [AOC] We're not using Application.Screenshot() since we want to have the screenshot in a texture rather than on an image in disk, for sharing and previewing it
		//		 From FGOL
		// Aux vars
		int width = Screen.width;
		int height = Screen.height;

		// If texture is not created, do it now
		if(m_picture == null) {
			m_picture = new Texture2D(width, height, TextureFormat.RGB24, false);
		}

		// Read screen contents into the texture
		m_picture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		m_picture.Apply();

		// Launch Flash FX! (AFTER the screenshot, of course! :D)
		m_flashFX.gameObject.SetActive(true);
		m_flashFX.DORestart();

		// Give it some time
		yield return new WaitForSeconds(0.25f);

		// Restore disabled objects
		for(int i = 0; i < m_objectsToShow.Count; i++) {
			m_objectsToShow[i].SetActive(true);
		}

		// Restore HUD as well
		InstanceManager.menuSceneController.hud.gameObject.SetActive(true);

		// Open "Share" popup
		PopupPhotoShare popup = PopupManager.OpenPopupInstant(PopupPhotoShare.PATH).GetComponent<PopupPhotoShare>();
		popup.Init(m_picture);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Screen is about to be open.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPreAnimation(ShowHideAnimator _animator) {
		// Aux vars
		MenuSceneController menuController = InstanceManager.menuSceneController;
		DragonData dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Initialize dragon info
		if(m_dragonName != null) m_dragonName.Localize(dragonData.def.GetAsString("tidName"));
		if(m_dragonDesc != null) m_dragonDesc.Localize(dragonData.def.GetAsString("tidDesc"));
		if(m_dragonTierIcon != null) m_dragonTierIcon.sprite = ResourcesExt.LoadFromSpritesheet(UIConstants.UI_SPRITESHEET_PATH, dragonData.tierDef.GetAsString("icon"));

		// Initialize drag controller with current dragon preview
		MenuScreenScene scene3D = menuController.screensController.GetScene((int)MenuScreens.PHOTO);
		MenuDragonPreview dragonPreview = scene3D.GetComponent<MenuDragonScroller>().GetDragonPreview(menuController.selectedDragon);
		m_dragController.target = dragonPreview.transform;
	}

	/// <summary>
	/// Take the picture!
	/// </summary>
	public void OnTakePictureButton() {
		// Do it in a coroutine to wait until the end of the frame
		StartCoroutine(TakePicture());
	}
}