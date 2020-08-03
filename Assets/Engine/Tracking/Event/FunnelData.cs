using UnityEngine;

public abstract class FunnelData : TrackingEventData {
	
	protected string[] m_stepNames;
	private float[] m_stepTimeStamp;

	private int m_stepCount;
	public int stepCount { get{ return m_stepCount; } }
	private int m_currentStep;
    public int currentStep { get{ return (m_currentStep < 0)? 0 : m_currentStep; } }

	protected FunnelData(string _name) : base(_name) { }

	protected void Setup(int _stepCount) {
		m_stepNames = new string[_stepCount];
		m_stepTimeStamp = new float[_stepCount];

		m_stepCount = _stepCount;
		Reset();
	}

	protected void SetupStep(int _index, string _name) {
		m_stepNames[_index] = _name;
		ResetStep(_index);
	}

	public void Reset() {
		for (int i = 0; i < m_stepCount; ++i) {
			ResetStep(i);
		}
		m_currentStep = -1;
	}

	private void ResetStep(int _index) {
		m_stepTimeStamp[_index] = 0f;
	}

	protected string GetStepName(int _index) {
		return m_stepNames[_index];
	}

	/// <summary>
	/// Gets the duration in milliseconds of the step.
	/// </summary>
	/// <returns>The step duration in milliseconds.</returns>
	/// <param name="_index">Setp index.</param>
	protected int GetStepDuration(int _index) {
		if (_index > m_currentStep + 1) {
			Debug.LogWarning("[" + name + "] Game is Notifying the step " + m_stepNames[_index] + " but the expected step is " + m_stepNames[m_currentStep + 1]);
		}

		float now = Time.realtimeSinceStartup;
		float last = now;

		if (m_currentStep >= 0) {
			last = m_stepTimeStamp[m_currentStep];
		}

		m_stepTimeStamp[_index] = now;
		m_currentStep = _index;

		return (int)((now - last) * 1000);
	}

	/// <summary>
	/// Gets the step total time in milliseconds since the start of the funnel.
	/// </summary>
	/// <returns>The step total time in milliseconds.</returns>
	/// <param name="_index">Step index.</param>
	protected int GetStepTotalTime(int _index) {
		return (int)((m_stepTimeStamp[_index] - m_stepTimeStamp[0]) * 1000);
	}

}
