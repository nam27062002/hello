﻿using UnityEngine;
using System.Collections;

namespace AI {
	namespace Behaviour {		
		[CreateAssetMenu(menuName = "Behaviour/Fury Toggle Check")]
		public class FuryToggleCheck : StateComponent, IBroadcastListener {
			
			[StateTransitionTrigger]
			protected static string OnFuryOn = "onFuryOn";
			[StateTransitionTrigger]
			protected static string OnFuryOff = "onFuryOff";


			protected override void OnEnter(State oldState, object[] param) {
                Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
			}

			protected override void OnExit(State newState) {
				Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
			}
            
            public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
            {
                switch( eventType )
                {
                    case BroadcastEventType.FURY_RUSH_TOGGLED:
                    {
                        FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                        OnFuryToggled( furyRushToggled.activated, furyRushToggled.type );
                    }break;
                }
            }

			private void OnFuryToggled(bool toggle, DragonBreathBehaviour.Type type) {
				if (toggle) Transition(OnFuryOn);
				else		Transition(OnFuryOff);
			}
		}
	}
}