using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Is Target Close Enough")]
		public class IsTargetCloseEnough : StateComponent {
			
			[StateTransitionTrigger]
			private static readonly int onCloseEnough = UnityEngine.Animator.StringToHash("onCloseEnough");

			[System.Serializable]
			public class IsTargetCloseEnoughData : StateComponentData {
				public float m_distance;
			}

			private float m_distance;
			private Transform m_target;
			protected Entity m_targetEntity;

			public override StateComponentData CreateData() {
				return new IsTargetCloseEnoughData();
			}

			public override System.Type GetDataType() {
				return typeof(IsTargetCloseEnoughData);
			}

			protected override void OnInitialise() {
				IsTargetCloseEnoughData data = m_pilot.GetComponentData<IsTargetCloseEnoughData>();
				m_distance = data.m_distance * data.m_distance;
			}

			protected override void OnEnter(State _oldState, object[] _param) {
				if ( _param != null && _param.Length > 0 )
				{
					m_target = _param[0] as Transform;
					if ( m_target ) {
						m_targetEntity = m_target.GetComponent<Entity>();
					}
				}
			}

			protected override void OnUpdate() {
				if (m_targetEntity)
				{
					CircleArea2D circle = m_targetEntity.circleArea;
					float d = (circle.center - m_machine.position).sqrMagnitude - (circle.radius * circle.radius);
					if ( d < m_distance )
					{
						Transition(onCloseEnough);
					}
				}else if ( m_target ){
					float d = (m_target.position - m_machine.position).sqrMagnitude;
					if ( d < m_distance )
					{
						Transition(onCloseEnough);
					}
				}
			}
		}
	}
}