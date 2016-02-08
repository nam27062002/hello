﻿// AOCQuickTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on DD/MM/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[ExecuteInEditMode]
public class AOCQuickTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	void Start() {
		
	}
	
	/// <summary>
	/// Called once per frame.
	/// </summary>
	void Update() {
		
	}

	/// <summary>
	/// Multi-purpose callback.
	/// </summary>
	public void OnCustomCallback() {
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		if(mesh) {
			Debug.Log(gameObject.name + " --------------------------------------------");
			Debug.Log(" v: " + mesh.vertices.Length);
			Debug.Log(" t: " + mesh.triangles.Length);
			Debug.Log(" n: " + mesh.normals.Length);
			Debug.Log("uv: " + mesh.uv.Length);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	private void OnDrawGizmos() {
		Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
		if(mesh) {
			Gizmos.matrix = transform.localToWorldMatrix;
			for(int i = 0; i < mesh.vertices.Length; i++) {
				// v
				Gizmos.color = Colors.red;
				Gizmos.DrawSphere(mesh.vertices[i], 0.025f);

				// n
				if(i < mesh.normals.Length) {
					Gizmos.color = Colors.magenta;
					Gizmos.DrawLine(mesh.vertices[i], mesh.vertices[i] + mesh.normals[i]);
				}

				// uv
				#if UNITY_EDITOR
				if(i < mesh.uv.Length) {
					Handles.matrix = transform.localToWorldMatrix;
					Handles.color = Colors.silver;
					Handles.Label(mesh.vertices[i], "uv[" + i + "] (" + mesh.uv[i].x + ", " + mesh.uv[i].y + ")");
				}
				#endif
			}
		}
	}
}