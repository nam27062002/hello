using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {

		[System.Serializable]
		public class PetPlayerInfiniteBoostData : StateComponentData {
			public float m_duration;
			public string m_particleNormal;
			public string m_particleBoost;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Give Player Infinite Boost")]
		public class PetPlayerInfiniteBoost : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onBoostFinished = UnityEngine.Animator.StringToHash("onBoostFinished");

			DragonBoostBehaviour m_playerBoost;
			PetPlayerInfiniteBoostData m_data;
			float m_timer;
			GameObject m_visualCue;

			ParticleSystem m_normalParticle;
			ParticleSystem m_boostParticle;

			public override StateComponentData CreateData() {
				return new PetPlayerInfiniteBoostData();
			}

			public override System.Type GetDataType() {
				return typeof(PetPlayerInfiniteBoostData);
			}

			protected override void OnInitialise() {
				m_playerBoost = InstanceManager.player.dragonBoostBehaviour;
				m_data = m_pilot.GetComponentData<PetPlayerInfiniteBoostData>();

				m_normalParticle = m_pilot.FindObjectRecursive(m_data.m_particleNormal).GetComponent<ParticleSystem>();
				m_boostParticle = m_pilot.FindObjectRecursive(m_data.m_particleBoost).GetComponent<ParticleSystem>();
			}

			protected override void OnEnter(State oldState, object[] param) {
				m_playerBoost.petInfiniteBoost = true;
				m_pilot.PressAction(Pilot.Action.Button_A);
				// Activate visual placeholder
				m_timer = m_data.m_duration;
				if (m_normalParticle != null)
					m_normalParticle.Stop();
				if ( m_boostParticle != null)
					m_boostParticle.Play();

			}

			protected override void OnUpdate(){
				m_timer -= Time.deltaTime;
				if ( m_timer <= 0 )	
				{
					Transition(onBoostFinished );
				}
			}

			protected override void OnExit(State _newState){
				m_playerBoost.petInfiniteBoost = false;
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
				// Deactivate visual placeholder
				m_timer = m_data.m_duration;
				if (m_normalParticle != null)
					m_normalParticle.Play();
				if ( m_boostParticle != null)
					m_boostParticle.Stop();
			}
		}
	}
}