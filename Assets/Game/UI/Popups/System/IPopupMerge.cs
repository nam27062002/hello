using System;
using UnityEngine;

public abstract class IPopupMerge : PopupPauseBase 
{
	public PopupMergeProfilePill m_leftPill;
	public PopupMergeProfilePill m_rightPill;

    protected PersistenceComparatorSystem m_profile1;
    protected PersistenceComparatorSystem m_profile2;
   
    /* 
    // Uncomment to test from SC_Popups scene
    private void Awake()
    {
        Test();
    }
    */
}
