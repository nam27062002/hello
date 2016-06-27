// PopupEggShop.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 04/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controller for the Eggs shop popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupEggShop : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string PATH = "UI/Popups/Shop/PF_PopupEggShop";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private GameObject m_pillPrefab = null;
	[SerializeField] private SnappingScrollRect m_scrollList = null;

	[Space]
	[SerializeField] private Text m_rewardsText = null;
	[SerializeField] private Text m_priceText = null;

	// Special initialization settings
	private string m_initialDragonSku = "";

	// Internal
	private List<PopupEggShopPill> m_pills = new List<PopupEggShopPill>();
	private int m_selectedPill = -1;
	private bool m_showIntroScroll = true;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_pillPrefab != null, "Required field!");
		Debug.Assert(m_scrollList != null, "Required field!");

		Debug.Assert(m_rewardsText != null, "Required field!");
		Debug.Assert(m_priceText != null, "Required field!");

		m_initialDragonSku = "";

	
		// Create all the Egg pills and add them inside the scroll

		// Get the the content
		List<DefinitionNode> defList = new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.EGGS, ref defList);
		DefinitionsManager.SharedInstance.SortByProperty(ref defList, "shopOrder", DefinitionsManager.SortType.NUMERIC);

		for (int i = 0; i < defList.Count; i++) {
			GameObject pill = GameObject.Instantiate<GameObject>(m_pillPrefab);
			PopupEggShopPill eggPill = pill.GetComponent<PopupEggShopPill>();

			eggPill.InitFromDef(defList[i]);
			pill.transform.parent = m_scrollList.content;
		}

		// Subscribe to events
		GetComponent<PopupController>().OnOpenPreAnimation.AddListener(OnOpenPreAnimation);
		GetComponent<PopupController>().OnOpenPostAnimation.AddListener(OnOpenPostAnimation);

		// Subscribe to external events
		Messenger.AddListener<string>(EngineEvents.SCENE_UNLOADED, OnSceneUnloaded);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe to external events
		Messenger.RemoveListener<string>(EngineEvents.SCENE_UNLOADED, OnSceneUnloaded);
	}

	//------------------------------------------------------------------//
	// CUSTOM SETUP METHODS												//
	//------------------------------------------------------------------//
	/// <summary>
	/// Define the egg to scroll to once the popup is opened.
	/// Ideally to be called immediately after requesting the popup manager to open the popup.
	/// </summary>
	/// <param name="_initialDragonSku">Sku of the dragon whose egg we want to focus.</param>
	public void SetInitialEgg(string _initialDragonSku) {
		// Just store it
		m_initialDragonSku = _initialDragonSku;
	}

	/// <summary>
	/// Define which eggs should be displayed.
	/// </summary>
	/// <param name="_dragonSkus">Skus of the dragons whose related eggs we want to be in the shop. Empty will show all available eggs.</param>
	public void SetVisibleEggs(string[] _dragonSkus) {
		// [AOC] TODO!!
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Popup is going to be opened.
	/// </summary>
	private void OnOpenPreAnimation() {
		// [AOC] TODO!! Dynamically instantiating the pills cause Unity editor to hang. Figure out how to fix it :(
		/*
		// Populate eggs list
		// [AOC] TODO!! Do it async? Not 100% required in this case, but may be necessary for longer lists
		List<DefinitionNode> eggDefs = Definitions.GetDefinitions(Definitions.Category.EGGS);
		Definitions.SortByProperty(ref eggDefs, "shopOrder", Definitions.SortType.NUMERIC);

		// Create a pill for each egg
		GameObject newPillObj = null;
		PopupEggShopPill newPill = null;
		for(int i = 0; i < eggDefs.Count; i++) {
			// Instantiate pill
			newPillObj = GameObject.Instantiate<GameObject>(m_pillPrefab);

			// Initialize with the given definition
			newPill = newPillObj.GetComponent<PopupEggShopPill>();
			newPill.InitFromDef(eggDefs[i]);

			// Add pill to scroll list
			newPillObj.transform.SetParent(m_scrollList.content.transform);

			// Add pill to local list for further access
			m_pills.Add(newPill);
		}
		*/

		// For now just grab all the instantiated pills
		m_pills.Clear();
		m_pills.AddRange(GetComponentsInChildren<PopupEggShopPill>(true));

		// By default show all the eggs
		SetVisibleEggs(null);

		// Force a scroll animation by setting the scroll instantly to last pill then scrolling to the first one
		// Only the first time
		if(m_showIntroScroll) {
			m_scrollList.SelectPoint(m_pills.Last().snapPoint, false);
		}
	}

	/// <summary>
	/// Popup has been opened.
	/// </summary>
	private void OnOpenPostAnimation() {
		// Scroll animation pt 2
		// Only the first time
		if(m_showIntroScroll) {
			m_scrollList.SelectPoint(m_pills.First().snapPoint, true);
			m_showIntroScroll = false;
		}

		// Scroll to initial egg
		if(m_initialDragonSku != "") {
			for(int i = 0; i < m_pills.Count; i++) {
				if (m_pills[i].eggDef.GetAsString("dragonSku") == m_initialDragonSku) {
					m_scrollList.SelectPoint(m_pills[i].snapPoint);
					break;
				}
			}
		}
	}

	/// <summary>
	/// Selected pill has changed.
	/// </summary>
	/// <param name="_selectedPoint">The newly selected point.</param>
	public void OnSelectionChanged(ScrollRectSnapPoint _selectedPoint) {
		// Find selected pill
		for(int i = 0; i < m_pills.Count; i++) {
			// Is it the selected pill?
			if(m_pills[i].snapPoint == _selectedPoint) {
				// Yes!! Update index and break loop
				m_selectedPill = i;
				break;
			}
		}

		// Skip if new selection is not valid
		if(m_selectedPill < 0 || m_selectedPill >= m_pills.Count) return;

		// Refresh info with the pill data
		DefinitionNode eggDef = m_pills[m_selectedPill].eggDef;
		if(eggDef == null) return;	// Could happen if pills haven't already been initialized
		DefinitionNode dragonDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, eggDef.GetAsString("dragonSku"));
		if(dragonDef == null) return;

		// Rewards text
		StringBuilder sb = new StringBuilder();
		sb.AppendLine(Localization.Localize("TID_EGG_SHOP_REWARDS_DISGUISE", dragonDef.GetLocalized("tidName")));
		sb.AppendLine(Localization.Localize("TID_EGG_SHOP_REWARDS_PET"));
		sb.AppendLine(Localization.Localize("TID_EGG_SHOP_REWARDS_SPECIAL_DRAGON"));
		m_rewardsText.text = sb.ToString();

		// Price
		m_priceText.text = StringUtils.FormatNumber(eggDef.GetAsInt("pricePC"));
	}

	/// <summary>
	/// The purchase button has been pressed.
	/// </summary>
	public void OnPurchaseButton() {
		// Check that current selection is valid
		if(m_selectedPill < 0 || m_selectedPill >= m_pills.Count) return;

		// Can the egg be purchased?
		// Dragon must be owned
		string dragonSku = m_pills[m_selectedPill].eggDef.GetAsString("dragonSku");
		DragonData requiredDragon = DragonManager.GetDragonData(dragonSku);
		if(!requiredDragon.isOwned) {
			// Show feedback and return
			string text = Localization.Localize("TID_EGG_SHOP_DRAGON_REQUIRED", requiredDragon.def.GetLocalized("tidName"));
			UIFeedbackText textObj = UIFeedbackText.CreateAndLaunch(text, new Vector2(0.5f, 0.5f), (RectTransform)this.transform);
			textObj.GetComponent<Text>().color = Colors.red;
			return;
		}

		// Get price and start purchase flow
		long pricePC = m_pills[m_selectedPill].eggDef.GetAsLong("pricePC");
		if(UserProfile.pc >= pricePC) {
			// Perform transaction
			UserProfile.AddPC(-pricePC);
			PersistenceManager.Save();

			// Create a new egg instance
			Egg purchasedEgg = Egg.CreateFromDef(m_pills[m_selectedPill].eggDef);
			purchasedEgg.ChangeState(Egg.State.READY);	// Already ready for collection!

			// Go to OPEN_EGG screen and start flow
			MenuScreensController screensController = InstanceManager.sceneController.GetComponent<MenuScreensController>();
			OpenEggScreenController openEggScreen = screensController.GetScreen((int)MenuScreens.OPEN_EGG).GetComponent<OpenEggScreenController>();
			screensController.GoToScreen((int)MenuScreens.OPEN_EGG);
			openEggScreen.StartFlow(purchasedEgg);

			// Close this popup!
			// Don't destroy it since it's very possible we reuse it
			GetComponent<PopupController>().Close(false);
		} else {
			// Open PC shop popup
			//PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

			// Currency popup / Resources flow disabled for now
			UIFeedbackText.CreateAndLaunch(Localization.Localize("TID_PC_NOT_ENOUGH"), new Vector2(0.5f, 0.5f), this.GetComponentInParent<Canvas>().transform as RectTransform);
		}
	}

	/// <summary>
	/// Show extra info.
	/// </summary>
	public void OnInfoButton() {
		// [AOC] TODO!!
		UIFeedbackText textObj = UIFeedbackText.CreateAndLaunch(Localization.Localize("TID_GEN_COMING_SOON"), new Vector2(0.5f, 0.5f), (RectTransform)this.transform);
		textObj.GetComponent<Text>().color = Colors.white;
	}

	/// <summary>
	/// A scene has been unloaded.
	/// </summary>
	/// <param name="_sceneName">The name of the scen that has been unloaded.</param>
	private void OnSceneUnloaded(string _sceneName) {
		// Since this popup is reused (not destroyed when closing), destroy it upon changing scene
		GameObject.Destroy(this.gameObject);	// We don't care at all about animations at this point
	}
}
