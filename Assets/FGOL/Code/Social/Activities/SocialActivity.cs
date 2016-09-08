using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class SocialActivity
{
    protected int m_relevance = -1;
    protected string m_fgolID = null;
    protected SocialFacade.Network m_socialNetwork = SocialFacade.Network.Default;
    protected string m_socialID = null;
    protected bool m_interactive = false;

    public SocialFacade.Network Network
    {
        get { return m_socialNetwork; }
    }

    public string SocialID
    {
        get { return m_socialID; }
    }

    public string FgolID
    {
        get { return m_fgolID; }
    }

    public bool IsInteractive
    {
        get { return m_interactive; }
    }

    public SocialActivity(Dictionary<string, object> json = null)
    {
        Init(json);
    }

    public void Init(Dictionary<string, object> json = null)
    {
        if(json != null)
        {
            if(json.ContainsKey("relevance"))
            {
                m_relevance = Convert.ToInt32(json["relevance"]);
            }

            if(json.ContainsKey("network"))
            {
                try
                {
                    m_socialNetwork = (SocialFacade.Network)Enum.Parse(typeof(SocialFacade.Network), json["network"] as string, true);
                }
                catch(Exception)
                {
                    Debug.LogWarning("SocialActivity :: Unable to parse SocialFacade.Network Enum!");
                    m_socialNetwork = SocialFacade.Network.Default;
                }
            }

            if(json.ContainsKey("socialID"))
            {
                m_socialID = json["socialID"] as string;
            }

            if(json.ContainsKey("fgolID"))
            {
                m_fgolID = json["fgolID"] as string;
            }
        }
    }

    public abstract string GetActivityType();
    public abstract string GetActivityDescription();

    public Dictionary<string, string> GetActivityParams()
    {
        return new Dictionary<string, string>
        {
            { "relevance", m_relevance.ToString() }
        };
    }

    public virtual void OnClick(Action<bool, bool> onClickActionFinished)
    {
        onClickActionFinished(true, false);
    }
}