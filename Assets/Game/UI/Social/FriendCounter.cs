using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controller for the friend referral counter
/// </summary>
public class FriendCounter : MonoBehaviour
{

    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    [SerializeField]
    private GameObject m_friendIcon;

    [SerializeField]
    private GameObject m_friendIconHighlighted;

    [SerializeField]
    private TextMeshProUGUI m_counterText;

    [SerializeField]
    private Color m_textColor;

    [SerializeField]
    private Color m_textColorHighlighted;

    //Cache
    private OfferPackReferral m_offerPack;
    private int m_friendsCount;
    private int m_maxFriendsAmount;
    private int [] m_friendsMilestones;

    //------------------------------------------------------------------------//
    // GENERIC METHODS      												  //
    //------------------------------------------------------------------------//
    // Start is called before the first frame update
    void Start()
    {
    }

    /// <summary>
    /// Check every frame
    /// </summary>
    private void Update()
    {
        if (m_offerPack != null)
        {
            if (m_friendsCount != UsersManager.currentUser.totalReferrals)
            {
                // If friends count changed, refresh the visuals
                m_friendsCount = UsersManager.currentUser.totalReferrals;
                Refresh();
            }
        }
    }


    //------------------------------------------------------------------------//
    // OTHER METHODS            											  //
    //------------------------------------------------------------------------//
    public void InitFromOfferPack (OfferPackReferral _offer = null)
    {
        m_offerPack = _offer;

        if (m_offerPack != null)
        {
            m_friendsCount = UsersManager.currentUser.totalReferrals;

            // Get the milestones
            m_friendsMilestones = _offer.GetFriendsRequired();
            m_maxFriendsAmount = m_friendsMilestones[m_friendsMilestones.Length - 1];

            
            Refresh();
        }
    }

    /// <summary>
    /// Refresh the visuals
    /// </summary>
    private void Refresh()
    {

        // Friends progression is cyclic so when reaches the final milestone, it starts from the begining
        if (m_friendsCount > m_maxFriendsAmount)
            m_friendsCount = (m_friendsCount - 1) % m_maxFriendsAmount + 1;

        bool highlighted = IsMilestone(m_friendsCount);

        if (m_friendIcon != null)
            m_friendIcon.SetActive(!highlighted);

        if (m_friendIconHighlighted != null)
            m_friendIconHighlighted.SetActive(highlighted);

        Color color = highlighted ? m_textColorHighlighted : m_textColor;

        string text = "<color=" + color.ToHexString("#", false) + ">" +
            m_friendsCount.ToString() +
            " / " +
            m_maxFriendsAmount.ToString();

        m_counterText.text = text;
        

    }

    /// <summary>
    /// Check if there is a reward for a certain amount of friends invites
    /// </summary>
    /// <param name="_amount">The amount of friends confirmed</param>
    /// <returns></returns>
    private bool IsMilestone(int _amount)
    {
        for (int i = 0; i< m_friendsMilestones.Length; i++)
        {
            if (m_friendsMilestones[i] == _amount)
                return true;
        }
        return false;
    }
}
