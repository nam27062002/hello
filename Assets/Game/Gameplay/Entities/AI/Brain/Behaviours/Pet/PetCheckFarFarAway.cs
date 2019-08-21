using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Pet/Check Far Far Away")]
		public class PetCheckFarFarAway : StateComponent {

			[StateTransitionTrigger]
            private static readonly int onTooFarAway = UnityEngine.Animator.StringToHash("onTooFarAway");

            DragonPlayer m_owner;
			protected const float m_reallyFarDistance = 35.0f;		// TODO: confirm this is off-screen
			protected const float m_minDistanceToAppear = 20.0f; //For the pet to respawn in front of the shark instead of behind

			private float m_approxSize;
			protected static object[] m_transitionParam = new object[]{0};
			

			protected override void OnInitialise() {
				base.OnInitialise();
				m_owner = InstanceManager.player;
				m_approxSize =  m_owner.transform.lossyScale.x;				
			}

			protected override void OnUpdate() {
				Vector3 pos = m_pilot.transform.position;
		        Vector3 ownerPos = m_owner.transform.position;
		        Vector3 delta = pos - ownerPos;
		        float d2 = delta.sqrMagnitude;
		        if (d2 > (m_reallyFarDistance * m_reallyFarDistance))
		        {
                    delta.Normalize();
					float minDistanceToAppear = Mathf.Min(m_reallyFarDistance, m_minDistanceToAppear * m_approxSize) - 1;
		            //Decide if it's better to re-position the pet behind the shark, or in front ( when recently un-beached, spawning behind might cause clipping! )
		            RaycastHit rh;
		            //Launch a ray from where the dragon thorugh the pet direction
                    if (!Physics.Raycast(ownerPos, delta, out rh, minDistanceToAppear, GameConstants.Layers.GROUND))
		            {
                        // Make appear behind dragon
                        m_pilot.transform.position = ownerPos + (delta.normalized * minDistanceToAppear );
		            }
                    // Try the other direction
		            else if ( Physics.Raycast(ownerPos, -delta, out rh, minDistanceToAppear, GameConstants.Layers.GROUND) )
		            {
		                m_pilot.transform.position = ownerPos - (delta.normalized * minDistanceToAppear );
		            }
                    else
                    {
                        delta = delta.RotateXYDegrees(90);
                        m_pilot.transform.position = ownerPos + (delta * minDistanceToAppear);
                    }

		            Transition(onTooFarAway, m_transitionParam);

		        }
			}
		}
	}
}