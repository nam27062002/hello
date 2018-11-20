// HDTournamentBuild.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 06/06/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections.Generic;
using SimpleJSON;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[Serializable]
public class HDTournamentBuild {
		public string m_dragon = "";
		public string m_skin = "";
		public List<string> m_pets = new List<string>();

		public HDTournamentBuild()
		{

		}

		~HDTournamentBuild()
		{
			
		}

		public void Clean()
		{
			m_dragon = "";
			m_skin = "";
			if ( m_pets != null )
				m_pets.Clear();
		}

		public void ParseBuild( SimpleJSON.JSONNode _build )
		{
			Clean();
			if ( _build.ContainsKey("dragon") ){
				m_dragon = _build["dragon"];
			}

			if ( _build.ContainsKey("skin") ){
				m_skin = _build["skin"];
			}

			if ( _build.ContainsKey("pets") ){
				JSONArray _pets = _build["pets"].AsArray;
				for (int i = 0; i < _pets.Count; i++) {
					m_pets.Add( _pets[i] );
				}
			}
		}

		public SimpleJSON.JSONClass ToJson()
		{
			SimpleJSON.JSONClass _build = new SimpleJSON.JSONClass();
            _build.Add("dragon", m_dragon);
			_build.Add("skin", m_skin);
			SimpleJSON.JSONArray arr = new SimpleJSON.JSONArray();
			int max = m_pets.Count;
			for (int i = 0; i < max; i++) {
				arr.Add( m_pets[i] );
			}
			_build.Add("pets", arr);
			return _build;
		}
}