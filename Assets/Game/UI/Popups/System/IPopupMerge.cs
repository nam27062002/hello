using System;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public abstract class IPopupMerge : MonoBehaviour 
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
