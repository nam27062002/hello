// HideConditionallyByABTest.cs
// Hungry Dragon
// 
// Created by  on 28/07/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

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
public class HideConditionallyByABTest : MonoBehaviour {

	//------------------------------------------------------------------------//
	// ENUM                     											  //
	//------------------------------------------------------------------------//
    public enum Boolean
    {
        FALSE,
        TRUE
    }

	public enum Action
	{
		DISABLE,
		DESTROY
	}


	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[SerializeField]
	private ABTest.Test m_test;

	[SerializeField]
	private Boolean m_ifTestEquals;

	[SerializeField]
	private Action m_action;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Start() {

        // If the value selected matches the value of the AB test
        if ( ABTest.GetValue(m_test) == (m_ifTestEquals == Boolean.TRUE) )
        {
			switch (m_action)
            {
				case Action.DISABLE:
					gameObject.SetActive(false);
					break;

				case Action.DESTROY:
					GameObject.Destroy(gameObject);
					break;
            }
        }

	}
        
}