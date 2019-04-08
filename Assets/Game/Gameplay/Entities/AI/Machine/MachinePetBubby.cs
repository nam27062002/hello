using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI {
    public class MachinePetBubby : MachineAir {

        private bool m_insideWater = false;

        private MachineEatBehaviour m_eatBehaviour;


        public virtual void Start() {
            m_transform = transform;
            m_eatBehaviour = GetComponent<MachineEatBehaviour>();
        }

        public override void CustomUpdate() {
            base.CustomUpdate();
            if (GetSignal(Signals.Type.InWater)) {
                if (m_insideWater) {
                    if (m_pilot.IsActionPressed(Pilot.Action.Button_A)) {

                    }
                } else {
                    m_eatBehaviour.enabled = false;
                    m_insideWater = true;
                }
            } else {
                if (m_insideWater) {
                    m_eatBehaviour.enabled = true;
                    m_insideWater = false;
                }
            }
        }
    }
}
