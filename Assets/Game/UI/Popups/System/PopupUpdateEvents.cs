using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(PopupController))]
public class PopupUpdateEvents : MonoBehaviour
{

    public const string PATH = "UI/Popups/Message/PF_PopupUpdateEvents";
    public TextMeshProUGUI m_verion;

    void Awake()
    {
		m_verion.text = GameSettings.internalVersion.ToString() + " ("+ ServerManager.SharedInstance.GetRevisionVersion() +")";
    }

    /// <summary>
    /// Method called when the user clicks on any Confirm buttom. It's called by the editor
    /// </summary>
    public void OnConfirm()
    {
        ApplicationManager.Apps_OpenAppInStore(ApplicationManager.EApp.HungryDragon);		
    }



}
