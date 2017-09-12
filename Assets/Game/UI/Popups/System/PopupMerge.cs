using System;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupMerge : MonoBehaviour 
{
	public const string PATH = "UI/Popups/Message/PF_PopupMerge";

	public PopupMergeProfilePill m_leftPill;
	public PopupMergeProfilePill m_rightPill;
    public GameObject m_closeBtn;

    PersistenceComparatorSystem m_profile1;
    PersistenceComparatorSystem m_profile2;

    private PersistenceStates.EConflictState ConflictState { get; set; }

    private Action<PersistenceStates.EConflictResult> OnResolved { get; set; }
   
    /* 
    // Uncomment to test from SC_Popups scene
    private void Awake()
    {
        Test();
    }
    */

    /// <summary>
    /// Setup the popup with the two given user profiles.
    /// </summary>
    /// <param name="conflictState">The state of conflict between the local and the cloud profiles that has trigged this popup</param>
    /// <param name="_profile1">First profile, usually the player's current profile.</param>
    /// <param name="_profile2">Second profile, usually the profile received from the server.</param>
    public void Setup(PersistenceStates.EConflictState conflictState, PersistenceComparatorSystem _profile1, PersistenceComparatorSystem _profile2, 
                      bool dismissable, Action<PersistenceStates.EConflictResult> onResolved) {
        ConflictState = conflictState;
        OnResolved = onResolved;

		// Initialize left pill with profile 1
		m_profile1 = _profile1;
		m_leftPill.Setup(_profile1, _profile2, conflictState == PersistenceStates.EConflictState.RecommendLocal);

		// Initialize right pill with the other profile
		m_profile2 = _profile2;
		m_rightPill.Setup(_profile2, _profile1, conflictState == PersistenceStates.EConflictState.RecommendCloud);

        m_closeBtn.SetActive(dismissable);        
    }

	public void OnUseLeftOption()
	{        
        // Use local save
        if (ConflictState != PersistenceStates.EConflictState.RecommendLocal && ConflictState != PersistenceStates.EConflictState.UserDecision)
        {
            ShowConfirmationPopup(PersistenceStates.EConflictResult.Local);
        }
        else
        {
            DismissPopup(PersistenceStates.EConflictResult.Local);
        }
    }

	public void OnUseRightOption()
	{
        // Use cloud save
        if (ConflictState != PersistenceStates.EConflictState.RecommendCloud && ConflictState != PersistenceStates.EConflictState.UserDecision)
        {
            ShowConfirmationPopup(PersistenceStates.EConflictResult.Cloud);
        }
        else
        {
            DismissPopup(PersistenceStates.EConflictResult.Cloud);
        }
    }

    private void ShowConfirmationPopup(PersistenceStates.EConflictResult result)
    {
        PersistenceManager.Popups_OpenMergeConfirmation(
            delegate ()
            {
                DismissPopup(result);
            }
        );
    }

    private void DismissPopup(PersistenceStates.EConflictResult result)
    {
        if (OnResolved != null)
        {
            OnResolved(result);
        }

        // Close and destroy popup
        GetComponent<PopupController>().Close(true);
    }

    /// <summary>
    /// Called by the player when the user clicks on the close button
    /// </summary>
    public void OnDismiss()
    {
        DismissPopup(PersistenceStates.EConflictResult.Dismissed);
    }

    /// <summary>
    /// For testing purposes!
    /// </summary>
    public void Test() {
        //[DGR] Not needed anymore since we're using FGOL technology for persistence now        

        PersistenceData p1 = new PersistenceData("p1");
        TextAsset universe1 = Resources.Load("__TEMP/test_universe_1") as TextAsset;
        p1.LoadFromString(universe1.ToString());        
        PersistenceComparatorSystem progress1 = new PersistenceComparatorSystem();
        progress1.data = p1;
        progress1.Load();
        Debug.Log("PROFILE 1:\n" + p1.ToString());

        PersistenceData p2 = new PersistenceData("p1");
        TextAsset universe2 = Resources.Load("__TEMP/test_universe_2") as TextAsset;
        p2.LoadFromString(universe1.ToString());
        PersistenceComparatorSystem progress2 = new PersistenceComparatorSystem();
        progress2.data = p2;
        progress2.Load();
        Debug.Log("PROFILE 2:\n" + p2.ToString());
     
		Setup(PersistenceStates.EConflictState.RecommendCloud, progress1, progress2, false, null);        
    }
}
