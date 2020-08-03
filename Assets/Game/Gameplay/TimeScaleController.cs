using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleController : MonoBehaviour {

    
    public enum PlayerState
    {
        NONE,
        DEAD,
        REVIVING,
    }
    protected PlayerState m_playerState = PlayerState.NONE;
    public bool m_paused = false;
    
    // HIT STOP
    // public int m_hitStop = 0;
    public float m_hitStopTimer = 0;
    public float m_hitStopDuration = 0.1f;
    public float m_hitStopDelay = 0.1f;
    public bool m_ignoreHitStops = false;
    
    
    // SLOW MO
    public float m_slowMoTimer = 0;
    public float m_slowMoDuration = 0;
    public AnimationCurve m_slowMoCurve = null;
    public float m_slowMoTimescale = 0;

    private void Awake()
    {
        InstanceManager.timeScaleController = this;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
        InstanceManager.timeScaleController = null;
    }

    private void Update()
    {
        float scale = 1;
        if ( m_paused  )
        {
            scale = 0;
        }
        else if ( m_playerState != PlayerState.NONE )
        {
            scale = 0.25f;
        }
        else if ( m_slowMoTimer > 0 )
        {
            float delta = 1.0f - m_slowMoTimer / m_slowMoDuration;
            if (m_slowMoCurve != null)
            {
                scale = m_slowMoCurve.Evaluate( delta );
            }
            else
            {
                scale = m_slowMoTimescale;
            }
        }
        else if ( m_hitStopTimer > 0 && m_hitStopDelay <= 0)
        {
            scale = 1.0f - (m_hitStopTimer / m_hitStopDuration);
        }

        if (m_slowMoTimer > 0)
            m_slowMoTimer -= Time.unscaledDeltaTime;
        
        
        if ( m_hitStopDelay > 0 )
        {
            m_hitStopDelay -= Time.unscaledDeltaTime;
        }
        else if(m_hitStopTimer > 0)
        {
            m_hitStopTimer -= Time.unscaledDeltaTime;
        }
            

        Time.timeScale = scale;
    }


    public void Pause()
    {
        m_paused = true;
    }
    
    public void Resume()
    {
        m_paused = false;
    }
    
    public void Dead()
    {
        m_playerState = PlayerState.DEAD;
    }
    
    public void ReviveStart()
    {
        m_playerState = PlayerState.REVIVING;
    }
    
    public void Revived()
    {
        m_playerState = PlayerState.NONE;
    }
    
    public void HitStop( int frames = 1 )
    {
        if (!m_ignoreHitStops && DebugSettings.hitStopEnabled)
        {
            if ( m_hitStopTimer <= 0)
            {
                m_hitStopDelay = 0.1f;
            }
            m_hitStopTimer = m_hitStopDuration;
        }
    }
    
    public void StartSlowMotion( float duration ,  AnimationCurve slowMoCurve = null) 
    {
        m_slowMoTimer = m_slowMoDuration = duration;
        m_slowMoCurve = slowMoCurve;
    }
    
    public void StartSlowMotion( float duration ,  float timescale ) 
    {
        m_slowMoTimer = m_slowMoDuration = duration;
        m_slowMoCurve = null;
        m_slowMoTimescale = timescale;
    }

    public void StopSlowMotion()
    {
        m_slowMoTimer = 0;
    }
    
    public void GoingToResults()
    {
        Time.timeScale = 1;
        Destroy( gameObject );
    }
}
