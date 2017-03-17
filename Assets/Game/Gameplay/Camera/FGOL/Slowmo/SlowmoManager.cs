using UnityEngine;

public class SlowmoManager : MonoBehaviour
{

	//------------------------------------------------------------
	// Inspector Variables:
	//------------------------------------------------------------

	[SerializeField]
	protected bool m_showDebugMessages;
	[SerializeField]
	protected float m_cooldown = 5f;

	//------------------------------------------------------------
	// Private Variables:
	//------------------------------------------------------------

	private bool m_isQuitting;
	private bool m_inProgress;
	private bool m_externalStopRequested;
	private float m_startSlowmoTime;
	private float m_startCooldownTime;
	private float m_prevTimescale = 1f;
	private GameSceneController m_game;

	//------------------------------------------------------------
	// Public Properties:
	//------------------------------------------------------------

	public bool inProgress { get { return m_inProgress; } }

	public bool canBeStarted {
		get {
			return CooldownExpired() && !InProgress();
		}
	}

	public float slowmoTime
	{
		get
		{
			return Time.unscaledTime - m_startSlowmoTime;
		}
	}

	//------------------------------------------------------------
	// Unity Lifecycle:
	//------------------------------------------------------------

	protected void Awake()
	{
#if !UNITY_EDITOR
		m_showDebugMessages = false;
#endif
	}

	protected void Start()
	{
		// let's initialize so that the first time will always happen (also if happens at 0s after the level has been loaded).
		m_startCooldownTime = -m_cooldown - 1f;
		m_game = InstanceManager.gameSceneController;
	}

	protected void OnApplicationQuit()
	{
		m_isQuitting = true;
	}

	protected void OnDestroy()
	{
		if(!m_isQuitting && m_inProgress)
		{
			Debug.LogError("before: " + Time.timeScale);
			// reset the timescale.
			Time.timeScale = m_prevTimescale;
			Debug.LogError("after: " + Time.timeScale);
		}
	}

	//------------------------------------------------------------
	// Protected Methods:
	//------------------------------------------------------------

	protected void ShowDebugMessage(string message)
	{
		if(m_showDebugMessages)
		{
			Debug.Log(message);
		}
	}

	protected bool InProgress()
	{
		if(m_inProgress)
		{
			ShowDebugMessage("<color=cyan><b>slowmo</b></color> -> <color=green><b>start</b></color> -> already in progress");
		}
		return m_inProgress;
	}

	private bool CooldownExpired()
	{
		// check if the timeout has not expired.
		if(Time.unscaledTime - m_startCooldownTime < m_cooldown)
		{
			ShowDebugMessage("<color=yellow><b>slowmo</b></color> -> <color=green><b>start</b></color> -> cooldown not expired: " + (Time.unscaledTime - m_startCooldownTime) + "<color=white> of </color>" + m_cooldown);
			return false;
		}
		return true;
	}

	//------------------------------------------------------------
	// Public Methods:
	//------------------------------------------------------------

	public bool RequestStartSlowmo(float timescaleToApply)
	{
		// ignore if the cooldown has not expired.
		if(!CooldownExpired())
		{
			return false;
		}

		// ignore if the slowmo is already in progress.
		if(InProgress())
		{
			return false;
		}

		ShowDebugMessage("<color=cyan><b>slowmo</b></color> -> <color=green><b>start</b></color> -> starting");

		// apply the new timescale.
		m_prevTimescale = Time.timeScale;
		Time.timeScale = timescaleToApply;
		m_startSlowmoTime = Time.unscaledTime;
		m_inProgress = true;
		return true;
	}

	public void RequestStopSlowmo()
	{

		// TODO (MALH) Recover this
		if(m_inProgress)
		{
			ShowDebugMessage("<color=cyan><b>slowmo</b></color> -> <color=red><b>stop</b></color> -> stopping");

			// check if the game was paused and hadle the case.
			if (m_game != null && m_game.paused)
			{
				// reset the chached timescale in the App script so to not resume the game with the slowmo timescale.
				m_game.ResetCachedTimeScale();
			}
			else
			{
				// reset the timescale.
				Time.timeScale = m_prevTimescale;
			}
			// do other stuff...
			m_inProgress = false;
			m_startCooldownTime = Time.unscaledTime;
		}
		else
		{
			ShowDebugMessage("<color=cyan><b>slowmo</b></color> -> <color=red><b>stop</b></color> -> not in progress");
		}

	}
}