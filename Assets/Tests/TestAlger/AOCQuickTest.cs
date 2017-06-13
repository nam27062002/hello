// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
//[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public class ServerResponse : Dictionary<string, object> {
		/// <summary>
		/// Nice string output.
		/// </summary>
		override public string ToString() {
			// Special case if empty
			int remaining = this.Count;
			if(remaining == 0) return "{ }";

			// Json-like formatting
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("{");
			foreach(KeyValuePair<string, object> kvp in this) {
				// Add entry
				sb.Append("    \"").Append(kvp.Key).Append("\" : ");

				// Special case for strings, surraund with quotation marks
				if(kvp.Value.GetType() == typeof(string)) {
					sb.Append("\"").Append(kvp.Value.ToString()).Append("\"");
				} else {
					sb.Append(kvp.Value.ToString());
				}
				remaining--;

				// If not last one, add separator
				if(remaining > 0) sb.Append(",");

				// New line
				sb.AppendLine();
			}
			sb.AppendLine("}");

			return sb.ToString();
		}
	}

	//public class ServerCallback : Action<FGOL.Server.Error, ServerResponse> {}
	//public class ServerCallbackNoResponse : Action<FGOL.Server.Error> {}
	public delegate void ServerCallback(FGOL.Server.Error _error, ServerResponse _response);
	public delegate void ServerCallbackNoResponse(FGOL.Server.Error _error);

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
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
	/// Something changed on the inspector.
	/// </summary>
	private void OnValidate() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	private void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnTestButton() {
		DoCallback(OnServerResponse);
	}

	private void DoCallback(ServerCallback _callback) {
		ServerResponse response = new ServerResponse();
		response["param0"] = "response param 0";
		response["param1"] = 4;
		_callback(new FGOL.Server.FileNotFoundError(), response);
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {

	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	private void OnServerResponse(FGOL.Server.Error _error, ServerResponse _response) {
		Debug.Log("<color=cyan>Received server response!</color>\n" +
			"<color=red>Error:\n" + _error.message + "</color>\n" +
			"<color=yellow>Response:\n" + _response.ToString() + "</color>");
	}
}