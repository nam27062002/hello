using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {

		[System.Serializable]
		public class PetPointingCollectibleTargetData : StateComponentData {
			public float m_speedMultiplier = 2;
			public float m_playerDistance = 5;
			public float m_closeRangeMultiplier = 10;
			public float m_farRangeMultiplier = 30;
		}

		[CreateAssetMenu(menuName = "Behaviour/Pet/PointCollectible")]
		public class PetPointingCollectible : StateComponent {

			[StateTransitionTrigger]
			private static string OnTooFarPointing = "onTooFarPointing";

			[StateTransitionTrigger]
			private static string OnCollected = "onCollected";


			float m_playerDistance;
			float m_closeRange;
			float m_farRange;

			DragonPlayer m_player;
			GameObject m_pointToObject;
			Transform m_pointToTransform;

			CollectibleEgg egg = null;
			CollectibleChest chest = null;
			HungryLetter letter = null;

			PetPointingCollectibleTargetData m_data;
			float m_speed;

            UIGameEntitySpawn m_visualCue;

			public override StateComponentData CreateData() {
				return new PetPointingCollectibleTargetData();
			}

			public override System.Type GetDataType() {
				return typeof(PetPointingCollectibleTargetData);
			}


			protected override void OnInitialise() {
                base.OnInitialise();
				m_data = m_pilot.GetComponentData<PetPointingCollectibleTargetData>();

				m_player = InstanceManager.player;
				float maxLevelScale = m_player.data.maxScale;

				m_playerDistance = maxLevelScale * m_data.m_playerDistance;

				m_closeRange = maxLevelScale * m_data.m_closeRangeMultiplier;
				m_closeRange = m_closeRange * m_closeRange;

				m_farRange = maxLevelScale * m_data.m_farRangeMultiplier;
				m_farRange = m_farRange * m_farRange;
                
				m_speed = InstanceManager.player.dragonMotion.absoluteMaxSpeed * m_data.m_speedMultiplier;
                m_visualCue = m_pilot.GetComponent<UIGameEntitySpawn>();

			}

			protected override void OnEnter(State oldState, object[] param){
                base.OnEnter( oldState, param );
				m_pointToObject = param[0] as GameObject;
				m_pointToTransform = m_pointToObject.transform;

				egg = m_pointToObject.GetComponent<CollectibleEgg>();
				chest = m_pointToObject.GetComponent<CollectibleChest>();
				letter = m_pointToObject.GetComponent<HungryLetter>();

				m_pilot.SlowDown(true);
				m_pilot.SetMoveSpeed(m_speed);

				// Show some placeholder visual clue
				if (m_visualCue != null && m_visualCue.instance != null)
				{
					m_visualCue.instance.SetActive(true);
                    // Setup visual
                    Transform tr = m_visualCue.instance.transform.FindTransformRecursive(GetSignalId());
                    if (tr != null)
                        tr.gameObject.SetActive( true );
				}

			}

			override protected void OnUpdate(){
                base.OnUpdate();
				// TODO Improve this to use events
				if ( egg )
				{
					if ( egg.collected )
						Transition(OnCollected);
				}
				else if ( chest )
				{
					if (chest.chestData.collected )
						Transition(OnCollected);
				}
				else if ( letter )
				{
					if (InstanceManager.hungryLettersManager.IsLetterCollected( letter.letter))
						Transition(OnCollected);
				}



				Vector3 diff = (m_pointToTransform.position - m_player.transform.position);
				float sqrMagnitude = diff.sqrMagnitude;
				if ( sqrMagnitude < m_closeRange )	// if player is close enough
				{
					// Just go for the collectible
					Vector3 destPos = m_pointToTransform.position + (Vector3.up * 4);
					m_pilot.GoTo(destPos);
				}
				else if ( sqrMagnitude > m_farRange )
				{
					// Stop pointing
					Transition(OnTooFarPointing);
				}
				else
				{
					// show player where to go
					diff.Normalize();
					Vector3 destPos = m_player.transform.position + diff * m_playerDistance;
					m_pilot.GoTo(destPos);
				}

			}

			protected override void OnExit(State _newState){
                // Hide placeholder visual
                base.OnExit(_newState);
				if (m_visualCue && m_visualCue.instance )
				{
                    m_visualCue.instance.SetActive(false);
                    Transform tr = m_visualCue.instance.transform.FindTransformRecursive(GetSignalId());
                    if (tr != null)
                        tr.gameObject.SetActive( false );
				}
			}
            
            private string GetSignalId()
            {
                string ret = "";
                if ( egg )
                {
                    ret = "egg";
                }
                else if ( chest )
                {
                    ret = "treasure";
                }
                else if ( letter )
                {
                    ret = "letter_" + HungryLettersManager.ToChar( letter.letter );
                }
                return ret;
            }
		}
	}
}