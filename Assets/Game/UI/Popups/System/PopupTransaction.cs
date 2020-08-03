using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupTransaction : MonoBehaviour {
    //------------------------------------------------------------------------//
    // CONSTANTS															  //
    //------------------------------------------------------------------------//
    public const string PATH = "UI/Popups/Economy/PF_PopupTransaction";

    [SerializeField]
    private Button m_buttonConfirm;

    [SerializeField]
    private GameObject m_buttonCloseGO;

    [SerializeField]
    private TextMeshProUGUI m_amountText;

    [SerializeField]
    private Image m_currencyImage;

    private UnityAction m_onCancel;

    public void Init(int amount, UserProfile.Currency currency, UnityAction onConfirm, UnityAction onCancel) {
        if (m_amountText != null) {

            UIConstants.IconType iconType = UIConstants.GetCurrencyIcon(currency);            
            if (iconType == UIConstants.IconType.NONE) {
                Debug.LogWarning("Currency " + currency.ToString() + " not supported by PopupTransaction");
            }
                        
            m_amountText.text = UIConstants.GetIconString(amount, iconType, UIConstants.IconAlignment.LEFT);                        
        }

        if (m_buttonConfirm != null) {
            m_buttonConfirm.onClick.RemoveAllListeners();                        
            m_buttonConfirm.onClick.AddListener(onConfirm);
        }

        // By default the popup has the icon for UserProfile.Currency.SOFT, so if it's not that currency we need to load it
        if (m_currencyImage != null && currency == UserProfile.Currency.HARD) {
            m_currencyImage.sprite = Resources.Load<Sprite>(UIConstants.SHOP_ICONS_PATH + "icon_shop_gems_2");
        }

        if (m_buttonCloseGO != null)
        {
            m_buttonCloseGO.SetActive(onCancel != null);
        }

        m_onCancel = onCancel;
    }          

    private bool CanBeClosed()
    {
        return m_onCancel != null;
    }

    public void OnBackButton()
    {
        if (CanBeClosed())
        {
            OnCloseButton();
        }
    }  

    public void OnCloseButton()
    {
        if (CanBeClosed() && m_onCancel != null)
        {
            m_onCancel();
        }    
    }    
}
