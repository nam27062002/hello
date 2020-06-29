using System;
using UnityEngine;

public class PopupMergeDNA : IPopupMerge 
{
    public const string PATH = "UI/Popups/Message/PF_PopupMergeDNA";

    public Action OnKeep = null;
    public Action OnRestore = null;

    /// <summary>
    /// Initialize the popup with the given data.
    /// </summary>
    /// <param name="_localProgress">Current progress data stored in the device.</param>
    /// <param name="_cloudProgress">Recovered progress data from the server.</param>
    /// <param name="_onKeep">Action to perform when keeping the local progress.</param>
    /// <param name="_onRestore">Action to perform when restoring the server progress.</param>
    public void Setup(PersistenceComparatorSystem _localProgress, PersistenceComparatorSystem _cloudProgress, PersistenceStates.EConflictState _conflictState, Action _onKeep, Action _onRestore) {
        // Store callbacks
        OnKeep = _onKeep;
        OnRestore = _onRestore;

        bool useCloud = (_conflictState == PersistenceStates.EConflictState.UseCloud || _conflictState == PersistenceStates.EConflictState.RecommendCloud);

		// Initialize left pill with current progress
		m_profile1 = _localProgress;
		m_leftPill.Setup(_localProgress, _cloudProgress, !useCloud);  // Force highlighting the remote progress

		// Initialize right pill with recovered progress
		m_profile2 = _cloudProgress;
		m_rightPill.Setup(_cloudProgress, _localProgress, useCloud);  // Force highlighting the remote progress
    }

    /// <summary>
    /// Keep current profile button has been pressed.
    /// </summary>
    public void OnKeepButton() {
        // Propagate result
        if(OnKeep != null) {
            OnKeep();
        }

        // Not allowed to close upon hitting Keep because there's some flow (no connection/server error) to handle with this popup open
        //GetComponent<PopupController>().Close(true);
    }

    /// <summary>
    /// Restore previous progress button has been pressed.
    /// </summary>
    public void OnRestoreButton() {
        // Propagate result
        if(OnRestore != null) {
            OnRestore();
        }

        // Close the popup!
        GetComponent<PopupController>().Close(true);
    }
}
