using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {	
		[System.Serializable]
		public class AttackStunArchData : AttackData {
			public float m_archIntro;
			public float m_archOutro;
			public float m_archDuration;

			public float m_archLength;
			public float m_archAngle;

			public string m_beamAnchorPoint;
			public float m_stunDuration;
			public string m_beamSound;
		}
		

		[CreateAssetMenu(menuName = "Behaviour/Attack/Stun Arch")]
		public class AttackStunArch : Attack {

			[StateTransitionTrigger]
			private static string OnAttackDone = "onAttackDone";

			private ViewControl m_viewControl;

			private AttackStunArchData m_stunData;
			private int m_numCheckEntities = 0;
			private Entity[] m_checkEntities = new Entity[30];
			private Transform m_stunAnchor;
			private bool m_attacking = false;
			private float m_attackingTimer = 0;
			private ParticleSystem m_particle;
			private AudioObject m_beamSoundAO;

			public override StateComponentData CreateData() {
				return new AttackStunArchData();
			}

			public override System.Type GetDataType() {
				return typeof(AttackStunArchData);
			}

			protected override void OnInitialise() {
				m_stunData = m_pilot.GetComponentData<AttackStunArchData>();
				m_data = m_stunData;
				m_viewControl = m_pilot.GetComponent<ViewControl>();
				base.OnInitialise();

				m_stunAnchor = m_pilot.FindTransformRecursive( m_stunData.m_beamAnchorPoint );

				ParticleSystem particlePrefab = null;
				switch(FeatureSettingsManager.instance.Particles) 
				{
					case FeatureSettings.ELevel5Values.very_low:							
					case FeatureSettings.ELevel5Values.low:
					{
						particlePrefab = Resources.Load<ParticleSystem>("Particles/Low/PS_PetStun");
					}break;
					default:
					{
						particlePrefab = Resources.Load<ParticleSystem>("Particles/Master/PS_PetStun");
					}break;
				}

				if ( particlePrefab )
				{
					m_particle = Instantiate<ParticleSystem>(particlePrefab);
					m_particle.transform.parent = m_stunAnchor;
					m_particle.transform.localPosition = Vector3.zero;
					m_particle.transform.localRotation = Quaternion.identity;
				}

			}

			protected override void StartAttack() 
			{
				if ( m_particle )
					m_particle.Play();
				if (!string.IsNullOrEmpty( m_stunData.m_beamSound ))
				{
					m_beamSoundAO = AudioController.Play(m_stunData.m_beamSound, m_pilot.transform);
				}
				m_attacking = true;
				m_attackingTimer = m_stunData.m_archDuration + m_stunData.m_archIntro + m_stunData.m_archOutro;

				base.StartAttack();
				if (m_data.forceFaceToShoot && m_viewControl != null) {
					// Tell view position to attack
					m_viewControl.attackTargetPosition = m_facingTarget;
				}


			}

			protected override void OnEnter(State _oldState, object[] _param) {
				base.OnEnter(_oldState, _param);
				m_pilot.PressAction(Pilot.Action.Aim);
				m_machine.SetSignal(Signals.Type.Ranged, true);

			}

			protected override void OnUpdate() {
				base.OnUpdate();

				if ( m_attacking && m_attackingTimer > 0 )
				{
					m_attackingTimer -= Time.deltaTime;
					if ( m_attackingTimer <= 0 )
					{
						if ( m_particle )
							m_particle.Stop();
						StopSound();
						m_pilot.ReleaseAction(Pilot.Action.Attack);
						m_machine.DisableSensor(m_data.retreatTime);
						Transition(OnAttackDone);
					}
					else
					{
						Vector3 dir = m_machine.enemy.position - m_machine.position;
						m_pilot.SetDirection(dir.normalized, true);

							// Intro Outro
						float delta = 1;
						if ( m_attackingTimer > (m_stunData.m_archDuration + m_stunData.m_archOutro) )
						{
							// intro
							delta = m_attackingTimer - (m_stunData.m_archDuration + m_stunData.m_archOutro);
							delta = 1.0f - delta / m_stunData.m_archIntro;
						}else if ( m_attackingTimer < m_stunData.m_archOutro){
							// outro
							delta = m_attackingTimer / m_stunData.m_archOutro;
						}

						float arcLength = m_stunData.m_archLength * delta;
						float arcAngle = m_stunData.m_archAngle * delta;

						Vector3 arcOrigin = m_stunAnchor.position;
						Vector3 arcOrigin_0 = arcOrigin;
						arcOrigin_0.z = 0;
						Vector3 arcDir = m_stunAnchor.forward;
						arcDir.z = 0;
						arcDir.Normalize();

						m_numCheckEntities =  EntityManager.instance.GetOverlapingEntities(arcOrigin, arcLength, m_checkEntities);

						// To test 
						float arcDot = Vector3.Dot( m_stunAnchor.forward, arcDir);
						arcLength = arcLength * arcDot;

						#if UNITY_EDITOR
							Debug.DrawLine( arcOrigin_0, arcOrigin_0 + arcDir * arcLength, Color.white);
							Vector2 d = (Vector2)arcDir;
							Vector2 dUp = d.RotateDegrees(arcAngle/2.0f);
							Debug.DrawLine( arcOrigin_0, arcOrigin_0 + (Vector3)(dUp * arcLength), Color.red);
							Vector2 dDown = d.RotateDegrees(-arcAngle/2.0f);
							Debug.DrawLine( arcOrigin_0, arcOrigin_0 + (Vector3)dDown * arcLength, Color.red);
						#endif


						for (int e = 0; e < m_numCheckEntities; e++) 
						{
							Entity entity = m_checkEntities[e];
							// Start bite attempt
							Vector3 heading = (entity.transform.position - arcOrigin);
							float dot = Vector3.Dot(heading, arcDir);
							if ( dot > 0)
							{
								// Check arc
								Vector3 circleCenter = entity.circleArea.center;
								circleCenter.z = 0;
								if (MathUtils.TestArcVsCircle( arcOrigin_0, arcAngle, arcLength, arcDir, circleCenter, entity.circleArea.radius))
								{
									// stun entity
									(entity.machine as Machine).Stun( m_stunData.m_stunDuration );
								}
							}
						}
					}
				}
			}

			protected override void OnExit(State _newState) {
				base.OnExit(_newState);
				m_pilot.ReleaseAction(Pilot.Action.Aim);
				m_machine.SetSignal(Signals.Type.Ranged, false);
				if ( m_particle )
					m_particle.Stop();
				StopSound();
			}

			private void StopSound()
			{
				if (m_beamSoundAO != null && m_beamSoundAO.IsPlaying())
				{
					m_beamSoundAO.Stop();
					m_beamSoundAO = null;
				}
			}
		}
	}
}