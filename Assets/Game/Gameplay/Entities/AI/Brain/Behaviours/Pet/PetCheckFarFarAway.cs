using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Pet/Check Far Far Away")]
		public class PetCheckFarFarAway : StateComponent {

			[StateTransitionTrigger]
    		public const string onTooFarAway = "onTooFarAway";

			DragonPlayer m_owner;
			protected const float m_reallyFarDistance = 35.0f;		// TODO: confirm this is off-screen
			protected const float m_reallyFarRecoverDistance = 30.0f;	// TODO: confirm this is off-screen


			protected override void OnInitialise() {
				base.OnInitialise();
				m_owner = InstanceManager.player;
			}

			protected override void OnUpdate() {
				Vector3 pos = m_pilot.transform.position;
		        Vector3 ownerPos = m_owner.transform.position;
		        Vector3 delta = pos - ownerPos;
		        float d2 = delta.sqrMagnitude;
		        if (d2 > (m_reallyFarDistance * m_reallyFarDistance))
		        {
					m_pilot.transform.position = m_owner.transform.position + m_owner.dragonMotion.direction * m_reallyFarRecoverDistance; 
		        }
			}
		}
	}
}