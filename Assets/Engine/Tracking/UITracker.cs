// UITracker.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 24/10/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// This component listens to the button in the element and sends a 
/// tracking notification every time this button is pressed
/// </summary>

[RequireComponent(typeof(Button))]
public class UITracker : MonoBehaviour {

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	    
    [SerializeField]
    [Tooltip ("Identifier of the button. Please, document this id in confluence.")]
    private string m_buttonName = "";

    // Cached button
    private Button m_button;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

        // Dont waste my time
        if (string.IsNullOrEmpty(m_buttonName))
            return;

        // Add a listener to the asociated button 
        m_button = GetComponent<Button>();
        m_button.onClick.AddListener(OnButtonClick);

	}

    /// <summary>
    /// Destructor
    /// </summary>
    private void OnDestroy()
    {
        if (m_button != null)
        {
            m_button.onClick.RemoveListener(OnButtonClick);
        }
    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// The user pressed the associated button
    /// </summary>
    private void OnButtonClick ()
    {

        // Notify it to the tracking manager
        HDTrackingManager.Instance.Notify_UIButton(m_buttonName);

    }

}