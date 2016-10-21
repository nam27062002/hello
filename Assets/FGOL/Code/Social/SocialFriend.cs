using System;
using UnityEngine;

public class SocialFriend
{
    private SocialFacade.Network m_network = SocialFacade.Network.Default;

    private string m_socialID = null;
    private string m_fgolID = null;

    private string m_name = null;
    private Texture2D m_profilePicture = null;

    public string ID
    {
        get { return m_fgolID; }
    }

    public string Name
    {
        get { return m_name; }
    }

    public SocialFriend(SocialFacade.Network network, string socialID, string fgolID, string name)
    {
        m_network = network;

        m_socialID = socialID;
        m_fgolID = fgolID;

        m_name = name;
    }

    public void GetProfilePicture(Action<Texture2D> onGetProfilePicture)
    {
        //TODO we may need a queue here as we could be asking for the same thing multiple times at once
        if(m_profilePicture != null)
        {
            onGetProfilePicture(m_profilePicture);
        }
        else
        {
            SocialFacade.Instance.GetProfilePicture(m_network, m_socialID, delegate(Texture2D profileImage)
            {
                m_profilePicture = profileImage;
                onGetProfilePicture(m_profilePicture);
            }, 64, 64);
        }
    }
}
