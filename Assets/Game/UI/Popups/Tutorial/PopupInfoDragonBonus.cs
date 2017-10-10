// PopupInfoDragonBonus.cs
// 

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tiers info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoDragonBonus : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoDragonBonus";

	//------------------------------------------------------------------------//
	// ATTRIBUTES															  //
	//------------------------------------------------------------------------//
	[SerializeField] private Image m_icon;
	[SerializeField] private TextMeshProUGUI m_text;

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	public void OnOpenPreAnimation() {
		DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DRAGONS, GlobalEventManager.currentEvent.bonusDragonSku);
		m_text.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_BONUS_DRAGON_INFO_MESSAGE", def.GetLocalized("tidName"));

		GlobalEvent evt = GlobalEventManager.currentEvent;
		m_icon.sprite = Resources.Load<Sprite>(UIConstants.DISGUISE_ICONS_PATH + evt.bonusDragonSku + "/icon_disguise_0");	// Default skin
	}
}
