using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerBaseTime : TrackerBase, IBroadcastListener {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	protected bool m_updateTime;
	private float m_deltaTime;



	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBaseTime() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBaseTime() {

	}

    public virtual void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        
    }
    


	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	public override void Clear() {
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);

		m_updateTime = false;
		m_deltaTime = 0f;

		base.Clear();
	}

	/// <summary>
	/// Localizes and formats a value according to this tracker's type
	/// (i.e. "52", "500 meters", "10 minutes").
	/// </summary>
	/// <returns>The localized and formatted value for this tracker's type.</returns>
	/// <param name="_value">Value to be formatted.</param>
	override public string FormatValue(long _value) {
		// Format value as time
		// [AOC] Different formats for global events!
		TimeUtils.EFormat format = TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES;
		if(m_mode == Mode.GLOBAL_EVENT) {
			format = TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES;
		}
		return TimeUtils.FormatTime(_value, format, 3, TimeUtils.EPrecision.DAYS);
	}

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	override public long RoundTargetValue(long _targetValue) {
		return _targetValue;
	}

	/// <summary>
	/// Gets the progress string, custom formatted based on tracker type.
	/// </summary>
	/// <returns>The progress string properly formatted.</returns>
	/// <param name="_currentValue">Current value to be evaluated.</param>
	/// <param name="_targetValue">Target value to be evaulated.</param>
	/// <param name="_showTarget">Show target value? (i.e. "25/40"). Some types might override this setting if not appliable.</param>
	public override string GetProgressString(long _currentValue, long _targetValue, bool _showTarget = true) {
		//Time trackers will show a percentage as a progress string
		ulong p =  ((ulong)_currentValue * 100) / (ulong)_targetValue;
		return StringUtils.FormatNumber(p, 0, false) + "%";
	}

	/// <summary>
	/// Sets the initial value for the tracker.
	/// Doesn't perform any check or trigger any event.
	/// Use for initialization/reset/restore persistence.
	/// Use also by heirs to reset any custom vars that needed to be reset.
	/// </summary>
	/// <param name="_initialValue">Initial value.</param>
	override public void InitValue(long _initialValue) {
		// Call parent
		base.InitValue(_initialValue);

		// Reset local vars
		m_deltaTime = 0f;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	protected virtual void OnGameStarted() {
		m_updateTime = false;
		m_deltaTime = 0f;
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anything
		if (m_updateTime) {	
			m_deltaTime += Time.deltaTime;

			while (m_deltaTime > 1f) {
				currentValue++;
				m_deltaTime -= 1f;
			}
		}
	}
}
