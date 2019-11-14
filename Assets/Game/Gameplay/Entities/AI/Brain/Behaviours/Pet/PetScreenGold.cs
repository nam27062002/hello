using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[System.Serializable]
		public class PetScreenGoldData : StateComponentData {
			public string audio;
			public string particle;
			public string m_powerSetup = "transform_gold";
		}


		[CreateAssetMenu(menuName = "Behaviour/Pet/Screen Gold")]
		public class PetScreenGold : StateComponent {

			[StateTransitionTrigger]
			private static readonly int onAnimFinished = UnityEngine.Animator.StringToHash("onAnimFinished");

			PetScreenGoldData m_data;
			float m_timer;
			private GameObject m_particle;
			private ParticleSystem m_particleSystem;

			public override StateComponentData CreateData() {
				return new PetScreenGoldData();
			}

			public override System.Type GetDataType() {
				return typeof(PetScreenGoldData);
			}

			protected override void OnInitialise() {

				m_data = m_pilot.GetComponentData<PetScreenGoldData>();
				
				State st = m_stateMachine.GetState("Wander");
				if ( st != null )
				{
					AI.Behaviour.TimerRanged behaviour = st.GetComponent<AI.Behaviour.TimerRanged>();
					if ( behaviour != null )
					{
						DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.POWERUPS, m_data.m_powerSetup);
						float timed = def.GetAsFloat( "param1", 5.0f );
						behaviour.data.seconds.Set( timed, timed );
					}
				}
				
				
				string version = "";
				switch(FeatureSettingsManager.instance.Particles)
				{
					default:
					case FeatureSettings.ELevel5Values.very_low:							
					case FeatureSettings.ELevel5Values.low:
						// No particle!
						break;
					case FeatureSettings.ELevel5Values.mid:
					case FeatureSettings.ELevel5Values.very_high:
					case FeatureSettings.ELevel5Values.high:
					{
						string path = "Particles/Master/" + m_data.particle;
						GameObject prefab = Resources.Load<GameObject>(path);
						if ( prefab )
						{
							m_particle = Instantiate<GameObject>(prefab);
							if ( m_particle )
							{
								// Anchor
								Transform p = m_pilot.transform;
								m_particle.transform.SetParent(p, true);
								m_particle.transform.localPosition = GameConstants.Vector3.zero;
								m_particle.transform.localRotation = GameConstants.Quaternion.identity;
								m_particleSystem = m_particle.GetComponent<ParticleSystem>();
							}
						}

					}
					break;
				}

			}

			protected override void OnEnter(State oldState, object[] param) {
				m_pilot.PressAction(Pilot.Action.Button_A);
				m_timer = 1.0f;
				if (!string.IsNullOrEmpty(m_data.audio))
				{
					AudioController.Play(m_data.audio);
				}
				if ( m_particleSystem )
				{
					m_particleSystem.Play();
				}
			}

			protected override void OnUpdate(){
				m_timer -= Time.deltaTime;

				m_pilot.SlowDown(true);
				m_pilot.SetDirection( Vector3.forward, true );

				if ( m_timer <= 0 )	
				{
					EntityManager.instance.ForceOnScreenEntitiesGolden();
					Transition( onAnimFinished );
				}
			}

			protected override void OnExit(State _newState){
				m_pilot.ReleaseAction(Pilot.Action.Button_A);
				if ( m_particleSystem )
				{
					m_particleSystem.Stop();
				}

			}
		}
	}
}