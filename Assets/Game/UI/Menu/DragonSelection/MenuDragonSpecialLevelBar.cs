// MenuDragonSpecialLevelBar.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 22/07/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuDragonSpecialLevelBar : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
        // Subscribe to external events
        Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);

        // Do a first refresh
        Refresh(InstanceManager.menuSceneController.selectedDragon);
    }

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// A new dragon has been selected.
    /// </summary>
    /// <param name="_sku">The sku of the selected dragon.</param>
    private void OnDragonSelected(string _sku)
    {
        // Refresh after some delay to let the animation finish
        Refresh(_sku, 0.25f);
    }


    /// <summary>
    /// Refresh with data from a target dragon.
    /// </summary>
    /// <param name="_sku">The sku of the dragon whose data we want to use to initialize the bar.</param>
    /// <param name="_delay">Optional delay before refreshing the data. Useful to sync with other UI animations.</param>
    virtual public void Refresh(string _sku, float _delay = -1f)
    {

        // Only show special dragons bar
        if (DragonManager.GetDragonData(_sku).type != IDragonData.Type.SPECIAL)
        {
            gameObject.SetActive(false);
            return;
        }


        gameObject.SetActive(true);
        
        // Ignore delay if disabled (coroutines can't be started with the component disabled)
        if (isActiveAndEnabled && _delay > 0)
        {
            // Start internal coroutine
            //StartCoroutine(RefreshDelayed(_sku, _delay));
        }
        else
        {
            // Get new dragon's data from the dragon manager and do the refresh logic
            //Refresh(DragonManager.GetDragonData(_sku) as DragonDataSpecial);
        }
    }
    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}