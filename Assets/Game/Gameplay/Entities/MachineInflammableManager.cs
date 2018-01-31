﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineInflammableManager : UbiBCN.SingletonMonoBehaviour<MachineInflammableManager> { 
	private const float DISINTEGRATE_TIME = 1.25f;

	private Material m_ashes_wait;	 // starting queue, all renderers will be back until next queue is available
	private Material m_ashes_disintegrate;
	private Material m_ashes_end;

	private float m_timer;

	private List<AI.MachineInflammable> m_list_wait;
	private List<AI.MachineInflammable> m_list_disintegrate;


	public static void Add(AI.MachineInflammable _machine) {
		instance.__Add(_machine);
	}

	private void Awake() {
		Material sharedAshesMaterial = Resources.Load("Game/Materials/BurnToAshes") as Material;
		sharedAshesMaterial.renderQueue = 3000;

		m_ashes_wait = new Material(sharedAshesMaterial);
		m_ashes_wait.SetFloat( GameConstants.Material.ASH_LEVEL , 0f);
		m_ashes_disintegrate = new Material(sharedAshesMaterial);
		m_ashes_disintegrate.SetFloat( GameConstants.Material.ASH_LEVEL , 0f);
		m_ashes_end = new Material(sharedAshesMaterial);
		m_ashes_end.SetFloat( GameConstants.Material.ASH_LEVEL , 1f);

		m_list_wait = new List<AI.MachineInflammable>();
		m_list_disintegrate = new List<AI.MachineInflammable>();
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_AREA_EXIT, ClearQueues);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, ClearQueues);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_EXIT, ClearQueues);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, ClearQueues);
	}

	private void ClearQueues() {
		m_list_wait.Clear();
		m_list_disintegrate.Clear();
	}

	private void __Add(AI.MachineInflammable _machine) {
		if (m_timer <= 0.25f) {
			AddToDisintegrateList(_machine);
		} else {
			AddToWaitQueue(_machine);
		}
	}

	private void AddToWaitQueue(AI.MachineInflammable _machine) {
		ChangeMaterials(_machine, m_ashes_wait);
		m_list_wait.Add(_machine);
	}

	private void AddToDisintegrateList(AI.MachineInflammable _machine) {
		ChangeMaterials(_machine, m_ashes_disintegrate);
		m_list_disintegrate.Add(_machine);
	}

	private void ChangeMaterials(AI.MachineInflammable _machine, Material _material) {
		List<Renderer> renderers = _machine.GetBurnableRenderers();
		for (int i = 0; i < renderers.Count; ++i) {
			Material[] materials = renderers[i].materials;
			for (int m = 0; m < materials.Length; ++m) {
				materials[m] = _material;
			}
			renderers[i].materials = materials;
		}
	}

	//
	private void Update() {
		float dt = Time.deltaTime;

		// manage the renderers starting to burn
		if (m_list_disintegrate.Count == 0) {
			if (m_list_wait.Count > 0) {
				for (int i = 0; i < m_list_wait.Count; ++i) {
					AddToDisintegrateList(m_list_wait[i]);
				}
				m_list_wait.Clear();
				m_timer = 0f;
			}
		} else {
			m_timer += dt;
			Mathf.Clamp(m_timer, 0f, DISINTEGRATE_TIME);

			if (m_timer >= DISINTEGRATE_TIME) {
				for (int i = 0; i < m_list_disintegrate.Count; ++i) {
					m_list_disintegrate[i].Burned();
					ChangeMaterials(m_list_disintegrate[i], m_ashes_end);
				}
				m_list_disintegrate.Clear();
				m_timer = 0f;
			}

			m_ashes_disintegrate.SetFloat( GameConstants.Material.ASH_LEVEL , m_timer / DISINTEGRATE_TIME);
		}
	}
}
