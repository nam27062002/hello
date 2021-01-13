using UnityEngine;
using System.Collections;

public class SpiderViewControl : ViewControl {
	[SeparatorAttribute]
    [SerializeField] private FastLineRenderer m_web;
	[SerializeField] private Transform m_spinneret;
	[SerializeField] private ParticleData m_onAttackParticle;
	[SerializeField] private GameObject m_onAttackParticleAnchor;

	private bool m_hanging;
	private Vector3 m_startHangingPos;
	private Vector3 m_startBitePos;
	private float m_startBiteDistance;
	private AI.Behaviour.AttackRanged m_rangedAttack;
	private float m_startWebWidth;
	private bool m_bite = false;

	protected override void Awake() {
		base.Awake();

        if (m_web != null) {
            m_startWebWidth = m_web.size;
        }

		if (m_onAttackParticle.IsValid()) {
			m_onAttackParticle.CreatePool();
		}
	}

	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

        if (m_web != null) {
            m_web.size = m_startWebWidth;
            m_web.gameObject.SetActive(false);
        }

        m_hanging = false;		
		m_bite = false;
	}

	public override void Bite( Transform _transform) {
		base.Bite(_transform);

		m_bite = true;
		m_startBitePos = transform.position;
		m_startBiteDistance = (m_startBitePos - m_startHangingPos).sqrMagnitude;
	}

	// Update is called once per frame
	private void LateUpdate() {
		if (m_hanging) {
            m_web.SetPoint(1, m_spinneret.position);

			// Check if eating!
			if (m_bite) {
				float newBiteDistance = (transform.position - m_startHangingPos).sqrMagnitude;
				if (newBiteDistance > m_startBiteDistance) {
					float proportionToStretch = m_startBiteDistance/newBiteDistance;
                    m_web.size = m_startWebWidth * proportionToStretch;
				} else {
                    m_web.size = m_startWebWidth;
				}
			}

            m_web.UpdateSegments();
		}

		base.CustomUpdate();
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		if (_anim == SpecialAnims.A) {
			m_hanging = true;
            m_web.gameObject.SetActive(true);

            m_startHangingPos = transform.position + Vector3.up * 0.5f;
			m_web.SetPoint(0, m_startHangingPos);
			m_web.SetPoint(1, m_spinneret.position);
            m_web.UpdateSegments();
        }
	}
		
	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		if (_anim == SpecialAnims.A) {
			m_hanging = false;
            m_web.gameObject.SetActive(false);
        }
	}

	override protected void animEventsOnAttackDealDamage() {
		base.animEventsOnAttackDealDamage();

		if (m_onAttackParticle.IsValid()) {
			GameObject go = m_onAttackParticle.Spawn(m_onAttackParticleAnchor.transform.position);
			if (go != null) {
				// go.transform.rotation = m_onAttackParticleAnchor.transform.rotation;
				go.transform.LookAt( attackTargetPosition );
			}
		}
	}
}
