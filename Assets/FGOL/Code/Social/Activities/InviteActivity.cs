using System.Collections.Generic;

public class InviteActivity : SocialActivity
{
    public InviteActivity(Dictionary<string, object> json = null)
        : base(json)
    {
        if(m_relevance < 0)
        {
            m_relevance = 1;
        }
    }

    public override string GetActivityType()
    {
        return "Invite";
    }

    public override string GetActivityDescription()
    {
        return "Sent an invite!";
    }
}
