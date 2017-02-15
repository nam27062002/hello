﻿// EggUIScene3D.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/03/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Custom controller for an egg 3D scene prefab for the UI.
/// </summary>
public class EggUIScene3D : UIScene3D {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public static readonly string RESOURCES_PREFAB_PATH = "UI/Metagame/Eggs/PF_EggUIScene3D";	// Just for comfort, change it if path changes
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private EggView m_eggView = null;
	public EggView egg {
		get { return m_eggView; }
	}
	
	//------------------------------------------------------------------------//
	// FACTORY METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Creates a new UIScene3D, adds it to the manager and initializes it with no egg associated.
	/// The egg can always be defined via the SetEgg method, as much times as needed.
	/// </summary>
	/// <returns>The newly created scene.</returns>
	public static EggUIScene3D CreateEmpty() {
		// Just use the other factory method
		return CreateFromEgg(null);
	}

	/// <summary>
	/// Creates a new UIScene3D, adds it to the manager and initializes it with 
	/// the given egg.
	/// </summary>
	/// <returns>The newly created scene.</returns>
	/// <param name="_newEgg">The egg to be used for the initialization. Can be <c>null</c>. It's state will be changed to SHOWROOM.</param>
	public static EggUIScene3D CreateFromEgg(Egg _newEgg) {
		// Create from the default prefab
		EggUIScene3D newScene = UIScene3DManager.CreateFromResources<EggUIScene3D>(RESOURCES_PREFAB_PATH);

		// If prefab could not be found, create a new object for it
		if(newScene == null) {
			newScene = UIScene3DManager.Create<EggUIScene3D>();
		}

		// Initialize with the egg
		newScene.SetEgg(_newEgg);

		// Done!
		return newScene;
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Defines the egg to be rendered by this scene.
	/// </summary>
	/// <param name="_newEgg">The egg to be rendered. It's state will be changed to SHOWROOM. Use <c>null</c> to clear view. If another egg is being rendered, its view will be destroyed.</param>
	public void SetEgg(Egg _newEgg) {
		// If not null, create a new view for the target egg
		EggView eggView = null;
		if(_newEgg != null) {
			// Unless it's the same egg we have loaded, in wich case we won't do anything
			if(m_eggView != null && m_eggView.eggData == _newEgg) {
				return;
			}

			// Create the new egg view
			eggView = EggView.CreateFromData(_newEgg);
		}

		// Use view setter
		SetEgg(eggView);
	}

	/// <summary>
	/// Defines the egg to be rendered by this scene.
	/// </summary>
	/// <param name="_newEggView">The view to be rendered. Use <c>null</c> to clear view. If another egg is being rendered, it will be destroyed.</param>
	public void SetEgg(EggView _newEggView) {
		// If egg is different than current one, clear current one
		if(_newEggView != m_eggView) {
			if(m_eggView != null) {
				GameObject.Destroy(m_eggView.gameObject);
				m_eggView = null;
			}
		}

		// If new egg is not null, attach it
		if(_newEggView != null) {
			_newEggView.transform.SetParent(this.transform, false);
			_newEggView.transform.localPosition = Vector3.zero;
			_newEggView.gameObject.SetLayerRecursively(UIScene3DManager.LAYER_NAME);
			m_eggView = _newEggView;
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}