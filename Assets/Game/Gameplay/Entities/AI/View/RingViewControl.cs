using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingViewControl : CollectibleViewControl {
	private enum State {
		Idle = 0,
		Glow,
		FadeOut,
		End
	};

	private float m_timer;
	private State m_state;



	protected override void Awake() {
		m_instantiateMaterials = true;
		base.Awake();
	}

	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		//restore emissive value
		for (int i = 0; i < m_materialList.Count; ++i) {
			m_materialList[i].SetFloat(GameConstants.Material.BLEND_MODE, 0f);
			m_materialList[i].SetFloat(GameConstants.Material.EMISSIVE_POWER, 0f);
			m_materialList[i].DisableKeyword("TINT");
			m_materialList[i].renderQueue = 2000;
		}

		m_timer = 0f;
		m_state = State.Idle;
	}

	public override void Collect() {
		base.Collect();

		m_state = State.Glow;
	}

	public override bool HasCollectAnimationFinished() {
		return m_state == State.End;
	}

	public override void CustomUpdate() {
		switch (m_state) {
		case State.Glow: {
				m_timer += Time.deltaTime;

				if (m_timer < 0.25f) {
					float ep = Mathf.Lerp(0f, 2f, m_timer/0.25f);
					for (int i = 0; i < m_materialList.Count; ++i) {
						m_materialList[i].SetFloat(GameConstants.Material.EMISSIVE_POWER, ep);
					}
				} else {
					for (int i = 0; i < m_materialList.Count; ++i) {
						m_materialList[i].SetFloat(GameConstants.Material.BLEND_MODE, 1f);
						m_materialList[i].EnableKeyword("TINT");
						m_materialList[i].renderQueue = 3000;
					}

					m_timer = 0f;
					m_state = State.FadeOut;
				}
			} break;

		case State.FadeOut: {
				m_timer += Time.deltaTime;

				if (m_timer < 0.25f) {
					float a = 1f - (m_timer / 0.25f);
					for (int i = 0; i < m_renderers.Length; ++i) {			
						Material[] materials = m_renderers[i].sharedMaterials;
						for (int m = 0; m < materials.Length; m++) {
							Color tint = materials[m].GetColor(GameConstants.Material.TINT);
							tint.a = a;
							materials[m].SetColor(GameConstants.Material.TINT, tint);
						}
						m_renderers[i].sharedMaterials = materials;
					}
				} else {
					m_state = State.End;
				}
			} break;
		}
	}
}
