using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {

		[CreateAssetMenu(menuName = "Behaviour/Pet/PointCollectible")]
		public class PetPointingCollectible : StateComponent {

			[StateTransitionTrigger]
			private static string OnTooFarPointing = "onTooFarPointing";

			[StateTransitionTrigger]
			private static string OnCollected = "onCollected";


			float m_playerRangeDistance;
			float m_playerRangeDistanceSqr;
			DragonPlayer m_player;
			GameObject m_pointToObject;
			Transform m_pointToTransform;

			CollectibleEgg egg = null;
			CollectibleChest chest = null;
			HungryLetter letter = null;

			protected override void OnInitialise() {
				m_player = InstanceManager.player;
				m_playerRangeDistance = m_player.data.GetScaleAtLevel( m_player.data.progression.maxLevel ) * 11;
				m_playerRangeDistanceSqr = m_playerRangeDistance * m_playerRangeDistance;
			}

			protected override void OnEnter(State oldState, object[] param){
				m_pointToObject = param[0] as GameObject;
				m_pointToTransform = m_pointToObject.transform;

				egg = m_pointToObject.GetComponent<CollectibleEgg>();
				chest = m_pointToObject.GetComponent<CollectibleChest>();
				letter = m_pointToObject.GetComponent<HungryLetter>();
			}

			override protected void OnUpdate(){

				if ( egg )
				{
					if ( egg.collected )
					{
						Transition(OnCollected);
					}
				}
				else if ( chest )
				{
					
				}
				else if ( letter )
				{
					// if ( letter.letter )
				}



				Vector3 diff = (m_pointToTransform.position - m_player.transform.position);
				float sqrMagnitude = diff.sqrMagnitude;
				if ( sqrMagnitude < m_playerRangeDistanceSqr )	// if player is close enough
				{
					// Just go for the collectible
					Vector3 destPos = m_pointToTransform.position + (Vector3.up * 2);
					m_pilot.GoTo(destPos);
				}
				else if ( sqrMagnitude > m_playerRangeDistanceSqr * 2 )
				{
					// Stop pointing
					Transition(OnTooFarPointing);
				}
				else
				{
					// show player where to go
					diff.Normalize();
					Vector3 destPos = m_player.transform.position + diff * m_playerRangeDistance;
					m_pilot.GoTo(destPos);
				}

			}
		}
	}
}