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
	private bool mInfiniteDash = false;
	private float mOriginalEnergyDrain;

	// Infinite fire
	public bool infiniteFire = false;
	private bool mInfiniteFire = false;

	// Draw collision meshes
	public bool drawColliders = false;
	#endregion

	#region INTERNAL VARS ----------------------------------------------------------------------------------------------
	DragonPlayer mPlayer = null;
	#endregion

	#region PUBLIC METHODS ---------------------------------------------------------------------------------------------
	void Awake() {
		App app = App.Instance;
	}


	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {

		if (mPlayer == null){
			mPlayer = GameObject.Find("Player").GetComponent<DragonPlayer>();
		
			// Backup modifiable values
			mOriginalEnergyDrain = mPlayer.energyDrainPerSecond;
		}

		// Invulnerable
		if(mPlayer.invulnerable != invulnerable) {
			mPlayer.invulnerable = invulnerable;
		}

		// Infinite Dash
		if(mInfiniteDash != infiniteDash) {
			mInfiniteDash = infiniteDash;
			if(mInfiniteDash) {
				mPlayer.energyDrainPerSecond = 0;
			} else {
				mPlayer.energyDrainPerSecond = mOriginalEnergyDrain;
			}
		}

		// Infinite fire
		if(mInfiniteFire != infiniteFire) {
			mInfiniteFire = infiniteFire;
			App.Instance.gameLogic.ForceFuryRush(mInfiniteFire);
		} else if(mInfiniteFire) {	// Prevent fire to turn off
			App.Instance.gameLogic.ForceFuryRush(true);
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