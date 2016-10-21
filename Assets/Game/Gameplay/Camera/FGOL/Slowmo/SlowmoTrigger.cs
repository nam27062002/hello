// using FGOL;
using UnityEngine;

[RequireComponent(typeof(SlowmoManager))]
public abstract class SlowmoTrigger : MonoBehaviour
{
	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	protected bool m_showDebugMessages;

	[SerializeField]
	protected float m_timescaleToApply = 0.3f;
	[SerializeField]
	protected bool m_enableZoomIn;
	[SerializeField]
	[Range(0f, 20f)]
	protected float m_zoomValue;
	// [SerializeField]
	// protected PostProcessEffectAnimator[] m_postProcessAnimators;

	//------------------------------------------------------------
	// Protected Variables:
	//------------------------------------------------------------

	protected static SlowmoManager m_slowmo;
	protected static GameCamera m_gameCamera;

	protected bool m_inProgress;
	protected bool m_gamePaused;

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void Awake()
	{
#if !UNITY_EDITOR
		m_showDebugMessages = false;
#endif
		if(m_slowmo == null)
		{
			m_slowmo = GetComponent<SlowmoManager>();
			DebugUtils.Assert(m_slowmo != null, "No SlowmoManager");
		}

		if(m_gameCamera == null)
		{
			m_gameCamera = GetComponentInParent<GameCamera>();
			DebugUtils.Assert(m_gameCamera != null, "No GameCamera");
		}
	}

	// this function is only used to break the slowmo when the game is paused.
	protected void LateUpdate()
	{
		// TODO (MALH) : Recover this
		/*
		if(m_inProgress)
		{
			if(!m_gamePaused && App.Pause)
			{
				m_gamePaused = true;
				StopSlowmo();
				return;
			}
		}
		if(m_gamePaused && !App.Pause)
		{
			m_gamePaused = false;
		}
		*/
	}

	//------------------------------------------------------------
	// Protected Methods:
	//------------------------------------------------------------

	protected void ZoomCameraIn()
	{
		m_gameCamera.NotifySlowmoActivation(true, m_zoomValue);
	}

	protected void ZoomCameraOut()
	{
		m_gameCamera.NotifySlowmoActivation(false);
	}

	protected void StartSlowmo()
	{
		ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=green><b>start</b></color> -> starting");
						
		if(m_slowmo.RequestStartSlowmo(m_timescaleToApply))
		{
			// apply the zoom if necessary.
			if(m_enableZoomIn)
			{
				ZoomCameraIn();
			}

			// enable post process effects if necessary.
			/*if(m_postProcessAnimators.Length > 0)
			{
				EnablePostProcessEffects();
			}
			*/
			m_inProgress = true;
		}	
	}

	protected void StopSlowmo()
	{
		ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=red><b>stop</b></color> -> stopping");

		// reset the zoom if necessary.
		if(m_enableZoomIn)
		{
			ZoomCameraOut();
		}

		// disable post process effects if necessary.
		/*
		if(m_postProcessAnimators.Length > 0)
		{
			DisablePostProcessEffects();
		}
		*/
		m_slowmo.RequestStopSlowmo();
		m_inProgress = false;
	}

	protected void ShowDebugMessage(string message)
	{
		if(m_showDebugMessages)
		{
			Debug.Log(message);
		}
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public void Activate()
	{
        if (!enabled)
            return;

		// ignore if the slowmo is not available.
		if(!m_slowmo.canBeStarted)
		{
			ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=green><b>start</b></color> -> slowmo is not available");
			return;
		}

		// ignore if this trigger is already in progress.
		if(m_inProgress)
		{
			ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=green><b>start</b></color> -> already in progress");
			return;
		}

		// ignore if the child class will decide to not start.
		if(!InitializeSlowmoStart())
		{
			ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=green><b>start</b></color> -> child conditions not validated");
			return;
		}

		StartSlowmo();
	}

	public void Deactivate()
    {
        if (!enabled)
            return;

        // ignore if this trigger has not started.
        if (!m_inProgress)
		{
			ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=red><b>stop</b></color> -> not in progress");
			return;
		}

		if(!InitializeSlowmoStop())
		{
			ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=red><b>stop</b></color> -> the slowmo will be stopped by the child");
			return;
		}

		StopSlowmo();
	}

	//------------------------------------------------------------
	// Abstract Methods to Implement:
	//------------------------------------------------------------

	protected abstract bool InitializeSlowmoStart();
	protected abstract bool InitializeSlowmoStop();

	//------------------------------------------------------------
	// Virtual Methods:
	//------------------------------------------------------------

	protected virtual void EnablePostProcessEffects() { ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=green><b>enable post process effects</b></color> -> no actions defined"); }
	protected virtual void DisablePostProcessEffects() { ShowDebugMessage("<color=yellow><b>trigger</b></color> -> <color=red><b>disable post process effects</b></color> -> no actions defined"); }
}