// ResultsScreenFXTest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
public class ResultsScreenFXTest : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public bool m_toggle = true;

	[Separator("TEST 1")]
	public Transform m_source = null;
	public Transform m_target = null;
	public GameObject m_prefab = null;
	[Range(0f, 1f)] public float m_spawnInterval = 0.1f;

	[Space]
	public float m_duration = 0.5f;
	public Ease m_ease = Ease.InOutCubic;

	private float m_spawnTimer = 0f;
	private Pool m_pool = null;

	[Separator("TEST 2")]
	public RectTransform m_source2 = null;
	public RectTransform m_target2 = null;
	public List<ParticleSystem> m_particleSystems = new List<ParticleSystem>();
	public Range m_speed = new Range(1f, 2f);	// World units per second?


	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		m_pool = new Pool(m_prefab, m_prefab.name, null, this.transform.parent, 10, true, true, true);
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
		/*if(m_toggle) {
			m_spawnTimer += Time.deltaTime;
			if(m_spawnTimer >= m_spawnInterval) {
				m_spawnTimer = 0f;

				GameObject obj = m_pool.Get(true);
				MovingFXAnimator anim = obj.GetComponent<MovingFXAnimator>();
				anim.parentPool = m_pool;
				anim.sourcePos = m_source.position;
				//anim.targetTransform = m_target;
				anim.targetPos = m_target.position;
				anim.ease = m_ease;
				anim.duration = m_duration;
				anim.Launch();
			}
		}*/

		// Test 2 - Adjust emission size and compute speed and lifetime based on source and target positions
		if(m_toggle) {
			for(int i = 0; i < m_particleSystems.Count; i++) {
				// Put into position
				ParticleSystem fx = m_particleSystems[i];
				fx.transform.position = m_source2.position;

				// Compute particle lifetime based on distance between points and desired speed
				// Set also velocity to define direction and speed
				Vector3 diff = m_target2.position - m_source2.position;
				Vector3 dir = diff.normalized;
				float dist = diff.magnitude;
				Vector3 velocityMin = dir * m_speed.min;
				Vector3 velocityMax = dir * m_speed.max;

				ParticleSystem.VelocityOverLifetimeModule v = fx.velocityOverLifetime;
				v.space = ParticleSystemSimulationSpace.World;
				v.enabled = true;

				ParticleSystem.MinMaxCurve c = v.x;
				c.mode = ParticleSystemCurveMode.TwoConstants;
				c.constantMin = velocityMin.x;
				c.constantMax = velocityMax.x;
				v.x = c;

				c = v.y;
				c.mode = ParticleSystemCurveMode.Constant;
				c.constantMin = velocityMin.y;
				c.constantMax = velocityMax.y;
				v.y = c;

				c = v.z;
				c.mode = ParticleSystemCurveMode.Constant;
				c.constantMin = velocityMin.z;
				c.constantMax = velocityMax.z;
				v.z = c;

				//m_fx2.veloc = v;
				fx.startLifetime = dist/m_speed.max;
			}
		}
	}
}