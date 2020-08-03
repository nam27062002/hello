/// <summary>
/// This class is responsible for controlling a popup that can be used to show any message. It consists of the following elements that can be configurated:
/// 1)Title.
/// 2)Message to be shown in the body.
/// 3)Buttons. Two layouts are supported:
///    3.1)Two buttons: Confirm, cancel. A delegate can be assigned to each.
///    3.2)A single button: Confirm. A delegate can be assinged to it.
/// 
/// The configuration of the class is done by using the class <c>PopupMessage.Config</c>
/// </summary>

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(PopupController))]
public class PopupCountryBlacklisted : MonoBehaviour {

    public const string PATH = "UI/Popups/Message/PF_PopupCountryBlacklisted";

    void Awake() {

    }

    /// <summary>
    /// Method called when the user clicks on any Confirm buttom. It's called by the editor
    /// </summary>
    public void OnConfirm() {
        Application.OpenURL("http://hd.y2game.cn");
    }
}
