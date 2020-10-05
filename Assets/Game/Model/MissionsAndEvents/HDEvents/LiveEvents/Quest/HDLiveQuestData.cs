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
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDLiveQuestData : HDLiveEventData {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	public long m_globalScore;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public HDLiveQuestData() {
        m_definition = new HDLiveQuestDefinition();
	}


	public override void Clean()
	{
		base.Clean();
		m_globalScore = -1;
	}

	public override SimpleJSON.JSONClass ToJson ()
	{
		JSONClass ret = base.ToJson();
		ret.Add("globalScore", PersistenceUtils.SafeToString(m_globalScore));
		return ret;
	}

	public override void ParseState( SimpleJSON.JSONNode _data )
	{
		base.ParseState( _data );

		if ( _data.ContainsKey("globalScore") )
		{
			m_globalScore = PersistenceUtils.SafeParse<long>(_data["globalScore"]);
		}
	}

    public virtual void ParseProgress(SimpleJSON.JSONNode _data)
    {
		if ( _data.ContainsKey("globalScore") )
		{
			m_globalScore = _data["globalScore"].AsLong;
		}
    }
    

}