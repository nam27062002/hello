using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controller for the friend referral counter
/// </summary>
public class FriendCounter : MonoBehaviour
{
    
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



    //Debug
    public int m_friendsAmount = 0;
    public int m_maxFriendsAmount = 10;
    public List<int> m_friendsMilestones;


    // Start is called before the first frame update
    void Start()
    {
        m_friendsMilestones.Add(5);
        m_friendsMilestones.Add(10);
    }

    // Update is called once per frame
    void Update()
    {

        bool highlighted = IsMilestone(m_friendsAmount);

        if (m_friendIcon != null)
            m_friendIcon.SetActive(!highlighted);

        if (m_friendIconHighlighted != null)
            m_friendIconHighlighted.SetActive(highlighted);

        Color color = highlighted ? m_textColorHighlighted : m_textColor;

        string text = "<color=" + color.ToHexString("#", false) + ">" +
            m_friendsAmount.ToString() +
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
        return m_friendsMilestones.Contains(_amount);
    }
}
