// HDQuestData.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 23/05/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDQuestData : HDLiveEventData {
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
	/// Default constructor.
	/// </summary>
	public HDQuestData() {

	}

	protected override void BuildDefinition()
	{
		m_definition = new HDLiveEventDefinition();
	}


	/// <summary>
	/// Destructor
	/// </summary>
	~HDQuestData() {

	}

	public override void ParseState( SimpleJSON.JSONNode _data )
	{
		base.ParseState( _data );
	}

    public virtual void ParseProgress(SimpleJSON.JSONNode _data)
    {

    }


}