using FGOL.Save;
using FGOL.Save.SaveStates;
using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupMerge : MonoBehaviour 
{
	public const string PATH = "UI/Popups/Message/PF_PopupMerge";

	public PopupMergeProfilePill m_leftPill;
	public PopupMergeProfilePill m_rightPill;
    public GameObject m_closeBtn;

    ProgressComparatorSystem m_profile1;
    ProgressComparatorSystem m_profile2;

    private ConflictState ConflictState { get; set; }

    private Action<ConflictResult> OnResolved { get; set; }
   
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
    public void Setup(ConflictState conflictState, ProgressComparatorSystem _profile1, ProgressComparatorSystem _profile2, bool dismissable, Action<ConflictResult> onResolved) {
        ConflictState = conflictState;
        OnResolved = onResolved;

		// Initialize left pill with profile 1
		m_profile1 = _profile1;
		m_leftPill.Setup(_profile1, _profile2, conflictState == ConflictState.RecommendLocal);

		// Initialize right pill with the other profile
		m_profile2 = _profile2;
		m_rightPill.Setup(_profile2, _profile1, conflictState == ConflictState.RecommendCloud);

        m_closeBtn.SetActive(dismissable);        
    }

	public void OnUseLeftOption()
	{        
        // Use local save
        if (ConflictState != ConflictState.RecommendLocal && ConflictState != ConflictState.UserDecision)
        {
            ShowConfirmationPopup(ConflictResult.Local);
        }
        else
        {
            DismissPopup(ConflictResult.Local);
        }
    }

	public void OnUseRightOption()
	{
        // Use cloud save
        if (ConflictState != ConflictState.RecommendCloud && ConflictState != ConflictState.UserDecision)
        {
            ShowConfirmationPopup(ConflictResult.Cloud);
        }
        else
        {
            DismissPopup(ConflictResult.Cloud);
        }
    }

    private void ShowConfirmationPopup(ConflictResult result)
    {
        PersistenceManager.Popups_OpenMergeConfirmation(
            delegate ()
            {
                DismissPopup(result);
            }
        );
    }

    private void DismissPopup(ConflictResult result)
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
        DismissPopup(ConflictResult.Dismissed);
    }

    /// <summary>
    /// For testing purposes!
    /// </summary>
    public void Test() {
        //[DGR] Not needed anymore since we're using FGOL technology for persistence now        

        SaveData p1 = new SaveData("p1");
        TextAsset universe1 = Resources.Load("__TEMP/test_universe_1") as TextAsset;
        p1.LoadFromString(universe1.ToString());        
        ProgressComparatorSystem progress1 = new ProgressComparatorSystem();
        progress1.data = p1;
        progress1.Load();
        Debug.Log("PROFILE 1:\n" + p1.ToString());

        SaveData p2 = new SaveData("p1");
        TextAsset universe2 = Resources.Load("__TEMP/test_universe_2") as TextAsset;
        p2.LoadFromString(universe1.ToString());        
        ProgressComparatorSystem progress2 = new ProgressComparatorSystem();
        progress2.data = p2;
        progress2.Load();
        Debug.Log("PROFILE 2:\n" + p2.ToString());
     
		Setup(ConflictState.RecommendCloud, progress1, progress2, false, null);        
    }
}
