// IAssetsDownloadFlowProgressBar.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/03/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System.Text;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single widget to display the progress of a download.
/// Must be manually refreshed from outside.
/// </summary>
public class AssetsDownloadFlowProgressBar : MonoBehaviour, IBroadcastListener {
	//------------------------------------------------------------------------//
	// ENUM															          //
	//------------------------------------------------------------------------//

    public enum State
    {
        NOT_INITIALIZED, IN_PROGRESS, COMPLETED, ERROR
    }

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed members
	[SerializeField] private Slider m_progressBar = null;
    [SerializeField] protected GameObject m_progressBarFill;

    [SerializeField] private TextMeshProUGUI m_progressText = null;
    [SerializeField] public State m_state;


    // Internal
    private StringBuilder m_sb = new StringBuilder();
	private string m_localizedSeconds = "";

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_localizedSeconds = LocalizationManager.SharedInstance.Localize("TID_GEN_TIME_SECONDS_ABBR");
		Broadcaster.AddListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		Broadcaster.RemoveListener(BroadcastEventType.LANGUAGE_CHANGED, this);
	}

	/// <summary>
	/// Refresh to display the progress of the given download handler.
	/// </summary>
	/// <param name="_handle">Handle to be used.</param>
	public void Refresh(Downloadables.Handle _handle) {
		// Just for safety
		if(_handle == null) return;

		// Progress Bar
		if(m_progressBar != null) {
			m_progressBar.normalizedValue = _handle.Progress;
		}

        // Progress Text
        if (m_progressText != null)
        {
            m_sb.Length = 0;

            // Download progress
            // 300 KB/800 MB
            m_sb.Append(LocalizationManager.SharedInstance.Localize(
                "TID_FRACTION",
                StringUtils.FormatFileSize(_handle.GetDownloadedBytes(), 2),
                StringUtils.FormatFileSize(_handle.GetTotalBytes(), 0)  // No decimals looks better for total size
            ));

            // Speed - only if no error
            if (_handle.GetError() == Downloadables.Handle.EError.NONE)
            {
                // (256 KB/s)
                m_sb.Append(LocalizationManager.SharedInstance.ReplaceParameters(" (%U0/%U1)",
                    StringUtils.FormatFileSize(_handle.GetSpeed(), 2),
                    m_localizedSeconds
                ));

            }

            m_progressText.text = m_sb.ToString();
        }


        // Update colors and progress bar elements
        if (_handle.GetError() != Downloadables.Handle.EError.NONE)
        {
            RefreshProgressBarElements(State.ERROR);
        } else
        {
            if (_handle.Progress == 1)
            {
                RefreshProgressBarElements(State.COMPLETED);
            } 
            else
            {
                RefreshProgressBarElements(State.IN_PROGRESS);
            }
            
		}


    }


    /// <summary>
    /// Show the proper progress bar elements and color them
    /// according to the new state of the download.
    /// </summary>
    /// <param name="_handle">Handle to be used.</param>
    public virtual void RefreshProgressBarElements (State _newState)
    {
        
        // If state has changed
        if (m_state != _newState || m_state == State.NOT_INITIALIZED)
        {

            // Set the proper colors according to the new state of the progress bar
            switch (_newState)
            {
                case State.IN_PROGRESS:
                    {
                        m_progressBarFill.GetComponent<UIGradient>().SetValues(AssetsDownloadFlowSettings.progressBarDownloadingColor);
                        break;
                    }
                case State.COMPLETED:
                    {
                        m_progressBarFill.GetComponent<UIGradient>().SetValues(AssetsDownloadFlowSettings.progressBarFinishedColor);
                        break;
                    }
                default: // Error case
                    {
                        m_progressBarFill.GetComponent<UIGradient>().SetValues(AssetsDownloadFlowSettings.progressBarErrorColor);
                        break;
                    }
            }

        }

        m_state = _newState;

    }

	//------------------------------------------------------------------------//
	// DEBUG_METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Force a new state of the progress bar
	/// </summary>
	/// <param name="_state">New state</param>
	public void DEBUG_SetState(State _state) {
       
        // Change the state, and so the color of the elements
        RefreshProgressBarElements(_state);

	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Broadcast listener.
	/// </summary>
	/// <param name="_eventType">Event type.</param>
	/// <param name="_broadcastEventInfo">Event data.</param>
	public void OnBroadcastSignal(BroadcastEventType _eventType, BroadcastEventInfo _broadcastEventInfo) {
		switch(_eventType) {
			case BroadcastEventType.LANGUAGE_CHANGED: {
				// Update cached seconds translation
				m_localizedSeconds = LocalizationManager.SharedInstance.Localize("TID_GEN_TIME_SECONDS_ABBR");
			} break;
		}
	}
}