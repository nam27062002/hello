using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PromotedIAPShopPill : IPopupShopPill {
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES                                                 //
    //------------------------------------------------------------------------//
    private OfferPack m_offerPack;
    private Metagame.Reward.Data m_shopPack;



    //------------------------------------------------------------------------//
    // OTHER METHODS                                                          //
    //------------------------------------------------------------------------//
    public void InitFromSku(string _sku) {
        // maybe is an offer pack
        m_offerPack = OffersManager.GetOfferPackByIAP(_sku);

        if (m_offerPack == null) {
            m_def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.SHOP_PACKS, _sku);

            // then, is an standard shop pack
            m_shopPack = new Metagame.Reward.Data {
                sku = m_def.sku,
                typeCode = m_def.Get("type"),
                amount = m_def.GetAsLong("amount")
            };
            if (m_shopPack.typeCode == "hc")
                m_shopPack.typeCode = "pc";
        } else {
            m_def = m_offerPack.def;
        }

        m_currency = UserProfile.Currency.REAL;
    }



    //------------------------------------------------------------------------//
    // IPopupShopPill IMPLEMENTATION                                          //
    //------------------------------------------------------------------------//
    public override string GetIAPSku() {
        if (m_offerPack != null) {
            return m_def.Get("iapSku");
        } else {
            return m_def.sku;
        }
    }

    protected override void ApplyShopPack() {
        // Save rewards into profile
        if (m_offerPack != null) {
            m_offerPack.Apply();
        } else {
            UsersManager.currentUser.PushReward(Metagame.Reward.CreateFromData(m_shopPack, HDTrackingManager.EEconomyGroup.SHOP_PC_PACK, "promotedIAP"));
        }

        // Close all open popups
        PopupManager.Clear(true);

        // Move to the rewards screen
        PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
        scr.StartFlow(false);   // No intro
        InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);
    }

    protected override HDTrackingManager.EEconomyGroup GetTrackingId() {
        return HDTrackingManager.EEconomyGroup.SHOP_PROMOTED_IAP;
    }

    protected override void ShowPurchaseSuccessFeedback() {
      // do we need something?
    }
}
