using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PopupController))]
public class PopupBackButtonHandlerWithAction : BackButtonHandler
{
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//        
    public Action OnBackButton { get; set; }

    //
    PopupController m_popup = null;

    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake()
    {
        m_popup = GetComponent<PopupController>();
        m_popup.OnOpenPreAnimation.AddListener(Register);
        m_popup.OnClosePostAnimation.AddListener(Unregister);
    }

    public override void Trigger()
    {
        if (enabled && OnBackButton != null)
            OnBackButton();
    }
}
