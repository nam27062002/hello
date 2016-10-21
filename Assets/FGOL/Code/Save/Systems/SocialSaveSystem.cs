using FGOL.Save;
using FGOL.Server;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SocialSaveSystem : SaveSystem
{
    private Dictionary<SocialFacade.Network, bool> socialSystemsIncentivised = null;
    private DateTime lastTimeNewsFeedWasVisited;

    private Dictionary<string, DateTime> m_giftTimes;

    public bool WasSocialSystemIncentivised(SocialFacade.Network network)
    {
        if (socialSystemsIncentivised != null && socialSystemsIncentivised.ContainsKey(network))
        {
            return socialSystemsIncentivised[network];
        }
        return false;
    }

    public void SetSocialSystemIncentivised(SocialFacade.Network network)
    {
        if (socialSystemsIncentivised != null)
        {
            socialSystemsIncentivised[network] = true;
        }
        
    }

    public SocialSaveSystem()
    {
        socialSystemsIncentivised = new Dictionary<SocialFacade.Network, bool>();
        for (SocialFacade.Network i = SocialFacade.Network.Facebook; i<SocialFacade.Network.Default; i++)
        {
            socialSystemsIncentivised[i] = false;
        }

        m_systemName = "Social";

        Reset();
    }

    public override void Reset()
    {
        socialSystemsIncentivised = new Dictionary<SocialFacade.Network, bool>();
        for (SocialFacade.Network i = SocialFacade.Network.Facebook; i < SocialFacade.Network.Default; i++)
        {
            socialSystemsIncentivised[i] = false;
        }
    }

    public override void Load()
    {
        try
        {
            for (SocialFacade.Network i = SocialFacade.Network.Facebook; i < SocialFacade.Network.Default; i++)
            {
                socialSystemsIncentivised[i] = GetBool(i.ToString() + "LoginIncentivised", false);
            }
        }
        catch(Exception e)
        {
            Debug.LogError("SocialSaveSystem (Load) :: Exception - " + e);
            throw new CorruptedSaveException(e);
        }

        lastTimeNewsFeedWasVisited = DateTime.Parse(GetString("lastTimeNewsFeedWasVisited", new DateTime(1970, 1, 1).ToString() ) );

        PushKey("Gifting");
        {
            m_giftTimes = new Dictionary<string, DateTime>();
            string socialIDs = GetString("socialIDs");
            string[] splittedSocialIDs = socialIDs.Split(',');

            PushKey("IDs");
            {
                for (int i = 0; i < splittedSocialIDs.Length; ++i)
                {
                    string socialID = splittedSocialIDs[i];
                    if (!string.IsNullOrEmpty(socialID))
                    {
                        DateTime lastTimeGifted = new DateTime(1970, 1, 1);
                        int timeStamp = GetInt(socialID, 0);
                        double doubleTimeStamp = (double)timeStamp;
                        lastTimeGifted = lastTimeGifted.AddSeconds(doubleTimeStamp);

                        //DateTime lastTimeGifted = DateTime.Parse(GetString(socialID, new DateTime(1970, 1, 1).ToString()));

                        m_giftTimes.Add(socialID, lastTimeGifted);
                    }
                }
            }
            PopKey();
        }
        PopKey();
    }

    public override void Save()
    {
        for (SocialFacade.Network i = SocialFacade.Network.Facebook; i < SocialFacade.Network.Default; i++)
        {
            SetBool(i.ToString() + "LoginIncentivised", socialSystemsIncentivised[i]);
        }

        SetString("lastTimeNewsFeedWasVisited", lastTimeNewsFeedWasVisited.ToString());

        PushKey("Gifting");
        {

            string socialIDs = "";

            PushKey("IDs");
            {
                foreach (KeyValuePair<string, DateTime> kvp in m_giftTimes)
                {
                    socialIDs = socialIDs + kvp.Key + ",";

                    DateTime epoch = new DateTime(1970, 1, 1 );
                    TimeSpan span = (kvp.Value - epoch);
                    int timestamp = (int)span.TotalSeconds;
                    SetInt(kvp.Key, timestamp);
                }
            }
            PopKey();

            if (socialIDs.Length > 0)
            {
                socialIDs = socialIDs.Substring(0, socialIDs.Length - 1);
            }
            SetString("socialIDs", socialIDs);
        }
        PopKey();

    }

    public override bool Upgrade()
    {
        return false;
    }

    public override void Downgrade()
    {
    }

    public void  SetLastTimeNewsFeedWasVisited( DateTime newDate )
    {
        lastTimeNewsFeedWasVisited = newDate;
    }

    public DateTime GetLastTimeNewsFeedWasVisited( )
    {
        return lastTimeNewsFeedWasVisited;
    }

    public void SetGiftDate( string socialID, DateTime date )
    {
        if( m_giftTimes.ContainsKey(socialID) )
        {
            m_giftTimes[socialID] = date;
        }
        else
        {
            m_giftTimes.Add(socialID, date);
        }
    }

    public DateTime GetGiftDate( string socialID )
    {
        if (m_giftTimes.ContainsKey(socialID))
        {
            return m_giftTimes[socialID];
        }
        else
        {
            return new DateTime(1970, 1, 1);
        }
    }
    public void ResetAllGiftDates( )
    {
        List<string> keys = new List<string>(m_giftTimes.Keys);
        foreach( string key in keys )
        {
            m_giftTimes[key] = new DateTime(1970, 1, 1);
        }        
    }
}
