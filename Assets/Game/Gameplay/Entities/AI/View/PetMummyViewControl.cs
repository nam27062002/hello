﻿using UnityEngine;
using System.Collections;

public class PetMummyViewControl : ViewControl {
    private static int REVIVE_HASH = Animator.StringToHash("Revive");

    private enum State {
        IDLE = 0,
        POWER_ACTIVE,
        DYING
    }


    [Separator("Pet Mummy")]
    [SerializeField] private Vector3 m_basePosition = GameConstants.Vector3.zero;
    [SerializeField] private Vector3 m_topPosition = GameConstants.Vector3.zero;

    [SerializeField] private ParticleSystem m_effect = null;
    [SerializeField] private float m_dyingTimer = 1f;


    private DragonPlayer m_dragon;
    private Transform m_effectTransform;

    private AI.IMachine m_machine;

    private float m_powerStatus;
    private float m_timer;
    private State m_state;


	protected override void Awake() {
		base.Awake();
        StartEndStateMachineBehaviour[] behaviours = m_animator.GetBehaviours<StartEndStateMachineBehaviour>();
        for (int i = 0; i < behaviours.Length; i++) {
            behaviours[i].onStateExit += onStateExit;
        }

        m_dragon = InstanceManager.player;
        m_effectTransform = m_effect.transform;

        m_machine = GetComponent<AI.IMachine>();
        OnInit();
	}

    private void OnInit() {
        m_powerStatus = 1f;
        m_effect.Stop(true);
        m_state = State.IDLE;
    }

    private void onStateExit(int shortName) {
        if (shortName == REVIVE_HASH) {
            m_effectTransform.localPosition = m_basePosition;
            m_effect.Play(true);
            m_state = State.POWER_ACTIVE;
        }
    }

    public override void CustomUpdate() {
        base.CustomUpdate();

        if (m_state == State.POWER_ACTIVE) {
            m_powerStatus = 1f - (m_dragon.health / m_dragon.mummyHealthMax);

            m_effectTransform.localPosition = Vector3.Lerp(m_basePosition, m_topPosition, m_powerStatus);

            for (int i = 0; i < m_materialList.Count; ++i) {
                m_materialList[i].SetFloat("_DissolveAmount", m_powerStatus);
            }

            if (m_powerStatus <= float.Epsilon) {
                if (m_dragon.HasMummyPowerAvailable()) {
                    OnInit();
                } else {
                    m_timer = m_dyingTimer;
                    m_machine.SetSignal(AI.Signals.Type.FallDown, true);
                    m_animator.SetTrigger(GameConstants.Animator.DEAD);
                    m_state = State.DYING;
                }
            }
        } else if (m_state == State.DYING) {
            m_timer -= Time.deltaTime;
            if (m_timer <= 0f) {
                Object.Destroy(gameObject);
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawCube(transform.position + m_basePosition, GameConstants.Vector3.one * 0.25f);
        Gizmos.DrawCube(transform.position + m_topPosition, GameConstants.Vector3.one * 0.25f);
    }
}
