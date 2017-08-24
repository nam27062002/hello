using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineInflammableManager : UbiBCN.SingletonMonoBehaviour<MachineInflammableManager> { 

	private Material m_ashes_00;	 // starting queue, all renderers will be back until next queue is available
	private Material m_ashes_00_07;
	private Material m_ashes_07_15;

	private float m_timer_00_07; // timers 
	private float m_timer_07_15;

	private List<AI.MachineInflammable> m_queue_00;
	private List<AI.MachineInflammable> m_queue_00_07;
	private List<AI.MachineInflammable> m_queue_07_15;


	public static void Add(AI.MachineInflammable _machine) {
		instance.__Add(_machine);
	}

	private void Awake() {
		Material sharedAshesMaterial = Resources.Load ("Game/Materials/BurnToAshes") as Material;
		sharedAshesMaterial.renderQueue = 3000;

		m_ashes_00 = new Material(sharedAshesMaterial);
		m_ashes_00.SetFloat("_AshLevel", 0f);
		m_ashes_00_07 = new Material(sharedAshesMaterial);
		m_ashes_00_07.SetFloat("_AshLevel", 0f);
		m_ashes_07_15 = new Material(sharedAshesMaterial);
		m_ashes_07_15.SetFloat("_AshLevel", 0.5f);

		m_queue_00 = new List<AI.MachineInflammable>();
		m_queue_00_07 = new List<AI.MachineInflammable>();
		m_queue_07_15 = new List<AI.MachineInflammable>();

		m_timer_00_07 = 0f;
		m_timer_07_15 = 0.75f;
	}

	private void __Add(AI.MachineInflammable _machine) {
		if (m_timer_00_07 <= 0.15f) {
			PromoteTo_00_05(_machine);
		} else {
			PromoteTo_00(_machine);
		}
	}

	private void PromoteTo_00(AI.MachineInflammable _machine) {
		ChangeMaterials(_machine, m_ashes_00);
		m_queue_00.Add(_machine);
	}

	private void PromoteTo_00_05(AI.MachineInflammable _machine) {
		ChangeMaterials(_machine, m_ashes_00_07);
		m_queue_00_07.Add(_machine);
	}

	private void PromoteTo_05_10(AI.MachineInflammable _machine) {
		ChangeMaterials(_machine, m_ashes_07_15);
		m_queue_07_15.Add(_machine);
	}

	private void ChangeMaterials(AI.MachineInflammable _machine, Material _material) {
		List<Renderer> renderers = _machine.GetBurnableRenderers();
		for (int i = 0; i < renderers.Count; ++i) {
			Material[] materials = renderers[i].sharedMaterials;
			for (int m = 0; m < materials.Length; ++m) {
				materials[m] = _material;
			}
			renderers[i].sharedMaterials = materials;
		}
	}

	//
	private void Update() {
		float dt = Time.deltaTime;

		// manage the renderers starting to burn
		if (m_queue_00_07.Count == 0) {
			if (m_queue_00.Count > 0) {
				for (int i = 0; i < m_queue_00.Count; ++i) {
					PromoteTo_00_05(m_queue_00[i]);
				}
				m_queue_00.Clear();
				m_timer_00_07 = 0f;
			}
		} else {
			m_timer_00_07 += dt;
			Mathf.Clamp(m_timer_00_07, 0f, 0.75f);
			m_ashes_00_07.SetFloat("_AshLevel", m_timer_00_07 / 1.5f);

			if (m_timer_00_07 >= 0.75f) {
				if (m_queue_07_15.Count == 0) {
					for (int i = 0; i < m_queue_00_07.Count; ++i) {
						PromoteTo_05_10(m_queue_00_07[i]);
					}
					m_queue_00_07.Clear();
					m_timer_00_07 = 0f;
				}
			}
		}

		// advance time for those renderers almost burned
		if (m_queue_07_15.Count > 0) {
			m_timer_07_15 += dt;
			Mathf.Clamp(m_timer_07_15, 0.75f, 1.5f);
			m_ashes_07_15.SetFloat("_AshLevel", m_timer_07_15 / 1.5f);

			if (m_timer_07_15 >= 1.5f) {
				for (int i = 0; i < m_queue_07_15.Count; ++i) {
					m_queue_07_15[i].Burned();
				}
				m_queue_07_15.Clear();
				m_timer_07_15 = 0.75f;
			}
		}
	}
}
