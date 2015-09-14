// DEBUG_SETTINGS.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 05/05/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Aux class for development adjusting some settings for debug purposes.
/// </summary>
public class DEBUG_SETTINGS : MonoBehaviour {

	#region PROPERTIES -------------------------------------------------------------------------------------------------
	// Invincibility
	public bool invulnerable = false;

	// Infinite dash
	public bool infiniteDash = false;
	private bool m_infiniteDash = false;

	// Infinite fire
	public bool infiniteFire = false;
	private bool m_infiniteFire = false;

	// Draw collision meshes
	public bool drawColliders = false;
	#endregion

	#region INTERNAL VARS ----------------------------------------------------------------------------------------------
	DragonMotion m_playerMotion = null;
	DragonPlayer m_player = null;
	#endregion

	#region PUBLIC METHODS ---------------------------------------------------------------------------------------------
	void Awake() {
		App app = App.Instance;
	}


	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {

		if (m_playerMotion == null){
			m_playerMotion = GameObject.Find("Player").GetComponent<DragonMotion>();
			m_player = m_playerMotion.GetComponent<DragonPlayer>();
		}

		// Invulnerable
		if(m_playerMotion.invulnerable != invulnerable) {
			m_playerMotion.invulnerable = invulnerable;
		}

		// Infinite Dash
		if(m_infiniteDash != infiniteDash) {
			m_infiniteDash = infiniteDash;
		} else if(m_infiniteDash) {
			m_player.AddEnergy(m_player.data.energy);
		}

		// Infinite fire
		if(m_infiniteFire != infiniteFire) {
			m_infiniteFire = infiniteFire;
		} else if(m_infiniteFire) {	// Prevent fire to turn off
			m_player.AddFury(m_player.data.fury);
		}
	}

	/*
	#if UNITY_EDITOR
	/// <summary>
	/// Highly experimental :P
	/// </summary>
	[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
	void OnDrawGizmos() {
		// Only if option is activated
		if(!drawColliders) return;

		// Get all the colliders in the scene - this should be optimized
		Collider[] colliders = FindObjectsOfType<Collider>();
		foreach(Collider c in colliders) {
			// Different types of colliders require different drawing methods
			System.Type type = c.GetType();
			if(type == typeof(MeshCollider)) {
				Gizmos.color = Color.red;
				MeshCollider mc = c as MeshCollider;
				Gizmos.DrawWireMesh(mc.sharedMesh, mc.transform.position, mc.transform.rotation, mc.transform.lossyScale);
			} else if(type == typeof(BoxCollider)) {
				Gizmos.color = Color.green;
				BoxCollider bc = c as BoxCollider;
				Gizmos.DrawWireCube(bc.bounds.center, bc.bounds.size);
			} else if(type == typeof(SphereCollider)) {
				Gizmos.color = Color.green;
				SphereCollider sc = c as SphereCollider;
				Gizmos.DrawWireSphere(sc.center, sc.radius);
			} else if(type == typeof(CapsuleCollider)) {
				Gizmos.color = Color.green;
				CapsuleCollider cc = c as CapsuleCollider;
				Gizmos.DrawWireCube(cc.bounds.center, cc.bounds.size);
			}
		}
	}
	#endif
	*/
	#endregion
}
#endregion