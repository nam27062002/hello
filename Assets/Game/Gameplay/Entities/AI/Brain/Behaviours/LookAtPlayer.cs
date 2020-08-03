using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
        
		[CreateAssetMenu(menuName = "Behaviour/Look At Player")]
		public class LookAtPlayer : StateComponent {
            
            protected override void OnUpdate() {
                Vector3 targetDir = InstanceManager.player.transform.position - m_machine.transform.position;
                m_pilot.SetDirection( targetDir, true );
            }
		}
	}
}