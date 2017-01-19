using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {
		
		[CreateAssetMenu(menuName = "Behaviour/Fury Toggle Check")]
		public class FuryToggleCheck : StateComponent {
			
			[StateTransitionTrigger]
			protected static string OnFuryOn = "onFuryOn";
			[StateTransitionTrigger]
			protected static string OnFuryOff = "onFuryOff";


			protected override void OnEnter(State oldState, object[] param) 
			{
				Messenger.AddListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
			}

			protected override void OnExit(State newState)
			{
				Messenger.RemoveListener<bool, DragonBreathBehaviour.Type>(GameEvents.FURY_RUSH_TOGGLED, OnFuryToggled);
			}

			private void OnFuryToggled( bool toggle, DragonBreathBehaviour.Type type)
			{
				if ( toggle )
				{
					Transition( OnFuryOn );
				}
				else
				{
					Transition( OnFuryOff );
				}
			}

		}
	}
}