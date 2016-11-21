using UnityEngine;
using System.Collections;

public class SpiderViewControl : ViewControl {
	[SeparatorAttribute]
	[SerializeField] private LineRenderer m_web;
	[SerializeField] private Transform m_spinneret;
	[SerializeField] private ParticleData m_onAttackParticle;
	[SerializeField] private GameObject m_onAttackParticleAnchor;

	private bool m_hanging;

	protected override void Awake()
	{
		base.Awake();
		if ( m_onAttackParticle.IsValid()  )
			ParticleManager.CreatePool( m_onAttackParticle);
	}

	public override void Spawn(ISpawner _spawner) {
		base.Spawn(_spawner);

		m_hanging = false;
		m_web.enabled = false;
	}
	
	// Update is called once per frame
	protected override void Update() {
		if (m_hanging) {
			m_web.SetPosition(1, m_spinneret.position);
		}

		base.Update();
	}

	protected override void OnSpecialAnimationEnter(SpecialAnims _anim) {
		if (_anim == SpecialAnims.A) {
			m_hanging = true;
			m_web.enabled = true;

			m_web.SetPosition(0, transform.position + Vector3.up * 0.5f);
			m_web.SetPosition(1, m_spinneret.position);
		}
	}
		
	protected override void OnSpecialAnimationExit(SpecialAnims _anim) {
		if (_anim == SpecialAnims.A) {
			m_hanging = false;
			m_web.enabled = false;
		}
	}

	override protected void animEventsOnAttackDealDamage() {
		base.animEventsOnAttackDealDamage();
		if ( m_onAttackParticle.IsValid() )
		{
			GameObject go = ParticleManager.Spawn(m_onAttackParticle, m_onAttackParticleAnchor.transform.position);
			if (go != null)
			{
				// go.transform.rotation = m_onAttackParticleAnchor.transform.rotation;
				go.transform.LookAt( InstanceManager.player.transform );
			}
		}
	}
}
