using UnityEngine;
using System.Collections;

public class PetMummyViewControl : ViewControl {
    private static int REVIVE_HASH = Animator.StringToHash("Revive");

    private enum State {
        IDLE = 0,
        POWER_ACTIVE
    }


    private DragonPlayer m_dragon;
    private float m_powerStatus;
    private State m_state;


	protected override void Awake() {
		base.Awake();
        StartEndStateMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndStateMachineBehaviour>();
        for (int i = 0; i < behaviours.Length; i++) {
            behaviours[i].onStateExit += onStateExit;
        }

        m_dragon = InstanceManager.player;
        OnInit();
	}

    private void OnInit() {
        m_powerStatus = 1f;
        m_state = State.IDLE;
    }

    private void onStateExit(int shortName) {
        if (shortName == REVIVE_HASH) {
            m_state = State.POWER_ACTIVE;
        }
    }

    public override void CustomUpdate() {
        base.CustomUpdate();

        if (m_state == State.POWER_ACTIVE) {
            m_powerStatus = m_dragon.health / m_dragon.mummyHealthMax; 
            if (m_powerStatus <= float.Epsilon) {
                if (m_dragon.HasMummyPowerAvailable()) {
                    OnInit();
                } else {
                    // destroy it
                }
            }
        }
    }
}
