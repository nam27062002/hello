using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDMegaFireSlot : MonoBehaviour {

	private enum State {
		Empty = 0,
		Fill,
		Full,
		Consume
	};

	[SerializeField] private ParticleData m_burstParticle;
	[SerializeField] private ParticleData m_consumeParticle;


	private UIIconFireColors m_icon;
	private float m_delta;

	private State m_state;

	private GameObject m_consumeEffect;

	// Use this for initialization
	private void Start () {
		m_icon = transform.FindComponentRecursive<UIIconFireColors>("PF_IconFire");
	}

	public void CreatePools() {
		ParticleManager.CreatePool(m_burstParticle);
		ParticleManager.CreatePool(m_consumeParticle);
	}

	public void Empty() {
		m_icon.alpha = 0f;
		m_state = State.Empty;
	}

	public void Fill() {
		m_delta = 1f / 0.25f;

		GameObject go = ParticleManager.Spawn(m_burstParticle);
		go.transform.SetParent(transform, false);

		m_state = State.Fill;
	}

	public void Full() {
		m_icon.alpha = 1f;
		m_state = State.Full;
	}

	public void Consume(float _delta) {
		if (m_state != State.Empty) {
			m_icon.alpha = 1f * _delta;

			if (m_icon.alpha <= 0f) {				
				m_state = State.Empty;
			} else if (m_icon.alpha <= 0.25f) {
				if (m_consumeEffect != null) {
					m_consumeEffect.GetComponent<DisableInSeconds>().enabled = true;
					m_consumeEffect = null;
				}
			} else if (m_state != State.Consume) {
				m_consumeEffect = ParticleManager.Spawn(m_consumeParticle);
				if (m_consumeEffect != null) {
					m_consumeEffect.transform.SetParent(transform, false);
					m_consumeEffect.GetComponent<DisableInSeconds>().enabled = false;
				}
				m_state = State.Consume;
			}
		}
	}

	// Update is called once per frame
	void Update () {		
		switch (m_state) {
			case State.Fill: {
					m_icon.alpha += m_delta * Time.deltaTime;
					if (m_icon.alpha >= 1f) {
						m_icon.alpha = 1f;
						m_state = State.Full;
					}
				} break;
		}
	}
}
