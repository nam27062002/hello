using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    namespace Behaviour
    {
        [System.Serializable]
        public class AttackMeleeNoAnimationData : AttackMeleeData {
            public float duration = 5f;
        }
        
        [CreateAssetMenu(menuName = "Behaviour/Attack/Melee No Animation")]
        public class AttackMeleeNoAnimation : AttackMelee {

            protected float m_attaclTimer = 0;
            
            public override StateComponentData CreateData() {
                return new AttackMeleeNoAnimationData();
            }

            public override System.Type GetDataType() {
                return typeof(AttackMeleeNoAnimationData);
            }
            
            protected override void OnInitialise()
            {
                m_data = m_pilot.GetComponentData<AttackMeleeNoAnimationData>();
                base.OnInitialise();
            }

            protected override void OnEnter(State _oldState, object[] _param)
            {
                base.OnEnter( _oldState, _param );
                m_attaclTimer = ((AttackMeleeNoAnimationData)m_data).duration;
                OnAnimDealDamage();
            }

            protected override void StartAttack()
            {
                base.StartAttack();
                OnAnimDealDamage();
            }
            
            protected override void OnUpdate()
            {
                base.OnUpdate();
                m_attaclTimer -= Time.deltaTime;
                if ( m_attaclTimer <= 0 )
                {
                    OnAnimEnd();
                }
            }
        }
    }
}

