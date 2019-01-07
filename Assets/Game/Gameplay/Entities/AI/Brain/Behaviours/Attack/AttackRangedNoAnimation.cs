using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    namespace Behaviour
    {
        [System.Serializable]
        public class AttackRangedNoAnimationData : AttackRangedData {
            public float duration = 0.1f;
        }
        
        [CreateAssetMenu(menuName = "Behaviour/Attack/Ranged No Animation")]
        public class AttackRangedNoAnimation : AttackRanged {

            protected float m_attaclTimer = 0;
            
            public override StateComponentData CreateData() {
                return new AttackRangedNoAnimationData();
            }

            public override System.Type GetDataType() {
                return typeof(AttackRangedNoAnimationData);
            }
            
            protected override void OnInitialise()
            {
                m_data = m_pilot.GetComponentData<AttackRangedNoAnimationData>();
                base.OnInitialise();
            }

            protected override void OnEnter(State _oldState, object[] _param)
            {
                base.OnEnter( _oldState, _param );
                m_attaclTimer = ((AttackRangedNoAnimationData)m_data).duration;
                OnAnimDealDamage();
            }

            protected override void StartAttack()
            {
                base.StartAttack();
                OnAttachProjectile();
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

