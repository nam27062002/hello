// EquipmentScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/07/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class EquipmentScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum Subscreen {
		DISGUISES,
		PETS
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private Button m_disguisesButton = null;
	[SerializeField] private DisguisesScreenController m_disguisesScreen = null;

	[Space]
	[SerializeField] private Button m_petsButton = null;
	[SerializeField] private PetsScreenController m_petsScreen = null;

	[Space]
	[SerializeField] private Button m_buyButton = null;

	// Internal
	private Subscreen m_lastActiveScreen = Subscreen.DISGUISES;

	// Buy button egg preview
	private DefinitionNode m_eggDef = null;	// Egg matching dragon sku
	private EggUIScene3D m_eggPreviewScene = null;	// Container holding the preview scene (camera, egg, decos, etc.)
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Register to external events
		ShowHideAnimator animator = GetComponent<ShowHideAnimator>();
		if(animator != null) {
			animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
			animator.OnHidePreAnimation.AddListener(OnHidePreAnimation);
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		// Initialize with either of the screens!
		switch(m_lastActiveScreen) {
			case Subscreen.DISGUISES:	ShowDisguises();	break;
			case Subscreen.PETS: 		ShowPets();			break;
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Get egg corresponding to target dragon
		string dragonSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		m_eggDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.EGGS, Egg.SKU_STANDARD_EGG);

		// Initialize buy button
		if(m_eggDef != null) {
			// Price
			m_buyButton.FindComponentRecursive<Text>("TextPrice").text = StringUtils.FormatNumber(m_eggDef.GetAsInt("pricePC"));

			// If the 3D preview scene was not created, do it now and link it to the raw image
			if(m_eggPreviewScene == null) {
				RawImage eggPreviewArea = m_buyButton.GetComponentInChildren<RawImage>();
				m_eggPreviewScene = EggUIScene3D.CreateEmpty();
				m_eggPreviewScene.InitRawImage(ref eggPreviewArea);
			}

			// Initialize with the target egg
			// The scene will take care of everything
			m_eggPreviewScene.SetEgg(Egg.CreateFromDef(m_eggDef));
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
		// Unregister from external events
		ShowHideAnimator animator = GetComponent<ShowHideAnimator>();
		if(animator != null) {
			animator.OnShowPreAnimation.RemoveListener(OnShowPreAnimation);
			animator.OnHidePreAnimation.RemoveListener(OnHidePreAnimation);
		}

		// Destroy Egg 3D scene
		if(m_eggPreviewScene != null) {
			UIScene3DManager.Remove(m_eggPreviewScene);
			m_eggPreviewScene = null;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Shows the disguises screen and hides the pets.
	/// </summary>
	public void ShowDisguises() {
		// If we're in another screen, just change the lastActiveScreen so the disguises screen is displayed by default when opening the equipment screen
		if(isActiveAndEnabled) {
			m_disguisesScreen.Show();
			m_disguisesButton.interactable = false;
			m_petsScreen.Hide();
			m_petsButton.interactable = true;
		}
		m_lastActiveScreen = Subscreen.DISGUISES;
	}

	/// <summary>
	/// Shows the pets screen and hides the disguises.
	/// </summary>
	public void ShowPets() {
		// If we're in another screen, just change the lastActiveScreen so the pets screen is displayed by default when opening the equipment screen
		if(isActiveAndEnabled) {
			m_disguisesScreen.Hide();
			m_disguisesButton.interactable = true;
			m_petsScreen.Show();
			m_petsButton.interactable = false;
		}
		m_lastActiveScreen = Subscreen.PETS;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The screen is about to be hidden.
	/// </summary>
	/// <param name="_animator">The animator that is about to be hidden.</param>
	private void OnHidePreAnimation(ShowHideAnimator _animator) {
		// Hdie both sub-screens
		m_disguisesScreen.Hide();
		m_petsScreen.Hide();
	}

	/// <summary>
	/// The screen is about to be displayed.
	/// </summary>
	/// <param name="_animator">The animator that is about to be shown.</param>
	private void OnShowPreAnimation(ShowHideAnimator _animator) {
		// Restore last active screen and hide the rest
		switch(m_lastActiveScreen) {
			case Subscreen.DISGUISES:	ShowDisguises();	break;
			case Subscreen.PETS: 		ShowPets();			break;
		}
	}

	/// <summary>
	/// Buy button has been pressed.
	/// </summary>
	public void OnBuy() {
		// SFX
		AudioManager.instance.PlayClip("audio/sfx/UI/hsx_ui_button_select");

		// Get price and start purchase flow
		long pricePC = m_eggDef.GetAsLong("pricePC");
		if(UsersManager.currentUser.pc >= pricePC) {
			// Perform transaction
			UsersManager.currentUser.AddPC(-pricePC);
			PersistenceManager.Save();

			// Create a new egg instance
			Egg purchasedEgg = Egg.CreateFromDef(m_eggDef);
			purchasedEgg.ChangeState(Egg.State.READY);	// Already ready for collection!

			// Go to OPEN_EGG screen and start flow
			MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
			OpenEggScreenController openEggScreen = screensController.GetScreen((int)MenuScreens.OPEN_EGG).GetComponent<OpenEggScreenController>();
			screensController.GoToScreen((int)MenuScreens.OPEN_EGG);
			openEggScreen.StartFlow(purchasedEgg);
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}
}