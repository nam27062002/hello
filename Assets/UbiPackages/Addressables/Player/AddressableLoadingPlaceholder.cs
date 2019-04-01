// AddressableLoadingPlaceholder.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to display a placeholder while asynchronously loading an addressable asset.
/// Can be inherited for some customization.
/// </summary>
public class AddressableLoadingPlaceholder : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private float m_progress = 0f;
	public virtual float progress {
		set { m_progress = value; }
		get { return m_progress; }
	}
}