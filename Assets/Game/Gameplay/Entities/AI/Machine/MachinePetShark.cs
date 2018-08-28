using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class MachinePetShark : MachineAir {

        Vector3 m_startLocalScale;
        bool m_insideWater = false;
        DragonTier m_startEatingTier = DragonTier.TIER_0;
        MachineEatBehaviour m_eatBehaviour;
        private const float m_scaleDuration = 0.5f;


        public virtual void Start()
        {
            m_transform = transform;
            m_startLocalScale = m_transform.localScale;
            m_eatBehaviour = GetComponent<MachineEatBehaviour>();
            if ( m_eatBehaviour )
            {
                m_startEatingTier = m_eatBehaviour.eaterTier;
            }
        }

        public override void CustomUpdate()
        {
            base.CustomUpdate();
            if ( GetSignal(Signals.Type.InWater) ){
                if ( !m_insideWater )
                {
                    if (m_pilot)
                        m_pilot.speedFactor = 2;
                    StopCoroutine(ScaleDown());
                    StartCoroutine(ScaleUp());
                    if ( m_eatBehaviour != null )
                    {
                        m_eatBehaviour.eaterTier = m_startEatingTier + 1;
                    }
                    m_insideWater = true;
                }
            } else {
                if ( m_insideWater )
                {
                    if (m_pilot)
                        m_pilot.speedFactor = 1;
                    StopCoroutine(ScaleUp());
                    StartCoroutine(ScaleDown());
                    if (m_eatBehaviour != null)
                    {
                        m_eatBehaviour.eaterTier = m_startEatingTier;
                    }
                    m_insideWater = false;
                }
            }
        }

        IEnumerator ScaleUp()
        {
            float duration = m_scaleDuration;
            while( duration > 0 )
            {
                yield return null;
                m_transform.localScale = Vector3.Lerp(m_startLocalScale * 2, m_startLocalScale, duration / m_scaleDuration);
                duration -= Time.deltaTime;
            }
            m_transform.localScale = m_startLocalScale * 2;
            yield return null;
        }

        IEnumerator ScaleDown()
        {
            float duration = m_scaleDuration;
            while (duration > 0)
            {
                yield return null;
                m_transform.localScale = Vector3.Lerp(m_startLocalScale, m_startLocalScale * 2, duration / m_scaleDuration);
                duration -= Time.deltaTime;
            }
            m_transform.localScale = m_startLocalScale;
            yield return null;
        }
    }
}
