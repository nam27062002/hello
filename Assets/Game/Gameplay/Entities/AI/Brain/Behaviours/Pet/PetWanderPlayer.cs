using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[System.Serializable]
		public class WanderPlayerData : StateComponentData {
			// public float speedMultiplier = 1.5f;
			public string petWanderSku = "common";
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/Wander Player")]
		public class PetWanderPlayer : StateComponent {

			private static int m_groundMask;

			private SphereCollider m_collider;
			private Transform m_target;
			private Vector3 m_targetOffset;
			private float m_speed;

			private float m_targetDelta = 0;
			private float m_maxFarDistance;
			private float m_startRandom;
			private float m_minorRandom;
			private float m_circleTimer = 0;

			public override StateComponentData CreateData() {
				return new WanderPlayerData();
			}

			public override System.Type GetDataType() {
				return typeof(WanderPlayerData);
			}

			protected override void OnInitialise() {
				m_groundMask = LayerMask.GetMask("Ground", "GroundVisible", "PreyOnlyCollisions");
				WanderPlayerData data = m_pilot.GetComponentData<WanderPlayerData>();
				m_collider = m_pilot.GetComponent<SphereCollider>();
				m_target = m_machine.transform;
				DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.PET_MOVEMENT, data.petWanderSku);
				m_speed = InstanceManager.player.dragonMotion.absoluteMaxSpeed * def.GetAsFloat("wanderSpeedMultiplier");

				m_maxFarDistance = InstanceManager.player.data.GetScaleAtLevel( InstanceManager.player.data.progression.maxLevel) * def.GetAsFloat("wanderDistanceMultiplier");
				m_startRandom = Random.Range(0, 2 * Mathf.PI);
				m_minorRandom = Random.Range( 2, 7);
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				SelectTarget();
				m_pilot.SlowDown(false); // this wander state doesn't have an idle check
				m_pilot.SetMoveSpeed(m_speed); //TODO
			}

			protected override void OnUpdate() {

				Vector3 targetPos = InstanceManager.player.transform.position;
				Vector2 circleMove;

				m_circleTimer += Time.deltaTime;
				circleMove.x = Mathf.Sin(m_circleTimer + m_startRandom) * m_maxFarDistance;
				circleMove.y = Mathf.Cos(m_circleTimer + m_startRandom) * m_maxFarDistance;
				// circleMove *= (0.75f + (Mathf.Sin( Time.time + m_startRandom)/2.0f + 1) * 0.25f);
				circleMove *= 0.7f + ((Mathf.Sin( (Time.time + m_startRandom) * m_minorRandom) / 2.0f) + 1) * 0.3f;
				targetPos.x += circleMove.x;
				targetPos.y += circleMove.y;
				m_pilot.GoTo(targetPos);

				float magnitude = (targetPos - m_pilot.transform.position).sqrMagnitude;
				m_pilot.SetMoveSpeed(Mathf.Min( m_speed, magnitude));					
				if ( m_pilot.speed <= 0.1f )
				{
					m_pilot.SlowDown(true);
					m_pilot.SetDirection( m_target.forward );
				}
				else
				{
					m_pilot.SlowDown(false);
				}
				
			/*
				Vector3 anchorPos = m_target.position + m_targetOffset;

				Vector3 distanceToPlayer = InstanceManager.player.transform.position - m_pilot.transform.position;
				if ( distanceToPlayer.sqrMagnitude > m_maxFarDistance )
				{
					m_targetDelta += Time.deltaTime;
				}
				else
				{
					m_targetDelta -= Time.deltaTime;
				}
				m_targetDelta = Mathf.Clamp01( m_targetDelta );

				Vector3 targetPos = Vector3.Lerp( anchorPos, InstanceManager.player.transform.position, m_targetDelta);
				m_pilot.GoTo(targetPos);

				float magnitude = (targetPos - m_pilot.transform.position).sqrMagnitude;
				m_pilot.SetMoveSpeed(Mathf.Min( m_speed, magnitude));					
				if ( m_pilot.speed <= 0.1f )
				{
					m_pilot.SlowDown(true);
					m_pilot.SetDirection( m_target.forward );
				}
				else
				{
					m_pilot.SlowDown(false);
				}
				*/

			/*
				Vector3 targetPos = InstanceManager.player.transform.position;
				Vector3 direction = (targetPos - m_pilot.transform.position);
				float distance = InstanceManager.player.data.GetScaleAtLevel( InstanceManager.player.data.progression.maxLevel) * 7;
				if ( direction.magnitude < distance )
				{
					targetPos = m_pilot.homeTransform.position;
				}
				m_pilot.GoTo(targetPos);

				float magnitude = (targetPos - m_pilot.transform.position).sqrMagnitude;
				m_pilot.SetMoveSpeed(Mathf.Min( m_speed, magnitude));					

				if ( m_pilot.speed <= 0.1f )
				{
					m_pilot.SlowDown(true);
					m_pilot.SetDirection( m_target.forward );
				}
				else
				{
					m_pilot.SlowDown(false);
				}
				*/


				Debug.DrawLine(m_machine.position, targetPos, Colors.gold);
			}

			private void SelectTarget() {
				m_target = m_pilot.homeTransform;	//  Get Pet position??
				// check collision
				/*
					// FIX THIS!
				RaycastHit groundHit;
				if (Physics.Linecast(m_machine.position, m_target.position, out groundHit, m_groundMask)) {
					m_targetOffset = groundHit.point - m_target.position;
					m_targetOffset = m_targetOffset.normalized * (m_targetOffset.magnitude + m_collider.radius);
				} else {
					m_targetOffset = Vector3.zero;
				}
				*/


				m_targetOffset = Vector3.zero;
			}
		}
	}
}