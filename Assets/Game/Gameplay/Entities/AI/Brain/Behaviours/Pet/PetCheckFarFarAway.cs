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
			protected const float m_minDistanceToAppearInFront = 15.0f; //For the pet to respawn in front of the shark instead of behind
			protected const float m_reallyFarRecoverDistance = 30.0f;	// TODO: confirm this is off-screen

			private float m_approxSize;
			

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
					// m_pilot.transform.position = m_owner.transform.position + m_owner.dragonMotion.direction * m_reallyFarRecoverDistance; 

					float m_minDistanceToAppearInFrontInThisShark = m_minDistanceToAppearInFront * m_approxSize;//m_ownerFish.approxSize;
		            //Decide if it's better to re-position the pet behind the shark, or in front ( when recently un-beached, spawning behind might cause clipping! )
		            float minDistLeft = float.MaxValue;
		            float minDistRight = float.MaxValue;
		            RaycastHit rh;
		            bool makePetAppearBehind = false;
		            bool makePetAppearFront = false;

		            //Launch a ray from where the shark is, in the opposite direction of the pet ( normal scenario, as pet would normally be behind him )
                    if (Physics.Raycast(ownerPos, delta.normalized, out rh, Mathf.Infinity, GameConstants.Layers.GROUND))
		            {
		                minDistLeft = rh.distance;
		                if (minDistLeft > m_minDistanceToAppearInFrontInThisShark)
		                {
		                    makePetAppearBehind = true;
		                }
		            }
		            else
		            {
		                makePetAppearBehind = true;
		            }

		            //If we cannot spawn the pet behind, look for other options.
		            if (makePetAppearBehind == false)
		            {
                        if (Physics.Raycast(ownerPos, -delta.normalized, out rh, Mathf.Infinity, GameConstants.Layers.GROUND))
		                {
		                    minDistRight = rh.distance;
		                    if (minDistRight > m_minDistanceToAppearInFrontInThisShark)
		                    {
		                        makePetAppearFront = true;
		                    }
		                }
		                else
		                {
		                    makePetAppearFront = true;
		                }
		            }

		            //Spawn the pet based on the previous decision taken.
		            //EdgeCase: If we are teleporting ( i.e inside/outside the galleon ), make sure we always spawn behind the direction the shark is facing.
		            /*if (m_ownerIsTeleporting)
		            {
						m_pilot.transform.position = ownerPos + (- m_pilot.transform.right * m_reallyFarRecoverDistance ) ;
		            }
		            else */
		            if (makePetAppearBehind)  //Spawn the pet in front or behind the shark, as previously decided
		            {
						m_pilot.transform.position = ownerPos + (delta.normalized * m_reallyFarRecoverDistance );
		            }
		            else if (makePetAppearFront)
		            {
						m_pilot.transform.position = ownerPos - (delta.normalized * m_reallyFarRecoverDistance );
		            }
					else  //If we cannot spawn the pet neither in front or behind, spawn him below!
					{
						m_pilot.transform.position = ownerPos + (Vector3.down * m_reallyFarRecoverDistance );
		            }

		            /*
		            if (m_surfaceModifier)
		            {
		                m_surfaceModifier.UpdateLastPosition(transform.position);
		            }
		            */

		            Transition(onTooFarAway, 0);

		        }
			}
		}
	}
}