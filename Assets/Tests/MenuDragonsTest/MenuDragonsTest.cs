// MenuDragonsTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 17/07/2019.
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
public class MenuDragonsTest : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	public Transform m_dragonsRoot = null;
	public Vector3 m_offset = new Vector3(50f, 0f, 0f);

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnDistributeButton() {
		Vector3 pos = Vector3.zero;
		for(int i = 0; i < m_dragonsRoot.childCount; ++i) {
			m_dragonsRoot.GetChild(i).localPosition = pos;
			pos += m_offset;
		}
	}
}