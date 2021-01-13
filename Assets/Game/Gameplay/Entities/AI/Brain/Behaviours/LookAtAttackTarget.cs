using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		[System.Serializable]
		public class LookAtAttackTargetData : StateComponentData {
            public string pivotName;
		}

		[CreateAssetMenu(menuName = "Behaviour/Look At Attack Target")]
		public class LookAtAttackTarget : StateComponent {
        
			public override StateComponentData CreateData() {
                return new LookAtAttackTargetData();
            }

            public override System.Type GetDataType() {
                return typeof(LookAtAttackTargetData);
            }

            protected Transform m_pivot;
            protected Transform m_target;
            
            protected override void OnInitialise() {
                base.OnInitialise();
                // Look for pivot transform
                LookAtAttackTargetData data = m_pilot.GetComponentData<LookAtAttackTargetData>();

                m_pivot = m_pilot.FindTransformRecursive(data.pivotName);
            }

            protected override void OnEnter(State _oldState, object[] _param) {
                base.OnEnter(_oldState, _param);
                m_target = m_machine.enemy.transform;
            }
            
            protected override void OnUpdate() {
                // Look at target
                m_pivot.LookAt(m_target);
            }
		}
	}
}