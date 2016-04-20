using UnityEngine;
using System.Collections;

/// <summary>
/// Timer class independent of Unity's Update loop.
/// </summary>
public class DeltaTimer {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	private float m_startTime;
	private float m_stopTime;

	private float m_duration;
	private bool m_stopped;
	private bool m_loop;
	private CustomEase m_ease;
	private float m_speedMultiplier;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor
	/// </summary>
	public DeltaTimer() {
		m_startTime = 0;
		m_loop = false;
		m_duration = 0;
		m_stopTime = 0;
		m_stopped = false;
		m_ease = new CustomEase(CustomEase.EaseType.linear_01);
		m_speedMultiplier = 1;
	}

	/// <summary>
	/// Start the timer.
	/// </summary>
	/// <param name="_duration">Duration in seconds.</param>
	/// <param name="_loop">Loop?.</param>
	public virtual void Start(float _duration, bool _loop = false) {
		m_duration = _duration;
		m_stopTime = 0;
		m_loop = _loop;
		m_stopped = false;

		m_startTime = GetSystemTime();

		// Apply current speed multiplier to duration vars
		SetSpeedMultiplier(m_speedMultiplier);

	}

	/// <summary>
	/// Restart timer using its initial parameters.
	/// </summary>
	public virtual void Restart() {
		Start(m_duration, m_loop);
	}

	/// <summary>
	/// Stops the timer, subsequent calls to GetTime/GetDelta will return the same
	/// values until resume or start are called
	/// </summary>
	public virtual void Stop() {
		if(m_stopped) return;
			
		m_stopped = true;
		m_stopTime = GetSystemTime() - m_startTime;
	}

	/// <summary>
	/// Resume a previously stopped timer
	/// </summary>
	public void Resume() {
		if(!m_stopped) return;

		m_stopped = false;
		m_startTime = GetSystemTime() - m_stopTime;
	}

	/// <summary>
	/// Elapsed time since start in seconds
	/// </summary>
	/// <returns>The time elapsed since the timer started, in seconds.</returns>
	public virtual float GetTime() {
		if(!m_loop) {
			if(m_stopped) {
				return m_stopTime;
			} else {
				return GetSystemTime() - m_startTime;	
			}
		} else {
			if(m_stopped) {
				return (float)(m_stopTime % m_duration);
			} else {
				return (float)((GetSystemTime() - m_startTime) % m_duration);
			}
		}
	}

	/// <summary>
	/// Percentage of the timer completed, [0..1] range.
	/// Can be eeased by a given function.
	/// </summary>
	/// <returns>The delta.</returns>
	/// <param name="_ease">Ease curve function.</param>
	public float GetDelta(CustomEase.EaseType _ease = CustomEase.EaseType.linear_01) {
		m_ease.SetType(_ease);
		return m_ease.Get(GetTime(), m_duration);
	}

	/// <summary>
	/// Has the timer finished?
	/// </summary>
	/// <returns><c>true</c> if the timer has reached its end or it was never started</returns>
	public bool Finished() {
		return !m_loop && GetTime() > m_duration;
	}

	/// <summary>
	/// Is the timer stopped?
	/// </summary>
	/// <returns><c>true</c> if the <c>Stop()</c> method has been called.</returns>
	public bool IsStopped() {
		return m_stopped;
	}

	/// <summary>
	/// Add time to the timer (advance seconds), useful to start animation from a middle point for instance.
	/// </summary>
	/// <param name="_time">Time to be advanced in seconds.</param>
	public void AddTime(float _time) {
		m_startTime -= _time;
	}

	/// <summary>
	/// Force the timer to end.
	/// </summary>
	public void Finish() {
		m_loop = false;
		AddTime(m_duration);	// This will ensure that the Finished() method returns true.
	}

	/// <summary>
	/// Gets the time left.
	/// </summary>
	/// <returns>Remaining time in seconds.</returns>
	public float GetTimeLeft() {
		float d = GetDelta();
		if(d == 0) return m_duration;
		float t = GetTime();
		return ((1 - d) / d) * t;
	}

	/// <summary>
	/// Total duration of the timer.
	/// </summary>
	/// <returns>Duration of the timer in seconds.</returns>
	public float GetDuration() {
		// Correct speed multiplier modification
		return m_duration * m_speedMultiplier;
	}

	/// <summary>
	/// Accelerate/decelerate the time speed.
	/// </summary>
	/// <param name="_speedMultiplier">Factor.</param>
	public void SetSpeedMultiplier(float _speedMultiplier) {
		// Store new speed multiplier
		m_speedMultiplier = _speedMultiplier;
			
		// Change mDuration vars based on the new multiplier
		m_duration /= m_speedMultiplier;
	}
		
	//--------------------------------------------------------------------//
	// STATIC INIT METHODS												  //
	//--------------------------------------------------------------------//
	/// <summary>
	/// Current device's time in milliseconds.
	/// </summary>
	/// <returns>The system time in millis.</returns>
	static float GetSystemTime() {
		return Time.realtimeSinceStartup;
	}
}
