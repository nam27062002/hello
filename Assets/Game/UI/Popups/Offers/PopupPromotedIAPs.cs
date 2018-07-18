using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupPromotedIAPs : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS                                                              //
    //------------------------------------------------------------------------//
    public const string PATH = "UI/Popups/Economy/PF_PopupPromotedIAPs";



    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES                                                 //
    //------------------------------------------------------------------------//
    [SerializeField] private PromotedIAPShopPill m_pill = null;



    //------------------------------------------------------------------------//
    // GENERIC METHODS                                                        //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake() {
        // Subscribe to external events
        m_pill.OnPurchaseError.AddListener(OnPurchaseError);
    }
    /// <summary>
    /// Destructor.
    /// </summary>
    private void OnDestroy() {
        // Unsubscribe from external events
        m_pill.OnPurchaseError.RemoveListener(OnPurchaseError);
    }



    //------------------------------------------------------------------------//
    // CALLBACKS                                                              //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Open animation is about to start.
    /// </summary>
    public void OnOpenPostAnimation() {
        m_pill.InitFromSku(GameStoreManager.SharedInstance.GetNextPromotedIAP());
        m_pill.OnBuyButton();
    }

    private void OnPurchaseError(IPopupShopPill _pill) {
        GetComponent<PopupController>().Close(true);
    }
}
